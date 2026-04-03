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

using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Game.Application.Gameplay.Player.JumpKit.Modules;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
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

	public sealed class PlayerJumpKit : NomadBehaviour {
		public IGameEvent<PlayerDashBurnoutEventArgs> DashBurnout => _dashBurnout;
		private IGameEvent<PlayerDashBurnoutEventArgs> _dashBurnout;

		public IGameEvent<PlayerDashRechargedEventArgs> DashRecharged => _dashRecharged;
		private IGameEvent<PlayerDashRechargedEventArgs> _dashRecharged;

		private ISubscriptionHandle _dashAction;
		private readonly System.Timers.Timer _dashTimer = new();
		private readonly System.Timers.Timer _dashBurnoutCooldownTimer = new();

		private IAudioEmitter _emitter;
		private IDashModule _module = new DefaultModule();
		private EngineLight2D _light;

		private float _dashBurnoutAmount = 0.0f;
		private float _dashBurnoutCooldown = 0.0f;
		private float _dashDuration = 0.0f;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		public void OnInit( IGameEventRegistryService eventFactory ) {
			OnInit();

			_dashBurnout = eventFactory.GetEvent<PlayerDashBurnoutEventArgs>( EventNames.PLAYER_DASH_BURNOUT, nameof( PlayerJumpKit ) );
			_dashRecharged = eventFactory.GetEvent<PlayerDashRechargedEventArgs>( EventNames.PLAYER_DASH_RECHARGED, nameof( PlayerJumpKit ) );
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

			if ( _dashBurnoutCooldown == 0.0f ) {
				return;
			}

			_dashBurnoutCooldown += delta;
			if ( _dashBurnoutCooldown > _module.BurnoutCooldown ) {
				_dashBurnoutAmount = Math.Clamp( _dashBurnoutAmount - ( 0.10f * delta ), 0.0f, _dashBurnoutAmount );
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

			_dashBurnout?.Dispose();
			_dashRecharged?.Dispose();

			_dashAction?.Dispose();
		}

		private void TriggerDash() {
			if ( _dashBurnoutAmount >= 1.0f ) {
				return;
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
		private void OnDashActionTriggered( in EmptyEventArgs args ) {
			TriggerDash();
		}
	};
};