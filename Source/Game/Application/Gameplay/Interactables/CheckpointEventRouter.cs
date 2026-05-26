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
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Sdk.Interactables;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Events.Interactables;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	CheckpointEventRouter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CheckpointEventRouter : ICheckpointEventRouter
	{
		private readonly ICheckpointService _service;
		private readonly IPlayerRuntimeRegistry _players;

		private readonly IDisposable _leaveRequestedEvent;
		private readonly IDisposable _restRequestedEvent;
		private readonly IDisposable _activationRequestedEvent;

		private bool _isDisposed = false;

		public IGameEvent<CheckpointRestRequestedEventArgs> RestRequested => _restRequested;
		private readonly IGameEvent<CheckpointRestRequestedEventArgs> _restRequested = null;

		public IGameEvent<CheckpointLeaveRequestedEventArgs> LeaveRequested => _leaveRequested;
		private readonly IGameEvent<CheckpointLeaveRequestedEventArgs> _leaveRequested = null;

		public IGameEvent<CheckpointActivationRequestedEventArgs> ActivationRequested => _activationRequested;
		private readonly IGameEvent<CheckpointActivationRequestedEventArgs> _activationRequested = null;

		/*
		===============
		CheckpointEventRouter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="service"></param>
		/// <param name="eventFactory"></param>
		public CheckpointEventRouter( IPlayerRuntimeRegistry players, ICheckpointService service, IGameEventRegistryService eventFactory )
		{
			_service = service ?? throw new ArgumentNullException( nameof( service ) );
			_players = players ?? throw new ArgumentNullException( nameof( players ) );

			_restRequested = eventFactory.GetEvent<CheckpointRestRequestedEventArgs>(
				CheckpointRestRequestedEventArgs.Name,
				CheckpointRestRequestedEventArgs.NameSpace
			);
			_restRequestedEvent = _restRequested.Subscribe( OnRestRequested );

			_leaveRequested = eventFactory.GetEvent<CheckpointLeaveRequestedEventArgs>(
				CheckpointLeaveRequestedEventArgs.Name,
				CheckpointLeaveRequestedEventArgs.NameSpace
			);
			_leaveRequestedEvent = _leaveRequested.Subscribe( OnLeaveRequested );

			_activationRequested = eventFactory.GetEvent<CheckpointActivationRequestedEventArgs>(
				CheckpointActivationRequestedEventArgs.Name,
				CheckpointActivationRequestedEventArgs.NameSpace
			);
			_activationRequestedEvent = _activationRequested.Subscribe( OnActivationRequested );
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

			_restRequested.Dispose();
			_leaveRequested.Dispose();
			_activationRequested.Dispose();
			_restRequestedEvent.Dispose();
			_leaveRequestedEvent.Dispose();
			_activationRequestedEvent.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnLeaveRequested
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnLeaveRequested( in CheckpointLeaveRequestedEventArgs args )
		{
			args.RequesterId.ThrowIfInvalid( nameof( OnLeaveRequested ) );

			if ( !_players.TryGetState( args.RequesterId, out IPlayerStateReader? stateReader, out IPlayerStateWriter? stateWriter ) ) {
				return;
			}

			if ( stateReader.Current != PlayerStateId.RestingAtCheckpoint ) {
				return;
			}

			if ( _service.TryLeave( args.RequesterId ) ) {
				stateWriter.SetIdle( PlayerStateChangeReason.Checkpoint );
			}
		}

		private void OnRestRequested( in CheckpointRestRequestedEventArgs args )
		{
			args.RequesterId.ThrowIfInvalid( nameof( OnRestRequested ) );

			if ( !_players.TryGetState( args.RequesterId, out IPlayerStateReader? stateReader, out IPlayerStateWriter? stateWriter ) ) {
				return;
			}

			if ( stateReader.Current == PlayerStateId.RestingAtCheckpoint ) {
				return;
			}

			if ( _service.IsPermanentCheckpoint( args.CheckpointId ) ) {
				TryBeginRest( args.RequesterId, args.CheckpointId, stateWriter );
				return;
			}

			if ( _service.TryCreateTemporary( args.RequesterId, out CheckpointInstanceId checkpoint ) ) {
				TryBeginRest( args.RequesterId, checkpoint, stateWriter );
			}
		}

		private void OnActivationRequested( in CheckpointActivationRequestedEventArgs args )
		{
			args.RequesterId.ThrowIfInvalid( nameof( OnActivationRequested ) );

			if ( !args.CheckpointId.IsValid ) {
				return;
			}

			_service.TryActivateCheckpoint( args.RequesterId, args.CheckpointId );
		}

		private bool TryBeginRest( PlayerId playerId, CheckpointInstanceId checkpoint, IPlayerStateWriter stateWriter )
		{
			if ( !_service.TryRest( playerId, checkpoint ) ) {
				return false;
			}

			if ( stateWriter.SetRestingAtCheckpoint() ) {
				return true;
			}

			_service.TryLeave( playerId );
			return false;
		}
	};
};
