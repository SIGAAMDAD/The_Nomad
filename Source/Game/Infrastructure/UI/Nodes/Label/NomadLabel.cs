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

using Nomad.Core.Events;
using Nomad.EngineUtils.UserInterface;

namespace Game.Infrastructure.UI.Nodes.Label {
	/*
	===================================================================================

	NomadLabel
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class NomadLabel : EngineText {
		public bool IsFocused => _isFocused;
		private bool _isFocused = false;

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		public void OnFocused( in EmptyEventArgs args ) {
			_isFocused = true;
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		public void OnUnfocused( in EmptyEventArgs args ) {
			_isFocused = false;
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			Focused.Subscribe( OnFocused );
			Unfocused.Subscribe( OnUnfocused );
		}
	};
};
