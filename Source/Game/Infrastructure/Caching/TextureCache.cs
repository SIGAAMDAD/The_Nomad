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

using Nomad.Core.Engine.Globals;
using Nomad.ResourceCache;
using System;
using Nomad.Core.Engine.Assets;
using Nomad.Events.Globals;
using Nomad.Logger.Globals;

namespace Nomad.Game.Infrastructure.Caching {
	/*
	===================================================================================
	
	TextureCache
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class TextureCache {
		public static BaseCache<ITexture, string> Instance => _textureCache.Value;
		private static readonly Lazy<BaseCache<ITexture, string>> _textureCache = new Lazy<BaseCache<ITexture, string>>( Create, true );

		private static BaseCache<ITexture, string> Create() {
			return new BaseCache<ITexture, string>(
				Logging.Instance,
				GameEventRegistry.Instance,
				EngineService.GetResourceLoader()
			);
		}
	};
};
