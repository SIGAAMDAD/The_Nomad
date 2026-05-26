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

namespace Nomad.Game.Sdk.Mods
{
	public sealed record ModDiagnosticSnapshot
	{
		public ModuleManifest Mod { get; init; }
		public ModRuntimeStatus Status { get; init; }

		public int InfoCount { get; init; }
		public int WarningCount { get; init; }
		public int ErrorCount { get; init; }
		public int FatalCount { get; init; }

		public int ExceptionCount { get; init; }
		public int PolicyViolationCount { get; init; }
		public int ValidationFailureCount { get; init; }

		public int ConsecutiveRuntimeFailures { get; init; }

		public bool IsDisabled { get; init; }
		public string? DisableReason { get; init; }

		public string? LastErrorCode { get; init; }
		public string? LastErrorMessage { get; init; }
		public DateTimeOffset? LastFaultTime { get; init; }
	}
}
