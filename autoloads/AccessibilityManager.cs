using Godot;

public class AccessibilityManager {
	public static Theme DyslexiaTheme;
	public static Theme DefaultTheme;

	static AccessibilityManager() {
		DyslexiaTheme = ResourceLoader.Load<Theme>( "res://resources/dyslexia.tres" );
		DefaultTheme = ResourceLoader.Load<Theme>( "res://resources/default.tres" );
	}

	public static void RumbleController( float nAmount, float nDuration, int nDevice = 0 ) {
		float realAmount = nAmount * ( 1.0f / SettingsData.GetHapticStrength() );
		Input.StartJoyVibration( nDevice, nAmount, nAmount, nDuration );
	}
	public static bool IsControllerRumbling( int nDevice = 0 ) {
		return Input.GetJoyVibrationStrength( nDevice ) > Godot.Vector2.Zero;
	}
	public static string GetBindString( Resource mappingContext, Resource action ) {
		Godot.Collections.Array<RefCounted> items = (Godot.Collections.Array<RefCounted>)SettingsData.GetRemapper().Call( "get_remappable_items", mappingContext, "", action );
		Resource input = (Resource)SettingsData.GetRemapper().Call( "get_bound_input_or_null", items[0] );
		return input == null ? "[color=red]UNBOUND[/color]" : (string)SettingsData.GetMappingFormatter().Call( "input_as_richtext_async", input );
	}
};