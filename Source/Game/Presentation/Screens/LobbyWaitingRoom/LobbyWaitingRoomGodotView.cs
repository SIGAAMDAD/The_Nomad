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
using Nomad.Core.Engine.Globals;
using Nomad.Core.OnlineServices;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Presentation.Screens.LobbyWaitingRoom
{
	/*
	===================================================================================

	LobbyWaitingRoomGodotView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class LobbyWaitingRoomGodotView : Control, ILobbyWaitingRoomView
	{
		private readonly IUserAvatarService _avatarService;

		private VBoxContainer _memberList;
		private LobbyWaitingRoomPresenter _presenter;

		public event Action Leave;
		public event Action StartGame;
		public event Action VoteStart;
		public event Action ReadyUp;
		public event Action ChangeSettings;

		public LobbyWaitingRoomGodotView()
		{
			_avatarService = ServiceLocator.GetService<IOnlinePlatformService>().AvatarService;
		}

		public void AddMember( string displayName, PeerId peerId )
		{
			var banner = new PlayerBanner( _avatarService, peerId, displayName );
			_memberList.AddChild( banner );
		}

		public void RemoveMember( int index )
		{
			_memberList.RemoveChild( _memberList.GetChild( index ) );
		}

		public override void _Ready()
		{
			base._Ready();

			_memberList = GetNode<VBoxContainer>( "MemberList" );

			var locator = ServiceLocator.Instance;
			var waitingRoomService = locator.GetService<ILobbyWaitingRoomService>();
			GetNode<Button>( "ButtonContainer/LeaveButton" ).Pressed += () => Leave?.Invoke();
			Button lobbyOption1Button = GetNode<Button>( "ButtonContainer/LobbyOption1Button" );
			Button lobbyOption2Button = GetNode<Button>( "ButtonContainer/LobbyOption2Button" );

			// if we're not the host, we can only ready up.
			if ( !waitingRoomService.CanStart ) {
				lobbyOption1Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_VOTE_START" ) );
				lobbyOption2Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_GAME_READY" ) );
			} else {
				lobbyOption1Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_CHANGE_SETTINGS" ) );
				lobbyOption2Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_START_GAME" ) );
			}

			_presenter = ScreenPresenterFactory.CreateLobbyWaitingRoomPresenter( this );
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta )
		{
			base._Process( delta );

			_presenter.Update();
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree();

			_presenter?.Dispose();
			_presenter = null;
		}
	};
};
