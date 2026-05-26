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

using System.Collections.Generic;
using System.Linq;

namespace Nomad.Game.Sdk.Mods
{
	public sealed class ModuleValidationReport
	{
		private readonly List<ModuleValidationIssue> _issues = new();

		public IReadOnlyList<ModuleValidationIssue> Issues => _issues;
		public bool IsAllowed => !_issues.Any( issue => issue.Severity == ModuleValidationSeverity.Error );

		public void Error( string code, string message, string? location = null )
		{
			_issues.Add( new ModuleValidationIssue( ModuleValidationSeverity.Error, code, message, location ) );
		}

		public void Warning( string code, string message, string? location = null )
		{
			_issues.Add( new ModuleValidationIssue( ModuleValidationSeverity.Warning, code, message, location ) );
		}
	}
}
