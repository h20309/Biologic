import asyncio
import math
import json
import time
from concurrent.futures import Future, ThreadPoolExecutor
from collections import deque
from typing import Any, Dict, List, Optional

import pandas as pd
import plotly.graph_objects as go
import streamlit as st
from asyncua import Client

from opcua_connect import (
    DEFAULT_CHANNEL_NODE_ID,
    DEFAULT_MONITOR_NODES,
    discover_biologic_layout,
    get_available_biologic_methods,
    get_biologic_channel_node_id,
    get_opcua_node,
    get_sequence_result_marker,
    is_biologic_channel_active,
    parse_node_id,
    refresh_dashboard,
    render_status,
    resolve_biologic_method_target,
    resolve_monitor_nodes,
    set_status,
)


DISCOVERY_REQUEST_TIMEOUT = 1.5
STALE_FINALIZING_TIMEOUT_SECONDS = 15.0
TERMINAL_PHASES = {"completed", "failed", "warning", "canceled"}
METHOD_DISPLAY_ORDER = [
    "ConnectDevice",
    "DisconnectDevice",
    "LoadFirmware",
    "LoadTechnique",
    "RunOCV",
    "RunCV",
    "RunPEIS",
    "RunGEIS",
    "Charge",
    "Discharge",
    "ForceStopChannel",
]
METHOD_EXECUTION_RULES: Dict[str, Dict[str, Any]] = {
    "ConnectDevice": {"capability": "submission-only", "channel_field": None, "result_marker": None},
    "DisconnectDevice": {"capability": "submission-only", "channel_field": None, "result_marker": None},
    "LoadFirmware": {"capability": "submission-only", "channel_field": None, "result_marker": None},
    "LoadTechnique": {"capability": "submission-only", "channel_field": None, "result_marker": None},
    "RunOCV": {"capability": "state-tracked", "channel_field": "ChannelIndex", "result_marker": None},
    "RunCV": {"capability": "state-tracked", "channel_field": "ChannelIndex", "result_marker": None},
    "RunPEIS": {"capability": "state+result-tracked", "channel_field": "ChannelIndex", "result_marker": "eis"},
    "RunGEIS": {"capability": "state+result-tracked", "channel_field": "ChannelIndex", "result_marker": "eis"},
    "Charge": {"capability": "state-tracked", "channel_field": "ChannelIndex", "result_marker": "charge"},
    "Discharge": {"capability": "state-tracked", "channel_field": "ChannelIndex", "result_marker": None},
    "ForceStopChannel": {"capability": "submission-only", "channel_field": "ChannelIndex", "result_marker": None},
}
METHOD_CALL_EXECUTOR = ThreadPoolExecutor(max_workers=4)

METHOD_SCHEMAS: Dict[str, List[Dict[str, Any]]] = {
    "ConnectDevice": [],
    "DisconnectDevice": [],
    "LoadFirmware": [],
    "LoadTechnique": [],
    "RunOCV": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {"name": "Duration_s", "type": "float", "label": "Duration (s)", "default": 60.0, "min": 0.1, "step": 1.0},
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "RunCV": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {"name": "StartVoltage_V", "type": "float", "label": "Start Voltage (V)", "default": 0.0, "step": 0.01},
        {"name": "Vertex1_V", "type": "float", "label": "Vertex 1 (V)", "default": 1.0, "step": 0.01},
        {"name": "Vertex2_V", "type": "float", "label": "Vertex 2 (V)", "default": -1.0, "step": 0.01},
        {"name": "ScanRate_V_s", "type": "float", "label": "Scan Rate (V/s)", "default": 0.1, "min": 0.0001, "step": 0.01},
        {"name": "NCycles", "type": "int", "label": "Cycles", "default": 1, "min": 1, "max": 100},
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "RunPEIS": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {
            "name": "InitialFrequency_Hz",
            "type": "float",
            "label": "Initial Frequency (Hz)",
            "default": 100000.0,
            "min": 0.001,
            "step": 100.0,
        },
        {
            "name": "FinalFrequency_Hz",
            "type": "float",
            "label": "Final Frequency (Hz)",
            "default": 0.1,
            "min": 0.001,
            "step": 0.1,
        },
        {"name": "FrequencyPoints", "type": "int", "label": "Frequency Points", "default": 30, "min": 1, "max": 500},
        {"name": "DcVoltage_V", "type": "float", "label": "DC Voltage (V)", "default": 0.0, "step": 0.01},
        {
            "name": "AcAmplitude_V",
            "type": "float",
            "label": "AC Amplitude (V)",
            "default": 0.01,
            "min": 0.000001,
            "step": 0.001,
        },
        {"name": "Duration_step", "type": "float", "label": "Duration Step", "default": 0.0, "min": 0.0, "step": 0.1},
        {"name": "Record_every_dT", "type": "float", "label": "Record Every dT", "default": 0.0, "min": 0.0, "step": 0.1},
        {"name": "Record_every_dI", "type": "float", "label": "Record Every dI", "default": 0.0, "min": 0.0, "step": 0.0001},
        {"name": "Average_N_times", "type": "int", "label": "Average N Times", "default": 3, "min": 1, "max": 100},
        {"name": "Correction", "type": "bool", "label": "Correction", "default": True},
        {"name": "Wait_for_steady", "type": "float", "label": "Wait For Steady", "default": 0.0, "min": 0.0, "step": 0.1},
        {"name": "sweep", "type": "bool", "label": "Sweep", "default": False},
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "RunGEIS": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {
            "name": "InitialFrequency_Hz",
            "type": "float",
            "label": "Initial Frequency (Hz)",
            "default": 100000.0,
            "min": 0.001,
            "step": 100.0,
        },
        {
            "name": "FinalFrequency_Hz",
            "type": "float",
            "label": "Final Frequency (Hz)",
            "default": 0.1,
            "min": 0.001,
            "step": 0.1,
        },
        {"name": "FrequencyPoints", "type": "int", "label": "Frequency Points", "default": 30, "min": 1, "max": 500},
        {"name": "DcCurrent_A", "type": "float", "label": "DC Current (A)", "default": 0.0, "step": 0.0001},
        {
            "name": "AcAmplitude_A",
            "type": "float",
            "label": "AC Amplitude (A)",
            "default": 0.0001,
            "min": 0.0000001,
            "step": 0.0001,
        },
        {"name": "Duration_step", "type": "float", "label": "Duration Step", "default": 0.0, "min": 0.0, "step": 0.1},
        {"name": "Record_every_dT", "type": "float", "label": "Record Every dT", "default": 0.0, "min": 0.0, "step": 0.1},
        {"name": "Record_every_dE", "type": "float", "label": "Record Every dE", "default": 0.0, "min": 0.0, "step": 0.0001},
        {"name": "Average_N_times", "type": "int", "label": "Average N Times", "default": 3, "min": 1, "max": 100},
        {"name": "Correction", "type": "bool", "label": "Correction", "default": True},
        {"name": "Wait_for_steady", "type": "float", "label": "Wait For Steady", "default": 0.0, "min": 0.0, "step": 0.1},
        {"name": "sweep", "type": "bool", "label": "Sweep", "default": False},
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "Charge": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {"name": "Voltage_V", "type": "float", "label": "Target Voltage (V)", "default": 4.2, "step": 0.01},
        {"name": "Duration_s", "type": "float", "label": "Duration (s)", "default": 3600.0, "min": 0.1, "step": 1.0},
        {
            "name": "RecordInterval_s",
            "type": "float",
            "label": "Record Interval (s)",
            "default": 1.0,
            "min": 0.1,
            "step": 0.1,
        },
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "Discharge": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {"name": "Voltage_V", "type": "float", "label": "Cutoff Voltage (V)", "default": 2.5, "step": 0.01},
        {"name": "Duration_s", "type": "float", "label": "Duration (s)", "default": 3600.0, "min": 0.1, "step": 1.0},
        {
            "name": "RecordInterval_s",
            "type": "float",
            "label": "Record Interval (s)",
            "default": 1.0,
            "min": 0.1,
            "step": 0.1,
        },
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "ForceStopChannel": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
    ],
}


def normalize_optional_text(value: str) -> Optional[str]:
    stripped = value.strip()
    return stripped or None


def is_numeric_value(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool)


def safe_parse_json(value: Any) -> Any:
    if value in (None, "", b""):
        return None
    if isinstance(value, (dict, list)):
        return value
    if isinstance(value, str):
        try:
            return json.loads(value)
        except json.JSONDecodeError:
            return None
    return None


def pull_charge_snapshot(server_url: str, delay_seconds: float = 0.0) -> None:
    if delay_seconds > 0:
        time.sleep(delay_seconds)

    refresh_dashboard(server_url)
    values = (st.session_state.biologic_snapshot or {}).get("values", {})
    marker = values.get("LatestChargeUpdatedAt")
    if marker:
        st.session_state["last_charge_fetch_marker"] = str(marker)


def pull_eis_snapshot(server_url: str, delay_seconds: float = 0.0) -> None:
    if delay_seconds > 0:
        time.sleep(delay_seconds)

    refresh_dashboard(server_url)
    values = (st.session_state.biologic_snapshot or {}).get("values", {})
    marker = values.get("LatestEISTimestamp") or values.get("LatestEISRunId")
    if marker:
        st.session_state["last_eis_fetch_marker"] = str(marker)


def normalize_method_result(result: Any) -> Any:
    if isinstance(result, list) and len(result) == 1:
        return result[0]
    return result


def get_method_rule(method_name: str) -> Dict[str, Any]:
    return METHOD_EXECUTION_RULES.get(
        method_name,
        {"capability": "submission-only", "channel_field": "ChannelIndex", "result_marker": None},
    )


def get_method_display_order(method_name: str) -> int:
    try:
        return METHOD_DISPLAY_ORDER.index(method_name)
    except ValueError:
        return len(METHOD_DISPLAY_ORDER)


def get_sorted_method_names(server_url: str) -> List[str]:
    discovered = get_available_biologic_methods(server_url)
    method_names = set(discovered) if discovered else set(METHOD_SCHEMAS.keys())
    return sorted(method_names, key=lambda name: (get_method_display_order(name), name))


def get_active_sequence_run() -> Optional[Dict[str, Any]]:
    active_run = st.session_state.get("active_sequence_run")
    return active_run if isinstance(active_run, dict) else None


def has_pending_sequence_run() -> bool:
    active_run = get_active_sequence_run()
    if not active_run:
        return False
    return active_run.get("phase") not in TERMINAL_PHASES


def get_snapshot_values() -> Dict[str, Any]:
    return (st.session_state.biologic_snapshot or {}).get("values", {})


def build_result_markers(values: Dict[str, Any]) -> Dict[str, Optional[str]]:
    return {
        "eis": get_sequence_result_marker(values, "eis"),
        "charge": get_sequence_result_marker(values, "charge"),
    }


def marker_changed(baseline: Optional[str], current: Optional[str]) -> bool:
    return current not in (None, "") and current != baseline


def sequence_result_marker_advanced(active_run: Optional[Dict[str, Any]]) -> bool:
    if not active_run:
        return False

    rule = get_method_rule(active_run.get("method", ""))
    result_marker = rule.get("result_marker")
    if not result_marker:
        return False

    baseline_marker = (active_run.get("baseline_markers") or {}).get(result_marker)
    current_marker = (active_run.get("current_markers") or {}).get(result_marker)
    return marker_changed(baseline_marker, current_marker)


def get_run_channel_index(method_name: str, payload: Dict[str, Any]) -> Optional[int]:
    channel_field = get_method_rule(method_name).get("channel_field")
    if not channel_field:
        return None
    channel_value = payload.get(channel_field)
    return int(channel_value) if channel_value is not None else None


def run_method_call(server_url: str, method_name: str, payload: Dict[str, Any]) -> Dict[str, Any]:
    return asyncio.run(call_biologic_method(server_url, method_name, payload))


def start_method_submission(server_url: str, method_name: str, payload: Dict[str, Any]) -> Future:
    return METHOD_CALL_EXECUTOR.submit(run_method_call, server_url, method_name, payload)


def begin_sequence_run(server_url: str, method_name: str, payload: Dict[str, Any]) -> None:
    values = get_snapshot_values()
    future = start_method_submission(server_url, method_name, payload)
    created_at = time.time()
    st.session_state.last_method_response = None
    st.session_state.active_sequence_run = {
        "server_url": server_url,
        "method": method_name,
        "payload": payload,
        "phase": "submitting",
        "capability": get_method_rule(method_name).get("capability"),
        "channel_index": get_run_channel_index(method_name, payload),
        "baseline_markers": build_result_markers(values),
        "current_markers": build_result_markers(values),
        "run_started": False,
        "stop_requested": False,
        "submission_future": future,
        "submission_result": None,
        "created_at_epoch": created_at,
        "submitted_at": time.strftime("%Y-%m-%d %H:%M:%S"),
        "last_phase_change_epoch": created_at,
        "last_phase_change_at": time.strftime("%Y-%m-%d %H:%M:%S"),
        "final_status": None,
        "history_recorded": False,
    }


def update_run_phase(active_run: Dict[str, Any], phase: str) -> None:
    if active_run.get("phase") != phase:
        active_run["phase"] = phase
        active_run["last_phase_change_epoch"] = time.time()
        active_run["last_phase_change_at"] = time.strftime("%Y-%m-%d %H:%M:%S")


def get_run_phase_age_seconds(active_run: Dict[str, Any]) -> float:
    phase_epoch = active_run.get("last_phase_change_epoch")
    if isinstance(phase_epoch, (int, float)):
        return max(0.0, time.time() - float(phase_epoch))

    timestamp_text = active_run.get("last_phase_change_at") or active_run.get("submitted_at")
    if isinstance(timestamp_text, str):
        try:
            parsed = time.strptime(timestamp_text, "%Y-%m-%d %H:%M:%S")
            return max(0.0, time.time() - time.mktime(parsed))
        except ValueError:
            return 0.0

    return 0.0


def is_submission_accepted(submission_result: Optional[Dict[str, Any]]) -> bool:
    if not isinstance(submission_result, dict) or not submission_result.get("success"):
        return False

    parsed_response = submission_result.get("parsed_response")
    if isinstance(parsed_response, dict):
        message = parsed_response.get("Message") or parsed_response.get("message")
        if isinstance(message, str) and message.strip().lower() == "accepted":
            return True

    raw_response = submission_result.get("raw_response")
    return isinstance(raw_response, str) and raw_response.strip().lower() == "accepted"


def finalize_sequence_run(active_run: Dict[str, Any], phase: str) -> None:
    update_run_phase(active_run, phase)
    active_run["final_status"] = phase
    if not active_run.get("history_recorded"):
        record_sequence_run_history(active_run)
        active_run["history_recorded"] = True


def update_active_sequence_run(server_url: str) -> None:
    active_run = get_active_sequence_run()
    if not active_run or active_run.get("server_url") != server_url:
        return

    values = get_snapshot_values()
    active_run["current_markers"] = build_result_markers(values)
    rule = get_method_rule(active_run["method"])
    future = active_run.get("submission_future")

    if isinstance(future, Future) and active_run.get("submission_result") is None and future.done():
        try:
            submission_result = future.result()
        except Exception as ex:
            submission_result = {
                "success": False,
                "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
                "server_url": server_url,
                "method": active_run["method"],
                "payload": active_run["payload"],
                "error": str(ex),
            }
        active_run["submission_result"] = submission_result
        st.session_state.last_method_response = submission_result
        if rule.get("capability") == "submission-only":
            record_command_history(submission_result)

    submission_result = active_run.get("submission_result")
    current_active = is_biologic_channel_active(values)
    if current_active:
        active_run["run_started"] = True

    capability = rule.get("capability")
    result_marker = rule.get("result_marker")
    baseline_marker = active_run["baseline_markers"].get(result_marker) if result_marker else None
    current_marker = active_run["current_markers"].get(result_marker) if result_marker else None
    result_marker_advanced = marker_changed(baseline_marker, current_marker)

    if capability == "submission-only":
        if submission_result is None:
            update_run_phase(active_run, "submitting")
            return

        if submission_result.get("success"):
            finalize_sequence_run(active_run, "completed")
        else:
            finalize_sequence_run(active_run, "failed")
        return

    if current_active:
        update_run_phase(active_run, "running")
        return

    if result_marker_advanced:
        active_run["run_started"] = True
        if submission_result is not None and not submission_result.get("success"):
            finalize_sequence_run(active_run, "warning")
        else:
            finalize_sequence_run(active_run, "completed")
        return

    if not active_run.get("run_started"):
        if submission_result is None:
            update_run_phase(active_run, "submitting")
            return

        if submission_result.get("success"):
            update_run_phase(active_run, "waiting_to_start")
            return

        finalize_sequence_run(active_run, "failed")
        return

    if active_run.get("stop_requested"):
        finalize_sequence_run(active_run, "canceled")
        return

    if capability == "state+result-tracked" and result_marker:
        phase_age_seconds = get_run_phase_age_seconds(active_run)
        if (
            is_submission_accepted(submission_result)
            and phase_age_seconds >= STALE_FINALIZING_TIMEOUT_SECONDS
            and current_marker in (None, "")
        ):
            stale_error = (
                "The OPC UA method was accepted, but no long-lived result marker was published after the channel returned to STOP. "
                "The previous execution state was cleared so a new run can be started."
            )
            if isinstance(submission_result, dict) and not submission_result.get("error"):
                submission_result["error"] = stale_error
            finalize_sequence_run(active_run, "warning")
            return

        update_run_phase(active_run, "finalizing")
        return

    if submission_result is not None and not submission_result.get("success"):
        finalize_sequence_run(active_run, "warning")
    else:
        finalize_sequence_run(active_run, "completed")


async def browse_opcua_object(server_url: str, object_node_id: str) -> Dict[str, Any]:
    client = Client(url=server_url, timeout=DISCOVERY_REQUEST_TIMEOUT)
    connected = False

    try:
        await client.connect()
        connected = True
        obj_node = get_opcua_node(client, object_node_id)
        children = await obj_node.get_children()
        variables: List[Dict[str, str]] = []

        for child in children:
            try:
                node_class = await child.read_node_class()
                if node_class.name != "Variable":
                    continue

                browse_name = await child.read_browse_name()
                display_name = await child.read_display_name()

                try:
                    data_type_id = await child.read_data_type()
                    data_type_node = get_opcua_node(client, data_type_id)
                    data_type_name = await data_type_node.read_browse_name()
                    type_str = str(data_type_name.Name)
                except Exception:
                    type_str = "Unknown"

                variables.append(
                    {
                        "node_id": child.nodeid.to_string(),
                        "browse_name": str(browse_name.Name),
                        "display_name": str(display_name.Text),
                        "data_type": type_str,
                    }
                )
            except Exception:
                continue

        return {"success": True, "variables": variables, "count": len(variables)}
    except Exception as ex:
        return {"success": False, "error": str(ex)}
    finally:
        if connected:
            try:
                await client.disconnect()
            except Exception:
                pass


async def call_biologic_method(server_url: str, method_name: str, payload: Dict[str, Any]) -> Dict[str, Any]:
    client = Client(url=server_url, timeout=DISCOVERY_REQUEST_TIMEOUT)
    connected = False

    try:
        await client.connect()
        connected = True
        biologic_layout = await discover_biologic_layout(server_url)
        method_target = resolve_biologic_method_target(method_name, biologic_layout)
        if method_target is None:
            return {
                "success": False,
                "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
                "server_url": server_url,
                "method": method_name,
                "payload": payload,
                "error": f"Method target could not be resolved for {method_name}.",
            }

        resolved_node_ids = {
            "function_set_node_id": parse_node_id(method_target["function_set_node_id"]),
            "method_node_id": parse_node_id(method_target["method_node_id"]),
        }

        function_set_node = get_opcua_node(client, resolved_node_ids["function_set_node_id"])
        method_node = get_opcua_node(client, resolved_node_ids["method_node_id"])
        input_arguments = []

        try:
            for child in await method_node.get_children():
                browse_name = await child.read_browse_name()
                if str(browse_name.Name) != "InputArguments":
                    continue
                input_arguments = await child.read_value()
                break
        except Exception:
            input_arguments = []

        if isinstance(input_arguments, list) and len(input_arguments) == 0:
            if payload:
                return {
                    "success": False,
                    "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
                    "server_url": server_url,
                    "method": method_name,
                    "payload": payload,
                    "function_set_node_id": method_target["function_set_node_id"],
                    "method_node_id": method_target["method_node_id"],
                    "error": (
                        f"The OPC UA server currently exposes {method_name} with 0 input arguments. "
                        "This client prepared a JSON payload, so the live server model is out of sync with the Biologic runtime. "
                        "Update the Biologic NodeSet XML, rebuild so the generated OPC UA sources refresh, and restart ORBIT."
                    ),
                }

            raw_result = await function_set_node.call_method(resolved_node_ids["method_node_id"])
        else:
            payload_text = json.dumps(payload)
            raw_result = await function_set_node.call_method(resolved_node_ids["method_node_id"], payload_text)
        normalized_result = normalize_method_result(raw_result)

        return {
            "success": True,
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "server_url": server_url,
            "method": method_name,
            "payload": payload,
            "function_set_node_id": method_target["function_set_node_id"],
            "method_node_id": method_target["method_node_id"],
            "raw_response": normalized_result,
            "parsed_response": safe_parse_json(normalized_result),
        }
    except Exception as ex:
        biologic_layout = await discover_biologic_layout(server_url)
        method_target = resolve_biologic_method_target(method_name, biologic_layout)
        detailed_error = str(ex)
        if "BadNoMatch" in detailed_error and method_target is not None:
            detailed_error = (
                f"{detailed_error} | Server={server_url} | "
                f"FunctionSet={method_target['function_set_node_id']} | Method={method_target['method_node_id']}"
            )
        return {
            "success": False,
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "server_url": server_url,
            "method": method_name,
            "payload": payload,
            "function_set_node_id": method_target["function_set_node_id"] if method_target else None,
            "method_node_id": method_target["method_node_id"] if method_target else None,
            "error": detailed_error,
        }
    finally:
        if connected:
            try:
                await client.disconnect()
            except Exception:
                pass


def append_numeric_history(values: Dict[str, Any]) -> None:
    for key, value in values.items():
        if key not in st.session_state.data_history:
            st.session_state.data_history[key] = deque(maxlen=100)

        if is_numeric_value(value):
            st.session_state.data_history[key].append(float(value))


def update_chart(values: Dict[str, Any], chart_container: Any) -> None:
    append_numeric_history(values)

    x_axis_key = None
    if st.session_state.x_axis_node:
        x_axis_key = st.session_state.x_axis_node["browse_name"]
    elif st.session_state.data_history.get("ElapsedTime"):
        x_axis_key = "ElapsedTime"
    elif st.session_state.data_history.get("elapsed_time"):
        x_axis_key = "elapsed_time"

    if not x_axis_key or x_axis_key not in st.session_state.data_history:
        return

    x_data = list(st.session_state.data_history.get(x_axis_key, []))
    if not x_data:
        return

    if st.session_state.y_axis_nodes:
        y_axis_keys = [node["browse_name"] for node in st.session_state.y_axis_nodes]
    else:
        y_axis_keys = [key for key in st.session_state.data_history.keys() if key != x_axis_key]

    colors = ["#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE"]
    figure = go.Figure()

    for color_index, key in enumerate(y_axis_keys):
        if key not in st.session_state.data_history:
            continue

        y_data = list(st.session_state.data_history[key])
        if not y_data:
            continue

        point_count = min(len(x_data), len(y_data))
        if point_count == 0:
            continue

        figure.add_trace(
            go.Scatter(
                x=x_data[:point_count],
                y=y_data[:point_count],
                mode="lines+markers",
                name=key,
                line=dict(color=colors[color_index % len(colors)], width=2),
            )
        )

    if not figure.data:
        return

    x_label = x_axis_key
    if st.session_state.x_axis_node:
        x_label = st.session_state.x_axis_node["display_name"]

    figure.update_layout(
        title="Custom OPC UA Trend Monitor",
        xaxis=dict(title=x_label, gridcolor="#E8E8E8"),
        yaxis=dict(title="Values", gridcolor="#E8E8E8"),
        hovermode="x unified",
        plot_bgcolor="white",
        height=460,
        margin=dict(l=70, r=40, t=60, b=50),
        showlegend=True,
    )

    with chart_container:
        st.plotly_chart(figure, width="stretch")


def summarize_method_result(method_result: Dict[str, Any]) -> str:
    phase = method_result.get("phase")
    if phase == "completed":
        return method_result.get("summary", "Execution completed")
    if phase == "warning":
        return method_result.get("summary", "Execution completed with submission warning")
    if phase == "canceled":
        return method_result.get("summary", "Execution canceled")
    if phase == "failed":
        return method_result.get("summary", method_result.get("error", "Execution failed"))

    if not method_result.get("success"):
        return method_result.get("error", "Method call failed")

    parsed_response = method_result.get("parsed_response")
    if isinstance(parsed_response, dict):
        return str(parsed_response.get("Message") or parsed_response.get("message") or "Method completed")

    return str(method_result.get("raw_response", "Method completed"))


def record_sequence_run_history(active_run: Dict[str, Any]) -> None:
    phase = active_run.get("final_status") or active_run.get("phase") or "unknown"
    submission_result = active_run.get("submission_result") or {}
    submission_error = submission_result.get("error")
    success = phase in {"completed", "warning"}

    if phase == "warning" and submission_error:
        summary = f"Execution completed after submission warning: {submission_error}"
    elif phase == "completed":
        summary = "Execution completed"
    elif phase == "canceled":
        summary = "Execution canceled by ForceStopChannel"
    else:
        summary = submission_error or "Execution failed"

    record_command_history(
        {
            "timestamp": active_run.get("last_phase_change_at") or active_run.get("submitted_at"),
            "method": active_run.get("method"),
            "phase": phase,
            "success": success,
            "payload": active_run.get("payload", {}),
            "summary": summary,
            "error": submission_error,
        }
    )


def record_command_history(method_result: Dict[str, Any]) -> None:
    entry = {
        "timestamp": method_result.get("timestamp"),
        "method": method_result.get("method"),
        "success": method_result.get("success", False),
        "payload": json.dumps(method_result.get("payload", {}), ensure_ascii=False),
        "summary": summarize_method_result(method_result),
    }

    st.session_state.command_history.insert(0, entry)
    st.session_state.command_history = st.session_state.command_history[:20]


def format_number(value: Any) -> str:
    if is_numeric_value(value):
        return f"{float(value):.6g}"
    return "-" if value is None else str(value)


def render_metrics(snapshot: Optional[Dict[str, Any]]) -> None:
    if not snapshot:
        st.info("No Biologic snapshot available.")
        return

    if not snapshot.get("connected"):
        st.error(f"Connection failed: {snapshot.get('error', 'Unknown error')}")
        return

    values = snapshot.get("values", {})
    has_is_busy = "IsBusy" in values
    row1 = st.columns(6 if has_is_busy else 5)
    row1[0].metric("State", str(values.get("State", "-")))
    row1[1].metric("Voltage (V)", format_number(values.get("Voltage")))
    row1[2].metric("Current (A)", format_number(values.get("Current")))
    row1[3].metric("Elapsed Time (s)", format_number(values.get("ElapsedTime")))
    row1[4].metric("Technique", str(values.get("Technique", "-")))
    if has_is_busy:
        row1[5].metric("Is Busy", str(values.get("IsBusy", "-")))

    row2 = st.columns(5)
    row2[0].metric("Data Rows", str(values.get("DataRows", "-")))
    row2[1].metric("Data Cols", str(values.get("DataCols", "-")))
    row2[2].metric("Last Update", str(values.get("LastUpdate", "-")))
    row2[3].metric("Latest EIS Status", str(values.get("LatestEISStatus", "-")))
    row2[4].metric("Latest EIS Points", str(values.get("LatestEISPointCount", "-")))


def render_latest_method_response() -> None:
    active_run = get_active_sequence_run()
    result = st.session_state.last_method_response
    if not active_run and not result:
        st.info("No sequence has been started from this client yet.")
        return

    if active_run:
        phase = active_run.get("phase", "unknown")
        method_name = active_run.get("method", "Unknown")
        submitted_at = active_run.get("submitted_at", "-")
        marker_advanced = sequence_result_marker_advanced(active_run)
        if phase == "completed":
            st.success(f"{method_name} completed at {active_run.get('last_phase_change_at', submitted_at)}")
        elif phase == "warning" and marker_advanced:
            st.warning(
                f"{method_name} completed at {active_run.get('last_phase_change_at', submitted_at)} with a submission transport warning"
            )
        elif phase == "warning":
            st.warning(f"{method_name} warning at {active_run.get('last_phase_change_at', submitted_at)}")
        elif phase in {"failed", "canceled"}:
            st.error(f"{method_name} {phase} at {active_run.get('last_phase_change_at', submitted_at)}")
        else:
            st.info(f"{method_name} is {phase} since {submitted_at}")

        st.write("Execution")
        st.json(
            {
                "Method": method_name,
                "Phase": phase,
                "ResultMarkerAdvanced": marker_advanced,
                "Capability": active_run.get("capability"),
                "ChannelIndex": active_run.get("channel_index"),
                "SubmittedAt": submitted_at,
                "LastPhaseChangeAt": active_run.get("last_phase_change_at"),
                "RunStarted": active_run.get("run_started"),
                "StopRequested": active_run.get("stop_requested"),
                "BaselineMarkers": active_run.get("baseline_markers"),
                "CurrentMarkers": active_run.get("current_markers"),
            }
        )

        server_url = active_run.get("server_url")
    else:
        server_url = result.get("server_url")

    function_set_node_id = result.get("function_set_node_id") if result else None
    method_node_id = result.get("method_node_id") if result else None

    if server_url:
        st.write("Active Server")
        st.text(str(server_url))

    if function_set_node_id or method_node_id:
        st.write("Resolved Target")
        st.json(
            {
                "FunctionSet": function_set_node_id,
                "Method": method_node_id,
            }
        )

    st.write("Payload")
    st.json((active_run or result).get("payload", {}))

    if result:
        if result.get("success"):
            parsed_response = result.get("parsed_response")
            if parsed_response is not None:
                st.write("Submission Response")
                st.json(parsed_response)
            else:
                st.write("Submission Raw Response")
                st.text(str(result.get("raw_response", "")))
        else:
            marker_advanced = sequence_result_marker_advanced(active_run)
            if active_run and active_run.get("phase") in {"warning", "completed"} and marker_advanced:
                st.write("Submission Transport Warning")
                st.warning(
                    "The sequence result was published successfully, but the synchronous OPC UA method response did not return to the client. "
                    f"Transport detail: {result.get('error', 'Unknown error')}"
                )
            else:
                st.write("Submission Error")
                st.text(result.get("error", "Unknown error"))


def render_command_history() -> None:
    if not st.session_state.command_history:
        st.info("No local command history yet.")
        return

    history_df = pd.DataFrame(st.session_state.command_history)
    st.dataframe(history_df, width="stretch", hide_index=True)


def render_spectrum_view(selected_items: List[Dict[str, Any]]) -> None:
    if not selected_items:
        st.info("Select at least one history item to render the Nyquist plot.")
        return

    st.markdown("#### Spectrum Viewer")

    combined_spectrum_frames: List[pd.DataFrame] = []
    raw_results: Dict[str, Any] = {}
    chart = go.Figure()

    for index, item in enumerate(selected_items, start=1):
        if not isinstance(item, dict):
            continue

        result = item.get("Result")
        spectrum = result.get("Spectrum") if isinstance(result, dict) else None
        if not isinstance(spectrum, list) or not spectrum:
            continue

        trace_label = str(
            item.get("RunId")
            or item.get("CompletedAtUtc")
            or item.get("SequenceName")
            or f"Trace {index}"
        )
        spectrum_df = pd.DataFrame(spectrum)
        spectrum_df.insert(0, "Trace", trace_label)
        combined_spectrum_frames.append(spectrum_df)
        raw_results[trace_label] = result

        if {"ImpedanceReal_Ohm", "NyquistImaginary_Ohm"}.issubset(spectrum_df.columns):
            nyquist_df = spectrum_df.dropna(subset=["ImpedanceReal_Ohm", "NyquistImaginary_Ohm"]).copy()
        elif {"Impedance_Ohm", "PhaseZwe"}.issubset(spectrum_df.columns):
            nyquist_df = spectrum_df.dropna(subset=["Impedance_Ohm", "PhaseZwe"]).copy()
            nyquist_df["ImpedanceReal_Ohm"] = nyquist_df["Impedance_Ohm"] * nyquist_df["PhaseZwe"].map(math.cos)
            nyquist_df["NyquistImaginary_Ohm"] = -nyquist_df["Impedance_Ohm"] * nyquist_df["PhaseZwe"].map(math.sin)
        else:
            nyquist_df = pd.DataFrame()

        if nyquist_df.empty:
            continue

        if "Frequency_Hz" in nyquist_df.columns:
            nyquist_df = nyquist_df.sort_values(by="Frequency_Hz", ascending=False)

        chart.add_trace(
            go.Scatter(
                x=nyquist_df["ImpedanceReal_Ohm"],
                y=nyquist_df["NyquistImaginary_Ohm"],
                mode="lines+markers",
                name=trace_label,
                customdata=nyquist_df.get("Frequency_Hz"),
                hovertemplate="Trace: %{fullData.name}<br>Re(Z): %{x:.6g} Ohm<br>-Im(Z): %{y:.6g} Ohm<br>f: %{customdata:.6g} Hz<extra></extra>",
            )
        )

    if combined_spectrum_frames:
        combined_spectrum_df = pd.concat(combined_spectrum_frames, ignore_index=True)
        st.dataframe(combined_spectrum_df, width="stretch", hide_index=True)
    else:
        st.info("The selected history items do not contain spectrum points.")
        return

    if chart.data:
        chart.update_layout(
            title="Nyquist Plot",
            xaxis=dict(title="Re(Z) (Ohm)"),
            yaxis=dict(title="-Im(Z) (Ohm)", scaleanchor="x", scaleratio=1),
            hovermode="closest",
            height=420,
            margin=dict(l=60, r=30, t=60, b=60),
            showlegend=True,
            legend=dict(title="Trace"),
        )
        st.plotly_chart(chart, width="stretch")
    else:
        st.info("The selected spectrum data does not contain enough impedance fields to render a Nyquist plot.")

    with st.expander("Raw Result JSON", expanded=False):
        st.json(raw_results)


def render_eis_panels(snapshot: Optional[Dict[str, Any]], server_url: str) -> None:
    if not snapshot or not snapshot.get("connected"):
        st.info("No Biologic output snapshot available.")
        return

    values = snapshot.get("values", {})
    latest_result = safe_parse_json(values.get("LatestEISResultJson"))
    history = safe_parse_json(values.get("LatestEISHistoryJson"))

    status_col1, status_col2, status_col3 = st.columns(3)
    status_col1.metric("Latest EIS Status", str(values.get("LatestEISStatus", "-")))
    status_col2.metric("Latest EIS Points", str(values.get("LatestEISPointCount", "-")))
    status_col3.metric("Latest EIS Updated", str(values.get("LatestEISTimestamp", "-")))

    action_col1, action_col2 = st.columns([1, 2])
    with action_col1:
        if st.button("Pull EIS Data Now", key="pull_eis_data_now", use_container_width=True):
            pull_eis_snapshot(server_url, delay_seconds=1.0)
            st.rerun()
    with action_col2:
        st.caption("After GEIS reaches STOP, the client can wait briefly and pull the final OPC UA result snapshot once.")

    left_col, right_col = st.columns([1.2, 1.8])

    with left_col:
        st.subheader("Latest EIS Output")
        if latest_result is None:
            st.info("No long-lived EIS result is currently available on the OPC UA nodes.")
        else:
            st.json(latest_result)

    with right_col:
        st.subheader("History Query")
        if not isinstance(history, list) or not history:
            st.info("No EIS history has been published yet. Current long-lived history comes from EIS runs.")
            return

        summary_rows: List[Dict[str, Any]] = []
        for item in history:
            if not isinstance(item, dict):
                continue

            result = item.get("Result", {})
            summary_rows.append(
                {
                    "RunId": item.get("RunId"),
                    "CompletedAtUtc": item.get("CompletedAtUtc"),
                    "SequenceName": item.get("SequenceName"),
                    "ChannelIndex": item.get("ChannelIndex"),
                    "SpectrumPointCount": item.get("SpectrumPointCount"),
                    "Message": result.get("Message") if isinstance(result, dict) else "",
                }
            )

        history_df = pd.DataFrame(summary_rows)
        st.dataframe(history_df, width="stretch", hide_index=True)

        options = {
            f"RunId={item.get('RunId')} | {item.get('CompletedAtUtc')} | {item.get('SequenceName')}": item
            for item in history
            if isinstance(item, dict)
        }

        option_labels = list(options.keys())
        default_labels = option_labels[:1]
        selected_labels = st.multiselect("Select history items", option_labels, default=default_labels)
        selected_items = [options[label] for label in selected_labels]
        render_spectrum_view(selected_items)


def render_charge_trend_panel(snapshot: Optional[Dict[str, Any]], server_url: str) -> None:
    if not snapshot or not snapshot.get("connected"):
        st.info("No Biologic output snapshot available.")
        return

    values = snapshot.get("values", {})
    charge_status = str(values.get("LatestChargeStatus", "-"))
    charge_points = safe_parse_json(values.get("LatestChargeSeriesJson"))
    csv_path = values.get("LatestChargeCsvPath")

    status_col1, status_col2, status_col3 = st.columns(3)
    status_col1.metric("Charge Status", charge_status)
    status_col2.metric("Charge Points", str(values.get("LatestChargePointCount", "-")))
    status_col3.metric("Charge Updated", str(values.get("LatestChargeUpdatedAt", "-")))

    action_col1, action_col2 = st.columns([1, 2])
    with action_col1:
        if st.button("Pull Charge Data Now", key="pull_charge_data_now", use_container_width=True):
            pull_charge_snapshot(server_url, delay_seconds=1.0)
            st.rerun()
    with action_col2:
        st.caption("After Charge reaches Completed, the client can wait briefly and pull the final OPC UA snapshot once.")

    if csv_path:
        st.caption(f"CSV Path: {csv_path}")

    if not isinstance(charge_points, list) or not charge_points:
        st.info("No Charge time-series points have been published yet.")
        return

    charge_df = pd.DataFrame(charge_points)
    st.dataframe(charge_df, width="stretch", hide_index=True)

    if not {"Time_s", "Ewe_V", "I_A"}.issubset(charge_df.columns):
        st.warning("Charge series payload is missing required columns.")
        return

    chart = go.Figure()
    chart.add_trace(
        go.Scatter(
            x=charge_df["Time_s"],
            y=charge_df["Ewe_V"],
            mode="lines+markers",
            name="Ewe (V)",
            line=dict(color="#E4572E", width=2),
        )
    )
    chart.add_trace(
        go.Scatter(
            x=charge_df["Time_s"],
            y=charge_df["I_A"],
            mode="lines+markers",
            name="I (A)",
            yaxis="y2",
            line=dict(color="#2E86AB", width=2),
        )
    )
    chart.update_layout(
        title="Charge Time-Series",
        xaxis=dict(title="Time (s)", gridcolor="#E8E8E8"),
        yaxis=dict(title="Ewe (V)", gridcolor="#E8E8E8"),
        yaxis2=dict(title="I (A)", overlaying="y", side="right"),
        hovermode="x unified",
        height=460,
        margin=dict(l=60, r=60, t=60, b=50),
    )
    st.plotly_chart(chart, width="stretch")


def build_payload_from_form(method_name: str) -> Dict[str, Any]:
    payload: Dict[str, Any] = {}
    schema = METHOD_SCHEMAS.get(method_name, [])
    columns = st.columns(2)

    for index, field in enumerate(schema):
        column = columns[index % 2]
        field_key = f"{method_name}_{field['name']}"
        field_type = field["type"]

        with column:
            if field_type == "int":
                payload[field["name"]] = int(
                    st.number_input(
                        field["label"],
                        min_value=int(field.get("min", 0)),
                        max_value=int(field.get("max", 100000)),
                        value=int(field["default"]),
                        step=int(field.get("step", 1)),
                        key=field_key,
                    )
                )
            elif field_type == "float":
                payload[field["name"]] = float(
                    st.number_input(
                        field["label"],
                        min_value=float(field.get("min", -1000000.0)),
                        max_value=float(field.get("max", 1000000.0)),
                        value=float(field["default"]),
                        step=float(field.get("step", 0.1)),
                        key=field_key,
                    )
                )
            elif field_type == "bool":
                payload[field["name"]] = st.checkbox(
                    field["label"],
                    value=bool(field.get("default", False)),
                    key=field_key,
                )
            else:
                payload[field["name"]] = st.text_input(
                    field["label"],
                    value=str(field.get("default", "")),
                    key=field_key,
                )

    return payload


def render_method_form(method_name: str, server_url: str) -> None:
    active_run = get_active_sequence_run()
    pending_run = has_pending_sequence_run()
    submit_disabled = pending_run and method_name != "ForceStopChannel"

    with st.form(f"form_{method_name}"):
        payload = build_payload_from_form(method_name)
        submitted = st.form_submit_button(f"Start {method_name}", use_container_width=True, disabled=submit_disabled)

    if submit_disabled and active_run:
        st.caption(f"{active_run['method']} is currently {active_run['phase']}. Use ForceStopChannel to interrupt it before starting a new run.")

    if submitted:
        normalized_payload = {
            key: normalize_optional_text(value) if isinstance(value, str) else value
            for key, value in payload.items()
        }

        if method_name == "ForceStopChannel":
            result = asyncio.run(call_biologic_method(server_url, method_name, normalized_payload))
            st.session_state.last_method_response = result
            record_command_history(result)
            if result.get("success") and active_run:
                active_run["stop_requested"] = True
                update_run_phase(active_run, "canceled")
        else:
            begin_sequence_run(server_url, method_name, normalized_payload)

        refresh_dashboard(server_url)
        st.rerun()


def display_state_info(state_data: Dict[str, Any], chart_container: Any, data_container: Any) -> None:
    if state_data.get("connected"):
        values = state_data.get("values", {})

        with data_container:
            metric_columns = st.columns(min(len(values) + 1, 5))
            metric_columns[0].success("Connected")

            for index, (key, value) in enumerate(values.items(), start=1):
                if index >= len(metric_columns):
                    break

                if is_numeric_value(value):
                    metric_columns[index].metric(key, f"{float(value):.3f}")
                else:
                    metric_columns[index].metric(key, str(value) if value is not None else "Error")

            if values:
                st.dataframe(pd.DataFrame([values]), width="stretch", hide_index=True)

            with st.expander("Node Details", expanded=False):
                st.write("Monitored Nodes")
                if st.session_state.selected_nodes:
                    for node in st.session_state.selected_nodes:
                        st.write(f"- {node['display_name']} ({node['browse_name']}): {node['node_id']}")
                else:
                    for label, node_id in resolve_monitor_nodes(st.session_state.active_server_url).items():
                        st.write(f"- {label}: {node_id}")
                st.write(f"Last Read: {state_data['timestamp']}")

        update_chart(values, chart_container)
    else:
        with data_container:
            st.error(f"Connection failed: {state_data.get('error', 'Unknown error')}")
            st.write(f"Time: {state_data['timestamp']}")


def render_monitor_tab(active_server_url: str) -> None:
    st.subheader("Custom OPC UA Monitor")

    with st.expander("Browse OPC UA Object", expanded=True):
        browse_col1, browse_col2 = st.columns([3, 1])
        with browse_col1:
            default_channel_node_id = get_biologic_channel_node_id(active_server_url) if active_server_url else DEFAULT_CHANNEL_NODE_ID
            object_node_id = st.text_input(
                "Object Node ID",
                value=default_channel_node_id,
                help="Browse variables under a specific OPC UA object node.",
            )

        with browse_col2:
            st.write("")
            st.write("")
            if st.button("Browse", use_container_width=True):
                browse_result = asyncio.run(browse_opcua_object(active_server_url, object_node_id))
                if browse_result.get("success"):
                    st.session_state.available_nodes = browse_result["variables"]
                    set_status(f"Found {browse_result['count']} variables", "success")
                else:
                    set_status(f"Browse failed: {browse_result.get('error')}", "error")

        if st.session_state.available_nodes:
            st.markdown("#### Select Variables to Monitor")
            df_data: List[Dict[str, Any]] = []

            for index, node in enumerate(st.session_state.available_nodes):
                is_selected = any(existing["node_id"] == node["node_id"] for existing in st.session_state.selected_nodes)
                if not st.session_state.selected_nodes and index < 3:
                    is_selected = True

                df_data.append(
                    {
                        "Select": is_selected,
                        "Display Name": node["display_name"],
                        "Browse Name": node["browse_name"],
                        "Data Type": node["data_type"],
                        "Node ID": node["node_id"],
                    }
                )

            edited_df = st.data_editor(
                pd.DataFrame(df_data),
                width="stretch",
                hide_index=True,
                column_config={
                    "Select": st.column_config.CheckboxColumn("Select", default=False),
                    "Display Name": st.column_config.TextColumn("Display Name", width="medium"),
                    "Browse Name": st.column_config.TextColumn("Browse Name", width="medium"),
                    "Data Type": st.column_config.TextColumn("Data Type", width="small"),
                    "Node ID": st.column_config.TextColumn("Node ID", width="large"),
                },
                disabled=["Display Name", "Browse Name", "Data Type", "Node ID"],
            )

            selected_indices = edited_df[edited_df["Select"] == True].index.tolist()
            st.session_state.selected_nodes = [st.session_state.available_nodes[index] for index in selected_indices]

            if st.session_state.selected_nodes:
                st.info(f"{len(st.session_state.selected_nodes)} variables selected")
                axis_col1, axis_col2 = st.columns(2)

                with axis_col1:
                    x_options = {
                        f"{node['display_name']} ({node['browse_name']})": node
                        for node in st.session_state.selected_nodes
                    }
                    x_default = None
                    for label, node in x_options.items():
                        browse_name = node["browse_name"].lower()
                        display_name = node["display_name"].lower()
                        if "time" in browse_name or "time" in display_name:
                            x_default = label
                            break
                    if x_default is None:
                        x_default = list(x_options.keys())[0]

                    x_selected = st.selectbox(
                        "X-Axis",
                        options=list(x_options.keys()),
                        index=list(x_options.keys()).index(x_default),
                    )
                    st.session_state.x_axis_node = x_options[x_selected]

                with axis_col2:
                    y_options = {
                        label: node
                        for label, node in x_options.items()
                        if node["node_id"] != st.session_state.x_axis_node["node_id"]
                    }
                    if y_options:
                        y_selected = st.multiselect(
                            "Y-Axis",
                            options=list(y_options.keys()),
                            default=list(y_options.keys()),
                        )
                        st.session_state.y_axis_nodes = [y_options[label] for label in y_selected]
                    else:
                        st.warning("Select at least two variables to configure a chart.")

    monitor_chart_container = st.container()
    monitor_data_container = st.container()
    display_state_info(st.session_state.monitor_snapshot, monitor_chart_container, monitor_data_container)


def render_instrument_page(active_server_url: str) -> None:
    top_col1, top_col2, top_col3 = st.columns([1, 1.5, 2.5])
    with top_col1:
        auto_refresh = st.checkbox("Auto Refresh Dashboard", value=False)
    with top_col2:
        refresh_interval = st.slider("Refresh Interval (s)", 0.5, 10.0, 2.0, 0.5)
    with top_col3:
        if st.button("Refresh Dashboard Now", use_container_width=True):
            refresh_dashboard(active_server_url)

    active_run_pending = has_pending_sequence_run()
    if st.session_state.monitor_snapshot is None or st.session_state.biologic_snapshot is None:
        refresh_dashboard(active_server_url)
    elif auto_refresh or active_run_pending:
        refresh_dashboard(active_server_url)

    update_active_sequence_run(active_server_url)

    method_names = get_sorted_method_names(active_server_url)

    control_tab, output_tab, history_tab, monitor_tab = st.tabs(
        ["Test Control", "Biologic Output", "History & Spectrum", "Custom Monitor"]
    )

    with control_tab:
        st.subheader("Start Tests")
        st.caption("Available buttons are driven by the Biologic OPC UA FunctionSet methods.")
        method_tabs = st.tabs(method_names)

        for tab, method_name in zip(method_tabs, method_names):
            with tab:
                render_method_form(method_name, active_server_url)

        st.markdown("---")
        st.subheader("Execution Status")
        render_latest_method_response()

        st.markdown("---")
        st.subheader("Local Command History")
        render_command_history()

    with output_tab:
        st.subheader("Biologic Live Output")
        render_metrics(st.session_state.biologic_snapshot)

        snapshot_values = (st.session_state.biologic_snapshot or {}).get("values", {})
        output_rows = [
            {"Key": key, "Value": value}
            for key, value in snapshot_values.items()
            if key not in {"LatestEISResultJson", "LatestEISHistoryJson", "LatestChargeSeriesJson"}
        ]
        st.dataframe(pd.DataFrame(output_rows), width="stretch", hide_index=True)

        st.markdown("---")
        st.subheader("Charge Trend")
        render_charge_trend_panel(st.session_state.biologic_snapshot, active_server_url)

    with history_tab:
        st.subheader("EIS History Query")
        st.caption("Current long-lived OPC UA history is implemented for GEIS and PEIS results.")
        render_eis_panels(st.session_state.biologic_snapshot, active_server_url)

    with monitor_tab:
        render_monitor_tab(active_server_url)

    with st.expander("Instructions", expanded=False):
        st.markdown(
            """
            ### Dashboard Overview

            - Test Control: Start OCV, PEIS, GEIS, Charge, Discharge, and ForceStopChannel methods from OPC UA buttons.
            - Biologic Output: View the current live values published by the Biologic runtime.
            - History & Spectrum: Query the rolling GEIS history JSON and inspect returned spectra.
            - Custom Monitor: Keep the original free-form OPC UA browser and trend monitor.

            ### Notes

            - Connection and server discovery now live on the dedicated Connect Page.
            - Method calls are sent as JSON strings to the Biologic FunctionSet methods.
            - Long-lived history currently comes from the GEIS publication nodes.
            """
        )

    render_status()

    if auto_refresh or active_run_pending:
        time.sleep(refresh_interval)
        st.rerun()