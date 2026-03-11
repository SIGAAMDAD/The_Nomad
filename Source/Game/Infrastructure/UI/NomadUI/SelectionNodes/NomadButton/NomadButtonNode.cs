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
using Nomad.EngineUtils.UserInterface;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.NomadButton {
	/*
	===================================================================================
	
	NomadButton
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class NomadButtonNode : EngineButton {
		[Export( PropertyHint.Range, "0,10,0.001,or_greater" )]
		public float Duration = 1.0f;

		[Export]
		public bool AnimateScale = true;
		[Export]
		public bool AnimatePosition = false;
		[Export]
		public Tween.TransitionType TransitionType;

		[ExportGroup( "Scale Properties", "scale_" )]
		[Export]
		public float ScaleIntensity = 1.10f;

		[ExportGroup( "Position Properties", "position_" )]
		[Export]
		public Vector2 PositionValue = new Vector2( 0.0f, -4.0f );

		public bool IsFocused => Controller.IsFocused;

		public NomadButtonView View { get; private set; }
		public NomadButtonController Controller { get; private set; }

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			View = new NomadButtonView(
				this,
				new NomadButtonAnimation {
					Duration = Duration,
					AnimateScale = AnimateScale,
					AnimatePosition = AnimatePosition,
					TransitionType = TransitionType,
					ScaleIntensity = ScaleIntensity,
					PositionValue = PositionValue
				}
			);
			Controller = new NomadButtonController( View );
		}
	};
};
