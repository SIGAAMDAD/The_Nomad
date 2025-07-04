using Godot;
using System.Collections.Generic;

public partial class AccessibilityManager : Node {
	public static Theme DyslexiaTheme;
	public static Theme DefaultTheme;

	private static Dictionary<Resource, string> KeyboardInputMappings;
	private static Dictionary<Resource, string> GamepadInputMappings;

	static AccessibilityManager() {
		DyslexiaTheme = ResourceLoader.Load<Theme>( "res://resources/dyslexia.tres" );
		DefaultTheme = ResourceLoader.Load<Theme>( "res://resources/default.tres" );
	}

	public static Texture2D GetPrevMenuButtonTexture() {
		string name = Input.GetJoyName( 0 );
		switch ( name ) {
		case "XBox":
			return ResourceCache.GetTexture( "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_lb.png" );
		case "PS4":
		case "PS5":
		case "PlayStation":
			return ResourceCache.GetTexture( "res://textures/kenney_input-prompts/PlayStation Series/Default/playstation_trigger_l1_alternative.png" );
		};
		return ResourceCache.GetTexture( "res://textures/kenney_input-prompts/Steam Controller/Default/steam_lb.png" );
	}

	public static Texture2D GetNextMenuButtonTexture() {
		string name = Input.GetJoyName( 0 );
		switch ( name ) {
		case "XBox":
			return ResourceCache.GetTexture( "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_rb.png" );
		case "PS4":
		case "PS5":
		case "PlayStation":
			return ResourceCache.GetTexture( "res://textures/kenney_input-prompts/PlayStation Series/Default/playstation_trigger_r1_alternative.png" );
		};
		return ResourceCache.GetTexture( "res://textures/kenney_input-prompts/Steam Controller/Default/steam_rb.png" );
	}

	public static void LoadBinds() {
		ResourceCache.KeyboardInputMappings ??= ResourceLoader.Load( "res://resources/binds/binds_keyboard.tres" );
		ResourceCache.GamepadInputMappings ??= ResourceLoader.Load( "res://resources/binds/binds_gamepad.tres" );

		ResourceCache.LoadKeyboardBinds();
		ResourceCache.LoadGamepadBinds();

		KeyboardInputMappings = new Dictionary<Resource, string> {
			{ ResourceCache.MoveActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.MoveActionKeyboard ) },
			{ ResourceCache.DashActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.DashActionKeyboard ) },
			{ ResourceCache.SlideActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.SlideActionKeyboard ) },
			{ ResourceCache.UseWeaponActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.UseWeaponActionKeyboard ) },
			{ ResourceCache.ArmAngleActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.ArmAngleActionKeyboard ) },
			{ ResourceCache.MeleeActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.MeleeActionKeyboard ) },
			{ ResourceCache.BulletTimeActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.BulletTimeActionKeyboard ) },
			{ ResourceCache.NextWeaponActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.NextWeaponActionKeyboard ) },
			{ ResourceCache.PrevWeaponActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.PrevWeaponActionKeyboard ) },
			{ ResourceCache.OpenInventoryActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.OpenInventoryActionKeyboard ) },
			{ ResourceCache.InteractActionKeyboard, LoadBindString( ResourceCache.KeyboardInputMappings, ResourceCache.InteractActionKeyboard ) },
		};

		GamepadInputMappings = new Dictionary<Resource, string>();
		for ( int i = 0; i < 1; i++ ) {
			GamepadInputMappings.Add( ResourceCache.MoveActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.MoveActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.DashActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.DashActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.SlideActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.SlideActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.UseWeaponActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.UseWeaponActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.ArmAngleActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.ArmAngleActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.MeleeActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.MeleeActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.BulletTimeActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.BulletTimeActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.NextWeaponActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.NextWeaponActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.PrevWeaponActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.PrevWeaponActionGamepad[ i ] ) );
			GamepadInputMappings.Add( ResourceCache.OpenInventoryActionGamepad[ i ], LoadBindString( ResourceCache.GamepadInputMappings, ResourceCache.OpenInventoryActionGamepad[ i ] ) );
		}
	}
	private static string LoadBindString( Resource mappingContext, Resource action ) {
		Godot.Collections.Array<RefCounted> items = (Godot.Collections.Array<RefCounted>)SettingsData.GetRemapper().Call( "get_remappable_items", mappingContext, "", action );
		if ( items.Count == 0 ) {
			return null;
		}
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
		if ( KeyboardInputMappings.TryGetValue( action, out string bind ) ) {
			return bind;
		} else if ( GamepadInputMappings.TryGetValue( action, out bind ) ) {
			return bind;
		}
		Console.PrintError( string.Format( "AccessibilityManager.GetBindString: invalid GUIDEAction {0}", action.ResourcePath ) );
		return null;
	}
};