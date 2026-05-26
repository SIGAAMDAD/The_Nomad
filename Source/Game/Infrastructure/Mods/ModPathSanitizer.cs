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

namespace Nomad.Game.Infrastructure.Mods
{
	internal static class ModPathSanitizer
	{
		private static readonly char[] InvalidPathCharacters =
		{
			':',
			'*',
			'?',
			'"',
			'<',
			'>',
			'|'
		};

		public static string NormalizeSegment( string segment )
		{
			if ( string.IsNullOrWhiteSpace( segment ) ) {
				throw new ArgumentException( "Path segment cannot be null or whitespace.", nameof( segment ) );
			}

			string value = segment.Trim();

			if ( value == "." || value == ".." ) {
				throw new ModPolicyException(
					$"Illegal path segment '{segment}'."
				);
			}

			if ( value.IndexOfAny( InvalidPathCharacters ) >= 0 ||
				value.Contains( '/', StringComparison.Ordinal ) ||
				value.Contains( '\\', StringComparison.Ordinal ) ) {
				throw new ModPolicyException(
					$"Illegal path segment '{segment}'."
				);
			}

			return value;
		}

		public static string NormalizeRelativePath( string relativePath )
		{
			if ( string.IsNullOrWhiteSpace( relativePath ) ) {
				throw new ArgumentException( "Path cannot be null or whitespace.", nameof( relativePath ) );
			}

			string normalized = relativePath
				.Replace( '\\', '/' )
				.Trim();

			if ( normalized.StartsWith( "/", StringComparison.Ordinal ) ) {
				throw new ModPolicyException(
					$"Absolute mod paths are not allowed: '{relativePath}'."
				);
			}

			if ( normalized.IndexOfAny( InvalidPathCharacters ) >= 0 ) {
				throw new ModPolicyException(
					$"Mod path contains invalid characters: '{relativePath}'."
				);
			}

			string[] parts = normalized.Split(
				'/',
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
			);

			if ( parts.Length == 0 ) {
				throw new ModPolicyException(
					$"Mod path is empty after normalization: '{relativePath}'."
				);
			}

			var output = new List<string>( parts.Length );

			for ( int i = 0; i < parts.Length; i++ ) {
				string part = parts[i];

				if ( part == "." || part == ".." ) {
					throw new ModPolicyException(
						$"Mod path traversal is not allowed: '{relativePath}'."
					);
				}

				output.Add( part );
			}

			return string.Join( '/', output );
		}
	};
};
