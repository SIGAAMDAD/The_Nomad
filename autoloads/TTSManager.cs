using Godot;

public partial class TTSManager : Node {
	private static string[] TtsVoices;

	public static void Speak( StringName text ) {
		if ( !SettingsData.GetTextToSpeech() ) {
			return;
		}
		DisplayServer.TtsSpeak( TranslationServer.Translate( text ), TtsVoices[ SettingsData.GetTtsVoiceIndex() ] );
	}
	public static string[] GetAvailableVoices() {
		TtsVoices = DisplayServer.TtsGetVoicesForLanguage( TranslationServer.GetLocale() );
		return TtsVoices;
	}
};