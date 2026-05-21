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

using Nomad.Core.Events;
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player.State;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player.State;

namespace Nomad.Game.Application.Gameplay.Player.State
{
	/*
	===================================================================================

	PlayerStateCoordinator

	===================================================================================
	*/
	/// <summary>
	/// <para>Owns the player's high-level gameplay state.</para>
	///
	/// <para>Owns:</para>
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>current player state</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>previous player state</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>state version</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>state transition validation</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>state changed event publication</description>
	/// 	</item>
	/// </list>
	///
	/// <p>Does not own:</p>
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>movement</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>animation</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>combat</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>input</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>checkpoint behavior</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>multiplayer replication</description>
	/// 	</item>
	/// </list>
	/// </summary>

	internal sealed class PlayerStateCoordinator : IPlayerStateWriter, IPlayerStateReader
	{
		public PlayerStateId Current => _state;
		private PlayerStateId _state;

		public PlayerStateId Previous => _previousState;
		private PlayerStateId _previousState;

		public uint StateVersion => _stateVersion;
		private uint _stateVersion = 0;

		public PlayerStateChangeReason LastReason => _lastReason;
		private PlayerStateChangeReason _lastReason = PlayerStateChangeReason.None;

		public uint EnteredStateTick => _enteredStateTick;
		private uint _enteredStateTick = 0;

		public bool IsIdle => _state == PlayerStateId.Idle;
		public bool IsMoving => _state == PlayerStateId.Moving;
		public bool IsDead => _state == PlayerStateId.Dead;
		public bool IsRestingAtCheckpoint => _state == PlayerStateId.RestingAtCheckpoint;

		public bool CanMove => _state == PlayerStateId.Idle || _state == PlayerStateId.Moving;

		public bool CanTakeInput => _state != PlayerStateId.Dead &&
									_state != PlayerStateId.RestingAtCheckpoint;

		public bool CanTakeDamage => _state != PlayerStateId.Dead &&
									 _state != PlayerStateId.RestingAtCheckpoint;

		private readonly PlayerId _playerId = PlayerId.Invalid;

		public IGameEvent<PlayerStateChangedEventArgs> StateChanged => _stateChanged;
		private readonly IGameEvent<PlayerStateChangedEventArgs> _stateChanged = null;

		/*
		===============
		PlayerStateCoordinator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="initialState"></param>
		/// <param name="eventFactory"></param>
		public PlayerStateCoordinator( PlayerId playerId, PlayerStateId initialState, IGameEventRegistryService eventFactory )
		{
			RangeGuard.ThrowIfOutOfRange( (int)initialState, (int)PlayerStateId.Min, (int)PlayerStateId.Max, nameof( initialState ) );
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_playerId = playerId;

			_state = initialState;
			_previousState = initialState;

			_stateChanged = eventFactory
				.GetEvent<PlayerStateChangedEventArgs>(
					PlayerStateChangedEventArgs.Name,
					PlayerStateChangedEventArgs.NameSpace
				);
		}

		/*
		===============
		TrySetState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <returns></returns>
		public bool TrySetState( PlayerStateId newState )
		{
			return TrySetState( newState, PlayerStateChangeReason.None, 0 ).Success;
		}

		/*
		===============
		TrySetState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		public PlayerStateTransitionResult TrySetState( PlayerStateId newState, PlayerStateChangeReason reason, uint serverTick = 0 )
		{
			RangeGuard.ThrowIfOutOfRange( (int)newState, (int)PlayerStateId.Min, (int)PlayerStateId.Max, nameof( newState ) );

			if ( newState == _state ) {
				return PlayerStateTransitionResult.Failed(
					_state,
					newState,
					"Player is already in the requested state."
				);
			}

			if ( !CanTransitionTo( newState, out string? failureReason ) ) {
				return PlayerStateTransitionResult.Failed(
					_state,
					newState,
					failureReason ?? "Invalid player state transition."
				);
			}

			PlayerStateId oldState = _state;

			ApplyState( newState, reason, serverTick, publishEvent: true );

			return PlayerStateTransitionResult.Succeeded( oldState, newState );
		}

		/*
		===============
		ForceSetState
		===============
		*/
		/// <summary>
		/// Forces a state transition without transition validation.
		/// Intended for respawns, loading, cutscenes, and authoritative network correction.
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <param name="publishEvent"></param>
		public void ForceSetState( PlayerStateId newState, PlayerStateChangeReason reason, uint serverTick = 0, bool publishEvent = true )
		{
			RangeGuard.ThrowIfOutOfRange( (int)newState, (int)PlayerStateId.Min, (int)PlayerStateId.Max, nameof( newState ) );

			if ( newState == _state ) {
				return;
			}

			ApplyState( newState, reason, serverTick, publishEvent );
		}

		/*
		===============
		SetIdle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		public bool SetIdle( PlayerStateChangeReason reason = PlayerStateChangeReason.Movement, uint serverTick = 0 )
		{
			return TrySetState( PlayerStateId.Idle, reason, serverTick ).Success;
		}

		/*
		===============
		SetMoving
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		public bool SetMoving( PlayerStateChangeReason reason = PlayerStateChangeReason.Movement, uint serverTick = 0 )
		{
			return TrySetState( PlayerStateId.Moving, reason, serverTick ).Success;
		}

		/*
		===============
		SetDead
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		public bool SetDead( PlayerStateChangeReason reason = PlayerStateChangeReason.Death, uint serverTick = 0 )
		{
			return TrySetState( PlayerStateId.Dead, reason, serverTick ).Success;
		}

		/*
		===============
		SetRestingAtCheckpoint
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		public bool SetRestingAtCheckpoint( uint serverTick = 0 )
		{
			return TrySetState( PlayerStateId.RestingAtCheckpoint, PlayerStateChangeReason.Checkpoint, serverTick ).Success;
		}

		/*
		===============
		ReviveToIdle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="serverTick"></param>
		public void ReviveToIdle( uint serverTick = 0 )
		{
			ForceSetState( PlayerStateId.Idle, PlayerStateChangeReason.Revive, serverTick );
		}

		/*
		===============
		Respawn
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="serverTick"></param>
		public void Respawn( uint serverTick = 0 )
		{
			ForceSetState( PlayerStateId.Idle, PlayerStateChangeReason.Respawn, serverTick );
		}

		/*
		===============
		CanTransitionTo
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <returns></returns>
		public bool CanTransitionTo( PlayerStateId newState )
		{
			return CanTransitionTo( newState, out _ );
		}

		/*
		===============
		CanTransitionTo
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="failureReason"></param>
		/// <returns></returns>
		private bool CanTransitionTo( PlayerStateId newState, out string? failureReason )
		{
			failureReason = null;

			if ( newState == _state ) {
				failureReason = "State is unchanged.";
				return false;
			}

			switch ( _state ) {
				case PlayerStateId.Dead:
					return CanTransitionFromDead( newState, out failureReason );

				case PlayerStateId.RestingAtCheckpoint:
					return CanTransitionFromRestingAtCheckpoint( newState, out failureReason );

				case PlayerStateId.Idle:
					return CanTransitionFromIdle( newState, out failureReason );

				case PlayerStateId.Moving:
					return CanTransitionFromMoving( newState, out failureReason );

				default:
					failureReason = $"Unhandled player state: {_state}.";
					return false;
			}
		}

		/*
		===============
		CanTransitionFromDead
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="failureReason"></param>
		/// <returns></returns>
		private static bool CanTransitionFromDead( PlayerStateId newState, out string? failureReason )
		{
			switch ( newState ) {
				case PlayerStateId.Idle:
					failureReason = null;
					return true;

				case PlayerStateId.Moving:
				case PlayerStateId.RestingAtCheckpoint:
					failureReason = "Dead players must be revived or respawned before moving/resting.";
					return false;

				default:
					failureReason = "Invalid transition from Dead.";
					return false;
			}
		}

		/*
		===============
		CanTransitionFromRestingAtCheckpoint
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="failureReason"></param>
		/// <returns></returns>
		private static bool CanTransitionFromRestingAtCheckpoint( PlayerStateId newState, out string? failureReason )
		{
			switch ( newState ) {
				case PlayerStateId.Idle:
				case PlayerStateId.Dead:
					failureReason = null;
					return true;

				case PlayerStateId.Moving:
					failureReason = "Player must leave checkpoint rest before moving.";
					return false;

				default:
					failureReason = "Invalid transition from RestingAtCheckpoint.";
					return false;
			}
		}

		/*
		===============
		CanTransitionFromIdle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="failureReason"></param>
		/// <returns></returns>
		private static bool CanTransitionFromIdle( PlayerStateId newState, out string? failureReason )
		{
			switch ( newState ) {
				case PlayerStateId.Moving:
				case PlayerStateId.Dead:
				case PlayerStateId.RestingAtCheckpoint:
					failureReason = null;
					return true;

				default:
					failureReason = "Invalid transition from Idle.";
					return false;
			}
		}

		/*
		===============
		CanTransitionFromMoving
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="failureReason"></param>
		/// <returns></returns>
		private static bool CanTransitionFromMoving( PlayerStateId newState, out string? failureReason )
		{
			switch ( newState ) {
				case PlayerStateId.Idle:
				case PlayerStateId.Dead:
					failureReason = null;
					return true;

				case PlayerStateId.RestingAtCheckpoint:
					failureReason = "Player must stop moving before resting at a checkpoint.";
					return false;

				default:
					failureReason = "Invalid transition from Moving.";
					return false;
			}
		}

		/*
		===============
		ApplyState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <param name="publishEvent"></param>
		private void ApplyState( PlayerStateId newState, PlayerStateChangeReason reason, uint serverTick, bool publishEvent )
		{
			PlayerStateId oldState = _state;

			_previousState = oldState;
			_state = newState;
			_lastReason = reason;
			_enteredStateTick = serverTick;

			AdvanceVersion();

			if ( publishEvent ) {
				_stateChanged.Publish(
					new PlayerStateChangedEventArgs(
						oldState,
						newState
					)
				);
			}
		}

		/*
		===============
		AdvanceVersion
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void AdvanceVersion()
		{
			unchecked {
				_stateVersion++;
			}

			if ( _stateVersion == 0 ) {
				_stateVersion = 1;
			}
		}
	};
};
