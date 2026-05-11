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
using System.Collections.Generic;
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

	internal abstract class ModeBase : IGameMode, IDisposable
	{
		public abstract string ModeName { get; }
		public abstract Mode Mode { get; }

		public IGameEvent<GameStartEventArgs> GameStart => _gameStart;

		public bool IsSessionActive => sessionService.IsSessionActive;
		public bool IsHost => sessionService.IsHost;
		public bool IsClient => sessionService.IsClient;
		public bool IsReplicating => _isReplicating;
		public bool IsDisposed => _isDisposed;
		public uint StateVersion => _stateVersion;
		public NetworkSessionInfo? CurrentSession => sessionService.CurrentSession;
		public PeerId LocalPeerId => sessionService.CurrentSession?.LocalPeerId ?? default;
		public PeerId HostPeerId => sessionService.CurrentSession?.HostPeerId ?? default;

		protected readonly INetworkSessionService sessionService;
		protected readonly INetworkRpcBus rpcBus;
		protected readonly INetworkEventBus eventBus;
		protected readonly IGameEventRegistryService eventFactory;

		private readonly IGameEvent<GameStartEventArgs> _gameStart;
		private readonly List<Action> _cleanup = new();
		private bool _isDisposed;
		private bool _isReplicating;
		private uint _stateVersion;

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
		public ModeBase( INetworkSessionService sessionService, INetworkRpcBus rpcBus, INetworkEventBus eventBus, IGameEventRegistryService eventFactory )
		{
			this.sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );
			this.rpcBus = rpcBus ?? throw new ArgumentNullException( nameof( rpcBus ) );
			this.eventBus = eventBus ?? throw new ArgumentNullException( nameof( eventBus ) );
			this.eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_gameStart = eventFactory.GetEvent<GameStartEventArgs>(
				GameStartEventArgs.Name,
				GameStartEventArgs.NameSpace
			);

			Subscribe( sessionService.SessionChanged, HandleSessionChanged );
			Subscribe( sessionService.PeerConnected, HandlePeerConnected );
			Subscribe( sessionService.PeerDisconnected, HandlePeerDisconnected );
		}

		public bool TryStartGame()
		{
			return PublishHostEvent( _gameStart, default );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			Dispose( disposing: true );
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		protected virtual void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}

			for ( int i = _cleanup.Count - 1; i >= 0; i-- ) {
				_cleanup[i].Invoke();
			}
			_cleanup.Clear();
		}

		protected IGameEvent<TArgs> GetEvent<TArgs>( string name, string nameSpace )
			where TArgs : struct
		{
			return eventFactory.GetEvent<TArgs>( name, nameSpace );
		}

		protected void RegisterNetworkEvent<TArgs>( IGameEvent<TArgs> gameEvent )
			where TArgs : struct
		{
			eventBus.Register( gameEvent );
			_cleanup.Add( () => eventBus.Unregister<TArgs>() );
		}

		protected void RegisterRpc<TRpc>( NetworkRpcHandler<TRpc> handler )
			where TRpc : struct
		{
			rpcBus.Register( handler );
			_cleanup.Add( () => rpcBus.Unregister<TRpc>() );
		}

		protected void Subscribe<TArgs>( IGameEvent<TArgs> gameEvent, EventCallback<TArgs> callback )
			where TArgs : struct
		{
			gameEvent.Subscribe( callback );
			_cleanup.Add( () => gameEvent.Unsubscribe( callback ) );
		}

		protected bool PublishHostEvent<TArgs>( IGameEvent<TArgs> gameEvent, in TArgs payload, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TArgs : struct
		{
			if ( !IsHost ) {
				return false;
			}

			gameEvent.Publish( in payload );

			if ( !_isReplicating ) {
				ReplicateEventToRemotePeers( gameEvent, in payload, mode );
			}

			return true;
		}

		protected bool ReplicateEventToRemotePeers<TArgs>( IGameEvent<TArgs> gameEvent, in TArgs payload, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TArgs : struct
		{
			if ( !IsHost ) {
				return false;
			}

			_isReplicating = true;
			try {
				return eventBus.PublishToAll( gameEvent, in payload, mode );
			} finally {
				_isReplicating = false;
			}
		}

		protected bool SendRpcToHost<TRpc>( in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct
		{
			if ( IsHost ) {
				return false;
			}

			return rpcBus.SendToHost( in rpc, mode );
		}

		protected bool SendRpcToPeer<TRpc>( PeerId peerId, in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct
		{
			return rpcBus.SendToPeer( peerId, in rpc, mode );
		}

		protected bool BroadcastRpc<TRpc>( in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct
		{
			if ( !IsHost ) {
				return false;
			}

			return rpcBus.Broadcast( in rpc, mode );
		}

		protected uint AdvanceStateVersion()
		{
			unchecked {
				_stateVersion++;
			}
			return _stateVersion;
		}

		protected void ResetStateVersion()
		{
			_stateVersion = 0;
		}

		protected NetworkPeerInfo? GetLocalPeer()
		{
			return FindPeer( LocalPeerId );
		}

		protected NetworkPeerInfo? GetHostPeer()
		{
			return FindPeer( HostPeerId );
		}

		protected NetworkPeerInfo? FindPeer( PeerId peerId )
		{
			NetworkSessionInfo? session = CurrentSession;
			if ( session == null ) {
				return null;
			}

			for ( int i = 0; i < session.Peers.Count; i++ ) {
				if ( session.Peers[i].PeerId.Equals( peerId ) ) {
					return session.Peers[i];
				}
			}

			return null;
		}

		protected virtual void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
		}

		protected virtual void OnPeerConnected( in PeerConnectedEventArgs args )
		{
		}

		protected virtual void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
		}

		private void HandleSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			ResetStateVersion();
			OnSessionChanged( in args );
		}

		private void HandlePeerConnected( in PeerConnectedEventArgs args )
		{
			OnPeerConnected( in args );
		}

		private void HandlePeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			OnPeerDisconnected( in args );
		}
	};
};
