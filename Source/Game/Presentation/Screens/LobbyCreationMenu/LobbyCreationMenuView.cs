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

using Godot;
using Nomad.Game.Infrastructure.UI.Nodes.OptionList;
using Nomad.Game.Infrastructure.UI.Nodes.OptionSlider;
using System;
using System.Collections.Generic;

namespace Nomad.Game.Presentation.Screens.LobbyCreationMenu
{
	/*
	===================================================================================

	LobbyCreationMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class LobbyCreationMenuView : Control
	{
		public event Action Back;
		public event Action CreateLobby;
		public event Action Reset;

		private LineEdit _name;
		private OptionList _mapList;
		private OptionList _gameModeList;

		public void SetGameModeOptions( IReadOnlyList<string> options )
		{
			_gameModeList.SetOptions( options );
		}

		public void SetGameModeValue( int value )
		{
			_gameModeList.SetValue( value );
		}

		public void SetMapOptions( IReadOnlyList<string> options )
		{
			_mapList.SetOptions( options );
		}

		public void SetMapValue( int value )
		{
			_mapList.SetValue( value );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready()
		{
			base._Ready();

			_name = GetNode<LineEdit>( "MarginContainer/ConfigContainer/LobbyName" );
			_mapList = GetNode<OptionList>( "MarginContainer/ConfigContainer/MapList" );
			_gameModeList = GetNode<OptionList>( "MarginContainer/ConfigContainer/GameModeList" );

			GetNode<Button>( "MarginContainer/ButtonContainer/CreateButton" ).Pressed += () => CreateLobby?.Invoke();
			GetNode<Button>( "MarginContainer/ButtonContainer/BackButton" ).Pressed += () => Back?.Invoke();
		}
	};
};
