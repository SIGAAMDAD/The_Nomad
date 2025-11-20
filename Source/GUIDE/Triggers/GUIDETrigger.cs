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

namespace GUIDE {
	[Tool]
	[Icon( "res://addons/guide/triggers/guide_trigger.svg" )]
	public partial class GUIDETrigger : Resource {
		public enum GUIDETriggerState : uint {
			/// <summary>
			/// The trigger did not fire
			/// </summary>
			None,

			/// <summary>
			/// The trigger's conditions are partially met
			/// </summary>
			Ongoing,

			/// <summary>
			/// The trigger has fired
			/// </summary>
			Triggered
		};

		public enum GUIDETriggerType : uint {
			/// <summary>
			/// If there are more than one explicit triggers at least one must trigger
			/// for the action to trigger.
			/// </summary>
			Explicit = 1,

			/// <summary>
			/// All implicit triggers must trigger for the action to trigger
			/// </summary>
			Implicit = 2,

			/// <summary>
			/// All blocking triggers prevent the action from triggering
			/// </summary>
			Blocking = 3
		};

		[Export]
		public float ActuationThreshold = 0.5f;

		public Vector3 LastValue = Vector3.Zero;

		/*
		===============
		IsSameAs
		===============
		*/
		/// <summary>
		/// Returns whether this trigger is the same as the other trigger.
		/// This is used to determine if a trigger can be reused during context switching.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public virtual bool IsSameAs( GUIDETrigger other ) {
			return this == other;
		}

		/*
		===============
		GetTriggerType
		===============
		*/
		/// <summary>
		/// Returns the trigger type of this trigger
		/// </summary>
		/// <returns></returns>
		public virtual GUIDETriggerType GetTriggerType() {
			return GUIDETriggerType.Explicit;
		}

		/*
		===============
		UpdateState
		===============
		*/
		public virtual GUIDETriggerState UpdateState( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			return GUIDETriggerState.None;
		}

		/*
		===============
		IsActuated
		===============
		*/
		public virtual bool IsActuated( Vector3 input, GUIDEAction.GUIDEActionValueType valueType ) {
			return valueType switch {
				GUIDEAction.GUIDEActionValueType.Axis1D => IsAxis1DActuated( input ),
				GUIDEAction.GUIDEActionValueType.Bool => IsAxis1DActuated( input ),
				GUIDEAction.GUIDEActionValueType.Axis2D => IsAxis2DActuated( input ),
				GUIDEAction.GUIDEActionValueType.Axis3D => IsAxis3DActuated( input ),
				_ => false
			};
		}

		/*
		===============
		IsAxis1DActuated
		===============
		*/
		/// <summary>
		/// Checks if a 1D input is actuated
		/// </summary>
		/// <param name="input">The input value to check</param>
		/// <returns>Returns true if the input is actuated</returns>
		protected virtual bool IsAxis1DActuated( Vector3 input ) {
			return Mathf.IsFinite( input.X ) && Mathf.Abs( input.X ) > ActuationThreshold;
		}

		/*
		===============
		IsAxis2DActuated
		===============
		*/
		/// <summary>
		/// Checks if a 2D input is actuated
		/// </summary>
		/// <param name="input">The input value to check</param>
		/// <returns>Returns true if the input is actuated</returns>
		protected virtual bool IsAxis2DActuated( Vector3 input ) {
			return Mathf.IsFinite( input.X ) && Mathf.IsFinite( input.Y ) && new Vector2( input.X, input.Y ).LengthSquared() > ActuationThreshold * ActuationThreshold;
		}

		/*
		===============
		IsAxis3DActuated
		===============
		*/
		/// <summary>
		/// Checks if a 3D input is actuated
		/// </summary>
		/// <param name="input">The input value to check</param>
		/// <returns>Returns true if the input is actuated</returns>
		protected virtual bool IsAxis3DActuated( Vector3 input ) {
			return input.IsFinite() && input.LengthSquared() > ActuationThreshold * ActuationThreshold;
		}

		/*
		===============
		EditorName
		===============
		*/
		/// <summary>
		/// The name as it should be displayed in the editor
		/// </summary>
		/// <returns></returns>
		public virtual string EditorName() {
			return "";
		}

		/*
		===============
		EditorDescription
		===============
		*/
		/// <summary>
		/// The description as it should be displayed in the editor
		/// </summary>
		/// <returns></returns>
		public virtual string EditorDescription() {
			return "";
		}
	};
};