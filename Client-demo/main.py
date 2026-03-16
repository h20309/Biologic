import streamlit as st

from opcua_app import render_instrument_page
from opcua_connect import build_server_option_label, ensure_session_state, render_status


st.set_page_config(
    page_title="Biologic OPC UA Console",
    page_icon="🔌",
    layout="wide",
)

ensure_session_state()

active_server_url = st.session_state.active_server_url
if not active_server_url:
    st.switch_page("pages/1_Connect.py")

st.title("🔌 Biologic Instrument Page")
render_status()

header_col1, header_col2 = st.columns([4, 1.2])
with header_col1:
    st.caption(f"Active Server: {build_server_option_label(active_server_url)}")
with header_col2:
    if st.button("Open Connect Page", use_container_width=True):
        st.switch_page("pages/1_Connect.py")

render_instrument_page(active_server_url)
