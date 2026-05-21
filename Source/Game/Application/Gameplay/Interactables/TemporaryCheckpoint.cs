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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	internal sealed class TemporaryCheckpoint : ICheckpoint
	{
		public Vector2 Origin {
			get {
				throw new System.NotImplementedException();
			}
		}

		public CheckpointStatus Status {
			get {
				throw new System.NotImplementedException();
			}
		}

		public bool IsTemporary {
			get {
				throw new System.NotImplementedException();
			}
		}

		public PlayerInteractionStatus PlayerStatus {
			get {
				throw new System.NotImplementedException();
			}
		}

		public IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged {
			get {
				throw new System.NotImplementedException();
			}
		}
	};
};
