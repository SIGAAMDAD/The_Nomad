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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModDiagnostics : IModDiagnostics
	{
		private readonly ConcurrentDictionary<string, ModDiagnosticState> _states = new();
		private readonly ModDiagnosticsPolicy _policy;

		public ModDiagnostics( ModDiagnosticsPolicy policy )
		{
			_policy = policy ?? throw new ArgumentNullException( nameof( policy ) );
		}

		public void ReportInfo(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null )
		{
			AddRecord(
				new ModDiagnosticRecord(
					mod,
					ModDiagnosticSeverity.Info,
					ModDiagnosticKind.Info,
					code,
					message,
					operation ) );
		}

		public void ReportWarning(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null )
		{
			AddRecord(
				new ModDiagnosticRecord(
					mod,
					ModDiagnosticSeverity.Warning,
					ModDiagnosticKind.Warning,
					code,
					message,
					operation ) );
		}

		public void ReportException(
			ModuleManifest mod,
			Exception exception,
			string operation )
		{
			ArgumentNullException.ThrowIfNull( exception );

			ModDiagnosticRecord record = new ModDiagnosticRecord(
				mod,
				ModDiagnosticSeverity.Error,
				ModDiagnosticKind.Exception,
				"MOD_RUNTIME_EXCEPTION",
				$"Mod '{mod.Id}' threw during '{operation}'.",
				operation,
				exception );

			ModDiagnosticState state = AddRecord( record );
			state.ExceptionCount++;
			state.ConsecutiveRuntimeFailures++;
			state.LastFaultTime = record.Timestamp;
			state.LastErrorCode = record.Code;
			state.LastErrorMessage = exception.Message;

			if ( _policy.DisableOnRepeatedRuntimeExceptions &&
				state.ConsecutiveRuntimeFailures >= _policy.MaxRuntimeExceptionsBeforeDisable ) {
				Disable(
					mod,
					$"Exceeded runtime exception limit: {state.ConsecutiveRuntimeFailures}/{_policy.MaxRuntimeExceptionsBeforeDisable}." );
			}
		}

		public void ReportPolicyViolation(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null )
		{
			ModDiagnosticRecord record = new ModDiagnosticRecord(
				mod,
				ModDiagnosticSeverity.Fatal,
				ModDiagnosticKind.PolicyViolation,
				code,
				message,
				operation );

			ModDiagnosticState state = AddRecord( record );
			state.PolicyViolationCount++;
			state.LastFaultTime = record.Timestamp;
			state.LastErrorCode = code;
			state.LastErrorMessage = message;

			if ( _policy.DisableOnPolicyViolation ||
				state.PolicyViolationCount >= _policy.MaxPolicyViolationsBeforeDisable ) {
				Disable( mod, message );
			}
		}

		public void ReportValidationFailure(
			ModuleManifest mod,
			string code,
			string message,
			string? operation = null )
		{
			ModDiagnosticRecord record = new ModDiagnosticRecord(
				mod,
				ModDiagnosticSeverity.Fatal,
				ModDiagnosticKind.ValidationFailure,
				code,
				message,
				operation );

			ModDiagnosticState state = AddRecord( record );
			state.ValidationFailureCount++;
			state.LastFaultTime = record.Timestamp;
			state.LastErrorCode = code;
			state.LastErrorMessage = message;

			if ( _policy.RejectOnValidationFailure ||
				state.ValidationFailureCount >= _policy.MaxValidationFailuresBeforeReject ) {
				state.Status = ModRuntimeStatus.Rejected;
				state.IsDisabled = true;
				state.DisableReason = message;
			}
		}

		public bool ShouldDisable( ModuleManifest mod )
		{
			ModDiagnosticState state = GetState( mod );

			if ( state.IsDisabled ) {
				return true;
			}

			if ( _policy.DisableOnRepeatedRuntimeExceptions &&
				state.ConsecutiveRuntimeFailures >= _policy.MaxRuntimeExceptionsBeforeDisable ) {
				return true;
			}

			if ( _policy.DisableOnPolicyViolation &&
				state.PolicyViolationCount > 0 ) {
				return true;
			}

			return false;
		}

		public void Disable(
			ModuleManifest mod,
			string reason )
		{
			ModDiagnosticState state = GetState( mod );

			if ( state.IsDisabled ) {
				return;
			}

			state.IsDisabled = true;
			state.Status = ModRuntimeStatus.Disabled;
			state.DisableReason = reason;

			AddRecord(
				new ModDiagnosticRecord(
					mod,
					ModDiagnosticSeverity.Fatal,
					ModDiagnosticKind.Disabled,
					"MOD_DISABLED",
					reason ) );
		}

		public ModDiagnosticSnapshot GetSnapshot( ModuleManifest mod )
		{
			ModDiagnosticState state = GetState( mod );

			lock ( state.SyncRoot ) {
				return new ModDiagnosticSnapshot {
					Mod = mod,
					Status = state.Status,

					InfoCount = state.InfoCount,
					WarningCount = state.WarningCount,
					ErrorCount = state.ErrorCount,
					FatalCount = state.FatalCount,

					ExceptionCount = state.ExceptionCount,
					PolicyViolationCount = state.PolicyViolationCount,
					ValidationFailureCount = state.ValidationFailureCount,

					ConsecutiveRuntimeFailures = state.ConsecutiveRuntimeFailures,

					IsDisabled = state.IsDisabled,
					DisableReason = state.DisableReason,

					LastErrorCode = state.LastErrorCode,
					LastErrorMessage = state.LastErrorMessage,
					LastFaultTime = state.LastFaultTime
				};
			}
		}

		public IReadOnlyList<ModDiagnosticRecord> GetRecentRecords( ModuleManifest mod )
		{
			ModDiagnosticState state = GetState( mod );

			lock ( state.SyncRoot ) {
				return state.Records.ToArray();
			}
		}

		private ModDiagnosticState AddRecord( ModDiagnosticRecord record )
		{
			ModDiagnosticState state = GetState( record.Mod );

			lock ( state.SyncRoot ) {
				state.Records.Add( record );

				while ( state.Records.Count > _policy.MaxStoredRecordsPerMod ) {
					state.Records.RemoveAt( 0 );
				}

				switch ( record.Severity ) {
					case ModDiagnosticSeverity.Info:
						state.InfoCount++;
						break;

					case ModDiagnosticSeverity.Warning:
						state.WarningCount++;
						break;

					case ModDiagnosticSeverity.Error:
						state.ErrorCount++;
						break;

					case ModDiagnosticSeverity.Fatal:
						state.FatalCount++;
						break;
				}
			}

			return state;
		}

		private ModDiagnosticState GetState( ModuleManifest mod )
		{
			return _states.GetOrAdd(
				mod.Id,
				_ => new ModDiagnosticState( mod ) );
		}

		private sealed class ModDiagnosticState
		{
			public object SyncRoot { get; } = new object();

			public ModuleManifest Mod { get; }
			public ModRuntimeStatus Status { get; set; } = ModRuntimeStatus.Discovered;

			public List<ModDiagnosticRecord> Records { get; } = new();

			public int InfoCount { get; set; }
			public int WarningCount { get; set; }
			public int ErrorCount { get; set; }
			public int FatalCount { get; set; }

			public int ExceptionCount { get; set; }
			public int PolicyViolationCount { get; set; }
			public int ValidationFailureCount { get; set; }

			public int ConsecutiveRuntimeFailures { get; set; }

			public bool IsDisabled { get; set; }
			public string? DisableReason { get; set; }

			public string? LastErrorCode { get; set; }
			public string? LastErrorMessage { get; set; }
			public DateTimeOffset? LastFaultTime { get; set; }

			public ModDiagnosticState( ModuleManifest mod )
			{
				Mod = mod;
			}
		}
	};
};
