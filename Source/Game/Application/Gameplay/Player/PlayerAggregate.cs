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
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.EngineUtils;
using Nomad.Game.Prefabs;
using Nomad.Input.Events;

namespace Nomad.Game.Application.Gameplay.Player {
	internal sealed class PlayerAggregate : PlayerBase {
		private readonly ISubscriptionHandle _lookAngle;
		private readonly ISubscriptionHandle _moveAction;
		private readonly Sprite2D _headSprite;

		private readonly PlayerPrefab _prefab;

		public PlayerAggregate( Guid guid, PlayerPrefab prefab, IServiceRegistry scope, IGameEventRegistryService eventFactory, ILoggerService logger )
			: base( guid, prefab, scope, eventFactory, logger )
		{
			_prefab = prefab;

			_lookAngle = eventFactory.GetEvent<AxisActionEventArgs>( $"Look:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnLookAngleChanged );
			
			_moveAction = eventFactory.GetEvent<AxisActionEventArgs>( $"Move:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnMoveAction );
			
			_headSprite = prefab.GetNode<Sprite2D>( "HeadSprite" );
		}

		private void OnMoveAction( in AxisActionEventArgs args ) {
			_prefab.GlobalPosition += args.Value.ToGodot();
		}

		private void OnLookAngleChanged( in AxisActionEventArgs args ) {
			_headSprite.Rotation = _prefab.GetLocalMousePosition().AngleTo( _prefab.GlobalPosition );
		}
	};
};