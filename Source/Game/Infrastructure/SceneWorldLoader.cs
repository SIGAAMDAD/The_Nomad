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
using System.Collections.Generic;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Gameplay;

namespace Nomad.Game.Infrastructure
{
	/*
	===================================================================================
	
	SceneWorldLoader
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class SceneWorldLoader : IWorldLoader
	{
		private static readonly Dictionary<InternString, string> _scenePaths = new() {
			[new( "world.single.default" )] = "Assets/Prefabs/SingleWorld/SingleWorld.tscn",
			[new( "world.network.default" )] = "Assets/Prefabs/NetworkWorld/NetworkWorld.tscn",
		};

		private readonly ISceneManager _sceneManager;

		public SceneWorldLoader( ISceneManager sceneManager )
		{
			_sceneManager = sceneManager ?? throw new ArgumentNullException( nameof( sceneManager ) );
		}

		public IWorldHandle Load( string worldId )
		{
			var id = new InternString( worldId );
			if ( !_scenePaths.TryGetValue( id, out string? scenePath ) ) {
				throw new InvalidOperationException( $"No scene path mapped for world '{worldId}'." );
			}

			_sceneManager.LoadScene( scenePath );

			IWorldHandle handle = new LoadedWorldHandle(
				id: Guid.NewGuid(),
				worldId: id
			);

			return handle;
		}

		private sealed class LoadedWorldHandle : IWorldHandle
		{
			public Guid Id { get; }
			public string WorldId { get; }

			public LoadedWorldHandle( Guid id, InternString worldId )
			{
				Id = id;
				WorldId = worldId;
			}
		}
	};
};
