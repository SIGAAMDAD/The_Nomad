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

using Nomad.Core.Abstractions;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Sdk.Gameplay;

namespace Nomad.Game.Infrastructure.Gameplay
{
	internal sealed class GameplayBootstrapper : IBootstrapper
	{
		private IWorldContentCache? _worldContentCache;

		public void Initialize( IServiceRegistry registry, IServiceLocator locator )
		{
			var fileSystem = locator.GetService<IFileSystem>();
			var logger = locator.GetService<ILoggerService>();
			var eventFactory = locator.GetService<IGameEventRegistryService>();

			_worldContentCache = new WorldContentCache(
				fileSystem,
				logger,
				eventFactory
			);

			registry.AddSingleton( _worldContentCache );
		}

		public void Shutdown()
		{
			_worldContentCache?.Dispose();
		}
	};
};
