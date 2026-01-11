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
using Nomad.Core.Util;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionSlider {
	/*
	===================================================================================
	
	OptionSlider
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionSlider : OptionNode.OptionNode {
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Min { get; private set; } = 0.0f;
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Max { get; private set; } = 100.0f;

		public OptionSliderView View { get; private set; }
		public InternString SliderId { get; private set; }

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			SliderId = new InternString( Name );
			View = new OptionSliderView( this, Min, Max );
		}
	};
};