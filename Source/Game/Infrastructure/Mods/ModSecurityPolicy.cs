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

namespace Nomad.Game.Infrastructure.Mods
{
	public sealed class ModSecurityPolicy
	{
		public bool AllowReflection { get; init; } = false;
		public bool AllowThreading { get; init; } = false;
		public bool AllowProcessStart { get; init; } = false;
		public bool AllowRawFileIO { get; init; } = false;
		public bool AllowNetwork { get; init; } = false;

		public int MaxLocalEvents { get; init; } = 256;
		public int MaxEventSubscriptions { get; init; } = 2048;
		public int MaxPublishesPerFrame { get; init; } = 1024;
		public int MaxCallbackFailuresBeforeDisable { get; init; } = 8;
		public bool AllowLocalEventPublishing { get; init; } = true;
		public bool AllowAsyncSubscriptions { get; init; } = false;
		public bool AllowGameEventPublishing { get; init; } = false;

		public int MaxSaveFields { get; init; } = 2048;
		public int MaxSaveCollectionLength { get; init; } = 4096;
		public int MaxLogLinesPerSecond { get; init; } = 64;

		public bool ForbidPInvoke { get; init; } = true;
		public bool ForbidUnsafePointers { get; init; } = true;
		public bool ForbidStaticConstructors { get; init; } = false;

		public string[] AllowedSharedAssemblies { get; init; } = new string[] {
			"Nomad.Modding",
			"Nomad.Game.Sdk",

			"System.Runtime",
			"System.Private.CoreLib",
			"netstandard"
		};

		public string[] ForbiddenAssemblyPrefixes { get; init; } = new string[] {
			"Nomad.Save",
			"Nomad.Events",
			"Nomad.Input",
			"Nomad.FileSystem",
			"Nomad.CVars",
			"Nomad.Logger"
		};

		public string[] ForbiddenTypePrefixes { get; init; } = new string[] {
			"System.IO.File",
			"System.IO.Directory",
			"System.Diagnostics.Process",
			"System.Environment",
			"System.Threading.Thread",
			"System.Runtime.Loader.AssemblyLoadContext",
			"System.Threading.ThreadPool",
			"System.Net",
			"System.Net.Http"
		};

		public string[] ForbiddenMethodFullNames { get; init; } = new string[] {
			"System.Void System.Environment::Exit(System.Int32)",
			"System.Diagnostics.Process System.Diagnostics.Process::Start(System.String)",
			"System.Diagnostics.Process System.Diagnostics.Process::Start(System.String,System.String)"
		};
	};
};
