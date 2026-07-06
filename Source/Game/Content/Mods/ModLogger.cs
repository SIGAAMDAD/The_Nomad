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
using Nomad.Core.Logger;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModLogger : IModLogger, IDisposable
	{
		private readonly ILoggerService _logger;
		private readonly IModDiagnostics _diagnostics;
		private readonly ModuleManifest _identity;
		private readonly ModSecurityPolicy _policy;
		private readonly ILoggerCategory _category;

		private DateTime _windowStartUtc = DateTime.UtcNow;
		private int _messagesThisWindow = 0;
		private int _suppressedMessages = 0;

		public ModLogger( ILoggerService logger, IModDiagnostics diagnostics, ModuleManifest identity, ModSecurityPolicy policy )
		{
			_logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
			_diagnostics = diagnostics ?? throw new ArgumentNullException( nameof( diagnostics ) );
			_identity = identity ?? throw new ArgumentNullException( nameof( identity ) );
			_policy = policy ?? throw new ArgumentNullException( nameof( policy ) );

			_category = _logger.CreateCategory(
				$"Mods.{identity.Id}",
				LogLevel.Debug,
				true
			);
		}

		public void Dispose()
		{
			_category.Dispose();
		}

		public void Info( string message )
		{
			if ( !TryEnterLogWindow() ) {
				return;
			}

			_category.PrintLine( Format( message ) );
		}

		public void Warning( string message )
		{
			if ( !TryEnterLogWindow() ) {
				return;
			}

			_category.PrintWarning( Format( message ) );
		}

		public void Error( string message )
		{
			if ( !TryEnterLogWindow() ) {
				return;
			}

			_category.PrintError( Format( message ) );
		}

		public void Debug( string message )
		{
			if ( !TryEnterLogWindow() ) {
				return;
			}

			_category.PrintDebug( Format( message ) );
		}

		public void Exception( Exception exception, string message )
		{
			ArgumentGuard.ThrowIfNull( exception, nameof( exception ) );

			if ( !TryEnterLogWindow() ) {
				return;
			}

			_category.PrintError(
				Format(
					$"{message}{Environment.NewLine}" +
					$"{exception.GetType().FullName}: {exception.Message}{Environment.NewLine}" +
					$"{exception.StackTrace}"
				)
			);
		}

		private bool TryEnterLogWindow()
		{
			DateTime now = DateTime.UtcNow;

			if ( (now - _windowStartUtc).TotalSeconds >= 1.0f ) {
				FlushSuppressedMessageCount();

				_windowStartUtc = now;
				_messagesThisWindow = 0;
				_suppressedMessages = 0;
			}

			if ( _messagesThisWindow >= _policy.MaxLogLinesPerSecond ) {
				_suppressedMessages++;
				return false;
			}

			_messagesThisWindow++;
			return true;
		}

		private void FlushSuppressedMessageCount()
		{
			if ( _suppressedMessages <= 0 ) {
				return;
			}

			_category.PrintWarning(
				Format(
					$"Suppressed {_suppressedMessages} log message(s) due to mod log rate limiting."
				)
			);
		}

		private string Format( string message )
		{
			string safeMessage = message ?? string.Empty;

			return $"[{_identity.Id} v{_identity.Version}] {safeMessage}";
		}
	};
};
