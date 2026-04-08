/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;
using Nomad.Audio.Interfaces;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Application.Gameplay.Player.JumpKit.Modules;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Input.Events;
using Nomad.Input.ValueObjects;
using Nomad.Logger.Globals;
using Nomad.Scene.GameObjects;
using System;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit {
	/*
	===================================================================================

	PlayerJumpKit

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerJumpKit : NomadBehaviour {
		public float BurnoutAmount => _runtime.BurnoutAmount;
		public bool IsDashing => _runtime.IsDashing;
		public bool IsBurnedOut => _runtime.IsBurnedOut;
		public bool CanDash => _runtime.CanStartDash();

		private readonly ISubscriptionHandle _dashAction;

		private EngineLight2D? _light;
		private GpuParticles2D _particles;

		private PlayerPrefab _prefab;

		private IDashModule _module = new DefaultModule();
		private readonly DashRuntime _runtime = default;

		public IGameEvent<PlayerDashStartEventArgs> DashStarted => _dashStarted;
		private readonly IGameEvent<PlayerDashStartEventArgs> _dashStarted;

		public IGameEvent<EmptyEventArgs> DashEnded => _dashEnded;
		private readonly IGameEvent<EmptyEventArgs> _dashEnded;

		public IGameEvent<PlayerDashBurnoutEventArgs> DashBurnout => _dashBurnout;
		private readonly IGameEvent<PlayerDashBurnoutEventArgs> _dashBurnout = default;

		public IGameEvent<PlayerDashRechargedEventArgs> DashRecharged => _dashRecharged;
		private readonly IGameEvent<PlayerDashRechargedEventArgs> _dashRecharged = default;

		private readonly IGameEvent<PlayerResourceChangedEventArgs> _resourceChanged = default;

		/*
		===============
		PlayerJumpKit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public PlayerJumpKit() {
			var eventFactory = GameEventRegistry.Instance;

			_dashBurnout = eventFactory.GetEvent<PlayerDashBurnoutEventArgs>(
				EventNames.PLAYER_DASH_BURNOUT,
				EventNames.NAMESPACE
			);

			_dashRecharged = eventFactory.GetEvent<PlayerDashRechargedEventArgs>(
				EventNames.PLAYER_DASH_RECHARGED,
				EventNames.NAMESPACE
			);

			_dashStarted = eventFactory.GetEvent<PlayerDashStartEventArgs>(
				EventNames.PLAYER_DASH_STARTED,
				EventNames.NAMESPACE
			);

			_dashEnded = eventFactory.GetEvent<EmptyEventArgs>(
				EventNames.PLAYER_DASH_ENDED,
				EventNames.NAMESPACE
			);

			_resourceChanged = eventFactory.GetEvent<PlayerResourceChangedEventArgs>(
				EventNames.PLAYER_RESOURCE_CHANGED,
				EventNames.NAMESPACE
			);

			_dashAction = eventFactory
				.GetEvent<ButtonActionEventArgs>( $"Dash:{Input.Constants.Events.BUTTON_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnDashActionTriggered );

			_runtime = new DashRuntime(
				initialDashDuration: _module.DashDuration,
				burnoutRechargeDuration: 2.5f
			);
		}

		public override void OnInit() {
			base.OnInit();
			
			_prefab = Object.CastAs<PlayerPrefab>();

			_light = _prefab.FindChild<EngineLight2D>( "JumpKitEffect/PointLight2D" );
			_particles = _prefab.GetNode<GpuParticles2D>( "JumpKitEffect" );
		}

		/*
		===============
		OnUpdate
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void OnUpdate( float delta ) {
			base.OnUpdate( delta );

			DashUpdateResult result = _runtime.Update( delta, _module );

			if ( result.DashEnded ) {
				_light.Visible = false;
				_particles.Emitting = false;
				_dashEnded.Publish( default );
			}
			if ( result.BurnedOutThisFrame ) {
				_light.Visible = false;
				_particles.Emitting = false;
				PublishDashBurnout( result );
			}
			if ( result.RechargedThisFrame ) {
				PublishDashRecharged( result );
			}
			if ( result.BurnoutChangedThisFrame ) {
				OnBurnoutAmountChanged( result );
			}
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnShutdown() {
			base.OnShutdown();

			_dashAction?.Dispose();

			_dashBurnout?.Dispose();
			_dashRecharged?.Dispose();
		}

		/*
		===============
		SetModule
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="module"></param>
		public void SetModule( IDashModule module ) {
			ArgumentGuard.ThrowIfNull( module );

			_module = module;
			_runtime.ResetDashDuration( module.DashDuration );
		}

		/*
		===============
		TryStartDash
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		private void TryStartDash() {
			DashStartResult result = _runtime.TryStartDash( _module );

			switch ( result.Status ) {
				case DashStartStatus.Rejected:
					break;
				case DashStartStatus.BurnedOut:
					PublishDashBurnout( result );
					break;
				case DashStartStatus.Started:
					_light.Visible = true;
					_particles.Emitting = true;
					_dashStarted.Publish( new PlayerDashStartEventArgs( _runtime.BurnoutAmount ) );
					break;
				default:
					throw new InvalidOperationException( $"Unhandled dash start status '{result.Status}'." );
			}
		}

		/*
		===============
		OnDashActionTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDashActionTriggered( in ButtonActionEventArgs args ) {
			if ( args.Phase != InputActionPhase.Started ) {
				return;
			}
			TryStartDash();
		}

		/*
		===============
		OnBurnoutAmountChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void OnBurnoutAmountChanged( in DashUpdateResult result ) {
			_resourceChanged.Publish( new PlayerResourceChangedEventArgs( result.BurnoutAmount, 0.0f, PlayerResourceType.JumpKitHeat ) );
			// Optional integration point:
			// - update HUD meter
			// - publish a burnout-changed event
			// - update player-facing UI state
		}

		/*
		===============
		PublishDashBurnout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void PublishDashBurnout( in DashStartResult result ) {
			_dashBurnout.Publish( default );
		}

		/*
		===============
		PublishDashBurnout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void PublishDashBurnout( in DashUpdateResult result ) {
			_dashBurnout.Publish( default );
		}

		/*
		===============
		PublishDashRecharged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void PublishDashRecharged( in DashUpdateResult result ) {
			_dashRecharged.Publish( default );
		}
	};
};