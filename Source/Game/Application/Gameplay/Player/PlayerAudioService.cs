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

using Godot;
using Nomad.Audio.Interfaces;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Audio;

namespace Nomad.Game.Application.Gameplay.Player
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

	internal sealed class PlayerAudioService : NomadBehaviour
	{
		public PlayerId Id { get; set; }
		public IPlayerFlagService FlagService { get; set; }
		private PlayerPrefab _prefab;

		private readonly IAudioEmitter _walkEffectEmitter;
		private readonly IAudioEmitter _dashEffectEmitter;
		private readonly IAudioEmitter _slideEffectEmitter;

		/// <summary>
		/// An emitter meant for sounds only the local player can hear, such as bullet time, low health, heavy breathing, etc.
		/// </summary>
		private readonly IAudioEmitter _internalEffectsEmitter;
		private readonly IListenerService _listenerService;

		/*
		===============
		PlayerAudioService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public PlayerAudioService()
		{
			var emitterFactory = ServiceLocator.GetService<IEmitterFactory>();
			_walkEffectEmitter = emitterFactory.CreateEmitter( "SoundCategory:Foley" );
			_dashEffectEmitter = emitterFactory.CreateEmitter( "SoundCategory:Foley" );
			_slideEffectEmitter = emitterFactory.CreateEmitter( "SoundCategory:Foley" );
			_internalEffectsEmitter = emitterFactory.CreateEmitter( "SoundCategory:FX" );

			_listenerService = ServiceLocator.GetService<IListenerService>();
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

			var legAnimator = _prefab.GetNode<AnimatedSprite2D>( "Animations/LegAnimator" );
			legAnimator.AnimationLooped += OnLegAnimationLooped;

			FlagService.FlagsChanged.Subscribe( OnFlagsChanged );

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDashStartEventArgs>(
					PlayerDashStartEventArgs.Name,
					PlayerDashStartEventArgs.NameSpace
				)
				.Subscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashBurnoutEventArgs>(
					PlayerDashBurnoutEventArgs.Name,
					PlayerDashBurnoutEventArgs.NameSpace
				)
				.Subscribe( OnDashBurnout );

			eventFactory
				.GetEvent<PlayerDashRechargedEventArgs>(
					PlayerDashRechargedEventArgs.Name,
					PlayerDashRechargedEventArgs.NameSpace
				)
				.Subscribe( OnDashRecharged );

			eventFactory
				.GetEvent<WeaponSlotChangedEventArgs>(
					WeaponSlotChangedEventArgs.Name,
					WeaponSlotChangedEventArgs.NameSpace
				)
				.Subscribe( OnWeaponSlotChanged );
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
		OnDashStarted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDashStarted( in PlayerDashStartEventArgs args )
		{
			_dashEffectEmitter.Pitch = 1.0f + args.BurnoutAmount;
			_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
			_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashActivate ).Path );
		}

		/*
		===============
		OnDashBurnout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDashBurnout( in PlayerDashBurnoutEventArgs args )
		{
			_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
			_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashBurnout ).Path );
		}

		/*
		===============
		OnDashRecharged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDashRecharged( in PlayerDashRechargedEventArgs args )
		{
			_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
			_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashRecharge ).Path );
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
			if ( _prefab.Velocity != Vector2.Zero ) {
				_walkEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_walkEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFoleyPlayerWalkSand ).Path );
			}
		}
	};
};
