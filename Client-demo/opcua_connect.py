import asyncio
import ipaddress
import json
import socket
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional
from urllib.parse import urlsplit

import streamlit as st
from asyncua import Client


ROOT_DIR = Path(__file__).resolve().parents[2]
PROFILE_SETTINGS_DIR = ROOT_DIR / "common" / "ORBIT" / "profiles"
DISCOVERY_REQUEST_TIMEOUT = 1.5
LOCAL_DISCOVERY_TIMEOUT = 0.25
LAN_DISCOVERY_TIMEOUT = 0.15
LAN_SCAN_CONCURRENCY = 48

DEFAULT_MONITOR_NODES: Dict[str, str] = {
    "Voltage": "ns=3;i=6026",
    "Current": "ns=3;i=6027",
    "ElapsedTime": "ns=3;i=6038",
}

BIOLOGIC_OUTPUT_NODES: Dict[str, str] = {
    "State": "ns=3;i=6025",
    "Voltage": "ns=3;i=6026",
    "Current": "ns=3;i=6027",
    "ElapsedTime": "ns=3;i=6038",
    "Technique": "ns=3;i=6039",
    "DataCols": "ns=3;i=6040",
    "LastUpdate": "ns=3;i=6041",
    "DataRows": "ns=3;i=6042",
    "LatestEISStatus": "ns=3;i=6100",
    "LatestEISTimestamp": "ns=3;i=6101",
    "LatestEISResultJson": "ns=3;i=6102",
    "LatestEISHistoryJson": "ns=3;i=6103",
    "LatestEISPointCount": "ns=3;i=6104",
    "LatestEISRunId": "ns=3;i=6105",
}


def ensure_session_state() -> None:
    defaults: Dict[str, Any] = {
        "data_history": {},
        "available_nodes": [],
        "selected_nodes": [],
        "x_axis_node": None,
        "y_axis_nodes": [],
        "monitor_snapshot": None,
        "biologic_snapshot": None,
        "last_method_response": None,
        "command_history": [],
        "discovered_servers": [],
        "selected_server_url": None,
        "active_server_url": None,
        "manual_server_url": "",
        "status_message": None,
        "status_level": "info",
        "local_discovery_completed": False,
        "lan_scan_completed": False,
        "last_discovery_time": None,
    }

    for key, value in defaults.items():
        if key not in st.session_state:
            st.session_state[key] = value


def set_status(message: str, level: str = "info") -> None:
    st.session_state.status_message = message
    st.session_state.status_level = level


def clear_status() -> None:
    st.session_state.status_message = None
    st.session_state.status_level = "info"


def render_status() -> None:
    message = st.session_state.status_message
    if not message:
        return

    level = st.session_state.status_level
    if level == "success":
        st.success(message)
    elif level == "warning":
        st.warning(message)
    elif level == "error":
        st.error(message)
    else:
        st.info(message)


def normalize_opcua_url(raw_value: str) -> str:
    stripped = raw_value.strip()
    if not stripped:
        return ""
    if "://" not in stripped:
        stripped = f"opc.tcp://{stripped}"
    return stripped


def parse_profile_settings() -> List[Dict[str, Any]]:
    profiles: List[Dict[str, Any]] = []
    if not PROFILE_SETTINGS_DIR.exists():
        return profiles

    for setting_path in sorted(PROFILE_SETTINGS_DIR.glob("*/setting.json")):
        try:
            data = json.loads(setting_path.read_text(encoding="utf-8"))
        except Exception:
            continue

        common = data.get("Properties", {}).get("Common", {})
        port_value = common.get("ServerPort")
        if port_value is None:
            continue

        try:
            port = int(port_value)
        except (TypeError, ValueError):
            continue

        profiles.append(
            {
                "name": data.get("Name") or data.get("SystemType") or setting_path.parent.name,
                "port": port,
            }
        )

    return profiles


def get_candidate_ports() -> List[int]:
    ports = {profile["port"] for profile in parse_profile_settings()}
    if not ports:
        ports = {4840}
    return sorted(ports)


def build_local_candidates() -> List[Dict[str, Any]]:
    profile_map: Dict[int, List[str]] = {}
    for profile in parse_profile_settings():
        profile_map.setdefault(profile["port"], []).append(profile["name"])

    candidates: Dict[str, Dict[str, Any]] = {}
    for port in get_candidate_ports():
        seed_name = ", ".join(sorted(profile_map.get(port, [f"Port {port}"])))
        for host in ("localhost", "127.0.0.1"):
            url = f"opc.tcp://{host}:{port}"
            candidates[url] = {
                "url": url,
                "source": "local",
                "seed_name": seed_name,
            }

    return list(candidates.values())


def get_local_ipv4_networks() -> List[ipaddress.IPv4Network]:
    ip_addresses = set()

    try:
        hostname = socket.gethostname()
        for info in socket.getaddrinfo(hostname, None, socket.AF_INET, socket.SOCK_STREAM):
            ip_addresses.add(info[4][0])
    except socket.gaierror:
        pass

    try:
        probe_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        probe_socket.connect(("8.8.8.8", 80))
        ip_addresses.add(probe_socket.getsockname()[0])
        probe_socket.close()
    except OSError:
        pass

    networks: Dict[str, ipaddress.IPv4Network] = {}
    for ip_address in ip_addresses:
        try:
            ip_obj = ipaddress.ip_address(ip_address)
        except ValueError:
            continue

        if ip_obj.is_loopback or ip_obj.is_link_local:
            continue

        network = ipaddress.IPv4Network(f"{ip_address}/24", strict=False)
        networks[str(network)] = network

    return list(networks.values())[:3]


def get_network_summary() -> str:
    networks = get_local_ipv4_networks()
    if not networks:
        return "No active IPv4 subnet detected"
    return ", ".join(str(network) for network in networks)


async def is_tcp_port_open(host: str, port: int, timeout: float) -> bool:
    try:
        reader, writer = await asyncio.wait_for(asyncio.open_connection(host, port), timeout=timeout)
        writer.close()
        await writer.wait_closed()
        return True
    except Exception:
        return False


def simplify_security_policy(policy_uri: str) -> str:
    if not policy_uri:
        return "Unknown"
    if "#" in policy_uri:
        return policy_uri.rsplit("#", 1)[-1]
    if "/" in policy_uri:
        return policy_uri.rstrip("/").rsplit("/", 1)[-1]
    return policy_uri


async def discover_server_candidate(candidate: Dict[str, Any], tcp_timeout: float) -> Optional[Dict[str, Any]]:
    parsed = urlsplit(candidate["url"])
    host = parsed.hostname
    port = parsed.port
    if not host or port is None:
        return None

    if not await is_tcp_port_open(host, port, tcp_timeout):
        return None

    client = Client(url=candidate["url"], timeout=DISCOVERY_REQUEST_TIMEOUT)
    endpoints = []
    servers = []

    try:
        endpoints = await client.connect_and_get_server_endpoints()
    except Exception:
        endpoints = []

    try:
        servers = await client.connect_and_find_servers()
    except Exception:
        servers = []

    if not endpoints and not servers:
        return None

    display_name = candidate.get("seed_name") or f"{host}:{port}"
    security_mode = "Unknown"
    security_policy = "Unknown"

    if servers:
        application_name = getattr(servers[0].ApplicationName, "Text", None)
        if application_name:
            display_name = str(application_name)

    if endpoints:
        endpoint_name = getattr(endpoints[0].Server.ApplicationName, "Text", None)
        if endpoint_name:
            display_name = str(endpoint_name)
        security_mode = str(getattr(endpoints[0].SecurityMode, "name", endpoints[0].SecurityMode))
        security_policy = simplify_security_policy(str(endpoints[0].SecurityPolicyUri))

    return {
        "url": candidate["url"],
        "display_name": display_name,
        "host": host,
        "port": port,
        "sources": [candidate["source"]],
        "security_mode": security_mode,
        "security_policy": security_policy,
    }


async def discover_candidates(candidates: List[Dict[str, Any]], tcp_timeout: float, concurrency: int) -> List[Dict[str, Any]]:
    unique_candidates: Dict[str, Dict[str, Any]] = {}
    for candidate in candidates:
        unique_candidates[candidate["url"]] = candidate

    semaphore = asyncio.Semaphore(concurrency)

    async def worker(candidate: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        async with semaphore:
            return await discover_server_candidate(candidate, tcp_timeout)

    results = await asyncio.gather(*(worker(candidate) for candidate in unique_candidates.values()))
    discovered = [result for result in results if result is not None]
    return sorted(discovered, key=lambda item: (item["display_name"].lower(), item["port"], item["url"]))


async def discover_local_servers() -> List[Dict[str, Any]]:
    return await discover_candidates(build_local_candidates(), LOCAL_DISCOVERY_TIMEOUT, concurrency=8)


async def discover_lan_servers() -> List[Dict[str, Any]]:
    candidates: List[Dict[str, Any]] = []
    for network in get_local_ipv4_networks():
        for host in network.hosts():
            host_str = str(host)
            for port in get_candidate_ports():
                candidates.append(
                    {
                        "url": f"opc.tcp://{host_str}:{port}",
                        "source": "lan",
                        "seed_name": f"LAN scan {network}",
                    }
                )

    return await discover_candidates(candidates, LAN_DISCOVERY_TIMEOUT, concurrency=LAN_SCAN_CONCURRENCY)


async def validate_manual_server(url: str) -> Optional[Dict[str, Any]]:
    normalized_url = normalize_opcua_url(url)
    if not normalized_url:
        return None
    candidate = {"url": normalized_url, "source": "manual", "seed_name": "Manual URL"}
    return await discover_server_candidate(candidate, LOCAL_DISCOVERY_TIMEOUT)


def merge_discovered_servers(new_servers: List[Dict[str, Any]]) -> int:
    existing = {server["url"]: server for server in st.session_state.discovered_servers}
    added = 0

    for server in new_servers:
        current = existing.get(server["url"])
        if current is None:
            existing[server["url"]] = server
            added += 1
            continue

        merged_sources = sorted(set(current.get("sources", [])) | set(server.get("sources", [])))
        current.update(server)
        current["sources"] = merged_sources

    st.session_state.discovered_servers = sorted(
        existing.values(),
        key=lambda item: (item["display_name"].lower(), item["port"], item["url"]),
    )

    if st.session_state.discovered_servers and not st.session_state.selected_server_url:
        st.session_state.selected_server_url = st.session_state.discovered_servers[0]["url"]

    return added


def get_discovered_server_map() -> Dict[str, Dict[str, Any]]:
    return {server["url"]: server for server in st.session_state.discovered_servers}


def build_server_option_label(url: str) -> str:
    server = get_discovered_server_map().get(url)
    if not server:
        return url

    sources = ", ".join(source.upper() for source in server.get("sources", []))
    return (
        f"{server['display_name']} | {server['host']}:{server['port']} | "
        f"{sources} | {server['security_mode']} / {server['security_policy']}"
    )


def resolve_monitor_nodes() -> Dict[str, str]:
    if st.session_state.selected_nodes:
        return {node["browse_name"]: node["node_id"] for node in st.session_state.selected_nodes}
    return DEFAULT_MONITOR_NODES


async def read_node_values(server_url: str, nodes_config: Dict[str, str]) -> Dict[str, Any]:
    client = Client(url=server_url, timeout=DISCOVERY_REQUEST_TIMEOUT)
    connected = False

    try:
        await client.connect()
        connected = True
        result: Dict[str, Any] = {
            "connected": True,
            "timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "values": {},
        }

        for key, node_id in nodes_config.items():
            try:
                node = client.get_node(node_id)
                result["values"][key] = await node.read_value()
            except Exception as ex:
                result["values"][key] = None
                result[f"{key}_error"] = str(ex)

        return result
    except Exception as ex:
        return {
            "connected": False,
            "error": str(ex),
            "timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "values": {},
        }
    finally:
        if connected:
            try:
                await client.disconnect()
            except Exception:
                pass


def refresh_dashboard(server_url: Optional[str]) -> None:
    if not server_url:
        st.session_state.monitor_snapshot = None
        st.session_state.biologic_snapshot = None
        return

    st.session_state.monitor_snapshot = asyncio.run(read_node_values(server_url, resolve_monitor_nodes()))
    st.session_state.biologic_snapshot = asyncio.run(read_node_values(server_url, BIOLOGIC_OUTPUT_NODES))


def run_local_discovery() -> int:
    discovered = asyncio.run(discover_local_servers())
    added = merge_discovered_servers(discovered)
    st.session_state.local_discovery_completed = True
    st.session_state.last_discovery_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    set_status(f"Local discovery finished. Added {added} server(s).", "success")
    return added


def run_lan_scan() -> int:
    networks = get_local_ipv4_networks()
    if not networks:
        set_status("LAN scan skipped because no active IPv4 subnet was detected.", "warning")
        return 0

    discovered = asyncio.run(discover_lan_servers())
    added = merge_discovered_servers(discovered)
    st.session_state.lan_scan_completed = True
    st.session_state.last_discovery_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    set_status(f"LAN deep scan finished. Added {added} server(s).", "success")
    return added


def add_manual_server() -> bool:
    manual_url = normalize_opcua_url(st.session_state.manual_server_url)
    if not manual_url:
        set_status("Manual URL is empty.", "warning")
        return False

    discovered = asyncio.run(validate_manual_server(manual_url))
    if not discovered:
        set_status(f"Manual URL validation failed: {manual_url}", "error")
        return False

    merge_discovered_servers([discovered])
    st.session_state.selected_server_url = discovered["url"]
    set_status(f"Manual URL added: {discovered['url']}", "success")
    return True


def connect_selected_server() -> bool:
    selected_url = st.session_state.selected_server_url
    if not selected_url:
        set_status("Select a discovered server before connecting.", "warning")
        return False

    refresh_dashboard(selected_url)
    monitor_snapshot = st.session_state.monitor_snapshot
    biologic_snapshot = st.session_state.biologic_snapshot

    if not monitor_snapshot or not monitor_snapshot.get("connected"):
        st.session_state.active_server_url = None
        error = monitor_snapshot.get("error", "Unknown error") if monitor_snapshot else "Unknown error"
        set_status(f"Connect failed: {error}", "error")
        return False

    if not biologic_snapshot or not biologic_snapshot.get("connected"):
        st.session_state.active_server_url = None
        error = biologic_snapshot.get("error", "Unknown error") if biologic_snapshot else "Unknown error"
        set_status(f"Connect failed: {error}", "error")
        return False

    st.session_state.active_server_url = selected_url
    set_status(f"Connected to {build_server_option_label(selected_url)}", "success")
    return True


def disconnect_active_server() -> None:
    st.session_state.active_server_url = None
    st.session_state.monitor_snapshot = None
    st.session_state.biologic_snapshot = None
    st.session_state.available_nodes = []
    st.session_state.selected_nodes = []
    st.session_state.data_history = {}
    set_status("Disconnected client session.", "info")