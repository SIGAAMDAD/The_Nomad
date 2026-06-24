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

namespace Nomad.Game.Application.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunSurfaceClassifier

	===================================================================================
	*/
	/// <summary>
	/// Converts Godot raycast hits into wall-running surface candidates. This is the only
	/// wall-running type that understands node-group authoring rules.
	/// </summary>

	internal sealed class WallRunSurfaceClassifier
	{
		private readonly WallRunSettings _settings;

		public WallRunSurfaceClassifier( WallRunSettings settings )
		{
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
		}

		public bool TryClassify(
			Godot.Collections.Dictionary hit,
			Vector3 origin,
			int priorityBias,
			out WallSurfaceCandidate candidate
		)
		{
			candidate = default;

			if ( hit.Count == 0 ) {
				return false;
			}

			Vector3 normal = WallRunMath.SafeNormalized( hit["normal"].AsVector3(), Vector3.Zero );
			if ( normal.LengthSquared() <= WallRunMath.Epsilon ) {
				return false;
			}

			Node collider = hit["collider"].AsGodotObject() as Node;
			Node3D collider3D = collider as Node3D;
			float verticality = 1.0f - MathF.Abs( normal.Dot( Vector3.Up ) );

			bool blocked = collider != null && collider.IsInGroup( WallRunGroups.Blocked );
			bool explicitRunnable = collider != null && collider.IsInGroup( WallRunGroups.Runnable );
			bool isRunnable = !blocked && (explicitRunnable || verticality >= _settings.MinimumSurfaceVerticality);
			if ( !isRunnable ) {
				return false;
			}

			bool hasSpecificMode = collider != null && WallRunGroups.HasSpecificModeGroup( collider );
			bool allowsHorizontal = AllowsMode( collider, hasSpecificMode, WallRunGroups.Horizontal, WallRunGroups.NoHorizontal );
			bool allowsUp = AllowsMode( collider, hasSpecificMode, WallRunGroups.Up, WallRunGroups.NoUp );
			bool allowsDown = AllowsMode( collider, hasSpecificMode, WallRunGroups.Down, WallRunGroups.NoDown );

			int priority = priorityBias;
			if ( explicitRunnable ) {
				priority += 2;
			}
			if ( hasSpecificMode ) {
				priority += 1;
			}

			Vector3 point = hit["position"].AsVector3();
			Rid colliderRid = hit.ContainsKey( "rid" ) ? hit["rid"].AsRid() : default;

			candidate = new WallSurfaceCandidate(
				collider3D,
				colliderRid,
				point,
				normal,
				verticality,
				origin.DistanceTo( point ),
				true,
				allowsHorizontal,
				allowsUp,
				allowsDown,
				priority
			);

			return true;
		}

		private static bool AllowsMode( Node collider, bool hasSpecificMode, StringName positiveGroup, StringName negativeGroup )
		{
			if ( collider == null ) {
				return true;
			}

			if ( collider.IsInGroup( negativeGroup ) ) {
				return false;
			}

			return !hasSpecificMode || collider.IsInGroup( positiveGroup ) || collider.IsInGroup( WallRunGroups.Runnable );
		}
	}
}
