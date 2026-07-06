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
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourRayValidator : IDisposable
	{
		private readonly PlayerPrefab _prefab;
		private readonly PlayerParkourSettings _settings;
		private readonly PhysicsRayQueryParameters3D _rayQuery = new PhysicsRayQueryParameters3D();
		private readonly Godot.Collections.Array<Rid> _rayExclude = new Godot.Collections.Array<Rid>();
		private bool _isDisposed = false;

		public PlayerParkourRayValidator( PlayerPrefab prefab, PlayerParkourSettings settings )
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );

			_rayExclude.Add( prefab.GetRid() );
			_rayQuery.CollideWithAreas = false;
			_rayQuery.CollideWithBodies = true;
			_rayQuery.Exclude = _rayExclude;
		}

		public bool ValidateLine( Vector3 from, Vector3 to )
		{
			_rayQuery.From = from;
			_rayQuery.To = to;
			_rayQuery.CollisionMask = _settings.ValidationMask;
			_rayQuery.Exclude = _rayExclude;

			Godot.Collections.Dictionary hit = _prefab.GetWorld3D().DirectSpaceState.IntersectRay( _rayQuery );
			return hit.Count == 0;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_rayQuery.Dispose();
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};
