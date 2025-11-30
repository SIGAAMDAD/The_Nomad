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

using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Godot;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList {
	/*
	===================================================================================
	
	OptionList
		
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionList : OptionNode.OptionNode {
		[Export]
		public string[] Items { get; private set; }

		public override IOptionNodeView View => _view;
		private IOptionListView _view;

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			_view = new OptionListView( this );
		}
	};
};