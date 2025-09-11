/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using System;
using Godot;
using ResourceCache;
using Menus;
using System.Runtime.CompilerServices;

namespace PlayerSystem.Upgrades {
	/*
	===================================================================================
	
	DashKit
	
	===================================================================================
	*/

	public sealed partial class DashKit : GodotObject, IDashKit {
		private static readonly float DASH_TIMER_BASE = 0.3f;

		public IDashModule? Module { get; private set; } = new DefaultModule();

		private float DashBurnout = 0.0f;
		private float DashTimer = DASH_TIMER_BASE;
		private float DashBurnoutCooldown = 0.0f;

		private Timer? DashTime;
		private Timer? DashBurnoutCooldownTimer;

		private Player? Owner;

		private AudioStreamPlayer2D? AudioChannel;

		[Signal]
		public delegate void DashEndEventHandler();
		[Signal]
		public delegate void DashStartEventHandler();
		[Signal]
		public delegate void DashBurnedOutEventHandler();
		[Signal]
		public delegate void DashBurnoutChangedEventHandler( float amount );

		public DashKit( Player? owner ) {
			ArgumentNullException.ThrowIfNull( owner );

			Owner = owner;

			DashTime = new Timer() {
				Name = nameof( DashTime ),
				WaitTime = DashTimer,
				OneShot = true
			};
			GameEventBus.ConnectSignal( DashTime, Timer.SignalName.Timeout, this, OnDashTimeTimeout );
			owner.AddChild( DashTime );

			DashBurnoutCooldownTimer = new Timer() {
				Name = nameof( DashBurnoutCooldownTimer ),
				WaitTime = 2.5f,
				OneShot = true
			};
			GameEventBus.ConnectSignal( DashBurnoutCooldownTimer, Timer.SignalName.Timeout, this, OnDashBurnoutCooldownTimerTimeout );
			owner.AddChild( DashBurnoutCooldownTimer );

			AudioChannel = new AudioStreamPlayer2D() {
				Name = nameof( AudioChannel ),
				VolumeDb = SettingsData.GetEffectsVolumeLinear()
			};
			owner.AddChild( AudioChannel );
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
		public void SetModule( IDashModule? module ) {
			ArgumentNullException.ThrowIfNull( module );
			Module = module;
		}

		/*
		===============
		OnDash
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void OnDash() {
			if ( DashBurnout >= 1.0f ) {
				AudioChannel.Stream = SoundCache.StreamCache[ SoundEffect.DashExplosion ];
				AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );

				DashBurnoutCooldownTimer.Start();

				EmitSignalDashBurnedOut();
				EmitSignalDashBurnoutChanged( 0.0f );

				return;
			}

			DashTime.WaitTime = DashTimer;
			DashTime.Start();

			AudioChannel.PitchScale = 1.0f + DashBurnout;
			AudioChannel.Stream = SoundCache.GetEffectRange( SoundEffect.DashBurn0, 2 );
			AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
			EmitSignalDashStart();

			Module?.ApplyEffect( this );

			DashBurnout += 0.30f;
			if ( DashTimer >= 0.10f ) {
				DashTimer -= 0.05f;
			}
			DashBurnoutCooldown = 0.0f;

			EmitSignalDashBurnoutChanged( DashBurnout );
		}

		/*
		===============
		CanDash
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool CanDash() {
			return DashBurnoutCooldownTimer.TimeLeft == 0.0f;
		}

		/*
		===============
		OnDashBurnoutCooldownTimerTimeout
		===============
		*/
		private void OnDashBurnoutCooldownTimerTimeout() {
			DashBurnout = 0.0f;
			DashTimer = Module.DashDuration;
			DashBurnoutCooldown = 0.0f;

			AudioChannel.Stream = AudioCache.GetStream( "res://sounds/player/dash_chargeup.ogg" );
			AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
		}

		/*
		===============
		OnDashTimeTimeout
		===============
		*/
		private void OnDashTimeTimeout() {
			EmitSignalDashEnd();
		}

		/*
		===============
		Update
		===============
		*/
		public void Update( float delta ) {
			UpdateBurnoutCooldown( delta );
		}

		/*
		===============
		UpdateBurnoutCooldown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		private void UpdateBurnoutCooldown( float delta ) {
			if ( DashBurnout == 0.0f ) {
				return;
			}

			DashBurnoutCooldown += delta;
			if ( DashBurnoutCooldown > Module.BurnoutCooldown ) {
				DashBurnout = Mathf.Clamp( DashBurnout - ( 0.10f * delta ), 0.0f, DashBurnout );
				DashTimer += 0.05f * delta;
				EmitSignalDashBurnoutChanged( DashBurnout );
			}
		}
	};
};