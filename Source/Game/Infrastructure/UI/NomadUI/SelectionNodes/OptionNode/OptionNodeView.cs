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
using Nomad.Core.Memory;
using Nomad.Core.Util;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionNode {
	/*
	===================================================================================
	
	OptionNodeView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>

	public class OptionNodeView<T> : IOptionNodeView where T : OptionNode {
		public InternString Description => _descriptionCached;
		private readonly InternString _descriptionCached;

		public OptionNode Owner => _owner;
		protected readonly T _owner;

		protected readonly Label _titleLabel;

		/*
		===============
		OptionNodeView
		===============
		*/
		public OptionNodeView( T owner ) {
			_owner = owner;

			_titleLabel = _owner.GetNode<Label>( "Title" );
			_titleLabel.Text = TranslationServer.Translate( owner.Title );

			_descriptionCached = ( _owner.Description == null || _owner.Description.IsEmpty ) ? InternString.Empty : StringPool.Intern( TranslationServer.Translate( owner.Description ) );

			LinkFocusNodes();
		}

		/*
		===============
		LinkFocusNodes
		===============
		*/
		private void LinkFocusNodes() {
			NodePath path = _owner.GetPath();

			_owner.FocusNeighborLeft = path;
			_owner.FocusNeighborRight = path;

			int index = _owner.GetIndex();
			Node parent = _owner.GetParent();
			int childCount = parent.GetChildCount();
			if ( childCount == 1 ) {
				_owner.FocusNeighborTop = path;
				_owner.FocusNeighborBottom = path;
			} else if ( index == 0 ) {
				_owner.FocusNeighborTop = parent.GetChild( childCount - 1 ).GetPath();
				_owner.FocusNeighborBottom = parent.GetChild( index + 1 ).GetPath();
			} else if ( index == childCount - 1 ) {
				_owner.FocusNeighborTop = parent.GetChild( index - 1 ).GetPath();
				_owner.FocusNeighborBottom = parent.GetChild( 0 ).GetPath();
			} else {
				_owner.FocusNeighborTop = parent.GetChild( index - 1 ).GetPath();
				_owner.FocusNeighborBottom = parent.GetChild( index + 1 ).GetPath();
			}
			_owner.FocusNeighborLeft = path;
			_owner.FocusNeighborRight = path;
			_owner.FocusNext = path;
			_owner.FocusPrevious = path;

			_owner.FocusMode = HBoxContainer.FocusModeEnum.All;
		}
	};
};