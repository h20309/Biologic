import asyncio
import json
import time
from collections import deque
from typing import Any, Dict, List, Optional

import pandas as pd
import plotly.graph_objects as go
import streamlit as st
from asyncua import Client

from opcua_connect import (
    DEFAULT_MONITOR_NODES,
    refresh_dashboard,
    render_status,
    resolve_monitor_nodes,
    set_status,
)


FUNCTION_SET_NODE_ID = "ns=3;i=5009"
DEFAULT_CHANNEL_NODE_ID = "ns=3;i=5002"
DISCOVERY_REQUEST_TIMEOUT = 1.5

METHOD_NODE_IDS: Dict[str, str] = {
    "RunOCV": "ns=3;i=7000",
    "RunPEIS": "ns=3;i=7001",
    "RunGEIS": "ns=3;i=7004",
    "Charge": "ns=3;i=7005",
    "Discharge": "ns=3;i=7009",
}

METHOD_SCHEMAS: Dict[str, List[Dict[str, Any]]] = {
    "RunOCV": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {"name": "Duration_s", "type": "float", "label": "Duration (s)", "default": 60.0, "min": 0.1, "step": 1.0},
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
        {"name": "OutputFile", "type": "text", "label": "Output File", "default": ""},
    ],
    "Charge": [
        {"name": "ChannelIndex", "type": "int", "label": "Channel Index", "default": 0, "min": 0, "max": 15},
        {"name": "Voltage_V", "type": "float", "label": "Voltage (V)", "default": 4.2, "step": 0.01},
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
        {"name": "Voltage_V", "type": "float", "label": "Voltage (V)", "default": 2.5, "step": 0.01},
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


def normalize_method_result(result: Any) -> Any:
    if isinstance(result, list) and len(result) == 1:
        return result[0]
    return result


async def browse_opcua_object(server_url: str, object_node_id: str) -> Dict[str, Any]:
    client = Client(url=server_url, timeout=DISCOVERY_REQUEST_TIMEOUT)
    connected = False

    try:
        await client.connect()
        connected = True
        obj_node = client.get_node(object_node_id)
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
                    data_type_node = client.get_node(data_type_id)
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
        function_set_node = client.get_node(FUNCTION_SET_NODE_ID)
        payload_text = json.dumps(payload)
        raw_result = await function_set_node.call_method(METHOD_NODE_IDS[method_name], payload_text)
        normalized_result = normalize_method_result(raw_result)

        return {
            "success": True,
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "method": method_name,
            "payload": payload,
            "raw_response": normalized_result,
            "parsed_response": safe_parse_json(normalized_result),
        }
    except Exception as ex:
        return {
            "success": False,
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "method": method_name,
            "payload": payload,
            "error": str(ex),
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
    if not method_result.get("success"):
        return method_result.get("error", "Method call failed")

    parsed_response = method_result.get("parsed_response")
    if isinstance(parsed_response, dict):
        return str(parsed_response.get("Message") or parsed_response.get("message") or "Method completed")

    return str(method_result.get("raw_response", "Method completed"))


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
    row1 = st.columns(5)
    row1[0].metric("State", str(values.get("State", "-")))
    row1[1].metric("Voltage (V)", format_number(values.get("Voltage")))
    row1[2].metric("Current (A)", format_number(values.get("Current")))
    row1[3].metric("Elapsed Time (s)", format_number(values.get("ElapsedTime")))
    row1[4].metric("Technique", str(values.get("Technique", "-")))

    row2 = st.columns(5)
    row2[0].metric("Data Rows", str(values.get("DataRows", "-")))
    row2[1].metric("Data Cols", str(values.get("DataCols", "-")))
    row2[2].metric("Last Update", str(values.get("LastUpdate", "-")))
    row2[3].metric("Latest EIS Status", str(values.get("LatestEISStatus", "-")))
    row2[4].metric("Latest EIS Points", str(values.get("LatestEISPointCount", "-")))


def render_latest_method_response() -> None:
    result = st.session_state.last_method_response
    if not result:
        st.info("No method has been started from this client yet.")
        return

    if result.get("success"):
        st.success(f"{result['method']} submitted at {result['timestamp']}")
    else:
        st.error(f"{result['method']} failed at {result['timestamp']}")

    st.write("Payload")
    st.json(result.get("payload", {}))

    if result.get("success"):
        parsed_response = result.get("parsed_response")
        if parsed_response is not None:
            st.write("Response")
            st.json(parsed_response)
        else:
            st.write("Raw Response")
            st.text(str(result.get("raw_response", "")))
    else:
        st.write("Error")
        st.text(result.get("error", "Unknown error"))


def render_command_history() -> None:
    if not st.session_state.command_history:
        st.info("No local command history yet.")
        return

    history_df = pd.DataFrame(st.session_state.command_history)
    st.dataframe(history_df, width="stretch", hide_index=True)


def render_spectrum_view(result: Dict[str, Any]) -> None:
    spectrum = result.get("Spectrum") if isinstance(result, dict) else None
    if not isinstance(spectrum, list) or not spectrum:
        st.info("The selected result does not contain spectrum points.")
        return

    st.markdown("#### Spectrum Viewer")
    spectrum_df = pd.DataFrame(spectrum)
    st.dataframe(spectrum_df, width="stretch", hide_index=True)

    if {"Frequency_Hz", "Impedance_Ohm"}.issubset(spectrum_df.columns):
        chart = go.Figure()
        chart.add_trace(
            go.Scatter(
                x=spectrum_df["Frequency_Hz"],
                y=spectrum_df["Impedance_Ohm"],
                mode="lines+markers",
                name="|Z|",
                line=dict(color="#1f77b4", width=2),
            )
        )
        chart.update_layout(
            title="EIS Magnitude",
            xaxis=dict(title="Frequency (Hz)", type="log", autorange="reversed"),
            yaxis=dict(title="Impedance (Ohm)"),
            hovermode="x unified",
            height=420,
            margin=dict(l=60, r=30, t=60, b=60),
        )
        st.plotly_chart(chart, width="stretch")

    with st.expander("Raw Result JSON", expanded=False):
        st.json(result)


def render_eis_panels(snapshot: Optional[Dict[str, Any]]) -> None:
    if not snapshot or not snapshot.get("connected"):
        st.info("No Biologic output snapshot available.")
        return

    values = snapshot.get("values", {})
    latest_result = safe_parse_json(values.get("LatestEISResultJson"))
    history = safe_parse_json(values.get("LatestEISHistoryJson"))

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
            st.info("No EIS history has been published yet. Current long-lived history comes from GEIS runs.")
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

        selected_label = st.selectbox("Select a history item", list(options.keys()))
        selected_item = options[selected_label]
        selected_result = selected_item.get("Result", {}) if isinstance(selected_item, dict) else {}
        render_spectrum_view(selected_result)


def build_payload_from_form(method_name: str) -> Dict[str, Any]:
    payload: Dict[str, Any] = {}
    schema = METHOD_SCHEMAS[method_name]
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
            else:
                payload[field["name"]] = st.text_input(
                    field["label"],
                    value=str(field.get("default", "")),
                    key=field_key,
                )

    return payload


def render_method_form(method_name: str, server_url: str) -> None:
    with st.form(f"form_{method_name}"):
        payload = build_payload_from_form(method_name)
        submitted = st.form_submit_button(f"Start {method_name}", use_container_width=True)

    if submitted:
        normalized_payload = {
            key: normalize_optional_text(value) if isinstance(value, str) else value
            for key, value in payload.items()
        }
        result = asyncio.run(call_biologic_method(server_url, method_name, normalized_payload))
        st.session_state.last_method_response = result
        record_command_history(result)
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
                    for label, node_id in DEFAULT_MONITOR_NODES.items():
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
            object_node_id = st.text_input(
                "Object Node ID",
                value=DEFAULT_CHANNEL_NODE_ID,
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

    if st.session_state.monitor_snapshot is None or st.session_state.biologic_snapshot is None:
        refresh_dashboard(active_server_url)

    if auto_refresh:
        refresh_dashboard(active_server_url)

    control_tab, output_tab, history_tab, monitor_tab = st.tabs(
        ["Test Control", "Biologic Output", "History & Spectrum", "Custom Monitor"]
    )

    with control_tab:
        st.subheader("Start Tests")
        st.caption("Available buttons are driven by the Biologic OPC UA FunctionSet methods.")
        method_tabs = st.tabs(list(METHOD_SCHEMAS.keys()))

        for tab, method_name in zip(method_tabs, METHOD_SCHEMAS.keys()):
            with tab:
                render_method_form(method_name, active_server_url)

        st.markdown("---")
        st.subheader("Last Method Response")
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
            if key not in {"LatestEISResultJson", "LatestEISHistoryJson"}
        ]
        st.dataframe(pd.DataFrame(output_rows), width="stretch", hide_index=True)

    with history_tab:
        st.subheader("GEIS History Query")
        st.caption("Current long-lived OPC UA history is implemented for GEIS results.")
        render_eis_panels(st.session_state.biologic_snapshot)

    with monitor_tab:
        render_monitor_tab(active_server_url)

    with st.expander("Instructions", expanded=False):
        st.markdown(
            """
            ### Dashboard Overview

            - Test Control: Start OCV, PEIS, GEIS, Charge, and Discharge methods from OPC UA buttons.
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

    if auto_refresh:
        time.sleep(refresh_interval)
        st.rerun()