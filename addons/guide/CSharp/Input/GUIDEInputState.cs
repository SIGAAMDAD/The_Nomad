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
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDEInputState
	
	===================================================================================
	*/
	/// <summary>
	/// The GUIDEInputState holds the current state of all input. It is basically a wrapper around Godot's Input
	/// class that provides some additional functionality like getting the information if any key or mouse button
	/// is currently pressed. It also is the single entry point for all input events from Godot, so we don't have 
	/// process them in every GUIDEInput object and duplicate input handling code everywere. This also improves performance.
	/// </summary>

	public partial class GUIDEInputState : GodotObject {
		/// <summary>
		/// Device ID for a virtual joystick that means "any joystick".
		/// This relies on the fact that Godot's device IDs for joysticks are always >= 0.
		/// https://github.com/godotengine/godot/blob/80a3d205f1ad22e779a64921fb56d62b893881ae/core/input/input.cpp#L1821
		/// </summary>
		public const int ANY_JOY_DEVICE_ID = -1;

		/// <summary>
		/// Signalled, when the keyboard state has changed
		/// </summary>
		[Signal]
		public delegate void KeyboardStateChangedEventHandler();

		/// <summary>
		/// Signalled, when the mouse motion state has changed
		/// </summary>
		[Signal]
		public delegate void MousePositionChangedEventHandler();

		/// <summary>
		/// Signalled, when the mouse button state has changed
		/// </summary>
		[Signal]
		public delegate void MouseButtonStateChangedEventHandler();

		/// <summary>
		/// Signalled, when the joy button state has changed
		/// </summary>
		[Signal]
		public delegate void JoyButtonStateChangedEventHandler();

		/// <summary>
		/// Signalled, when the joy axis state has changed
		/// </summary>
		[Signal]
		public delegate void JoyAxisStateChangedEventHandler();

		/// <summary>
		/// Signalled, when the touch state has changed
		/// </summary>
		[Signal]
		public delegate void TouchStateChangedEventHandler();

		/// <summary>
		/// Keys that are currently pressed. Key is the key index, value is not important. The presence of a key in the dictionary
		/// indicates that the key is currently pressed
		/// </summary>
		private Dictionary<Key, bool> Keys = new Dictionary<Key, bool>();

		/// <summary>
		/// Fingers that are currently touching the screen. Key is the finger index, value is the position (Vector2)
		/// </summary>
		private Dictionary<int, Vector2> FingerPositions = new Dictionary<int, Vector2>();

		/// <summary>
		/// The mouse movement since the last frame
		/// </summary>
		private Vector2 MouseMovement = Vector2.Zero;

		/// <summary>
		/// Mouse buttons that are currently pressed. Key is the button index, value not important. The presence of a key
		/// in the dictionary indicates that the button is currently pressed
		/// </summary>
		private Dictionary<MouseButton, bool> MouseButtons = new Dictionary<MouseButton, bool>();

		/// <summary>
		/// Joy buttons that are currently pressed. Key is device id, value is a dictionary with the button index as key. The 
		/// value of the inner dictionary is not important. The presence of a key in the inner dictionary indicates that the button 
		/// is currently pressed
		/// </summary>
		private Dictionary<int, Dictionary<JoyButton, bool>> JoyButtons = new Dictionary<int, Dictionary<JoyButton, bool>>();

		/// <summary>
		/// Current values of joy axes. Key is device id, value is a dictionary with the axis index as key. 
		/// The value of the inner dictionary is the axis value. Once an axis is actuated, it will be added to the dictionary.
		/// We will not remove it anymore after that
		/// </summary>
		private Dictionary<int, Dictionary<JoyAxis, float>> JoyAxes = new Dictionary<int, Dictionary<JoyAxis, float>>();

		/// <summary>
		/// The current mapping of joy index to device id. This is used to map the joy index to the device id. A joy index
		/// of -1 means "any device id"
		/// </summary>
		private Dictionary<int, int> JoyIndexToDeviceIndex = new Dictionary<int, int>();

		/// <summary>
		/// This holds the state of keys that have changed this frame. The key is the key, the value is true if the key
		/// was last pressed and false if it was last released
		/// </summary>
		private Dictionary<Key, bool> PendingKeys = new Dictionary<Key, bool>();

		/// <summary>
		/// This holds the state of mouse buttons that have changed this frame. The key is the mouse button index, the value is
		/// true, if the mouse button was last pressed and false if it was last released
		/// </summary>
		private Dictionary<MouseButton, bool> PendingMouseButtons = new Dictionary<MouseButton, bool>();

		/// <summary>
		/// This holds the state of joy buttons that have changed this frame. The key is the joy device id, the value is
		/// a nested dictionary. The nested dictionary has the button index as key and true as value if the button was last
		/// pressed or false if it was last released
		/// </summary>
		private Dictionary<int, Dictionary<JoyButton, bool>> PendingJoyButtons = new Dictionary<int, Dictionary<JoyButton, bool>>();

		public GUIDEInputState() {
			GameEventBus.Subscribe<Input.JoyConnectionChangedEventHandler>( this, RefreshJoyIds );
			Clear();
		}

		private void HandleKeyboardInput( InputEventKey keyButton ) {
			Key index = keyButton.PhysicalKeycode;
			bool pressed = keyButton.Pressed;

			if ( PendingKeys.ContainsKey( index ) ) {
				PendingKeys[ index ] = pressed;
				return;
			}

			PendingKeys[ index ] = pressed;

			if ( pressed && !Keys.ContainsKey( index ) ) {
				Keys[ index ] = true;
				EmitSignalKeyboardStateChanged();
				return;
			}
			if ( !pressed && Keys.ContainsKey( index ) ) {
				Keys.Remove( index );
				EmitSignalKeyboardStateChanged();
				return;
			}
		}

		private void HandleMouseMotionInput( InputEventMouseMotion mouseMotion ) {
			MouseMovement += mouseMotion.Relative;
			EmitSignalMousePositionChanged();
		}

		private void HandleMouseButtonInput( InputEventMouseButton mouseButton ) {
			MouseButton index = mouseButton.ButtonIndex;
			bool pressed = mouseButton.Pressed;

			if ( PendingMouseButtons.ContainsKey( index ) ) {
				PendingMouseButtons[ index ] = pressed;
				return;
			}

			PendingMouseButtons[ index ] = pressed;

			if ( pressed && !MouseButtons.ContainsKey( index ) ) {
				MouseButtons[ index ] = true;
				EmitSignalMouseButtonStateChanged();
				return;
			}
			if ( !pressed && MouseButtons.ContainsKey( index ) ) {
				MouseButtons.Remove( index );
				EmitSignalMouseButtonStateChanged();
				return;
			}
		}

		private void HandleJoyButtonInput( InputEventJoypadButton joyButton ) {
			int deviceId = joyButton.Device;
			JoyButton button = joyButton.ButtonIndex;
			bool pressed = joyButton.Pressed;

			if ( PendingJoyButtons[ deviceId ].ContainsKey( button ) ) {
				PendingJoyButtons[ deviceId ][ button ] = pressed;
				return;
			}

			PendingJoyButtons[ deviceId ][ button ] = pressed;

			bool changed = false;
			if ( pressed && !JoyButtons[ deviceId ].ContainsKey( button ) ) {
				JoyButtons[ deviceId ][ button ] = true;
				changed = true;
				return;
			}
			if ( !pressed && JoyButtons[ deviceId ].ContainsKey( button ) ) {
				JoyButtons[ deviceId ].Remove( button );
				changed = true;
				return;
			}

			if ( changed ) {
				bool anyValue = false;
				foreach ( var inner in JoyButtons.Keys ) {
					if ( inner != ANY_JOY_DEVICE_ID && JoyButtons[ inner ].ContainsKey( button ) ) {
						anyValue = true;
						break;
					}
				}
				if ( anyValue ) {
					JoyButtons[ ANY_JOY_DEVICE_ID ][ button ] = true;
				} else {
					JoyButtons[ ANY_JOY_DEVICE_ID ].Remove( button );
				}
				EmitSignalJoyButtonStateChanged();
			}
		}

		private void HandleJoyMotionInput( InputEventJoypadMotion joyMotion ) {
			int deviceId = joyMotion.Device;
			JoyAxis axis = joyMotion.Axis;

			JoyAxes[ deviceId ][ axis ] = joyMotion.AxisValue;

			float anyValue = 0.0f;
			float maximumActuation = 0.0f;
			foreach ( var inner in JoyAxes.Keys ) {
				if ( inner != ANY_JOY_DEVICE_ID && JoyAxes[ inner ].ContainsKey( axis ) ) {
					float strength = Mathf.Abs( JoyAxes[ inner ][ axis ] );
					if ( strength > maximumActuation ) {
						maximumActuation = strength;
						anyValue = JoyAxes[ inner ][ axis ];
					}
				}
			}

			JoyAxes[ ANY_JOY_DEVICE_ID ][ axis ] = anyValue;

			EmitSignalJoyAxisStateChanged();
		}

		private void HandleScreenTouchInput( InputEventScreenTouch touch ) {
			if ( touch.Pressed ) {
				FingerPositions[ touch.Index ] = touch.Position;
			} else {
				FingerPositions.Remove( touch.Index );
			}
			EmitSignalTouchStateChanged();
		}

		private void HandleScreenDragInput( InputEventScreenDrag drag ) {
			FingerPositions[ drag.Index ] = drag.Position;
			EmitSignalTouchStateChanged();
		}

		public void Input( InputEvent @event ) {
			if ( @event is InputEventKey keyButton ) {
				HandleKeyboardInput( keyButton );
			} else if ( @event is InputEventMouseMotion mouseMotion ) {
				HandleMouseMotionInput( mouseMotion );
			} else if ( @event is InputEventMouseButton mouseButton ) {
				HandleMouseButtonInput( mouseButton );
			} else if ( @event is InputEventJoypadButton joyButton ) {
				HandleJoyButtonInput( joyButton );
			} else if ( @event is InputEventJoypadMotion joyMotion ) {
				HandleJoyMotionInput( joyMotion );
			}
		}

		public bool IsKeyPressed( Key key ) {
			return Keys.ContainsKey( key );
		}
		public bool IsAtLeastOneKeyPressed( Key[] keys ) {
			for ( int i = 0; i < keys.Length; i++ ) {
				if ( Keys.ContainsKey( keys[ i ] ) ) {
					return true;
				}
			}
			return false;
		}
		public bool AreAllKeysPressed( Key[] keys ) {
			for ( int i = 0; i < keys.Length; i++ ) {
				if ( !Keys.ContainsKey( keys[ i ] ) ) {
					return false;
				}
			}
			return true;
		}
		public bool IsAnyKeyPressed() {
			return Keys.Count > 0;
		}
		public Vector2 GetMouseDeltaSinceLastFrame() {
			return MouseMovement;
		}
		public Vector2 GetMousePosition() {
			return Engine.GetMainLoop().Get( "Root" ).AsGodotObject().Call( "get_mouse_position" ).AsVector2();
		}
		public bool IsMouseButtonPressed( MouseButton buttonIndex ) {
			return MouseButtons.ContainsKey( buttonIndex );
		}
		public bool IsAnyMouseButtonPressed() {
			return MouseButtons.Count > 0;
		}
		public float GetJoyAxisValue( int index, JoyAxis axis ) {
			if ( !JoyIndexToDeviceIndex.TryGetValue( index, out int deviceId ) ) {
				return 0.0f;
			}
			if ( JoyAxes.TryGetValue( deviceId, out Dictionary<JoyAxis, float>? inner ) ) {
				return inner[ axis ];
			}
			return 0.0f;
		}
		public bool IsAnyJoyButtonPressed() {
			foreach ( var inner in JoyButtons.Values ) {
				if ( inner.Count > 0 ) {
					return true;
				}
			}
			return false;
		}
		public bool IsAnyJoyAxisActuated( float miniumStrength ) {
			foreach ( var inner in JoyAxes.Values ) {
				foreach ( var value in inner.Values ) {
					if ( Mathf.Abs( value ) >= miniumStrength ) {
						return true;
					}
				}
			}
			return false;
		}
		public Vector2 GetFingerPosition( int fingerIndex, int fingerCount ) {
			if ( FingerPositions.Count == 0 ) {
				return Vector2.Zero;
			}
			if ( FingerPositions.Count != fingerCount ) {
				return Vector2.Inf;
			}
			if ( fingerIndex > -1 ) {
				return FingerPositions.TryGetValue( fingerIndex, out Vector2 value ) ? value : Vector2.Inf;
			}

			Vector2 result = Vector2.Zero;
			foreach ( var value in FingerPositions ) {
				result += value.Value;
			}
			return result / fingerCount;
		}
		public bool IsAnyFingerDown() {
			return FingerPositions.Count > 0;
		}

		/*
		===============
		Clear
		===============
		*/
		/// <summary>
		/// Used by the automated tests to make sure we don't have any leftovers from the
		/// last test
		/// </summary>
		private void Clear() {
			Keys.Clear();
			FingerPositions.Clear();
			MouseMovement = Vector2.Zero;
			MouseButtons.Clear();
			JoyButtons.Clear();
			JoyAxes.Clear();

			RefreshJoyIds( 0, 0 );

			JoyButtons[ ANY_JOY_DEVICE_ID ] = new Dictionary<JoyButton, bool>();
			JoyAxes[ ANY_JOY_DEVICE_ID ] = new Dictionary<JoyAxis, float>();
		}

		/*
		===============
		RefreshJoyIds
		===============
		*/
		private void RefreshJoyIds( int ignore1, int ignore2 ) {
			JoyIndexToDeviceIndex.Clear();

			Godot.Collections.Array<int> connectedJoys = Godot.Input.GetConnectedJoypads();
			for ( int i = 0; i < connectedJoys.Count; i++ ) {
				int deviceId = connectedJoys[ i ];

				JoyIndexToDeviceIndex.Add( i, deviceId );

				if ( !JoyButtons.ContainsKey( deviceId ) ) {
					JoyButtons[ deviceId ] = new Dictionary<JoyButton, bool>();
				}
				if ( !JoyAxes.ContainsKey( deviceId ) ) {
					JoyAxes[ deviceId ] = new Dictionary<JoyAxis, float>();
				}
				if ( !PendingJoyButtons.ContainsKey( deviceId ) ) {
					PendingJoyButtons[ deviceId ] = new Dictionary<JoyButton, bool>();
				}
			}

			JoyIndexToDeviceIndex.Add( -1, ANY_JOY_DEVICE_ID );

			foreach ( var deviceId in PendingJoyButtons.Keys ) {
				if ( deviceId != ANY_JOY_DEVICE_ID && !connectedJoys.Contains( deviceId ) ) {
					PendingJoyButtons.Remove( deviceId );
				}
			}

			bool dirty = false;
			foreach ( var deviceId in JoyButtons.Keys ) {
				if ( deviceId != ANY_JOY_DEVICE_ID && !connectedJoys.Contains( deviceId ) ) {
					dirty = true;
					JoyButtons.Remove( deviceId );
				}
			}

			if ( dirty ) {
				EmitSignalJoyButtonStateChanged();
			}

			dirty = false;
			foreach ( var deviceId in JoyAxes.Keys ) {
				if ( deviceId != ANY_JOY_DEVICE_ID && !connectedJoys.Contains( deviceId ) ) {
					dirty = true;
					JoyAxes.Remove( deviceId );
				}
			}

			if ( dirty ) {
				EmitSignalJoyAxisStateChanged();
			}
		}

		/*
		===============
		Reset
		===============
		*/
		private void Reset() {
			MouseMovement = Vector2.Zero;

			foreach ( var key in PendingKeys ) {
				bool isDown = key.Value;
				if ( isDown && !Keys.ContainsKey( key.Key ) ) {
					Keys[ key.Key ] = true;
					EmitSignalKeyboardStateChanged();
				} else if ( !isDown && Keys.ContainsKey( key.Key ) ) {
					Keys.Remove( key.Key );
					EmitSignalKeyboardStateChanged();
				}
			}

			PendingKeys.Clear();

			foreach ( var button in PendingMouseButtons ) {
				bool isDown = button.Value;
				if ( isDown && !MouseButtons.ContainsKey( button.Key ) ) {
					MouseButtons[ button.Key ] = true;
					EmitSignalMouseButtonStateChanged();
				} else if ( !isDown && MouseButtons.ContainsKey( button.Key ) ) {
					MouseButtons.Remove( button.Key );
					EmitSignalMouseButtonStateChanged();
				}
			}

			PendingMouseButtons.Clear();

			foreach ( var joy in PendingJoyButtons ) {
				foreach ( var button in PendingJoyButtons[ joy.Key ] ) {
					bool changed = false;
					bool isDown = PendingJoyButtons[ joy.Key ][ button.Key ];
					bool contains = JoyButtons[ joy.Key ].ContainsKey( button.Key );

					if ( isDown && !contains ) {
						JoyButtons[ joy.Key ][ button.Key ] = true;
						changed = true;
					} else if ( !isDown && contains ) {
						JoyButtons[ joy.Key ].Remove( button.Key );
						changed = true;
					}

					if ( changed ) {
						bool anyValue = false;
						foreach ( var inner in JoyButtons.Keys ) {
							if ( inner != ANY_JOY_DEVICE_ID && JoyButtons[ inner ].ContainsKey( button.Key ) ) {
								anyValue = true;
								break;
							}
						}
						if ( anyValue ) {
							JoyButtons[ ANY_JOY_DEVICE_ID ][ button.Key ] = true;
						} else {
							JoyButtons[ ANY_JOY_DEVICE_ID ].Remove( button.Key );
						}
						EmitSignalJoyButtonStateChanged();
					}
				}

				PendingJoyButtons[ joy.Key ].Clear();
			}
		}
	};
};