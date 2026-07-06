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
using Nomad.Core.CVars;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding.CVars;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModCVar<T> : IModCVar<T>
	{
		private readonly ModuleManifest _identity;
		private readonly ICVar<T> _cvar;
		private readonly IModDiagnostics _diagnostics;
		private readonly ModCVarPolicy _policy;

		public string LocalName { get; }
		public string QualifiedName { get; }

		public string Description => _cvar.Description;
		public Type ValueType => typeof( T );

		public bool IsReadOnly => _cvar.IsReadOnly;
		public bool IsSaved => _cvar.IsSaved;

		public object? BoxedValue => Value;

		public T DefaultValue => _cvar.DefaultValue;

		public T Value {
			get => _cvar.Value;
			set {
				if ( _cvar.IsReadOnly ) {
					throw new ModPolicyException(
						$"CVar '{LocalName}' is read-only."
					);
				}

				if ( value is string text && text.Length > _policy.MaxStringValueLength ) {
					_diagnostics.ReportPolicyViolation(
						_identity,
						"MOD_CVAR_STRING_TOO_LONG",
						$"Mod '{_identity.Id}' attempted to set CVar '{LocalName}' to a string longer than {_policy.MaxStringValueLength} characters.",
						"Set CVar"
					);

					throw new ModPolicyException(
						$"CVar '{LocalName}' string value exceeds max length {_policy.MaxStringValueLength}."
					);
				}

				_cvar.Value = value;
			}
		}

		public ModCVar(
			ModuleManifest identity,
			string localName,
			string qualifiedName,
			ICVar<T> cvar,
			IModDiagnostics diagnostics,
			ModCVarPolicy policy
		)
		{
			_identity = identity;
			LocalName = localName;
			QualifiedName = qualifiedName;
			_cvar = cvar ?? throw new ArgumentNullException( nameof( cvar ) );
			_diagnostics = diagnostics ?? throw new ArgumentNullException( nameof( diagnostics ) );
			_policy = policy ?? throw new ArgumentNullException( nameof( policy ) );
		}

		public void Reset()
		{
			_cvar.Reset();
		}
	}
}
