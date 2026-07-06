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
using Nomad.Audio.Interfaces;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Events.Player.JumpKit;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Audio;
using Nomad.Game.Sdk.Player.JumpKit;
using Nomad.Core.Events;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Gameplay.Player
{
	/*
	===================================================================================

	PlayerAudioService

	===================================================================================
	*/
	/// <summary>
	/// Handles player foley sound effects as well as the ambient sounds that come from
	/// anywhere in the player subsystem.
	/// </summary>

	internal sealed class PlayerAudioService : IDisposable
	{
		public readonly IPlayerFlagService _flagService;
		private readonly PlayerPrefab _prefab;

		private readonly IAudioEmitter _walkEffectEmitter;
		private readonly IAudioEmitter _dashEffectEmitter;
		private readonly IAudioEmitter _slideEffectEmitter;

		/// <summary>
		/// An emitter meant for sounds only the local player can hear, such as bullet time, low health, heavy breathing, etc.
		/// </summary>
		private readonly IAudioEmitter _internalEffectsEmitter;
		private readonly IListenerService _listenerService;

		private readonly IDisposable _flagsChanged = null;
		private readonly IDisposable _jumpKitStatusChanged = null;
		private readonly IDisposable _weaponSlotChanged = null;

		private bool _isDisposed = false;

		/*
		===============
		PlayerAudioService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public PlayerAudioService(
			PlayerPrefab prefab,
			IPlayerFlagService flagService,
			IGameEventRegistryService eventFactory
		)
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_flagService = flagService ?? throw new ArgumentNullException( nameof( flagService ) );

			var emitterFactory = ServiceLocator.GetService<IEmitterFactory>();
			_walkEffectEmitter = emitterFactory.CreateEmitter( "SoundCategory:Foley" );
			_dashEffectEmitter = emitterFactory.CreateEmitter( "SoundCategory:Foley" );
			_slideEffectEmitter = emitterFactory.CreateEmitter( "SoundCategory:Foley" );
			_internalEffectsEmitter = emitterFactory.CreateEmitter( "SoundCategory:FX" );

			_listenerService = ServiceLocator.GetService<IListenerService>();

			_flagsChanged = _flagService.FlagsChanged.Subscribe( OnFlagsChanged );

			_jumpKitStatusChanged = eventFactory
				.GetEvent<PlayerJumpKitStatusChangedEventArgs>(
					PlayerJumpKitStatusChangedEventArgs.Name,
					PlayerJumpKitStatusChangedEventArgs.NameSpace
				)
				.Subscribe( OnJumpKitStatusChanged );

			_weaponSlotChanged = eventFactory
				.GetEvent<WeaponSlotChangedEventArgs>(
					WeaponSlotChangedEventArgs.Name,
					WeaponSlotChangedEventArgs.NameSpace
				)
				.Subscribe( OnWeaponSlotChanged );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_flagsChanged.Dispose();
			_jumpKitStatusChanged.Dispose();
			_weaponSlotChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnProcessFrame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public void OnProcessFrame( float delta )
		{
			var position = _prefab.GlobalPosition.ToSystem();
			if ( _listenerService.ActiveListener != position ) {
				_listenerService.SetListenerPosition( 0, position );
			}

			_walkEffectEmitter.Position = position;
			_dashEffectEmitter.Position = position;
			_slideEffectEmitter.Position = position;
		}

		/*
		===============
		OnJumpKitStatusChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnJumpKitStatusChanged( in PlayerJumpKitStatusChangedEventArgs args )
		{
			if ( !args.PlayerId.Equals( _prefab.PeerId ) ) {
				return;
			}

			PlayerJumpKitFlags flags = args.NewStatus.Flags;

			if ( (flags & PlayerJumpKitFlags.DashStarted) != 0 ) {
				_dashEffectEmitter.Pitch = 1.0f + args.NewStatus.BurnoutAmount;
				_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashActivate ).Path );
			}

			if ( (flags & PlayerJumpKitFlags.BurnoutEntered) != 0 ) {
				_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashBurnout ).Path );
			}

			if ( (flags & PlayerJumpKitFlags.Recharged) != 0 ) {
				_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashRecharge ).Path );
			}
		}

		/*
		===============
		OnFlagsChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnFlagsChanged( in PlayerFlagsChangedEventArgs args )
		{
			if ( args.NewFlags.HasFlag( PlayerFlags.Sliding ) ) {
				_slideEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_slideEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFoleyPlayerSlide ).Path );
			}
			if ( args.NewFlags.HasFlag( PlayerFlags.BulletTime ) ) {
				_internalEffectsEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerSlowMoStart ).Path );
			} else if ( args.OldFlags.HasFlag( PlayerFlags.BulletTime ) ) {
				_internalEffectsEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerSlowMoEnd ).Path );
			}
		}

		private void OnWeaponSlotChanged( in WeaponSlotChangedEventArgs args )
		{
			_internalEffectsEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerChangeWeapon ).Path );
		}

		/*
		===============
		OnLegAnimationLooped
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnLegAnimationLooped()
		{
			if ( _prefab.Velocity != Vector3.Zero ) {
				_walkEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_walkEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFoleyPlayerWalkSand ).Path );
			}
		}
	};
};
