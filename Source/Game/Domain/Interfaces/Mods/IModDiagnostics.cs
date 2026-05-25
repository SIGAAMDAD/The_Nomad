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
using Nomad.Game.Domain.Data.Mods;

namespace Nomad.Game.Domain.Interfaces.Mods
{
	/// <summary>
	/// A blackbox recorder + circuit breaker for modules.
	/// </summary>
	public interface IModDiagnostics
	{
		void ReportInfo(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null
		);

		void ReportWarning(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null
		);

		void ReportException(
			ModuleManifest mod,
			Exception exception,
			string operation
		);

		void ReportPolicyViolation(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null
		);

		void ReportValidationFailure(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null
		);

		bool ShouldDisable( ModuleManifest mod );
		void Disable( ModuleManifest mod, string reason );

		ModDiagnosticSnapshot GetSnapshot( ModuleManifest mod );

		IReadOnlyList<ModDiagnosticRecord> GetRecentRecords( ModuleManifest mod );
	};
};
