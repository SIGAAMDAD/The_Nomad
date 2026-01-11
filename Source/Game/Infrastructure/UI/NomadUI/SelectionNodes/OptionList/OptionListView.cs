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
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionNode;
using Nomad.Core.Util;
using System;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList {
	/*
	===================================================================================
	
	OptionListView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class OptionListView : OptionNodeView<OptionList>, IOptionListView {
		public InternString ListId => _owner.ListId;
		
		private readonly Godot.Label _valueLabel;

		/*
		===============
		OptionListView
		===============
		*/
		public OptionListView( OptionList owner )
			: base( owner )
		{
			_valueLabel = _owner.GetNode<Godot.Label>( "Value" );
		}

		/*
		===============
		SetOption
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void SetOption( string value ) {
			_valueLabel.SetDeferred( Label.PropertyName.Text, value );
		}
	};
};