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
using Godot;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Data.Multiplayer;

namespace Nomad.Game.Prefabs
{
	internal abstract partial class InteractableRoot : Node2D
	{
		public static event Action<InteractableRoot, PlayerId> InteractionFocusEntered;
		public static event Action<InteractableRoot, PlayerId> InteractionFocusExited;

		public event Action<PlayerId> PlayerEntered;
		public event Action<PlayerId> PlayerExited;

		public virtual InternString InteractionPrompt => new InternString( "Interact" );
		public virtual EntityInteractionKind PrimaryInteractionKind => EntityInteractionKind.Use;

		public virtual void BuildInteractionOptions( List<InteractionMenuOption> options )
		{
			options.Add( new InteractionMenuOption( InteractionPrompt, PrimaryInteractionKind ) );
		}

		public virtual EntityInteractionResult RequestInteraction( PlayerId playerId, EntityInteractionKind kind )
		{
			return EntityInteractionResult.InvalidTarget();
		}

		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex )
		{
			if ( body is PlayerPrefab player ) {
				GD.Print( "Player Entered" );
				PlayerEntered?.Invoke( player.PeerId );
				InteractionFocusEntered?.Invoke( this, player.PeerId );
			}
		}

		private void OnBodyShapeExited( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex )
		{
			if ( body is PlayerPrefab player ) {
				GD.Print( "Player Exited" );
				PlayerExited?.Invoke( player.PeerId );
				InteractionFocusExited?.Invoke( this, player.PeerId );
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
