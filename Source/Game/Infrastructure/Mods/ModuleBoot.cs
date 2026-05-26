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
using System.IO;
using Nomad.Core.FileSystem;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	internal sealed class ModuleBoot
	{
		private readonly List<DiscoveredModule> _modules = new();

		public ModuleBoot( IFileSystem fileSystem )
		{
			string executableDir = Environment.CurrentDirectory;

			var scanner = new ModuleScanner( apiVersion: "1.0.0", fileSystem );
			scanner.AddScanRoot( Path.Combine( executableDir, "Modules" ) );

			string userMods = Path.Combine( fileSystem.GetUserDataPath(), "Modules" );
			scanner.AddScanRoot( userMods );

			IReadOnlyList<DiscoveredModule> modules = scanner.ScanAndLoad();
			_modules.AddRange( modules );
		}

		private void PreLoadModules()
		{
			foreach ( var module in _modules ) {
			}
		}
	};
};
