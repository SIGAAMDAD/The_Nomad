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
using Nomad.Events.Globals;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player.Stats;
using Nomad.Game.Sdk.Player.State;
using Nomad.Input;
using Nomad.Input.ValueObjects;
using Nomad.Game.Prefabs;
using Nomad.Core.Events;
using Nomad.Core.Compatibility.Guards;
using System.Runtime.CompilerServices;
using Nomad.Game.Gameplay.Player.Stats;
using Nomad.Game.Gameplay.Player.State;

namespace Nomad.Game.Gameplay.Player
{
	/*
	===================================================================================

	PlayerBulletTime

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerBulletTime : IDisposable
	{
		private readonly PlayerResourceService _resourceService;
		private readonly PlayerDerivedStatService _derivedStatService;
		private readonly PlayerFlagService _flagService;
		private readonly ITimeService _timeService;

		private readonly IDisposable _bulletTimeAction = null;
		private readonly IDisposable _derivedStatChanged = null;

		private float _bulletTimeRageDepletionRate = 0.0f;

		private bool _isActive = false;

		private bool _isDisposed = false;

		public PlayerBulletTime(
			ITimeService timeService,
			PlayerResourceService resourceService,
			PlayerDerivedStatService derivedStatService,
			PlayerFlagService flagService,
			IGameEventRegistryService eventFactory
		)
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_timeService = timeService ?? throw new ArgumentNullException( nameof( timeService ) );
			_resourceService = resourceService ?? throw new ArgumentNullException( nameof( resourceService ) );
			_derivedStatService = derivedStatService ?? throw new ArgumentNullException( nameof( derivedStatService ) );
			_flagService = flagService ?? throw new ArgumentNullException( nameof( flagService ) );

			_derivedStatChanged = derivedStatService.DerivedStatChanged.Subscribe( OnDerivedStatChanged );
			_bulletTimeRageDepletionRate = _derivedStatService.GetValue( DerivedStatType.BulletTimeDrainRate );

			_bulletTimeAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"BulletTime:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnBulletTimeTriggered );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_bulletTimeAction.Dispose();
			_derivedStatChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void OnProcessFrame( float delta )
		{
			if ( !_isActive ) {
				return;
			}

			float value = _resourceService.GetValue( PlayerResourceType.Rage );
			value = Math.Clamp( value - (_bulletTimeRageDepletionRate * delta), 0.0f, _resourceService.GetMaxValue( PlayerResourceType.Rage ) );
			_resourceService.SetValue( PlayerResourceType.Rage, value );
			if ( value == 0.0f ) {
				Toggle( false );
			}
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool ValidateState()
		{
			return _resourceService.GetValue( PlayerResourceType.Rage ) > 0.0f;
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
			if ( value && !ValidateState() ) {
				return;
			}

			_isActive = value;

			if ( !_isActive ) {
				_flagService.RemoveFlags( PlayerFlags.BulletTime );
				_timeService.SetTimeScale( 1.0f );
			} else {
				_flagService.AddFlags( PlayerFlags.BulletTime );
				_timeService.SetTimeScale( 0.5f );
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
			if ( args.StatId == DerivedStatType.BulletTimeDrainRate ) {
				_bulletTimeRageDepletionRate = args.NewValue;
			}
		}
	};
};
