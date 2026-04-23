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

namespace Nomad.Game.Domain.Data.Mods {
	public readonly struct ModuleScanResult {
		public bool IsValid { get; }
		public string? EntryType { get; }
		public string? Error { get; }

		private ModuleScanResult( bool isValid, string? entryType, string? error ) {
			IsValid = isValid;
			EntryType = entryType;
			Error = error;
		}

		public static ModuleScanResult Success( string entryType ) {
			return new ModuleScanResult(
				true,
				entryType,
				null
			);
		}

		public static ModuleScanResult Fail( string error ) {
			return new ModuleScanResult(
				false,
				null,
				error
			);
		}
	};
};