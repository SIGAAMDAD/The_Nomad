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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.FileSystem;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Content.Mods
{
	public sealed class ModFileSystemRoots
	{
		public string ContentRoot { get; }
		public string DataRoot { get; }

		public ModFileSystemRoots(
			string contentRoot,
			string dataRoot
		)
		{
			ContentRoot = NormalizeRoot( contentRoot );
			DataRoot = NormalizeRoot( dataRoot );
		}

		public static ModFileSystemRoots FromFileSystem(
			IFileSystem fileSystem,
			ModuleManifest identity
		)
		{
			ArgumentGuard.ThrowIfNull( fileSystem );

			string safeId = ModPathSanitizer.NormalizeSegment( identity.Id );

			return new ModFileSystemRoots(
				contentRoot: $"Mods/{safeId}/Content",
				dataRoot: $"{fileSystem.GetUserDataPath()}/Mods/{safeId}"
			);
		}

		private static string NormalizeRoot( string root )
		{
			if ( string.IsNullOrWhiteSpace( root ) ) {
				throw new ArgumentException( "Root path cannot be null or whitespace.", nameof( root ) );
			}

			return root
				.Replace( '\\', '/' )
				.TrimEnd( '/' );
		}
	};
};
