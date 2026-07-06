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
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding;
using Nomad.Modding.CVars;
using Nomad.Modding.Events;
using Nomad.Modding.FileSystem;

namespace Nomad.Game.Content.Mods
{
	/*
	===================================================================================

	ModuleContext

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class ModuleContext : IModuleContext, IDisposable
	{
		public string ModuleId => Module.Id;
		public ModuleInfo Module { get; }
		public string RuntimeApiVersion { get; }

		public IWorldContentCache ContentCache { get; }
		public INomadBehaviorRegistry Behaviors { get; }
		public IModEventRegistry Events { get; }
		public IModLogger Logger { get; }
		public IModFileSystem FileSystem { get; }
		public IModCVarSystem CVarSystem { get; }

		private bool _isDisposed = false;

		public ModuleContext(
			ModuleManifest manifest,
			string runtimeApiVersion,
			IWorldContentCache contentCache,
			INomadBehaviorRegistry behaviors,
			IModEventRegistry events,
			IModLogger logger,
			IModFileSystem fileSystem,
			IModCVarSystem cvarSystem
		)
		{
			ArgumentNullException.ThrowIfNull( manifest );

			Module = ModuleInfo.FromManifest( manifest );
			RuntimeApiVersion = runtimeApiVersion;
			ContentCache = contentCache ?? throw new ArgumentNullException( nameof( contentCache ) );
			Behaviors = new ModuleScopedBehaviorRegistry(
				behaviors ?? throw new ArgumentNullException( nameof( behaviors ) ),
				this
			);
			Events = events ?? throw new ArgumentNullException( nameof( events ) );
			Logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
			FileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );
			CVarSystem = cvarSystem ?? throw new ArgumentNullException( nameof( cvarSystem ) );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			if ( CVarSystem is IDisposable cvarSystem ) {
				cvarSystem.Dispose();
			}

			if ( Events is IDisposable events ) {
				events.Dispose();
			}

			if ( Logger is IDisposable logger ) {
				logger.Dispose();
			}

			_isDisposed = true;
			GC.SuppressFinalize( this );
		}
	};
};
