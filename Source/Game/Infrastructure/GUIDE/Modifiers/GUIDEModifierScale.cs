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
	public sealed partial class GUIDEModifierScale : GUIDEModifier {
		[Export]
		private Vector3 Scale {
			get => _scale;
			set {
				_scale = value;
				EmitChanged();
			}
		}
		private Vector3 _scale = Vector3.One;

		[Export]
		private bool ApplyDeltaTime {
			get => _applyDeltaTime;
			set {
				_applyDeltaTime = value;
				EmitChanged();
			}
		}
		private bool _applyDeltaTime;

		public override string? EditorName => "Scale";
		public override string? EditorDescription => "Scales the input by the given value and optionally, delta time.";

		/*
		===============
		IsSameAs
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool IsSameAs( GUIDEModifier other ) {
			return other is GUIDEModifierScale scale &&
				_applyDeltaTime == scale._applyDeltaTime && _scale.IsEqualApprox( scale._scale );
		}

		/*
		===============
		ModifyInput
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="delta"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		public override Vector3 ModifyInput( Vector3 input, float delta, GUIDEAction.GUIDEActionValueType valueType ) {
			if ( !input.IsFinite() ) {
				return Vector3.Inf;
			}
			return _applyDeltaTime ? input * _scale * delta : input * _scale;
		}
	};
};