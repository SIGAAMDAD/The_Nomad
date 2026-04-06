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
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Application.Gameplay.Player.Stats;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Input.Events;
using Nomad.Scene.GameObjects;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerBase
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public abstract class PlayerBase {
		private const float MoveSpeed = 200.0f;

		private readonly PlayerJumpKit _jumpKit;
		private readonly IPlayerStatsRepository _statsRepository;

		public Guid Id => _id;
		private readonly Guid _id;

		private readonly ISubscriptionHandle _lookAngle;
		private readonly ISubscriptionHandle _moveAction;
		private readonly ISubscriptionGroup _inputActions;
		private readonly Sprite2D _headSprite;

		private readonly PlayerPrefab _prefab;
		private NumericsVector2 _moveInput;

		public IGameEvent<PlayerDieEventArgs> Die => _die;
		private readonly IGameEvent<PlayerDieEventArgs> _die;

		public IGameEvent<PlayerStatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<PlayerStatChangedEventArgs> _statChanged;

		/*
		===============
		PlayerBase
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="prefab"></param>
		/// <param name="scope"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		public PlayerBase( Guid guid, PlayerPrefab prefab, IServiceRegistry scope, IGameEventRegistryService eventFactory, ILoggerService logger ) {
			_prefab = prefab;
			_prefab.Controller = this;
			_id = guid;
			_die = eventFactory.GetEvent<PlayerDieEventArgs>( $"{_id}:{EventNames.PLAYER_DIE}", EventNames.NAMESPACE );
			_statChanged = eventFactory.GetEvent<PlayerStatChangedEventArgs>( $"{_id}:{EventNames.PLAYER_STAT_CHANGED}", EventNames.NAMESPACE );

			_lookAngle = eventFactory.GetEvent<AxisActionEventArgs>( $"Look:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnLookAngleChanged );
			
			_moveAction = eventFactory.GetEvent<AxisActionEventArgs>( $"Move:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnMoveAction );
			
			_inputActions = eventFactory.GetGroup( nameof( PlayerAggregate ) );
			_inputActions.Add(
				eventFactory.GetEvent<ButtonActionEventArgs>( $"Dash:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE ),
				OnDashTriggered
			);
			_inputActions.Add(
				eventFactory.GetEvent<ButtonActionEventArgs>( $"Parry:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE ),
				OnParryTriggered
			);
			_inputActions.Add(
				eventFactory.GetEvent<ButtonActionEventArgs>( $"Interact:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE ),
				OnInteractTriggered
			);
			_inputActions.Add(
				eventFactory.GetEvent<ButtonActionEventArgs>( $"Use Weapon:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE ),
				OnUseWeaponTriggered
			);

			_headSprite = prefab.GetNode<Sprite2D>( "HeadSprite" );
			prefab.AddChild( new Node() );

			_jumpKit = _prefab.AddComponent<PlayerJumpKit>();
			_statsRepository = new PlayerStatsRepository( eventFactory, logger );
			scope.AddSingleton( _statsRepository );
		}

		private void OnParryTriggered( in ButtonActionEventArgs args ) {
		}

		private void OnInteractTriggered( in ButtonActionEventArgs args ) {
		}

		private void OnUseWeaponTriggered( in ButtonActionEventArgs args ) {
		}

		private void OnDashTriggered( in ButtonActionEventArgs args ) {
		}

		internal void OnPhysicsUpdate( float delta ) {
			var moveInput = _moveInput;

			if ( moveInput.LengthSquared() > 1.0f ) {
				moveInput = NumericsVector2.Normalize( moveInput );
			}

			_prefab.Velocity = moveInput.ToGodot() * MoveSpeed;
			_prefab.MoveAndSlide();
		}

		private void OnMoveAction( in AxisActionEventArgs args ) {
			_moveInput = args.Value;
		}

		private void OnLookAngleChanged( in AxisActionEventArgs args ) {
			_headSprite.Rotation = _prefab.GetLocalMousePosition().AngleTo( _prefab.Position.ToGodot() );
		}
	};
};
