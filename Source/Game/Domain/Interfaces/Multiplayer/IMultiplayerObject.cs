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

using Nomad.Core.OnlineServices;
using Nomad.Core.Events;
using Nomad.Networking.Session;
using Nomad.Networking.Rpc;
using Nomad.Networking.Messaging;
using System;
using Nomad.Game.Domain.Data.Multiplayer;

namespace Nomad.Game.Domain.Interfaces.Multiplayer
{
	public interface IMultiplayerObject
	{
		/// <summary>
		/// Is our current session active?
		/// </summary>
		bool IsSessionActive { get; }

		/// <summary>
		/// Are we the host of this session?
		/// </summary>
		bool IsHost { get; }

		/// <summary>
		/// Are we a client in this session?
		/// </summary>
		bool IsClient { get; }

		/// <summary>
		/// Are we replicating the session from the host as a client?
		/// </summary>
		bool IsReplicating { get; }

		/// <summary>
		/// Has this object been disposed of yet?
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// The currently active session belonging to this multiplayer object.
		/// </summary>
		NetworkSessionInfo? CurrentSession { get; }

		/// <summary>
		/// The <see cref="PeerId"/> of this machine.
		/// </summary>
		PeerId LocalPeerId { get; }

		/// <summary>
		/// The <see cref="PeerId"/> of the host machine.
		/// </summary>
		PeerId HostPeerId { get; }

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="name"></param>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		IGameEvent<TArgs> GetEvent<TArgs>( string name, string nameSpace )
			where TArgs : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="id"></param>
		/// <param name="gameEvent"></param>
		void RegisterNetworkEvent<TArgs>( MessageIds id, IGameEvent<TArgs> gameEvent )
			where TArgs : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="id"></param>
		/// <param name="handler"></param>
		void RegisterRpc<TRpc>( MessageIds id, NetworkRpcHandler<TRpc> handler )
			where TRpc : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		/// <param name="callback"></param>
		void Subscribe<TArgs>( IGameEvent<TArgs> gameEvent, EventCallback<TArgs> callback )
			where TArgs : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		/// <param name="payload"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		bool PublishHostEvent<TArgs>( IGameEvent<TArgs> gameEvent, in TArgs payload, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TArgs : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TArgs"></typeparam>
		/// <param name="gameEvent"></param>
		/// <param name="payload"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		bool ReplicateEventToRemotePeers<TArgs>( IGameEvent<TArgs> gameEvent, in TArgs payload, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TArgs : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="rpc"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		bool SendRpcToHost<TRpc>( in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="peerId"></param>
		/// <param name="rpc"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		bool SendRpcToPeer<TRpc>( PeerId peerId, in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TRpc"></typeparam>
		/// <param name="rpc"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		bool BroadcastRpc<TRpc>( in TRpc rpc, NetworkSendMode mode = NetworkSendMode.Reliable )
			where TRpc : struct;

		/// <summary>
		///
		/// </summary>
		/// <param name="cleanup"></param>
		void AddCleanup( Action cleanup );
	};
};
