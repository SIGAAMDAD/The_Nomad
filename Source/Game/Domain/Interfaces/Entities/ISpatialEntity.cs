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

using System.Numerics;
using Nomad.Game.Domain.Data.Entities;

namespace Nomad.Game.Domain.Interfaces.Entities
{
	public interface ISpatialEntity : IEntityBase
	{
		Vector2 Position { get; }
		float RotationRadians { get; }
		Vector2 Scale { get; }
		Vector2 Forward { get; }

		uint SpatialRevision { get; }

		void SetPosition( Vector2 position );
		void SetRotation( float radians );
		void SetScale( Vector2 scale );

		void SetTransform( Vector2 position, float rotationRadians, Vector2 scale );

		bool ContainsPoint( Vector2 point );
		bool IntersectsBounds( in EntityBounds bounds );
	};
};
