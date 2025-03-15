using Godot;

public class UISfxManager {
	public static AudioStream ButtonPressed;
	public static AudioStream ButtonFocused;
	public static AudioStream BeginGame;

	static UISfxManager() {
		ButtonPressed = ResourceLoader.Load<AudioStream>( "res://sounds/ui/button_pressed.ogg" );
		ButtonFocused = ResourceLoader.Load<AudioStream>( "res://sounds/ui/button_focused.ogg" );
		BeginGame = ResourceLoader.Load<AudioStream>( "res://sounds/ui/begin_game.ogg" );
	}
};