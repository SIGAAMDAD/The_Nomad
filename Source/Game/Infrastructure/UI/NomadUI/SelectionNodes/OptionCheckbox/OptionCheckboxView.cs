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
using Godot;
using Nomad.Core.Util;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox {
	/*
	===================================================================================
	
	OptionCheckboxView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class OptionCheckboxView : OptionNodeView<OptionCheckbox>, IOptionCheckboxView {
		private static readonly StringName ON_STRING = TranslationServer.Translate( "UI_ON" );
		private static readonly StringName OFF_STRING = TranslationServer.Translate( "UI_OFF" );

		public InternString CheckboxId => _owner.CheckboxId;

		public Button Left => _owner.GetNode<Button>( "LeftIcon" );
		public Button Right => _owner.GetNode<Button>( "RightIcon" );

		private readonly Godot.Label _valueLabel;

		/*
		===============
		OptionCheckboxView
		===============
		*/
		public OptionCheckboxView( OptionCheckbox owner )
			: base( owner )
		{
			_valueLabel = _owner.GetNode<Godot.Label>( "Value" );
			SetValue( false );
		}

		/*
		===============
		SetValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void SetValue( bool value ) {
			_valueLabel.Text = value ? ON_STRING : OFF_STRING;
		}
	};
};