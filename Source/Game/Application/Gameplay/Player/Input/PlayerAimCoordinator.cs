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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Core.Numerics;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Application.Gameplay.Player.Input
{
	internal sealed class PlayerAimCoordinator : IAimReader, IAimWriter
	{
		private const float EPSILON = 0.0001f;

		public float AimAngleRadians => _aimAngleRadians;
		public float AimAngleDegrees => AngleMath.ToDegrees( _aimAngleRadians );
		public Vector2 AimDirection => _aimDirection;

		private float _aimAngleRadians = 0.0f;
		private Vector2 _aimDirection = Vector2.Zero;

		private readonly PlayerId _playerId;

		private bool _isDisposed = false;

		public IGameEvent<AimAngleChangedEventArgs> AimAngleChanged => _aimAngleChanged;
		private readonly IGameEvent<AimAngleChangedEventArgs> _aimAngleChanged = null;

		/*
		===============
		PlayerAimCoordinator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		public PlayerAimCoordinator( PlayerId playerId, IGameEventRegistryService eventFactory )
		{
			playerId.ThrowIfInvalid( nameof( PlayerAimCoordinator ) );

			_playerId = playerId;

			_aimAngleChanged = eventFactory.GetEvent<AimAngleChangedEventArgs>(
				AimAngleChangedEventArgs.Name,
				AimAngleChangedEventArgs.NameSpace
			);
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_aimAngleChanged?.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		SetAimDirection
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="serverTick"></param>
		public void SetAimDirection( Vector2 direction, uint serverTick = 0 )
		{
			if ( direction.LengthSquared() <= EPSILON ) {
				return;
			}

			Vector2 normalized = Vector2.Normalize( direction );
			float angle = MathF.Atan2( normalized.Y, normalized.X );

			if ( Vector2.DistanceSquared( _aimDirection, normalized ) <= EPSILON ) {
				return;
			}

			Vector2 oldDirection = _aimDirection;
			float oldAngle = _aimAngleRadians;

			_aimDirection = normalized;
			_aimAngleRadians = angle;

			_aimAngleChanged.Publish(
				new AimAngleChangedEventArgs(
					_playerId,
					oldAngle,
					_aimAngleRadians,
					oldDirection,
					_aimDirection,
					serverTick
				)
			);
		}
	};
};
