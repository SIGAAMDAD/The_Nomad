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
using System;

namespace GUIDE {
	[Tool]
	[Icon( "res://addons/guide/guide_action.svg" )]
	public partial class GUIDEAction : Resource {
		public enum GUIDEActionValueType {
			Invalid = -1,
			Bool = 0,
			Axis1D = 1,
			Axis2D = 2,
			Axis3D = 3
		};
		public enum GUIDEActionState {
			Triggered,
			Ongoing,
			Completed
		};

		/// <summary>
		/// The name of this action. Required when this action should be used as
		/// Godot action. Also displayed in the debugger.
		/// </summary>
		[Export]
		public StringName? Name {
			get => _name;
			set {
				if ( _name == value ) {
					return;
				}
				_name = value;
				EmitSignalChanged();
			}
		}

		/// <summary>
		/// The action value type.
		/// </summary>
		[Export]
		public GUIDEActionValueType ActionValueType {
			get => _actionValueType;
			set {
				if ( _actionValueType == value ) {
					return;
				}
				_actionValueType = value;
				EmitSignalChanged();
			}
		}
		[Export]
		public bool BlockLowerPriorityActions {
			get => _blockLowerPriorityActions;
			set {
				if ( _blockLowerPriorityActions == value ) {
					return;
				}
				_blockLowerPriorityActions = value;
				EmitSignalChanged();
			}
		}

		[ExportCategory( "Godot Actions" )]

		/// <summary>
		/// If true, then this action will b eemitted into Godot's
		/// built-in action system. This can be helpful to interact with
		/// code using this system, like Godot's UI system. Actions
		/// will be emitted on trigger and completion (e.g. button down
		/// and button up).
		/// </summary>
		[Export]
		public bool EmitAsGodotActions {
			get => _emitAsGodotActions;
			set {
				if ( _emitAsGodotActions == value ) {
					return;
				}
				_emitAsGodotActions = value;
				EmitSignalChanged();
			}
		}

		/// <summary>
		/// If true, players can remap this action. To be remappable, make sure
		/// that a name and the action type are properly set.
		/// </summary>
		[Export]
		public bool IsRemappable {
			get => _isRemappable;
			set {
				if ( _isRemappable == value ) {
					return;
				}
				_isRemappable = value;
				EmitSignalChanged();
			}
		}

		/// <summary>
		/// The display name of the action shown to the player
		/// </summary>
		[Export]
		public string? DisplayName {
			get => _displayName;
			set {
				if ( _displayName == value ) {
					return;
				}
				_displayName = value;
				EmitSignalChanged();
			}
		}

		/// <summary>
		/// The display category of the action shown to the player
		/// </summary>
		[Export]
		public string? DisplayCategory {
			get => _displayCategory;
			set {
				if ( _displayCategory == value ) {
					return;
				}
				_displayCategory = value;
				EmitSignalChanged();
			}
		}

		/// <summary>
		/// Returns the value of this action as a bool
		/// </summary>
		public bool ValueBool { get; private set; }

		/// <summary>
		/// Returns the value of this action as a float
		/// </summary>
		public float ValueAxis1D { get; private set; }

		/// <summary>
		/// Returns the value of this action as a Vector2
		/// </summary>
		public Vector2 ValueAxis2D { get; private set; } = Vector2.Zero;

		/// <summary>
		/// Returns the value of this action as a Vector3
		/// </summary>
		public Vector3 ValueAxis3D { get; private set; } = Vector3.Zero;

		/// <summary>
		/// The amount of seconds elapsed since the action started evaluating
		/// </summary>
		public float ElapsedSeconds { get; private set; }

		/// <summary>
		/// The ratio of the elapsed time to the hold time. This is a percentage
		/// of the hold time that has passed. If the action has no hold time, this will
		/// be 0 when the action is not triggered and 1 when the action is triggered.
		/// Otherwise, this will be a value between 0 and 1.
		/// </summary>
		public float ElapsedRatio { get; private set; }

		/// <summary>
		/// The amount of seconds elapsed since the action triggered
		/// </summary>
		public float TriggeredSeconds { get; private set; }

		/// <summary>
		/// This is a hint for how long the input must remain actuated (in seconds) before the action triggers.
		/// It depends on the mapping in which this action is used. If the mapping has no hold trigger it will be -1.
		/// In general, you should not access this variable directly, but rather the `elapsed_ratio` property of the action
		/// which is a percentage of the hold time that has passed.
		/// </summary>
		private float _triggeredHoldThreshold = -1.0f;

		private GUIDEActionState _lastState = GUIDEActionState.Completed;

		private StringName? _name;
		private GUIDEActionValueType _actionValueType = GUIDEActionValueType.Bool;
		private bool _blockLowerPriorityActions = true;
		private bool _emitAsGodotActions = false;
		private bool _isRemappable;
		private string? _displayName = "";
		private string? _displayCategory = "";

		public event Action Triggered;
		public event Action Started;
		public event Action Ongoing;
		public event Action Completed;
		public event Action Cancelled;

		public bool IsTriggered() {
			return _lastState == GUIDEActionState.Triggered;
		}

		public bool IsCompleted() {
			return _lastState == GUIDEActionState.Completed;
		}

		public bool IsOngoing() {
			return _lastState == GUIDEActionState.Ongoing;
		}

		public string EditorName() {
			if ( _displayName.Length > 0 ) {
				return _displayName;
			}
			if ( !_name.IsEmpty ) {
				return _name;
			}
			return ResourcePath.GetFile().Replace( ".tres", "" );
		}

		/*
		===============
		OnTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="delta"></param>
		private void OnTriggered( in Vector3 value, float delta ) {
			TriggeredSeconds += delta;
			ElapsedRatio = 1.0f;

			UpdateValue( value );

			_lastState = GUIDEActionState.Completed;
			Triggered?.Invoke();

			EmitGodotActionMaybe( true );
		}

		/*
		===============
		OnStarted
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		private void OnStarted( in Vector3 value ) {
			ElapsedRatio = 0.0f;

			UpdateValue( value );

			_lastState = GUIDEActionState.Ongoing;
			Started?.Invoke();
			Ongoing?.Invoke();
		}

		/*
		===============
		OnOngoing
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="delta"></param>
		private void OnOngoing( in Vector3 value, float delta ) {
			ElapsedSeconds += delta;
			if ( _triggeredHoldThreshold > 0.0f ) {
				ElapsedRatio = ElapsedSeconds / _triggeredHoldThreshold;
			}

			UpdateValue( value );
			bool wasTriggered = _lastState == GUIDEActionState.Triggered;
			_lastState = GUIDEActionState.Ongoing;

			Ongoing?.Invoke();
			if ( wasTriggered ) {
				EmitGodotActionMaybe( false );
			}
		}

		/*
		===============
		OnCancelled
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		private void OnCancelled( in Vector3 value ) {
			ElapsedSeconds = 0.0f;
			ElapsedRatio = 0.0f;

			UpdateValue( value );

			_lastState = GUIDEActionState.Completed;

			Cancelled?.Invoke();
			Completed?.Invoke();
		}

		/*
		===============
		OnCompleted
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		private void OnCompleted( in Vector3 value ) {
			ElapsedSeconds = 0.0f;
			ElapsedRatio = 0.0f;
			TriggeredSeconds = 0.0f;

			UpdateValue( value );

			_lastState = GUIDEActionState.Completed;
			Completed?.Invoke();

			EmitGodotActionMaybe( false );
		}

		/*
		===============
		EmitGodotActionMaybe
		===============
		*/
		private void EmitGodotActionMaybe( bool pressed ) {
			if ( !EmitAsGodotActions ) {
				return;
			}
			if ( _name.IsEmpty ) {
				Console.PrintError( "Cannot emit aciton into Godot's system because name is empty" );
				return;
			}

			Input.ParseInputEvent( new InputEventAction() {
				Action = _name,
				Strength = ValueAxis3D.X, // would it be faster to just use ValueAxis1D?
				Pressed = pressed
			} );
		}

		/*
		===============
		UpdateValue
		===============
		*/
		private void UpdateValue( Vector3 value ) {
			switch ( _actionValueType ) {
				case GUIDEActionValueType.Bool:
				case GUIDEActionValueType.Axis1D:
					ValueBool = Mathf.Abs( value.X ) > 0.0f;
					ValueAxis2D = new Vector2( Mathf.Abs( value.X ), 0.0f );
					ValueAxis3D = new Vector3( value.X, 0.0f, 0.0f );
					break;
				case GUIDEActionValueType.Axis2D:
					ValueBool = Mathf.Abs( value.X ) > 0.0f;
					ValueAxis2D = new Vector2( value.X, value.Y );
					ValueAxis3D = new Vector3( value.X, value.Y, 0.0f );
					break;
				case GUIDEActionValueType.Axis3D:
					ValueBool = Mathf.Abs( value.X ) > 0.0f;
					ValueAxis2D = new Vector2( value.X, value.Y );
					ValueAxis3D = value;
					break;
			}
		}
	};
};