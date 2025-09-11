/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using ResourceCache;
using System.Collections.Generic;
using Menus;
using System;

/*
===================================================================================

AccessibilityManager

===================================================================================
*/

public partial class AccessibilityManager : Node {
	public static Theme DyslexiaTheme;
	public static Theme DefaultTheme;

	private static IReadOnlyDictionary<Resource, string> KeyboardInputMappings;
	private static IReadOnlyDictionary<Resource, string> GamepadInputMappings;

	/*
	===============
	AccessibilityManager
	===============
	*/
	static AccessibilityManager() {
		DyslexiaTheme = ResourceLoader.Load<Theme>( "res://resources/dyslexia.tres" );
		DefaultTheme = ResourceLoader.Load<Theme>( "res://resources/default.tres" );
	}

	/*
	===============
	GetPrevMenuButtonTexture
	===============
	*/
	public static Texture2D? GetPrevMenuButtonTexture() {
		return Input.GetConnectedJoypads().Count > 0 ?
				Input.GetJoyName( 0 ) switch {
					"XBox" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Xbox Series/Default/xbox_lb.png" ),
					"PS4" or "PS5" or "PlayStation" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/PlayStation Series/Default/playstation_trigger_l1_alternative.png" ),
					_ => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Steam Controller/Default/steam_lb.png" ),
				}
			:
				TextureCache.GetTexture( "res://textures/kenny_input-prompts/Keyboard & Mouse/Default/keyboard_q.png" );
	}

	/*
	===============
	GetNextMenuButtonTexture
	===============
	*/
	public static Texture2D? GetNextMenuButtonTexture() {
		return Input.GetConnectedJoypads().Count > 0 ?
				Input.GetJoyName( 0 ) switch {
					"XBox" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Xbox Series/Default/xbox_rb.png" ),
					"PS4" or "PS5" or "PlayStation" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/PlayStation Series/Default/playstation_trigger_r1_alternative.png" ),
					_ => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Steam Controller/Default/steam_rb.png" ),
				}
			:
				TextureCache.GetTexture( "res://textures/kenny_input-prompts/Keyboard & Mouse/Default/keyboard_e.png" );
	}

	/*
	===============
	GetSaveSettingsButtonTexture
	===============
	*/
	public static Texture2D? GetSaveSettingsButtonTexture() {
		return Input.GetConnectedJoypads().Count > 0 ?
				Input.GetJoyName( 0 ) switch {
					"XBox One Controller" or "XBox 360 Controller" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Xbox Series/Default/xbox_button_x.png" ),
					"PS4 Controller" or "PS5 Controller" or "Sony PlayStation Controller" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/PlayStation Series/Default/playstation_button_color_square.png" ),
					_ => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Steam Controller/Default/steam_button_color_x.png" ),
				}
			:
				TextureCache.GetTexture( "res://textures/kenny_input-prompts/Keyboard & Mouse/Default/keyboard_enter.png" );
	}

	/*
	===============
	GetResetSettingsButtonTexture
	===============
	*/
	public static Texture2D? GetResetSettingsButtonTexture() {
		return Input.GetConnectedJoypads().Count > 0 ?
				Input.GetJoyName( 0 ) switch {
					"XBox" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Xbox Series/Default/xbox_button_y.png" ),
					"PS4" or "PS5" or "PlayStation" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/PlayStation Series/Default/playstation_button_color_triangle.png" ),
					_ => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Steam Controller/Default/steam_button_color_y.png" ),
				}
			:
				TextureCache.GetTexture( "res://textures/kenny_input-prompts/Keyboard & Mouse/Default/keyboard_backspace.png" );
	}

	/*
	===============
	GetExitMenuButtonTexture
	===============
	*/
	public static Texture2D? GetExitMenuButtonTexture() {
		return Input.GetConnectedJoypads().Count > 0 ?
				Input.GetJoyName( 0 ) switch {
					"XBox One Controller" or "XBox 360 Controller" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Xbox Series/Default/xbox_button_b.png" ),
					"PS4 Controller" or "PS5 Controller" or "Sony PlayStation Controller" => TextureCache.GetTexture( "res://textures/kenny_input-prompts/PlayStation Series/Default/playstation_button_color_circle.png" ),
					_ => TextureCache.GetTexture( "res://textures/kenny_input-prompts/Steam Controller/Default/steam_button_color_b.png" ),
				}
			:
				TextureCache.GetTexture( "res://textures/kenny_input-prompts/Keyboard & Mouse/Default/keyboard_escape.png" );
	}

	/*
	===============
	LoadBinds
	===============
	*/
	public static void LoadBinds( in Player.InputController input ) {
		ArgumentNullException.ThrowIfNull( input );
		ArgumentNullException.ThrowIfNull( input.Bindings );

		var keyboardInputMappings = new Dictionary<Resource, string>( (int)Player.InputController.ControlBind.Count );
		for ( Player.InputController.ControlBind bind = Player.InputController.ControlBind.Move; bind < Player.InputController.ControlBind.Count; bind++ ) {
			Resource? action = input.Bindings[ Player.InputController.BindMapping.Keyboard ].Binds[ bind ];
			ArgumentNullException.ThrowIfNull( action );

			keyboardInputMappings.Add(
				action,
				LoadBindString( input.Bindings[ Player.InputController.BindMapping.Keyboard ].MappingContext, action )
			);
		}

		var gamepadInputMappings = new Dictionary<Resource, string>( (int)Player.InputController.ControlBind.Count * 4 );
		for ( Player.InputController.BindMapping mapper = Player.InputController.BindMapping.Gamepad0; mapper < Player.InputController.BindMapping.Count; mapper++ ) {
			for ( Player.InputController.ControlBind bind = Player.InputController.ControlBind.Move; bind < Player.InputController.ControlBind.Count; bind++ ) {
				Resource? action = input.Bindings[ mapper ].Binds[ bind ];
				ArgumentNullException.ThrowIfNull( action );

				gamepadInputMappings.Add(
					action,
					LoadBindString( input.Bindings[ mapper ].MappingContext, action )
				);
			}
		}
		GamepadInputMappings = gamepadInputMappings;
	}

	/*
	===============
	LoadBindStrings
	===============
	*/
	private static string? LoadBindString( Resource mappingContext, Resource action ) {
		Godot.Collections.Array<RefCounted> items = (Godot.Collections.Array<RefCounted>)SettingsData.Remapper.Call( "get_remappable_items", mappingContext, "", action );
		if ( items.Count == 0 ) {
			return null;
		}
		Resource input = (Resource)SettingsData.Remapper.Call( "get_bound_input_or_null", items[ 0 ] );
		return input == null ? "[color=red]UNBOUND[/color]" : (string)SettingsData.MappingFormatter.Call( "input_as_richtext_async", input );
	}

	/*
	===============
	RumbleController
	===============
	*/
	public static void RumbleController( float amount, float duration, int device = 0 ) {
		float realAmount = amount * ( 1.0f / SettingsData.HapticStrength );
		Input.StartJoyVibration( device, realAmount, realAmount, duration );
	}

	/*
	===============
	IsControllerRumbling
	===============
	*/
	public static bool IsControllerRumbling( int device = 0 ) {
		return Input.GetJoyVibrationStrength( device ) > Godot.Vector2.Zero;
	}

	/*
	===============
	GetBindString
	===============
	*/
	public static string? GetBindString( Resource action ) {
		if ( KeyboardInputMappings.TryGetValue( action, out string? bind ) ) {
			return bind;
		} else if ( GamepadInputMappings.TryGetValue( action, out bind ) ) {
			return bind;
		}
		Console.PrintError( string.Format( "AccessibilityManager.GetBindString: invalid GUIDEAction {0}", action.ResourcePath ) );
		return null;
	}
};