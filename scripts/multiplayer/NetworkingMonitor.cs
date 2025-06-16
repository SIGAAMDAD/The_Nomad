using Godot;
using Steamworks;

public partial class NetworkingMonitor : CanvasLayer {
	private Label OutputBytes;
	private Label OutputPackets;

	private Label InputBytes;
	private Label InputPackets;

	private Label Ping;

	public override void _Ready() {
		base._Ready();

		InputBytes = GetNode<Label>( "VBoxContainer/InputContainer/BytesLabel" );
		InputPackets = GetNode<Label>( "VBoxContainer/InputContainer/PacketsLabel" );

		OutputBytes = GetNode<Label>( "VBoxContainer/OutputContainer/BytesLabel" );
		OutputPackets = GetNode<Label>( "VBoxContainer/OutputContainer/PacketsLabel" );

		Ping = GetNode<Label>( "VBoxContainer/PingLabel" );

		VisibilityChanged += () => SetProcess( Visible );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		SteamNetConnectionRealTimeStatus_t status = new SteamNetConnectionRealTimeStatus_t();
		SteamNetConnectionRealTimeLaneStatus_t laneStatus = new SteamNetConnectionRealTimeLaneStatus_t();
		foreach ( var conn in SteamLobby.GetConnections() ) {
			if ( SteamNetworkingSockets.GetConnectionRealTimeStatus( conn.Value, ref status, 0, ref laneStatus ) == EResult.k_EResultOK ) {
				Ping.Text = string.Format( "PING: {0}ms", status.m_nPing );

				InputBytes.Text = string.Format( "Input Bytes: {0} KB/s", status.m_flInBytesPerSec / 1024.0f );
				InputPackets.Text = string.Format( "Input Packets: {0}s", status.m_flInPacketsPerSec );

				OutputBytes.Text = string.Format( "Output Bytes: {0} KB/s", status.m_flOutBytesPerSec / 1024.0f );
				OutputPackets.Text = string.Format( "Output Packets: {0}s", status.m_flOutPacketsPerSec );
			}
		}
	}
};