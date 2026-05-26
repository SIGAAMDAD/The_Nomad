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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer
{
	/*
	===================================================================================

	MultiplayerObject

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal class MultiplayerObject : IMultiplayerObject, IDisposable
	{
		public bool IsSessionActive => _sessionService.IsSessionActive;
		public bool IsHost => _sessionService.IsHost;
		public bool IsClient => _sessionService.IsClient;
		public bool IsReplicating => _isReplicating;
		public bool IsDisposed => _isDisposed;
		public NetworkSessionInfo? CurrentSession => _sessionService.CurrentSession;
		public PeerId LocalPeerId => _sessionService.CurrentSession?.LocalPeerId ?? default;
		public PeerId HostPeerId => _sessionService.CurrentSession?.HostPeerId ?? default;

		public uint Revision => _revision;
		private uint _revision = 0;

		public uint LastServerTick => lastServerTick;
		protected uint lastServerTick = 0;

		private readonly INetworkSessionService _sessionService;
		private readonly INetworkRpcBus _rpcBus;
		private readonly INetworkEventBus _eventBus;
		private readonly INetworkMessageRegistry _registry;
		private readonly IGameEventRegistryService _eventFactory;
		private readonly List<Action> _cleanup = new();

		private bool _isDisposed = false;
		private bool _isReplicating = false;

		/*
		===============
		MultiplayerObject
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
		public MultiplayerObject(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
		{
			_registry = messageRegistry ?? throw new ArgumentNullException( nameof( messageRegistry ) );
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );
			_rpcBus = rpcBus ?? throw new ArgumentNullException( nameof( rpcBus ) );
			_eventBus = eventBus ?? throw new ArgumentNullException( nameof( eventBus ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			for ( int i = _cleanup.Count - 1; i >= 0; i-- ) {
				_cleanup[i].Invoke();
			}
			_cleanup.Clear();

			Dispose( true );

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose( bool disposing )
		{
		}

		/*
		===============
		GetEvent
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="name"></param>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		public IGameEvent<TArgs> GetEvent<TArgs>( string name, string nameSpace )
			where TArgs : struct
		{
			return _eventFactory.GetEvent<TArgs>( name, nameSpace );
		}

		/*
		===============
		RegisterNetworkEvent
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		public void RegisterNetworkEvent<TArgs>( MessageIds id, IGameEvent<TArgs> gameEvent )
			where TArgs : struct
		{
			_registry.Register<TArgs>( (ushort)id, NetworkMessageKind.Event );
			_eventBus.Register( gameEvent );
			AddCleanup( () => _eventBus.Unregister<TArgs>() );
		}

		/*
		===============
		RegisterRpc
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="handler"></param>
		public void RegisterRpc<TRpc>( MessageIds id, NetworkRpcHandler<TRpc> handler )
			where TRpc : struct
		{
			_registry.Register<TRpc>( (ushort)id, NetworkMessageKind.Rpc );
			_rpcBus.Register( handler );
			AddCleanup( () => _rpcBus.Unregister<TRpc>() );
		}

		/*
		===============
		Subscribe
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		/// <param name="callback"></param>
		public void Subscribe<TArgs>( IGameEvent<TArgs> gameEvent, EventCallback<TArgs> callback )
			where TArgs : struct
		{
			gameEvent.Subscribe( callback );
			AddCleanup( () => gameEvent.Unsubscribe( callback ) );
		}

		/*
		===============
		PublishHostEvent
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		/// <param name="payload"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public bool PublishHostEvent<TArgs>( IGameEvent<TArgs> gameEvent, in TArgs payload, NetworkSendMode mode = NetworkSendMode.Reliable )
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

		/*
		===============
		ReplicateEventToRemotePeers
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		/// <param name="payload"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public bool ReplicateEventToRemotePeers<TArgs>( IGameEvent<TArgs> gameEvent, in TArgs payload, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TArgs : struct
		{
			if ( !IsHost ) {
				return false;
			}

			_isReplicating = true;
			try {
				return _eventBus.PublishToAll( gameEvent, in payload, mode );
			} finally {
				_isReplicating = false;
			}
		}

		/*
		===============
		SendRpcToHost
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="rpc"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public bool SendRpcToHost<TRpc>( in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct
		{
			if ( IsHost ) {
				return false;
			}

			return _rpcBus.SendToHost( in rpc, mode );
		}

		/*
		===============
		SendRpcToPeer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="peerId"></param>
		/// <param name="rpc"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public bool SendRpcToPeer<TRpc>( PeerId peerId, in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct
		{
			return _rpcBus.SendToPeer( peerId, in rpc, mode );
		}

		/*
		===============
		BroadcastRpc
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="rpc"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public bool BroadcastRpc<TRpc>( in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct
		{
			if ( !IsHost ) {
				return false;
			}

			return _rpcBus.Broadcast( in rpc, mode );
		}

		/*
		===============
		AddCleanup
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="cleanup"></param>
		public void AddCleanup( Action cleanup )
		{
			ArgumentGuard.ThrowIfNull( cleanup, nameof( cleanup ) );
			_cleanup.Add( cleanup );
		}

		protected uint IncrementRevision()
		{
			unchecked {
				_revision++;
			}
			if ( _revision == 0 ) {
				_revision = 1;
			}
			return _revision;
		}

		protected bool ShouldApplyRevision( uint incomingRevision )
		{
			return incomingRevision > _revision;
		}

		protected void SetRevision( uint revision )
		{
			_revision = revision;
		}
	};
};
