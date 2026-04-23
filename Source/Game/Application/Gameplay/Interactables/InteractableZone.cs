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
using Godot;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Interactables {
	internal sealed partial class InteractableZone : Area2D {
		public event Action PlayerEntered;
		public event Action PlayerExited;

		public override void _Ready() {
			base._Ready();

			BodyShapeEntered += OnBodyShapeEntered;
			BodyShapeExited += OnBodyShapeExited;
		}

		private void OnBodyShapeExited( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex ) {
			if ( body is PlayerPrefab ) {
				PlayerEntered?.Invoke();
			}
		}

		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex ) {
			if ( body is PlayerPrefab ) {
				PlayerExited?.Invoke();
			}
		}
	};
};