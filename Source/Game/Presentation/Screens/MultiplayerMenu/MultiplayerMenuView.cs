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

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu
{
	/*
	===================================================================================

	MultiplayerMenu

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
		}
	};
};
