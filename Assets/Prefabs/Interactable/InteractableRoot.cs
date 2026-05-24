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
using Nomad.Game.Domain.Data.Multiplayer;

namespace Nomad.Game.Prefabs
{
	internal abstract partial class InteractableRoot : Node2D
	{
		public event Action<PlayerId> PlayerEntered;
		public event Action<PlayerId> PlayerExited;

		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex )
		{
			if ( body is PlayerPrefab player ) {
				GD.Print( "Player Entered" );
				PlayerEntered?.Invoke( player.PeerId );
			}
		}

		private void OnBodyShapeExited( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex )
		{
			if ( body is PlayerPrefab player ) {
				GD.Print( "Player Exited" );
				PlayerExited?.Invoke( player.PeerId );
			}
		}

		public override void _Ready()
		{
			base._Ready();

			var area2D = GetNode<Area2D>( "Zone" );
			area2D.BodyShapeEntered += OnBodyShapeEntered;
			area2D.BodyShapeExited += OnBodyShapeExited;
		}
	};
};
