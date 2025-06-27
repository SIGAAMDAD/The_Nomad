using Godot;
using System.Collections.Generic;

public partial class AccessibilityManager : Node {
	public static Theme DyslexiaTheme;
	public static Theme DefaultTheme;

	private static Dictionary<Resource, Dictionary<Resource, string>> InputMappings;

	static AccessibilityManager() {
		DyslexiaTheme = ResourceLoader.Load<Theme>( "res://resources/dyslexia.tres" );
		DefaultTheme = ResourceLoader.Load<Theme>( "res://resources/default.tres" );
	}

	public static void LoadBinds() {
		ResourceCache.KeyboardInputMappings = ResourceLoader.Load( "res://resources/binds/binds_keyboard.tres" );
		ResourceCache.GamepadInputMappings = ResourceLoader.Load( "res://resources/binds/binds_gamepad.tres" );

		ResourceCache.LoadKeyboardBinds();
		ResourceCache.LoadGamepadBinds();

		Dictionary<Resource, string> keyboardMappings = new Dictionary<Resource, string> {
			{ ResourceCache.MoveActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.MoveActionKeyboard ) },
			{ ResourceCache.DashActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.DashActionKeyboard ) },
			{ ResourceCache.SlideActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.SlideActionKeyboard ) },
			{ ResourceCache.UseWeaponActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.UseWeaponActionKeyboard ) },
			{ ResourceCache.AimAngleActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.AimAngleActionKeyboard ) },
			{ ResourceCache.MeleeActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.MeleeActionKeyboard ) },
			{ ResourceCache.BulletTimeActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.BulletTimeActionKeyboard ) },
			{ ResourceCache.NextWeaponActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.NextWeaponActionKeyboard ) },
			{ ResourceCache.PrevWeaponActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.PrevWeaponActionKeyboard ) },
			{ ResourceCache.OpenInventoryActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.OpenInventoryActionKeyboard ) },
		};

		InputMappings = new Dictionary<Resource, Dictionary<Resource, string>>() {
			{ ResourceCache.KeyboardInputMappings, keyboardMappings }
//			{ ResourceCache.GamepadInputMappings, gamepadMappings }
		};
	}
	private static string LoadBindString( Resource mappingContext, Resource action ) {
		Godot.Collections.Array<RefCounted> items = (Godot.Collections.Array<RefCounted>)SettingsData.GetRemapper().Call( "get_remappable_items", mappingContext, "", action );
		Resource input = (Resource)SettingsData.GetRemapper().Call( "get_bound_input_or_null", items[ 0 ] );
		return input == null ? "[color=red]UNBOUND[/color]" : (string)SettingsData.GetMappingFormatter().Call( "input_as_richtext_async", input );
	}

	public static void RumbleController( float nAmount, float nDuration, int nDevice = 0 ) {
		float realAmount = nAmount * ( 1.0f / SettingsData.GetHapticStrength() );
		Input.StartJoyVibration( nDevice, realAmount, realAmount, nDuration );
	}
	public static bool IsControllerRumbling( int nDevice = 0 ) {
		return Input.GetJoyVibrationStrength( nDevice ) > Godot.Vector2.Zero;
	}
	public static string GetBindString( Resource action ) {
		if ( InputMappings.TryGetValue( LevelData.Instance.ThisPlayer.GetCurrentMappingContext(), out Dictionary<Resource, string> value ) ) {
			if ( value.TryGetValue( action, out string bind ) ) {
				return bind;
			}
			Console.PrintError( string.Format( "AccessibilityManager.GetBindString: invalid GUIDEAction {0}", action.ResourcePath ) );
			return null;
		}
		Console.PrintError( string.Format( "AccessibilityManager.GetBindString: invalid GUIDMappingContext {0}", LevelData.Instance.ThisPlayer.GetCurrentMappingContext().ResourcePath ) );
		return null;
	}
};