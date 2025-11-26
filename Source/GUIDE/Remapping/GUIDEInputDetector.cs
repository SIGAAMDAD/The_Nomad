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
using System.Collections.Generic;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDEInputDetector
	
	===================================================================================
	*/
	/// <summary>
	/// Helper node for detecting inputs. Detects the next input matching a specification and
	/// emits a signal with the detected input
	/// </summary>

	public partial class GUIDEInputDetector : Node {
		/// <summary>
		/// The device type for which the input should be filtered
		/// </summary>
		public enum DeviceType : uint {
			/// <summary>
			/// Only detect input from keyboard
			/// </summary>
			Keyboard = 1,

			/// <summary>
			/// Only detect input from the mouse
			/// </summary>
			Mouse = 2,

			/// <summary>
			/// Only detect input from joysticks/gamepads
			/// </summary>
			Joy = 4

			// touch doesn't make a lot of sense as this is usually
			// not remappable.
		};

		/// <summary>
		/// Which joy index should be used for detected joy events
		/// </summary>
		public enum JoyIndex : uint {
			/// <summary>
			/// Use -1, so the detected input will match any joystick
			/// </summary>
			Any = 0,

			/// <summary>
			/// Use the actual index of the detected joystick
			/// </summary>
			Detected = 1
		};

		public enum DetectionState : uint {
			/// <summary>
			/// The detector is currently idle
			/// </summary>
			Idle = 0,

			/// <summary>
			/// The detector is currently counting down before starting the detection
			/// </summary>
			Countdown = 3,

			/// <summary>
			/// The detector is currently detecting input
			/// </summary>
			Detecting = 1,

			/// <summary>
			/// The detector has finished detecting but is waiting for input to be released
			/// </summary>
			WaitingForInputClear = 2
		};

		/// <summary>
		/// A countdown between initiating a detection and the actual start of the
		/// detection. This is useful because when the user clicks a button to
		/// start a detection, we want to make sure that the player is actually
		/// ready (and not accidentally moves anything). If set to 0, no countdown
		/// will be started
		/// </summary>
		[Export( PropertyHint.Range, "0,2,0.1,or_greater" )]
		public float DetectionCountdownSeconds = 0.5f;

		/// <summary>
		/// Minimum aplitude to detect any axis
		/// </summary>
		[Export( PropertyHint.Range, "0,1,0.1,or_greater" )]
		public float MinimumAxisAmplitude = 0.2f;

		/// <summary>
		/// If any of these inputs is encountered, the detector will
		/// treat this as "abort detection"
		/// </summary>
		[Export]
		public GUIDEInput[]? AbortDetectionOn;

		/// <summary>
		/// Which joy index should be returned for detected joy events
		/// </summary>
		[Export]
		public JoyIndex UseJoyIndex = JoyIndex.Any;

		/// <summary>
		/// Whether trigger buttons on controllers should be detected when
		/// the action value type is limited to boolean
		/// </summary>
		[Export]
		public bool AllowTriggersForBooleanActions = true;

		/// <summary>
		/// The timer for the detection countdown
		/// </summary>
		private Timer Timer;

		/// <summary>
		/// Our copy of the input state
		/// </summary>
		private GUIDEInputState InputState;

		/// <summary>
		/// The current state of the detection
		/// </summary>
		private DetectionState Status = DetectionState.Idle;

		/// <summary>
		/// Mapping contexts that were active when the detection started. We need to restore these once the detection is
		/// finished or aborted
		/// </summary>
		private GUIDEMappingContext[] SavedMappingContext;

		/// <summary>
		/// The last detected input
		/// </summary>
		private GUIDEInput LastDetectedInput;

		/// <summary>
		/// Emitted when the detection has started (e.g. countdown has elapsed)
		/// Can be used to signal this to the player
		/// </summary>
		[Signal]
		public delegate void DetectionStartedEventHandler();

		/// <summary>
		/// Emitted when the input detector detects and input of the given type.
		/// If detection was aborted the given input is null
		/// </summary>
		/// <param name="input">The input detected</param>
		[Signal]
		public delegate void InputDetectedEventHandler( GUIDEInput input );

		public override void _Ready() {
			base._Ready();

			SetProcess( false );
		}
	};
};