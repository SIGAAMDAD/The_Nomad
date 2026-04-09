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

using System.Numerics;

namespace Nomad.Game.Domain.Events.Player {
	/// <summary>
	/// 
	/// </summary>
	public readonly struct PlayerMovementChangedEventArgs {
		public Vector2 OldVelocity { get; }
		public Vector2 NewVelocity { get; }
		public bool IsMoving { get; }
		public bool WalkingReverse { get; }
		public bool FacingLeft { get; }

		public PlayerMovementChangedEventArgs( Vector2 oldVelocity, Vector2 newVelocity, bool isMoving, bool walkingReverse, bool facingLeft ) {
			OldVelocity = oldVelocity;
			NewVelocity = newVelocity;
			IsMoving = isMoving;
			WalkingReverse = walkingReverse;
			FacingLeft = facingLeft;
		}
	};
};