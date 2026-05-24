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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Application.Gameplay.Player.JumpKit.Modules;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Input;
using Nomad.Input.ValueObjects;
using Nomad.Scene.GameObjects;
using System;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit
{
	/*
	===================================================================================

	PlayerJumpKit

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerJumpKit : NomadBehaviour, IJumpKit
	{
		public PlayerId Id { get; set; }

		public float BurnoutAmount => _runtime.BurnoutAmount;
		public bool IsDashing => _runtime.IsDashing;
		public bool IsBurnedOut => _runtime.IsBurnedOut;
		public bool CanDash => _runtime.CanStartDash();

		private EngineLight2D? _light;
		private GpuParticles2D _particles;

		private PlayerPrefab _prefab;

		public IDashModule Module => _module;
		private IDashModule _module = new DefaultModule();

		private readonly DashRuntime _runtime = default;

		public IGameEvent<PlayerDashStartEventArgs> DashStarted => _dashStarted;
		private IGameEvent<PlayerDashStartEventArgs> _dashStarted = default;

		public IGameEvent<PlayerDashEndedEventArgs> DashEnded => _dashEnded;
		private IGameEvent<PlayerDashEndedEventArgs> _dashEnded = default;

		public IGameEvent<PlayerDashBurnoutEventArgs> DashBurnout => _dashBurnout;
		private IGameEvent<PlayerDashBurnoutEventArgs> _dashBurnout = default;

		public IGameEvent<PlayerDashRechargedEventArgs> DashRecharged => _dashRecharged;
		private IGameEvent<PlayerDashRechargedEventArgs> _dashRecharged = default;

		private IGameEvent<PlayerResourceChangedEventArgs> _resourceChanged = default;

		/*
		===============
		PlayerJumpKit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public PlayerJumpKit()
		{
			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Dash:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnDashActionTriggered );

			_runtime = new DashRuntime(
				initialDashDuration: _module.DashDuration,
				burnoutRechargeDuration: 2.5f
			);
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void OnInit()
		{
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();

			_light = _prefab.FindChild<EngineLight2D>( "JumpKitEffect/PointLight2D" );
			_particles = _prefab.GetNode<GpuParticles2D>( "JumpKitEffect" );

			var eventFactory = GameEventRegistry.Instance;

			_dashBurnout = eventFactory.GetEvent<PlayerDashBurnoutEventArgs>(
				PlayerDashBurnoutEventArgs.Name,
				PlayerDashBurnoutEventArgs.NameSpace
			);

			_dashRecharged = eventFactory.GetEvent<PlayerDashRechargedEventArgs>(
				PlayerDashRechargedEventArgs.Name,
				PlayerDashRechargedEventArgs.NameSpace
			);

			_dashStarted = eventFactory.GetEvent<PlayerDashStartEventArgs>(
				PlayerDashStartEventArgs.Name,
				PlayerDashStartEventArgs.NameSpace
			);

			_dashEnded = eventFactory.GetEvent<PlayerDashEndedEventArgs>(
				PlayerDashEndedEventArgs.Name,
				PlayerDashEndedEventArgs.NameSpace
			);

			_resourceChanged = eventFactory.GetEvent<PlayerResourceChangedEventArgs>(
				PlayerResourceChangedEventArgs.Name,
				PlayerResourceChangedEventArgs.NameSpace
			);
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
		public override void OnUpdate( float delta )
		{
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
		public override void OnShutdown()
		{
			base.OnShutdown();

			_dashStarted?.Dispose();
			_dashEnded?.Dispose();
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
		public void SetModule( IDashModule module )
		{
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
		private void TryStartDash()
		{
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
		private void OnDashActionTriggered( in ButtonActionEventArgs args )
		{
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
		private void OnBurnoutAmountChanged( in DashUpdateResult result )
		{
			_resourceChanged.Publish(
				new PlayerResourceChangedEventArgs(
					Id,
					0.0f,
					result.BurnoutAmount,
					PlayerResourceType.JumpKitHeat
				)
			);
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
		private void PublishDashBurnout( in DashStartResult result )
		{
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
		private void PublishDashBurnout( in DashUpdateResult result )
		{
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
		private void PublishDashRecharged( in DashUpdateResult result )
		{
			_dashRecharged.Publish( default );
		}
	};
};
