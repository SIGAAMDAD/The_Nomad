/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Nomad.ResourceCache;
using Nomad.Core.Engine.Globals;
using System;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Logger.Globals;
using Nomad.Events.Globals;

namespace Game.Infrastructure.Caching {
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
