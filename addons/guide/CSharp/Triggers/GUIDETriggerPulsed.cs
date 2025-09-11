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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDETriggerPulse
	
	===================================================================================
	*/
	/// <summary>
	/// A trigger that activates when the input is pushed down and then repeatedly sends trigger events at a fixed interval.
	/// Note: the trigger will be either triggering or ongoing until the input is released.
	/// Note: at most one pulse will be emitted per frame.
	/// </summary>
	
	[Tool]
	public sealed partial class GUIDETriggerPulse : GUIDETrigger {
		/// <summary>
		/// If true, the trigger wil trigger immediately when the input is actuated.
		/// Otherwise, the trigger will wait for the initial delay.
		/// </summary>
		[Export]
		public bool TriggerOnStart = true;

		/// <summary>
		/// The delay after the initial actuation before pulsing begins
		/// </summary>
		[Export]
		public float InitialDelay {
			get => _initialDelay;
			set => _initialDelay = Mathf.Max( 0.0f, value );
		}

		/// <summary>
		/// The interval between pulses. Set to 0 to pulse every frame
		/// </summary>
		[Export]
		public float PulseInterval {
			get => _pulseInterval;
			set => _pulseInterval = Mathf.Max( 0.0f, value );
		}

		/// <summary>
		/// Maximum number of pulses. If <= 0, the trigger will pulse indefinitely
		/// </summary>
		[Export]
		public int MaxPulses = 0;

		private float _initialDelay = 0.3f;
		private float _pulseInterval = 0.1f;

		private float DelayUntilNextPulse = 0.0f;
		private int EmittedPulses = 0;

		/*
		===============
		IsSameAs
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsSameAs( GUIDETrigger other ) {
			return other is GUIDETriggerPulse pulse ?
					Mathf.IsEqualApprox( InitialDelay, pulse.InitialDelay ) && Mathf.IsEqualApprox( _pulseInterval, pulse._pulseInterval )
					&& MaxPulses == pulse.MaxPulses && TriggerOnStart == pulse.TriggerOnStart
				:
					false;
		}

		/*
		===============
		UpdateState
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override GUIDETriggerState UpdateState( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( IsActuated( input, valueType ) ) {
				if ( !IsActuated( LastValue, valueType ) ) {
					DelayUntilNextPulse = InitialDelay;
					return TriggerOnStart ? GUIDETriggerState.Triggered : GUIDETriggerState.Ongoing;
				}

				if ( MaxPulses > 0 && EmittedPulses >= MaxPulses ) {
					return GUIDETriggerState.None;
				}

				DelayUntilNextPulse -= delta;
				if ( DelayUntilNextPulse > 0.0f ) {
					return GUIDETriggerState.Ongoing;
				}

				if ( Mathf.IsEqualApprox( _pulseInterval, 0.0f ) ) {
					DelayUntilNextPulse = 0.0f;
					if ( MaxPulses > 0 ) {
						EmittedPulses++;
					}
					return GUIDETriggerState.Triggered;
				}

				DelayUntilNextPulse += _pulseInterval;
				if ( DelayUntilNextPulse <= 0.0f ) {
					int skippedPulses = (int)( -DelayUntilNextPulse / _pulseInterval );
					DelayUntilNextPulse += skippedPulses / _pulseInterval;
					if ( MaxPulses > 0 ) {
						EmittedPulses += skippedPulses;
						if ( EmittedPulses >= MaxPulses ) {
							return GUIDETriggerState.None;
						}
					}
				}

				if ( MaxPulses > 0 ) {
					EmittedPulses++;
				}
				return GUIDETriggerState.Triggered;
			}

			EmittedPulses = 0;
			DelayUntilNextPulse = 0.0f;
			return GUIDETriggerState.None;
		}

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorName() {
			return "Pulse";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string EditorDescription() {
			return "Fires at an interval while the input is actuated.";
		}
	};
};