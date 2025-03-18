using Godot;

public class AccessibilityManager {
	public static Theme DyslexiaTheme;
	public static Theme DefaultTheme;

	static AccessibilityManager() {
		DyslexiaTheme = ResourceLoader.Load<Theme>( "res://resources/dyslexia.tres" );
		DefaultTheme = ResourceLoader.Load<Theme>( "res://resources/default.tres" );
	}

	public static void RumbleController( float nAmount, float nDuration, int nDevice = 0 ) {
		Input.StartJoyVibration( nDevice, nAmount, nAmount, nDuration );
	}
	public static bool IsControllerRumbling( int nDevice = 0 ) {
		return Input.GetJoyVibrationStrength( nDevice ) > Godot.Vector2.Zero;
	}
};