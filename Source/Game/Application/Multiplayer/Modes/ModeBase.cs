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
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.Modes
{
	/*
	===================================================================================

	ModeBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class ModeBase : MultiplayerObject, IGameMode
	{
		public abstract string ModeName { get; }
		public abstract Mode Mode { get; }

		public IGameEvent<GameStartEventArgs> GameStart => _gameStart;
		private readonly IGameEvent<GameStartEventArgs> _gameStart = default;

		public uint StateVersion => lifecycle.StateVersion;

		protected readonly MultiplayerPeerRoster peerRoster;
		protected readonly MultiplayerStateMachine<ModeLifecycleState> lifecycle;

		/*
		===============
		ModeBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="sessionService"></param>
		/// <param name="rpcBus"></param>
		/// <param name="eventBus"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ModeBase(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			peerRoster = new MultiplayerPeerRoster( sessionService );
			lifecycle = new MultiplayerStateMachine<ModeLifecycleState>();

			_gameStart = GetEvent<GameStartEventArgs>(
				GameStartEventArgs.Name,
				GameStartEventArgs.NameSpace
			);

			Subscribe( sessionService.SessionChanged, HandleSessionChanged );
			Subscribe( sessionService.PeerConnected, HandlePeerConnected );
			Subscribe( sessionService.PeerDisconnected, HandlePeerDisconnected );
		}

		/*
		===============
		TryStartGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool TryStartGame()
		{
			if ( !lifecycle.TransitionHost( this, ModeLifecycleState.Active ) ) {
				return false;
			}

			return PublishHostEvent( _gameStart, default );
		}

		/*
		===============
		AdvanceStateVersion
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		protected uint AdvanceStateVersion()
		{
			return lifecycle.AdvanceVersion();
		}

		/*
		===============
		ResetStateVersion
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected void ResetStateVersion()
		{
			lifecycle.ResetVersion();
		}

		/*
		===============
		GetLocalPeer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		protected NetworkPeerInfo? GetLocalPeer()
		{
			return peerRoster.GetLocalPeer();
		}

		/*
		===============
		GetHostPeer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		protected NetworkPeerInfo? GetHostPeer()
		{
			return peerRoster.GetHostPeer();
		}

		/*
		===============
		FindPeer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <returns></returns>
		protected NetworkPeerInfo? FindPeer( PeerId peerId )
		{
			return peerRoster.FindPeer( peerId );
		}

		/*
		===============
		CreateStateMachine
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TState"></typeparam>
		/// <param name="initialState"></param>
		/// <param name="guard"></param>
		/// <returns></returns>
		protected static MultiplayerStateMachine<TState> CreateStateMachine<TState>( TState initialState = default, MultiplayerStateMachine<TState>.TransitionGuard guard = null )
			where TState : struct
		{
			return new MultiplayerStateMachine<TState>( initialState, guard );
		}

		/*
		===============
		OnSessionChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected virtual void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
		}

		/*
		===============
		OnPeerConnected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected virtual void OnPeerConnected( in PeerConnectedEventArgs args )
		{
		}

		/*
		===============
		OnPeerDisconnected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected virtual void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
		}

		/*
		===============
		HandleSessionChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void HandleSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			ResetStateVersion();
			OnSessionChanged( in args );
		}

		/*
		===============
		HandlePeerConnected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void HandlePeerConnected( in PeerConnectedEventArgs args )
		{
			OnPeerConnected( in args );
		}

		/*
		===============
		HandlePeerDisconnected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void HandlePeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			OnPeerDisconnected( in args );
		}
	};
};
