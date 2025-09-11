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

global using InputAction = System.Action<Godot.Resource>;
using Godot;
using ResourceCache;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using PlayerSystem;
using PlayerSystem.Input;

public partial class Player {
	/*
	===================================================================================
	
	InputController
	
	===================================================================================
	*/
	/// <summary>
	/// Manages player input through GUIDE (Godot Unified Input Detection Engine)
	/// </summary>

	public sealed partial class InputController : GodotObject, IDisposable {
		public enum ControlBind : uint {
			Move,
			Dash,
			Slide,
			Melee,
			Reload,
			UseWeapon,
			UseGadget,
			Interact,
			BulletTime,
			SwitchWeaponMode,
			PrevWeapon,
			NextWeapon,

			Count
		};
		public enum BindMapping : uint {
			Keyboard,
			Gamepad0,
			Gamepad1,
			Gamepad2,
			Gamepad3,

			Count
		};
		public readonly struct BindSet {
			public readonly Resource? MappingContext = null;
			public readonly string? Name = null;
			public readonly IReadOnlyDictionary<ControlBind, Resource>? Binds = null;

			/*
			===============
			BindSet
			===============
			*/
			/// <summary>
			/// Initializes a new <c>BindSet</c> by loading the specified input mapping context and associated control binds.
			/// </summary>
			/// <param name="mappingContextPath"> The resource path to the input mapping context (e.g., "res://resources/binds/binds_keyboard.tres").</param>
			/// <param name="isController">Indicates whether the binds should be loaded for a gamepad/controller (<c>true</c>) or keyboard (<c>false</c>).</param>
			/// <param name="inputDeviceIndex">The index of the input device (used for gamepads; ignored for keyboard).</param>
			/// <exception cref="ArgumentException">Thrown if <paramref name="mappingContextPath"/> is null or empty.</exception>
			/// <exception cref="Exception">Thrown if the mapping context or any action resource fails to load.</exception>
			public BindSet( string mappingContextPath, bool isController = false, int inputDeviceIndex = -1 ) {
				ArgumentException.ThrowIfNullOrEmpty( mappingContextPath );
				if ( isController && ( inputDeviceIndex < 0 || inputDeviceIndex > 3 ) ) {
					throw new ArgumentOutOfRangeException( $"The bindset is a controller, but the inputDeviceIndex ({inputDeviceIndex}) isn't valid, it must have a range of 0-3" );
				}

				MappingContext = PreLoader.GetResource( mappingContextPath );
				if ( MappingContext == null ) {
					throw new Exception( $"Error loading GUIDEMappingContext from resource {mappingContextPath}" );
				}

				Name = isController ? $"Gamepad{inputDeviceIndex}" : "Keyboard";

				Binds = LoadBinds( isController, inputDeviceIndex );
			}

			/*
			===============
			LoadBinds
			===============
			*/
			/// <summary>
			/// Loads the input action resources for each <see cref="ControlBind"/> based on the control scheme.
			/// </summary>
			/// <param name="isController">If <c>true</c>, loads gamepad binds; otherwise, loads keyboard binds.</param>
			/// <param name="inputDeviceIndex">The index of the input device (used for gamepads; ignored for keyboard).</param>
			/// <returns>A read-only dictionary mapping each <see cref="ControlBind"/> to its corresponding input action resource.</returns>
			/// <exception cref="Exception">Thrown if any action resource fails to load.</exception>
			private IReadOnlyDictionary<ControlBind, Resource>? LoadBinds( bool isController, int inputDeviceIndex ) {
				Dictionary<ControlBind, Resource>? binds = new Dictionary<ControlBind, Resource>( (int)ControlBind.Count );
				StringBuilder builder = new StringBuilder( 4096 );

				for ( ControlBind bind = ControlBind.Move; bind < ControlBind.Count; bind++ ) {
					builder.AppendFormat( "res://resources/binds/actions/{0}", isController ? "gamepad" : "keyboard" );
					builder.AppendFormat( "/{0}", Enum.GetName( bind ).ToLower() );
					if ( isController ) {
						builder.AppendFormat( "_player{0}.tres", inputDeviceIndex );
					} else {
						builder.Append( ".tres" );
					}
					binds.Add( bind, PreLoader.GetResource( builder.ToString() ) ?? throw new Exception( $"Error loading GUIDEAction from resource {builder}" ) );
				}

				return binds;
			}
		};
		public struct BindActionEvent {
			public InputAction? TriggeredEvent = null;
			public InputAction? StartedEvent = null;
			public InputAction? OngoingEvent = null;
			public InputAction? CompletedEvent = null;
			public InputAction? CancelledEvent = null;

			/*
			===============
			BindActionEvent
			===============
			*/
			/// <summary>
			/// 
			/// </summary>
			/// <param name="triggeredEvent"></param>
			/// <param name="startedEvent"></param>
			/// <param name="ongoingEvent"></param>
			/// <param name="completedEvent"></param>
			/// <param name="cancelledEvent"></param>
			public BindActionEvent( InputAction? triggeredEvent, InputAction? startedEvent, InputAction? ongoingEvent,
				InputAction? completedEvent, InputAction? cancelledEvent ) {
				TriggeredEvent = triggeredEvent;
				StartedEvent = startedEvent;
				OngoingEvent = ongoingEvent;
				CompletedEvent = completedEvent;
				CancelledEvent = cancelledEvent;
			}
		};

		/// <summary>
		/// The SignalName for when a GUIDEAction is triggered
		/// </summary>
		private static readonly StringName @TriggeredSignalName = "triggered";

		/// <summary>
		/// The SignalName for when a GUIDEAction is started
		/// </summary>
		private static readonly StringName @StartedSignalName = "started";

		/// <summary>
		/// The SignalName for when a GUIDEAction is ongoing
		/// </summary>
		private static readonly StringName @OngoingSignalName = "ongoing";

		/// <summary>
		/// The SignalName for when a GUIDEAction is completed
		/// </summary>
		private static readonly StringName @CompletedSignalName = "completed";

		/// <summary>
		/// The SignalName for when a GUIDEAction is cancelled
		/// </summary>
		private static readonly StringName @CancelledSignalName = "cancelled";

		/// <summary>
		/// A reference to /root/GUIDE
		/// </summary>
		private readonly Node? GUIDEInstance = null;

		/// <summary>
		/// The owning <see cref="Player"/> class
		/// </summary>
		private readonly Player? Owner = null;

		/// <summary>
		/// The input bindings for various input device types
		/// </summary>
		public IReadOnlyDictionary<BindMapping, BindSet>? Bindings { get; private set; } = null;
		public BindSet? CurrentBindSet { get; private set; } = null;

		private readonly Resource? SwitchToKeyboardAction = null;
		private readonly Resource? SwitchToGamepadAction = null;
		private readonly IReadOnlyDictionary<ControlBind, BindActionEvent>? InputMappings = null;
		private readonly IReadOnlyDictionary<ControlBind, Dictionary<StringName, Callable>>? ActionMappings = null;

		/*
		===============
		InputController
		===============
		*/
		/// <summary>
		/// Initializes input binds and control scheme
		/// </summary>
		/// <param name="owner">The owning <see cref="Player"/> object</param>
		public InputController( Player owner ) {
			ArgumentNullException.ThrowIfNull( owner );

			Owner = owner;

			SwitchToGamepadAction = PreLoader.GetResource( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
			if ( SwitchToGamepadAction == null ) {
				throw new Exception( "Error loading GUIDEAction switch_to_gamepad.tres!" );
			}
			// sanity
			ArgumentNullException.ThrowIfNull( SwitchToGamepadAction );

			SwitchToKeyboardAction = PreLoader.GetResource( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );
			if ( SwitchToKeyboardAction == null ) {
				throw new Exception( "Error loading GUIDEAction switch_to_keyboard.tres!" );
			}
			// sanity
			ArgumentNullException.ThrowIfNull( SwitchToKeyboardAction );

			GameEventBus.ConnectSignal( SwitchToGamepadAction, TriggeredSignalName, this, OnSwitchToGamepadActionTriggered );
			GameEventBus.ConnectSignal( SwitchToKeyboardAction, TriggeredSignalName, this, OnSwitchToKeyboardActionTriggered );

			var inputMappings = new Dictionary<ControlBind, BindActionEvent>( (int)ControlBind.Count );
			for ( ControlBind bind = ControlBind.Move; bind < ControlBind.Count; bind++ ) {
				// alocate an empty BindActionEvent so that we can access the binds without a null exception
				// and give them callbacks later
				inputMappings.Add( bind, new BindActionEvent() );
			}
			InputMappings = inputMappings;

			// allocate the action mappings
			var actionMappings = new Dictionary<ControlBind, Dictionary<StringName, Callable>>( (int)ControlBind.Count );
			for ( ControlBind bind = ControlBind.Move; bind < ControlBind.Count; bind++ ) {
				actionMappings.Add( bind, CreateActionEventMapping( bind, InputMappings[ bind ] ) );
			}
			ActionMappings = actionMappings;

			GUIDEInstance = owner.GetNode( "/root/GUIDE" );

			// load input bindings
			Bindings = new Dictionary<BindMapping, BindSet>() {
				{ BindMapping.Keyboard, new BindSet( "res://resources/binds/binds_keyboard.tres", false, -1 ) },
				{ BindMapping.Gamepad0, new BindSet( "res://resources/binds/binds_gamepad.tres", true, 0 ) },
				{ BindMapping.Gamepad1, new BindSet( "res://resources/binds/binds_gamepad.tres", true, 1 ) },
				{ BindMapping.Gamepad2, new BindSet( "res://resources/binds/binds_gamepad.tres", true, 2 ) },
				{ BindMapping.Gamepad3, new BindSet( "res://resources/binds/binds_gamepad.tres", true, 3 ) },
			};

			ConnectBindSet( Bindings[ BindMapping.Keyboard ] );

			AccessibilityManager.LoadBinds( this );
		}

		/*
		===============
		InputController
		===============
		*/
		/// <summary>
		/// Ensures we force the creation of an InputController with a given Owner object
		/// </summary>
		private InputController() {
		}

		/*
		===============
		GetBindActionResource
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bind"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public Resource? GetBindActionResource( ControlBind bind ) {
			ArgumentNullException.ThrowIfNull( CurrentBindSet );

			if ( CurrentBindSet.Value.Binds.TryGetValue( bind, out Resource action ) ) {
				return action;
			}
			throw new ArgumentOutOfRangeException( $"ControlBind {bind} doesn't exist" );
		}

		/*
		===============
		SetBindAction
		===============
		*/
		/// <summary>
		/// Binds a <see cref="InputAction"/> callback to a specific GUIDE signal. This is meant to abstract the process of input
		/// device differentiation away and provide a simple interface to bind actions instead of raw input values.
		/// </summary>
		/// <param name="bind">The control to bind to</param>
		/// <param name="signal">The GUIDEAction signal to connect to</param>
		/// <param name="action">The control's signal callback</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetBindAction( ControlBind bind, GUIDEActionSignal signal, InputAction action ) {
			ArgumentNullException.ThrowIfNull( InputMappings );
			
			if ( !InputMappings.TryGetValue( bind, out BindActionEvent handler ) ) {
				throw new ArgumentOutOfRangeException( $"invalid ControlBind {bind}" );
			}
			switch ( signal ) {
				case GUIDEActionSignal.Triggered:
					handler.TriggeredEvent = action;
					break;
				case GUIDEActionSignal.Started:
					handler.StartedEvent = action;
					break;
				case GUIDEActionSignal.Ongoing:
					handler.OngoingEvent = action;
					break;
				case GUIDEActionSignal.Completed:
					handler.CompletedEvent = action;
					break;
				case GUIDEActionSignal.Cancelled:
					handler.CancelledEvent = action;
					break;
				default:
					throw new ArgumentOutOfRangeException( $"signal is invalid - {signal}, it must be a valid GUIDEActionSignal" );
			}
		}

		/*
		===============
		OnSwitchToGamepadActionTriggered
		===============
		*/
		private void OnSwitchToGamepadActionTriggered() {
			ArgumentNullException.ThrowIfNull( SwitchToGamepadAction );

			int deviceIndex = SwitchToGamepadAction.Get( "joy_index" ).AsInt32();
			int numJoypads = Godot.Input.GetConnectedJoypads().Count;

			if ( deviceIndex < 0 || deviceIndex > numJoypads ) {
				throw new ArgumentOutOfRangeException( $"joy_index {deviceIndex} out of range!" );
			}

			ConnectBindSet( Bindings[ (BindMapping)( (int)BindMapping.Gamepad0 + deviceIndex ) ] );
		}

		/*
		===============
		OnSwitchToKeyboardActionTriggered
		===============
		*/
		private void OnSwitchToKeyboardActionTriggered() {
			ArgumentNullException.ThrowIfNull( SwitchToKeyboardAction );
		}

		/*
		===============
		CreateActionEventMapping
		===============
		*/
		/// <summary>
		/// Caches Godot.Callable lambdas for a specific GUIDEAction
		/// </summary>
		/// <param name="bind">The bind to map to</param>
		/// <param name="actionEvent">The callbacks to use</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private Dictionary<StringName, Callable> CreateActionEventMapping( ControlBind bind, BindActionEvent? actionEvent ) {
			if ( !actionEvent.HasValue ) {
				throw new ArgumentNullException( "actionEvent cannot be null!" );
			} else if ( bind < ControlBind.Move || bind >= ControlBind.Count ) {
				throw new ArgumentOutOfRangeException( $"ControlBind {bind} is out of range" );
			}

			return new Dictionary<StringName, Callable>( (int)GUIDEActionSignal.Count ) {
				{ TriggeredSignalName, Callable.From( () => actionEvent.Value.TriggeredEvent?.Invoke( CurrentBindSet.Value.Binds[ bind ] ) ) },
				{ StartedSignalName, Callable.From( () => actionEvent.Value.StartedEvent?.Invoke( CurrentBindSet.Value.Binds[ bind ] ) ) },
				{ OngoingSignalName, Callable.From( () => actionEvent.Value.OngoingEvent?.Invoke( CurrentBindSet.Value.Binds[ bind ] ) ) },
				{ CompletedSignalName, Callable.From( () => actionEvent.Value.CompletedEvent?.Invoke( CurrentBindSet.Value.Binds[ bind ] ) ) },
				{ CancelledSignalName, Callable.From( () => actionEvent.Value.CancelledEvent?.Invoke( CurrentBindSet.Value.Binds[ bind ] ) ) }
			};
		}

		/*
		===============
		DisconnectBindSet
		===============
		*/
		private void DisconnectBindSet() {
			if ( !CurrentBindSet.HasValue ) {
				throw new InvalidOperationException( "Cannot disconnect a BindSet when it hasn't been connected!" );
			}
			ArgumentNullException.ThrowIfNull( CurrentBindSet.Value.Binds );

			Console.PrintLine( $"Disconnecting BindSet {CurrentBindSet.Value.Name}..." );

			GUIDEInstance.Call( "disable_mapping_context", CurrentBindSet.Value.MappingContext );

			foreach ( var bind in CurrentBindSet.Value.Binds ) {
				if ( ActionMappings.TryGetValue( bind.Key, out Dictionary<StringName, Callable> events ) ) {
					GameEventBus.ConnectSignal( bind.Value, TriggeredSignalName, this, events[ TriggeredSignalName ] );
				} else {
					throw new Exception( $"No ActionMapping callback for bind {bind.Key}!" );
				}
			}
		}

		/*
		===============
		ConnectBindSet
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bindSet"></param>
		private void ConnectBindSet( in BindSet? bindSet ) {
			if ( !bindSet.HasValue ) {
				throw new ArgumentNullException( "invalid BindSet" );
			}
			if ( CurrentBindSet.HasValue && !CurrentBindSet.Equals( bindSet ) ) {
				// If there's currently something else bound, disconnect it
				DisconnectBindSet();
			}

			CurrentBindSet = bindSet;
			Console.PrintLine( $"Connecting BindSet {CurrentBindSet.Value.Name}..." );

			GUIDEInstance.Call( "enable_mapping_context", CurrentBindSet.Value.MappingContext, true );

			foreach ( var bind in bindSet.Value.Binds ) {
				if ( ActionMappings.TryGetValue( bind.Key, out Dictionary<StringName, Callable> events ) ) {
					GameEventBus.ConnectSignal( bind.Value, TriggeredSignalName, this, events[ TriggeredSignalName ] );
				} else {
					throw new Exception( $"No ActionMapping callback for bind {bind.Key}!" );
				}
			}

			Owner.EmitSignal( Player.SignalName.InputMappingContextChanged );
		}
	};
};