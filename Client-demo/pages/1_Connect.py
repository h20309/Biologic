import streamlit as st

from opcua_connect import (
    add_manual_server,
    build_server_option_label,
    clear_status,
    connect_selected_server,
    disconnect_active_server,
    ensure_session_state,
    get_network_summary,
    render_status,
    run_lan_scan,
    run_local_discovery,
)


st.set_page_config(
    page_title="Connect OPC UA Server",
    page_icon="🔌",
    layout="wide",
)

ensure_session_state()

st.title("🔌 Connect OPC UA Server")

if not st.session_state.local_discovery_completed and not st.session_state.discovered_servers:
    run_local_discovery()

render_status()

st.caption("Use local discovery first. Run LAN scan only when the target server is not on this machine.")

button_col1, button_col2, button_col3 = st.columns([1.2, 1.2, 1])
with button_col1:
    if st.button("Discover Servers", use_container_width=True):
        run_local_discovery()
with button_col2:
    if st.button("Scan LAN", use_container_width=True):
        run_lan_scan()
with button_col3:
    if st.button("Disconnect", use_container_width=True, disabled=st.session_state.active_server_url is None):
        disconnect_active_server()

st.caption(f"LAN scan target subnet(s): {get_network_summary()}")

discovered_urls = [server["url"] for server in st.session_state.discovered_servers]
select_col, connect_col = st.columns([4, 1.2])

if discovered_urls:
    if st.session_state.selected_server_url not in discovered_urls:
        st.session_state.selected_server_url = discovered_urls[0]

    with select_col:
        selected_index = discovered_urls.index(st.session_state.selected_server_url)
        st.session_state.selected_server_url = st.selectbox(
            "Discovered Servers",
            options=discovered_urls,
            index=selected_index,
            format_func=build_server_option_label,
        )

    with connect_col:
        st.write("")
        st.write("")
        if st.button("Connect", use_container_width=True):
            if connect_selected_server():
                st.switch_page("main.py")
else:
    with select_col:
        st.text_input("Discovered Servers", value="No discovered servers yet", disabled=True)
    with connect_col:
        st.write("")
        st.write("")
        st.button("Connect", use_container_width=True, disabled=True)

manual_col, add_col = st.columns([4, 1.2])
with manual_col:
    st.text_input(
        "Manual URL",
        key="manual_server_url",
        placeholder="opc.tcp://hostname:4840",
        help="Use this when discovery does not list the endpoint you want.",
    )
with add_col:
    st.write("")
    st.write("")
    if st.button("Add URL", use_container_width=True):
        add_manual_server()

if st.session_state.active_server_url:
    st.success(f"Connected target: {build_server_option_label(st.session_state.active_server_url)}")
    if st.button("Go To Instrument Page", use_container_width=False):
        clear_status()
        st.switch_page("main.py")
else:
    st.info("Select or add a server, then connect. The instrument page will stay unavailable until connection succeeds.")

with st.expander("Instructions", expanded=False):
    st.markdown(
        """
        ### Connection Flow

        - Discover Servers: quick probe on this machine using ports inferred from ORBIT profiles.
        - Scan LAN: optional deeper scan across the current subnet.
        - Discovered Servers: choose a validated OPC UA endpoint from the dropdown list.
        - Add URL: manually validate and add a fallback endpoint.
        - Connect: establish the client session and jump to the instrument page.
        """
    )