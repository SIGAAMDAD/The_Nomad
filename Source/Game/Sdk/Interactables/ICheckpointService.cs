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
using Nomad.Game.Sdk.Interactables;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Entities;

namespace Nomad.Game.Sdk.Interactables
{
	/// <summary>
	///
	/// </summary>
	public interface ICheckpointService : IDisposable
	{
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		List<ICheckpointEntity> GetActivatedCheckpoints();

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		List<ICheckpointEntity> GetTemporaryCheckpoints();

		/// <summary>
		///
		/// </summary>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		bool IsPermanentCheckpoint( CheckpointInstanceId checkpoint );

		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		bool TryRegisterPermanent( CheckpointDefinition definition, CheckpointInstanceId checkpoint );

		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		bool TryCreateTemporary( PlayerId playerId, out CheckpointInstanceId checkpoint );

		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		bool TryActivateCheckpoint( PlayerId playerId, CheckpointInstanceId checkpoint );

		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		bool TryRest( PlayerId playerId, CheckpointInstanceId checkpoint );

		/// <summary>
		/// Sends a request to the checkpoint service to attempt to leave the given PlayerId's current checkpoint.
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		bool TryLeave( PlayerId playerId );
	}
}
