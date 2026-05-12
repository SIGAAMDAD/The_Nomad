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
using Nomad.Core.Engine.Services;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Input;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerBulletTime

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerBulletTime : NomadBehaviour
	{
		public IPlayerResourceService ResourceService { get; set; }
		public IPlayerDerivedStatService DerivedStatService { get; set; }
		public IPlayerFlagService FlagService { get; set; }
		public ITimeService TimeService { get; set; }

		private float _maxRage = 0.0f;
		private float _bulletTimeRageDepletionRate = 2.5f;
		private bool _isActive = false;

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

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"BulletTime:{ButtonActionEventArgs.Name}", ButtonActionEventArgs.NameSpace )
				.Subscribe( OnBulletTimeTriggered );

			DerivedStatService.DerivedStatChanged.Subscribe( OnDerivedStatChanged );
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

			if ( _isActive ) {
				float value = ResourceService.GetValue( PlayerResourceType.Rage );
				value = Math.Clamp( value - (_bulletTimeRageDepletionRate * delta), 0.0f, DerivedStatService.GetValue( DerivedStatType.EffectiveRageMax ) );
				ResourceService.SetValue( PlayerResourceType.Rage, value );
				if ( value == 0.0f ) {
					Toggle( false );
				}
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

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"BulletTime:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Unsubscribe( OnBulletTimeTriggered );

			DerivedStatService.DerivedStatChanged.Unsubscribe( OnDerivedStatChanged );
		}

		/*
		===============
		ValidateState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private bool ValidateState()
		{
			return ResourceService.GetValue( PlayerResourceType.Rage ) > 0.0f;
		}

		/*
		===============
		Toggle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="value"></param>
		private void Toggle( bool value )
		{
			if ( !ValidateState() ) {
				return;
			}
			_isActive = value;
			if ( !_isActive ) {
				FlagService.RemoveFlags( PlayerFlags.BulletTime );
				TimeService.SetTimeScale( 0.5f );
			} else {
				FlagService.AddFlags( PlayerFlags.BulletTime );
				TimeService.SetTimeScale( 1.0f );
			}
		}

		/*
		===============
		OnBulletTimeTriggered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnBulletTimeTriggered( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				Toggle( !_isActive );
			}
		}

		/*
		===============
		OnDerivedStatChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDerivedStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
			if ( args.StatId == DerivedStatType.EffectiveRageMax ) {
				_maxRage = args.NewValue;
			}
		}
	};
};
