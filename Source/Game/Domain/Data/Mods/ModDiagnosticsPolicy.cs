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

namespace Nomad.Game.Domain.Data.Mods
{
	public sealed record ModDiagnosticsPolicy
	{
		public int MaxRuntimeExceptionsBeforeDisable { get; init; } = 8;
		public int MaxPolicyViolationsBeforeDisable { get; init; } = 1;
		public int MaxValidationFailuresBeforeReject { get; init; } = 1;

		public int MaxStoredRecordsPerMod { get; init; } = 128;

		public bool DisableOnPolicyViolation { get; init; } = true;
		public bool DisableOnRepeatedRuntimeExceptions { get; init; } = true;
		public bool RejectOnValidationFailure { get; init; } = true;
	};
};
