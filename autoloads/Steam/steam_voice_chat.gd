extends CanvasLayer

@onready var _network_playback: AudioStreamGeneratorPlayback = null
@onready var _network_buffer: PackedByteArray = PackedByteArray()

func _ready() -> void:
	Steam.steamInit( 3512240, true )
	Steam.startVoiceRecording()
	
	_network_playback = $Output.get_stream_playback()

func _exit_tree() -> void:
	Steam.stopVoiceRecording()

func _process( _delta: float ) -> void:
	check_for_voice()

func check_for_voice() -> void:
	var available_voice:Dictionary = Steam.getAvailableVoice()
	if !available_voice.has( "result" ):
		return
	if available_voice[ "result" ] == Steam.VOICE_RESULT_OK && available_voice[ "buffer" ] > 0:
		var voice_data:Dictionary = Steam.getVoice()
		if voice_data[ "result" ] == Steam.VOICE_RESULT_OK:
			SteamLobby.SendVoicePacket( voice_data[ "buffer" ] )

func process_incoming_voice( user_id: int, voice_data: Dictionary ) -> void:
	var decompressed_voice:Dictionary = Steam.decompressVoice( voice_data[ "buffer" ], Steam.getVoiceOptimalSampleRate() )
	
	if decompressed_voice[ "result" ] == Steam.VOICE_RESULT_OK && decompressed_voice[ "size" ] > 0:
		_network_buffer = decompressed_voice[ "uncompressed" ]
		_network_buffer.resize( decompressed_voice[ "size" ] )
		
		for i: int in _network_playback.get_frames_available():
			var rawValue: int = _network_buffer[ 0 ] | ( _network_buffer[ 1 ] << 8 )
			rawValue = ( rawValue + 32768 ) & 0xffff
			var amplitude: float = float( rawValue - 32768 ) / 32768.0
			
			_network_playback.push_frame( Vector2( amplitude, amplitude ) )
			_network_buffer.remove_at( 0 )
			_network_buffer.remove_at( 0 )
