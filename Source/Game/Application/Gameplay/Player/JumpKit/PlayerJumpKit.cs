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
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player.JumpKit;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.JumpKit;
using Nomad.Input;
using Nomad.Input.ValueObjects;
using System;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit
{
	/*
	===================================================================================

	PlayerJumpKit

	===================================================================================
	*/
	/// <summary>
	/// Event-driven player dash kit. Input events request dashes; timer events finish
	/// dashes, begin passive cooling, pulse heat recovery, and clear burnout lockout.
	/// </summary>

	internal sealed class PlayerJumpKit : NomadBehaviour, IJumpKit, IDisposable
	{
		private const PlayerJumpKitFlags TRANSIENT_EVENT_FLAGS = PlayerJumpKitFlags.DashStarted
			| PlayerJumpKitFlags.DashEnded
			| PlayerJumpKitFlags.DashRejected
			| PlayerJumpKitFlags.BurnoutEntered
			| PlayerJumpKitFlags.BurnoutChanged
			| PlayerJumpKitFlags.CoolingStarted
			| PlayerJumpKitFlags.Recharged
			| PlayerJumpKitFlags.ModuleChanged;

		public PlayerId Id { get; set; }

		public string DashLightPath { get; set; } = "DashLight";
		public string DashParticlesPath { get; set; } = "DashParticles";

		public float BurnoutAmount => _runtime?.BurnoutAmount ?? 0.0f;
		public bool IsDashing => _runtime != null && _runtime.IsDashing;
		public bool IsBurnedOut => _runtime != null && _runtime.IsBurnedOut;
		public bool CanDash => _runtime != null && _runtime.CanStartDash();

		public IDashModule Module => _module;
		private IDashModule _module = new DefaultModule();
		private DashModuleRuntimeSettings _settings;

		public JumpKitStatus Status => BuildStatus( PlayerJumpKitFlags.None );

		public IGameEvent<PlayerJumpKitStatusChangedEventArgs> StatusChanged => _events.StatusChanged;

		private DashRuntime _runtime = null;
		private PlayerJumpKitEventPublisher _events = null;
		private JumpKitTimerDriver _timers = null;
		private PlayerJumpKitVisualFeedback _visuals = null;
		private IDisposable _dashAction = null;
		private JumpKitStatus _lastPublishedStatus;

		private bool _isDisposed;

		public PlayerJumpKit()
		{
		}

		public override void OnInit()
		{
			base.OnInit();

			var prefab = Object.CastAs<PlayerPrefab>() ?? throw new InvalidOperationException(
				$"{nameof( PlayerJumpKit )} must be attached to a {nameof( PlayerPrefab )}."
			);

			var eventFactory = GameEventRegistry.Instance;

			_settings = DashModuleRuntimeSettings.FromModule( _module );
			_runtime = new DashRuntime( _settings.DashDuration );
			_events = new PlayerJumpKitEventPublisher( eventFactory );
			_timers = new JumpKitTimerDriver( prefab );
			_visuals = new PlayerJumpKitVisualFeedback( prefab, DashLightPath, DashParticlesPath );

			_timers.DashTimedOut += OnDashTimedOut;
			_timers.CoolingDelayTimedOut += OnCoolingDelayTimedOut;
			_timers.CoolingPulse += OnCoolingPulse;
			_timers.BurnoutTimedOut += OnBurnoutTimedOut;

			_dashAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Dash:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnDashActionTriggered );

			_lastPublishedStatus = default;
			PublishStatusChanged( PlayerJumpKitFlags.None );
		}

		public override void OnShutdown()
		{
			Dispose();
			base.OnShutdown();
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			if ( _timers != null ) {
				_timers.DashTimedOut -= OnDashTimedOut;
				_timers.CoolingDelayTimedOut -= OnCoolingDelayTimedOut;
				_timers.CoolingPulse -= OnCoolingPulse;
				_timers.BurnoutTimedOut -= OnBurnoutTimedOut;
			}

			_dashAction?.Dispose();
			_timers?.Dispose();
			_events?.Dispose();

			_visuals?.SetDashActive( false );

			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		public void SetModule( IDashModule module )
		{
			ArgumentGuard.ThrowIfNull( module, nameof( module ) );

			_module = module;
			_settings = DashModuleRuntimeSettings.FromModule( module );
			_runtime?.ResetDashDuration( _settings.DashDuration );

			PublishStatusChanged( PlayerJumpKitFlags.ModuleChanged );
		}

		private void OnDashActionTriggered( in ButtonActionEventArgs args )
		{
			if ( args.Phase != InputActionPhase.Started ) {
				return;
			}

			TryStartDash();
		}

		private void TryStartDash()
		{
			_timers.StopCooling();

			DashRuntimeChange change = _runtime.TryStartDash( in _settings );

			if ( change.HasAny( PlayerJumpKitFlags.BurnoutEntered ) ) {
				_visuals.SetDashActive( false );
				_timers.StartBurnoutTimer( change.BurnoutLockoutSeconds );
				PublishStatusChanged( change.Flags );
				return;
			}

			if ( change.HasAny( PlayerJumpKitFlags.DashStarted ) ) {
				_visuals.SetDashActive( true );
				_timers.StopBurnoutTimer();
				_timers.StartDashTimer( change.DashDurationSeconds );
				PublishStatusChanged( change.Flags );
				return;
			}

			if ( change.HasAny( PlayerJumpKitFlags.DashRejected ) ) {
				PublishStatusChanged( change.Flags );
			}
		}

		private void OnDashTimedOut()
		{
			DashRuntimeChange change = _runtime.CompleteDash( in _settings );
			if ( !change.HasAny( PlayerJumpKitFlags.DashEnded ) ) {
				return;
			}

			_visuals.SetDashActive( false );

			if ( change.HasAny( PlayerJumpKitFlags.BurnoutEntered ) ) {
				_timers.StartBurnoutTimer( change.BurnoutLockoutSeconds );
			} else if ( change.BurnoutAmount > 0.0f ) {
				_runtime.BeginCoolingDelay();
				_timers.StartCoolingDelay( _settings.BurnoutCooldown );
			}

			PublishStatusChanged( change.Flags );
		}

		private void OnCoolingDelayTimedOut()
		{
			DashRuntimeChange change = _runtime.BeginPassiveCooling();
			if ( change.HasAny( PlayerJumpKitFlags.CoolingStarted ) ) {
				_timers.StartCoolingPulse();
			}

			PublishStatusChanged( change.Flags );
		}

		private void OnCoolingPulse( float delta )
		{
			DashRuntimeChange change = _runtime.TickPassiveCooling( delta, in _settings );
			if ( !change.HasChange ) {
				return;
			}

			if ( change.HasAny( PlayerJumpKitFlags.Recharged ) ) {
				_timers.StopCooling();
			}

			PublishStatusChanged( change.Flags );
		}

		private void OnBurnoutTimedOut()
		{
			DashRuntimeChange change = _runtime.CompleteBurnoutRecharge( in _settings );
			if ( !change.HasChange ) {
				return;
			}

			PublishStatusChanged( change.Flags );
		}

		private JumpKitStatus BuildStatus( PlayerJumpKitFlags transientFlags )
		{
			if ( _runtime == null ) {
				return new JumpKitStatus( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, transientFlags & TRANSIENT_EVENT_FLAGS );
			}

			PlayerJumpKitFlags flags = _runtime.StateFlags | (transientFlags & TRANSIENT_EVENT_FLAGS);
			if ( _runtime.CanStartDash() ) {
				flags |= PlayerJumpKitFlags.CanDash;
			}

			return new JumpKitStatus(
				_runtime.BurnoutAmount,
				_runtime.BurnoutCooldownElapsed,
				_runtime.CurrentBurnoutLockoutSeconds,
				_runtime.CurrentDashDuration,
				_runtime.RemainingDashTime,
				flags
			);
		}

		private void PublishStatusChanged( PlayerJumpKitFlags transientFlags )
		{
			if ( _events == null ) {
				return;
			}

			JumpKitStatus oldStatus = _lastPublishedStatus;
			JumpKitStatus eventStatus = BuildStatus( transientFlags );
			bool hasTransientEvent = (eventStatus.Flags & TRANSIENT_EVENT_FLAGS) != 0;

			if ( hasTransientEvent || !eventStatus.Equals( oldStatus ) ) {
				_events.PublishStatusChanged( Id, in oldStatus, in eventStatus );
			}

			_lastPublishedStatus = BuildStatus( PlayerJumpKitFlags.None );
		}
	}
}
