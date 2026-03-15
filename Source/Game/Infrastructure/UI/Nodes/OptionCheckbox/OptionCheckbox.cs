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

using Game.Application.UI;
using Nomad.Core.Engine.Globals;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.EngineUtils.UserInterface;
using Nomad.Events.Globals;

namespace Game.Infrastructure.UI.Nodes.OptionCheckbox {
	/*
	===================================================================================
	
	OptionCheckbox
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionCheckbox : OptionNode.OptionNode {
		private static readonly string ON_STRING = LocalizationService.Translate( new InternString( "UI_ON" ) );
		private static readonly string OFF_STRING = LocalizationService.Translate( new InternString( "UI_OFF" ) );

		public bool Value {
			get => _value;
			set {
				_valueLabel.Text = value ? ON_STRING : OFF_STRING;
			}
		}
		private bool _value;

		public InternString CheckboxId => new InternString( Name );

		public EngineButton Left => FindChild<EngineButton>( "LeftIcon" );
		public EngineButton Right => FindChild<EngineButton>( "RightIcon" );

		private EngineText _valueLabel;

		public IGameEvent<bool> Toggled => _toggled;
		private IGameEvent<bool> _toggled;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_valueLabel = FindChild<EngineText>( "Value" );

			Left.Clicked.Subscribe( OnToggled );
			Right.Clicked.Subscribe( OnToggled );

			_value = false;

			_toggled = GameEventRegistry.GetEvent<bool>( $"{Name}:{UIConstants.OPTION_CHECKBOX_TOGGLED_EVENT}", UIConstants.NAMESPACE );
		}

		/*
		===============
		OnToggled
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnToggled( in EmptyEventArgs args ) {
			_value = !_value;
			_toggled.Publish( _value );
		}
	};
};
