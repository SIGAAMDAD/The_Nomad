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

using System;
using Godot;
using Nomad.Events.Globals;
using Nomad.Game.Presentation.Screens.MultiplayerMenu;

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu
{
	/*
	===================================================================================

	MultiplayerMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class MultiplayerMenuView : Control
	{
		public event Action CreateLobby;
		public event Action LobbyBrowser;
		public event Action Matchmake;
		public event Action Back;

		private MultiplayerMenuPresenter _presenter;

		private MarginContainer _lobbyBrowser;
		private Control _lobbyFactory;
		private VBoxContainer _optionsContainer;

		public void SetLobbyBrowserVisible( bool visible )
		{
			_lobbyBrowser.Visible = visible;
		}

		public void SetLobbyFactoryVisible( bool visible )
		{
			_lobbyFactory.Visible = visible;
		}

		public void SetOptionsContainerVisible( bool visible )
		{
			_optionsContainer.Visible = visible;
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

			GetNode<Button>( "OptionsContainer/CreateLobbyButton" ).Pressed += () => CreateLobby?.Invoke();
			GetNode<Button>( "OptionsContainer/MatchmakeButton" ).Pressed += () => Matchmake?.Invoke();
			GetNode<Button>( "OptionsContainer/LobbyBrowserButton" ).Pressed += () => LobbyBrowser?.Invoke();
			GetNode<Button>( "OptionsContainer/BackButton" ).Pressed += () => Back?.Invoke();

			_presenter = new MultiplayerMenuPresenter( this, new MultiplayerMenuModel(), GameEventRegistry.Instance );

			_lobbyBrowser = GetNode<MarginContainer>( "LobbyBrowserContainer" );
			_lobbyFactory = GetNode<Control>( "LobbyCreationMenu" );
			_optionsContainer = GetNode<VBoxContainer>( "OptionsContainer" );
		}
	};
};
