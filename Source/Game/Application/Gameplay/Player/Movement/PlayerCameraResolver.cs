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

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerCameraResolver

	===================================================================================
	*/

	internal static class PlayerCameraResolver
	{
		private static readonly string[] FallbackPaths = {
			"PlayerCamera3D",
			"Camera3D",
			"CameraRig/Camera3D",
			"CameraPivot/PlayerCamera3D",
			"CameraPivot/Camera3D",
			"VisualRoot/PlayerCamera3D",
			"VisualRoot/Camera3D"
		};

		public static PlayerCamera3D Resolve( PlayerPrefab prefab, string configuredPath )
		{
			if ( prefab == null ) {
				throw new ArgumentNullException( nameof( prefab ) );
			}

			if ( !string.IsNullOrWhiteSpace( configuredPath ) ) {
				PlayerCamera3D configured = prefab.GetNodeOrNull<PlayerCamera3D>( configuredPath );
				if ( configured != null ) {
					return configured;
				}
			}

			for ( int i = 0; i < FallbackPaths.Length; i++ ) {
				PlayerCamera3D camera = prefab.GetNodeOrNull<PlayerCamera3D>( FallbackPaths[i] );
				if ( camera != null ) {
					return camera;
				}
			}

			PlayerCamera3D recursive = FindRecursive( prefab );
			if ( recursive != null ) {
				return recursive;
			}

			throw new InvalidOperationException( "PlayerMovementController requires a PlayerCamera3D child. Set CameraNodePath on the movement controller or add a PlayerCamera3D under the player prefab." );
		}

		private static PlayerCamera3D FindRecursive( Node node )
		{
			for ( int i = 0; i < node.GetChildCount(); i++ ) {
				Node child = node.GetChild( i );
				if ( child is PlayerCamera3D camera ) {
					return camera;
				}

				PlayerCamera3D nested = FindRecursive( child );
				if ( nested != null ) {
					return nested;
				}
			}

			return null;
		}
	}
}
