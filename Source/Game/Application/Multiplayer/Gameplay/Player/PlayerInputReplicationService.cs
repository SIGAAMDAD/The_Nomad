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

using System.Collections.Generic;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Application.Gameplay.Player.Input;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.PlayerInput
{
	/*
	===================================================================================

	PlayerInputReplicationService

	===================================================================================
	*/
	/// <summary>
	/// Routes local player input to the host and feeds host-side remote input sources.
	/// </summary>

	internal sealed class PlayerInputReplicationService : MultiplayerObject
	{
		private readonly Dictionary<PeerId, RemotePlayerInputSource> _remoteSources = new Dictionary<PeerId, RemotePlayerInputSource>();

		/*
		===============
		PlayerInputReplicationService
		===============
		*/
		public PlayerInputReplicationService(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			RegisterRpc<PlayerInputCommandRpc>( MessageIds.PlayerInputCommandRpc, OnPlayerInputCommand );
		}

		/*
		===============
		RegisterRemoteSource
		===============
		*/
		public bool RegisterRemoteSource( RemotePlayerInputSource source )
		{
			ArgumentGuard.ThrowIfNull( source, nameof( source ) );

			if ( !source.PeerId.IsValid ) {
				return false;
			}

			_remoteSources[source.PeerId] = source;
			return true;
		}

		/*
		===============
		UnregisterRemoteSource
		===============
		*/
		public bool UnregisterRemoteSource( PeerId peerId )
		{
			return _remoteSources.Remove( peerId );
		}

		/*
		===============
		SendFrameToHost
		===============
		*/
		public bool SendFrameToHost( in PlayerInputFrame frame )
		{
			if ( IsHost ) {
				return false;
			}

			if ( !frame.PeerId.IsValid ) {
				return false;
			}

			var rpc = new PlayerInputCommandRpc( in frame );
			return SendRpcToHost( in rpc, NetworkSendMode.Unreliable );
		}

		/*
		===============
		SendCurrentFrameToHost
		===============
		*/
		public bool SendCurrentFrameToHost( IPlayerInputSource inputSource )
		{
			ArgumentGuard.ThrowIfNull( inputSource, nameof( inputSource ) );
			return SendFrameToHost( inputSource.Current );
		}

		/*
		===============
		OnPlayerInputCommand
		===============
		*/
		private void OnPlayerInputCommand( in NetworkRpcContext context, in PlayerInputCommandRpc rpc )
		{
			if ( !IsHost ) {
				return;
			}

			if ( !IsPeerInSession( rpc.PeerId ) ) {
				return;
			}

			if ( !_remoteSources.TryGetValue( rpc.PeerId, out RemotePlayerInputSource source ) ) {
				return;
			}

			PlayerInputFrame frame = rpc.ToFrame();
			source.PushFrame( in frame );
		}

		/*
		===============
		IsPeerInSession
		===============
		*/
		private bool IsPeerInSession( PeerId peerId )
		{
			if ( CurrentSession == null || !peerId.IsValid ) {
				return false;
			}

			if ( peerId == CurrentSession.LocalPeerId ) {
				return true;
			}

			IReadOnlyList<NetworkPeerInfo> peers = CurrentSession.Peers;
			for ( int i = 0; i < peers.Count; i++ ) {
				if ( peers[i].PeerId == peerId ) {
					return true;
				}
			}

			return false;
		}
	}
}
