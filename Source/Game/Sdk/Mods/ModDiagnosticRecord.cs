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
	public sealed record ModDiagnosticRecord
	{
		public DateTimeOffset Timestamp { get; }
		public ModuleManifest Mod { get; }
		public ModDiagnosticSeverity Severity { get; }
		public ModDiagnosticKind Kind { get; }
		public string Code { get; }
		public string Message { get; }
		public string? Operation { get; }
		public string? ExceptionType { get; }
		public string? ExceptionMessage { get; }
		public string? StackTrace { get; }

		public ModDiagnosticRecord(
			ModuleManifest mod,
			ModDiagnosticSeverity severity,
			ModDiagnosticKind kind,
			string code,
			string message,
			string? operation = null,
			Exception? exception = null
		)
		{
			Timestamp = DateTimeOffset.UtcNow;
			Mod = mod;
			Severity = severity;
			Kind = kind;
			Code = code;
			Message = message;
			Operation = operation;
			ExceptionType = exception?.GetType().FullName;
			ExceptionMessage = exception?.Message;
			StackTrace = exception?.StackTrace;
		}
	}
}
