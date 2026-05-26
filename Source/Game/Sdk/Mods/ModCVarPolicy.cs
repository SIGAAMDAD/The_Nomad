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

namespace Nomad.Game.Sdk.Mods
{
	public sealed record ModCVarPolicy
	{
		public int MaxCVarsPerMod { get; init; } = 64;

		public int MaxNameLength { get; init; } = 64;
		public int MaxDescriptionLength { get; init; } = 256;
		public int MaxStringValueLength { get; init; } = 1024;

		public bool AllowArchivedCVars { get; init; } = true;
		public bool AllowHiddenCVars { get; init; } = false;
		public bool AllowReadOnlyCVars { get; init; } = true;
		public bool AllowDeveloperCVars { get; init; } = false;
	}
}
