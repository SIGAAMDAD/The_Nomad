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

using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Application.Gameplay.Player.JumpKit.Modules;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Input.Events;
using Nomad.Logger.Globals;
using Nomad.Scene.GameObjects;
using System;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit {
	/*
	===================================================================================

	PlayerJumpKit

	===================================================================================
	*/
	internal sealed class PlayerJumpKit : NomadBehaviour {
		public float BurnoutAmount => _runtime.BurnoutAmount;
		public bool IsDashing => _runtime.IsDashing;
		public bool IsBurnedOut => _runtime.IsBurnedOut;
		public bool CanDash => _runtime.CanStartDash();

		public IGameEvent<PlayerDashBurnoutEventArgs> DashBurnout => _dashBurnout;
		private readonly IGameEvent<PlayerDashBurnoutEventArgs> _dashBurnout = default;

		public IGameEvent<PlayerDashRechargedEventArgs> DashRecharged => _dashRecharged;
		private readonly IGameEvent<PlayerDashRechargedEventArgs> _dashRecharged = default;

		private readonly IGameEvent<PlayerStatChangedEventArgs> _statChanged = default;

		private ISubscriptionHandle? _dashAction;

		private IAudioEmitter? _emitter;
		private EngineLight2D? _light;

		private IDashModule _module = new DefaultModule();
		private DashRuntime _runtime = default;
		private DashEffects _effects = default;

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
				nameof( PlayerJumpKit )
			);

			_dashRecharged = eventFactory.GetEvent<PlayerDashRechargedEventArgs>(
				EventNames.PLAYER_DASH_RECHARGED,
				nameof( PlayerJumpKit )
			);

			_statChanged = eventFactory.GetEvent<PlayerStatChangedEventArgs>(
				EventNames.PLAYER_STAT_CHANGED,
				EventNames.NAMESPACE
			);

			_dashAction = eventFactory
				.GetEvent<ButtonActionEventArgs>( $"Dash:{Constants.Events.BUTTON_CLICKED}", Constants.Events.NAMESPACE )
				.Subscribe( OnDashActionTriggered );

			_runtime = new DashRuntime(
				initialDashDuration: _module.DashDuration,
				burnoutRechargeDuration: 2.5f
			);

			_effects = new DashEffects( _emitter, _light );
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
				_effects.OnDashEnded();
			}
			if ( result.BurnedOutThisFrame ) {
				_effects.OnBurnoutTriggered( result );
				PublishDashBurnout( result );
			}
			if ( result.RechargedThisFrame ) {
				_effects.OnDashRecharged( result );
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
			ArgumentNullException.ThrowIfNull( module );

			_module = module;
			_runtime.ResetDashDuration( module.DashDuration );
		}

		/*
		===============
		BindPresentation
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="emitter"></param>
		/// <param name="light"></param>
		public void BindPresentation( IAudioEmitter? emitter, EngineLight2D? light ) {
			_emitter = emitter;
			_light = light;
			_effects = new DashEffects( _emitter, _light );
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
					return;
				case DashStartStatus.BurnedOut:
					_effects.OnBurnoutTriggered( result );
					PublishDashBurnout( result );
					return;
				case DashStartStatus.Started:
					Logging.PrintLine( "Dash started." );
					_effects.OnDashStarted( result );
					return;
				default:
					throw new InvalidOperationException( $"Unhandled dash start status '{result.Status}'." );
			}
		}

		/*
		===============
		OnDashActionTriggered
		===============
		*/
		private void OnDashActionTriggered( in ButtonActionEventArgs args ) {
			Logging.PrintLine( "Dash action triggered!" );
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
			_statChanged.Publish( new PlayerStatChangedEventArgs( result.BurnoutAmount, 0.0f, StatType.JumpKitHeat ) );
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