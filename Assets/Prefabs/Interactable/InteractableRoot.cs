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
using Nomad.Core.OnlineServices;
using Nomad.Game.Application.Gameplay.Interactables;

namespace Nomad.Game.Prefabs
{
	internal partial class InteractableRoot : Node2D
	{
		public event Action<PeerId> PlayerEntered;
		public event Action<PeerId> PlayerExited;

		private InteractableAggregate _aggregate;

		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex )
		{
			if ( body is PlayerPrefab player ) {
				PlayerEntered?.Invoke( player.PeerId );
			}
		}

		private void OnBodyShapeExited( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex )
		{
			if ( body is PlayerPrefab player ) {
				PlayerExited?.Invoke( player.PeerId );
			}
		}

		public override void _Ready()
		{
			base._Ready();

			var area2D = GetNode<Area2D>( "Zone" );
			area2D.BodyShapeEntered += OnBodyShapeEntered;
			area2D.BodyShapeExited += OnBodyShapeExited;

			_aggregate = new InteractableAggregate( this );
		}
	};
};
