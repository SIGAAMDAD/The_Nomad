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

namespace Nomad.Game.Infrastructure.Streaming
{
	internal enum RegionSceneLoadResult : byte
	{
		None,
		Requested,
		Pending,
		Loaded,
		Failed
	};

	internal sealed class RegionSceneLoader
	{
		public RegionSceneLoadResult RequestOrPoll( string path, out PackedScene scene )
		{
			scene = null;

			if ( string.IsNullOrEmpty( path ) ) {
				return RegionSceneLoadResult.Failed;
			}

			ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus( path );

			if ( status == ResourceLoader.ThreadLoadStatus.InvalidResource ) {
				Error err = ResourceLoader.LoadThreadedRequest( path );

				if ( err != Error.Ok && err != Error.AlreadyInUse ) {
					GD.PushWarning( $"Failed to request streamed region scene '{path}': {err}" );
					return RegionSceneLoadResult.Failed;
				}

				return RegionSceneLoadResult.Requested;
			}

			if ( status == ResourceLoader.ThreadLoadStatus.InProgress ) {
				return RegionSceneLoadResult.Pending;
			}

			if ( status == ResourceLoader.ThreadLoadStatus.Failed ) {
				GD.PushWarning( $"Threaded load failed for streamed region scene '{path}'." );
				return RegionSceneLoadResult.Failed;
			}

			if ( status == ResourceLoader.ThreadLoadStatus.Loaded ) {
				scene = ResourceLoader.LoadThreadedGet( path ) as PackedScene;

				if ( scene == null ) {
					GD.PushWarning( $"Streamed region resource is not a PackedScene: '{path}'." );
					return RegionSceneLoadResult.Failed;
				}

				return RegionSceneLoadResult.Loaded;
			}

			return RegionSceneLoadResult.Pending;
		}
	};
};
