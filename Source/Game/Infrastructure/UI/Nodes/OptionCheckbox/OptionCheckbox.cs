/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Core.Engine.Globals;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.UI;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Events.UI;

namespace Nomad.Game.Infrastructure.UI.Nodes.OptionCheckbox
{
	/*
	===================================================================================

	OptionCheckbox

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class OptionCheckbox : OptionNode.OptionNode
	{
		private static readonly string ON_STRING = LocalizationService.Translate( new InternString( "UI_ON" ) );
		private static readonly string OFF_STRING = LocalizationService.Translate( new InternString( "UI_OFF" ) );

		public bool Value {
			get => _value;
			set {
				_value = value;
				_valueLabel.Text = value ? ON_STRING : OFF_STRING;
			}
		}
		private bool _value;

		public EngineButton Left => FindChild<EngineButton>( "LeftIcon" );
		public EngineButton Right => FindChild<EngineButton>( "RightIcon" );

		private EngineText _valueLabel;

		[Event( nameSpace: "Nomad.Game.Domain.Events.UI", PayloadName = "OptionCheckboxValueChangedEventArgs" )]
		[EventPayload( "Value", typeof( bool ) )]
		public IGameEvent<OptionCheckboxValueChangedEventArgs> Toggled => _toggled;
		private readonly IGameEvent<OptionCheckboxValueChangedEventArgs> _toggled = default;

		public OptionCheckbox()
		{
			_toggled = GameEventRegistry
				.GetEvent<OptionCheckboxValueChangedEventArgs>(
					$"{GetHashCode()}:{OptionCheckboxValueChangedEventArgs.Name}",
					OptionCheckboxValueChangedEventArgs.NameSpace
				);
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnInit()
		{
			EngineText title = FindChild<EngineText>( "Title" );
			title.Text = Title;

			_valueLabel = FindChild<EngineText>( "Value" );

			Left.Clicked.Subscribe( OnToggled );
			Right.Clicked.Subscribe( OnToggled );

			Value = false;
		}

		/*
		===============
		OnToggled
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnToggled( in EmptyEventArgs args )
		{
			Value = !_value;
			_toggled.Publish( new OptionCheckboxValueChangedEventArgs( _value ) );
		}
	};
};
