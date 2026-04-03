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

using Nomad.ResourceCache;
using Nomad.Core.Engine.Globals;
using System;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Logger.Globals;
using Nomad.Events.Globals;

namespace Nomad.Game.Infrastructure.Caching {
	/*
	===================================================================================
	
	SceneCache
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class SceneCache {
		public static BaseCache<IScene, string> Instance => _sceneCache.Value;
		private static readonly Lazy<BaseCache<IScene, string>> _sceneCache = new Lazy<BaseCache<IScene, string>>( Create, true );

		private static BaseCache<IScene, string> Create() {
			return new BaseCache<IScene, string>(
				Logging.Instance,
				GameEventRegistry.Instance,
				EngineService.GetResourceLoader()
			);
		}
	};
};
