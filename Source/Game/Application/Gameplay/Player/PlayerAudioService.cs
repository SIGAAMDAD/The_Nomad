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
using System;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Audio;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerAudioService
	
	===================================================================================
	*/
	/// <summary>
	/// Handles player foley sound effects as well as the ambient sounds that come from
	/// anywhere in the player subsystem.
	/// </summary>
	
	internal sealed class PlayerAudioService : NomadBehaviour {
		public Guid Id { get; set; }
		private PlayerPrefab _prefab;

		private readonly IAudioEmitter _walkEffectEmitter;
		private readonly IAudioEmitter _dashEffectEmitter;
		private readonly IAudioEmitter _foleyEmitter;
		private readonly IListenerService _listenerService;

		public PlayerAudioService() {
			_walkEffectEmitter = ServiceLocator.GetService<IEmitterFactory>().CreateEmitter( "SoundCategory:Foley" );
			_dashEffectEmitter = ServiceLocator.GetService<IEmitterFactory>().CreateEmitter( "SoundCategory:Foley" );
			_foleyEmitter = ServiceLocator.GetService<IEmitterFactory>().CreateEmitter( "SoundCategory:Foley" );
			_listenerService = ServiceLocator.GetService<IListenerService>();
		}

		public override void OnInit() {
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();

			var legAnimator = _prefab.FindChild<EngineAnimatedSprite2D>( "LegAnimator" );
			legAnimator.AnimationLooped.Subscribe( OnLegAnimationLooped );

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDashStartEventArgs>( $"{Id}:{EventNames.PLAYER_DASH_STARTED}", EventNames.NAMESPACE )
				.Subscribe( OnDashStarted );
			
			eventFactory
				.GetEvent<PlayerDashBurnoutEventArgs>( $"{Id}:{EventNames.PLAYER_DASH_BURNOUT}", EventNames.NAMESPACE )
				.Subscribe( OnDashBurnout );
			
			eventFactory
				.GetEvent<PlayerDashRechargedEventArgs>( $"{Id}:{EventNames.PLAYER_DASH_RECHARGED}", EventNames.NAMESPACE )
				.Subscribe( OnDashRecharged );
		}

		public override void OnUpdate( float delta ) {
			base.OnUpdate( delta );

			var position = _prefab.GlobalPosition.ToSystem();
			if ( _listenerService.ActiveListener != position ) {
				_listenerService.SetListenerPosition( 0, position );
			}
			_walkEffectEmitter.Position = position;
			_dashEffectEmitter.Position = position;
		}

		private void OnDashStarted( in PlayerDashStartEventArgs args ) {
			_dashEffectEmitter.Pitch = 1.0f + args.BurnoutAmount;
			_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
			_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashActivate ).Path );
		}

		private void OnDashBurnout( in PlayerDashBurnoutEventArgs args ) {
			_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
			_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashBurnout ).Path );
		}

		private void OnDashRecharged( in PlayerDashRechargedEventArgs args ) {
			_dashEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
			_dashEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFXPlayerDashRecharge ).Path );
		}

		private void OnLegAnimationLooped( in EmptyEventArgs args ) {
			if ( _prefab.Velocity != Vector2.Zero ) {
				_walkEffectEmitter.Position = _prefab.GlobalPosition.ToSystem();
				_walkEffectEmitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.SoundEffectsFoleyPlayerWalkSand ).Path );
			}
		}
	};
};