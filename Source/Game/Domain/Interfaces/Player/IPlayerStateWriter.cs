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

using Nomad.Game.Domain.Data.Player.State;

namespace Nomad.Game.Domain.Interfaces.Player
{
	public interface IPlayerStateWriter
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <returns></returns>
		bool TrySetState( PlayerStateId newState );

		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		PlayerStateTransitionResult TrySetState( PlayerStateId newState, PlayerStateChangeReason reason, uint serverTick = 0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <param name="publishEvent"></param>
		void ForceSetState( PlayerStateId newState, PlayerStateChangeReason reason, uint serverTick = 0, bool publishEvent = true );

		/// <summary>
		///
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		bool SetIdle( PlayerStateChangeReason reason = PlayerStateChangeReason.Movement, uint serverTick = 0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		bool SetMoving( PlayerStateChangeReason reason = PlayerStateChangeReason.Movement, uint serverTick = 0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="reason"></param>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		bool SetDead( PlayerStateChangeReason reason = PlayerStateChangeReason.Death, uint serverTick = 0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="serverTick"></param>
		/// <returns></returns>
		bool SetRestingAtCheckpoint( uint serverTick = 0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="serverTick"></param>
		void ReviveToIdle( uint serverTick = 0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="serverTick"></param>
		void Respawn( uint serverTick =  0 );

		/// <summary>
		///
		/// </summary>
		/// <param name="newState"></param>
		/// <returns></returns>
		bool CanTransitionTo( PlayerStateId newState );
	};
};
