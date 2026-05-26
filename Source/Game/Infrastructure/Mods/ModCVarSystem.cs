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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Nomad.Core.CVars;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding.CVars;

namespace Nomad.Game.Infrastructure.Mods
{
	/*
	===================================================================================

	ModCVarSystem

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class ModCVarSystem : IModCVarSystem, IDisposable
	{
		private readonly ModuleManifest _identity;
		private readonly ICVarSystemService _cvarSystem;
		private readonly IModDiagnostics _diagnostics;
		private readonly ModCVarPolicy _policy;

		private readonly Dictionary<string, IModCVar> _ownedCVars = new Dictionary<string, IModCVar>(
			StringComparer.OrdinalIgnoreCase
		);

		private bool _isDisposed = false;

		public ModCVarSystem(
			ModuleManifest identity,
			ICVarSystemService cvarSystem,
			IModDiagnostics diagnostics,
			ModCVarPolicy policy
		)
		{
			_identity = identity ?? throw new ArgumentNullException( nameof( identity ) );
			_cvarSystem = cvarSystem ?? throw new ArgumentNullException( nameof( cvarSystem ) );
			_diagnostics = diagnostics ?? throw new ArgumentNullException( nameof( diagnostics ) );
			_policy = policy ?? throw new ArgumentNullException( nameof( policy ) );
		}

		public IModCVar<T> Register<T>(
			string localName,
			T defaultValue,
			string description = "",
			bool saved = true,
			Func<T, bool>? validator = null
		)
		{
			return Register(
				new ModCVarCreateInfo<T>
				{
					LocalName = localName,
					DefaultValue = defaultValue,
					Description = description,
					Saved = saved,
					Validator = validator
				}
			);
		}

		public IModCVar<T> Register<T>(
			ModCVarCreateInfo<T> createInfo
		)
		{
			ThrowIfDisposed();
			RequireCapability( ModCapabilities.CVars, "Register CVar" );

			ArgumentNullException.ThrowIfNull( createInfo );

			ValidateValueType<T>();
			ValidateDescription( createInfo.Description );

			string localName = NormalizeLocalName( createInfo.LocalName );
			string qualifiedName = GetQualifiedName( localName );

			if ( _ownedCVars.Count >= _policy.MaxCVarsPerMod ) {
				throw CreatePolicyViolation(
					"MOD_CVAR_QUOTA_EXCEEDED",
					$"Mod '{_identity.Id}' exceeded max CVar count {_policy.MaxCVarsPerMod}.",
					"Register CVar"
				);
			}

			if ( _ownedCVars.ContainsKey( localName ) ) {
				throw CreatePolicyViolation(
					"MOD_CVAR_DUPLICATE_LOCAL_NAME",
					$"Mod '{_identity.Id}' already registered local CVar '{localName}'.",
					"Register CVar"
				);
			}

			if ( _cvarSystem.CVarExists( qualifiedName ) ) {
				throw CreatePolicyViolation(
					"MOD_CVAR_DUPLICATE_ENGINE_NAME",
					$"Qualified CVar '{qualifiedName}' already exists.",
					"Register CVar"
				);
			}

			CVarFlags flags = BuildFlags( createInfo );

			Func<T, bool>? validator = WrapValidator(
				localName,
				createInfo.Validator
			);

			ICVar<T> coreCVar = _cvarSystem.Register(
				new CVarCreateInfo<T>
				{
					Name = qualifiedName,
					DefaultValue = createInfo.DefaultValue,
					Description = FormatDescription( createInfo.Description ),
					Group = GetGroupName(),
					Flags = flags,
					Validator = validator
				}
			);

			var modCVar = new ModCVar<T>(
				_identity,
				localName,
				qualifiedName,
				coreCVar,
				_diagnostics,
				_policy
			);

			_ownedCVars.Add( localName, modCVar );

			return modCVar;
		}

		public bool Exists(
			string localName
		)
		{
			ThrowIfDisposed();

			string normalizedName = NormalizeLocalName( localName );
			return _ownedCVars.ContainsKey( normalizedName );
		}

		public bool TryGet<T>(
			string localName,
			out IModCVar<T>? cvar
		)
		{
			ThrowIfDisposed();

			string normalizedName = NormalizeLocalName( localName );

			if ( !_ownedCVars.TryGetValue( normalizedName, out IModCVar? ownedCVar ) ) {
				cvar = null;
				return false;
			}

			if ( ownedCVar is not IModCVar<T> typedCVar ) {
				cvar = null;
				return false;
			}

			cvar = typedCVar;
			return true;
		}

		public IModCVar<T> Get<T>(
			string localName
		)
		{
			if ( TryGet( localName, out IModCVar<T>? cvar ) ) {
				return cvar;
			}

			throw new KeyNotFoundException(
				$"Mod '{_identity.Id}' does not own CVar '{localName}' with type '{typeof( T ).Name}'."
			);
		}

		public T GetValue<T>(
			string localName,
			T fallback = default!
		)
		{
			return TryGet( localName, out IModCVar<T>? cvar )
				? cvar.Value
				: fallback;
		}

		public bool TrySet<T>(
			string localName,
			T value
		)
		{
			ThrowIfDisposed();

			if ( !TryGet( localName, out IModCVar<T>? cvar ) ) {
				return false;
			}

			try {
				cvar.Value = value;
				return true;
			}
			catch ( Exception ex ) {
				_diagnostics.ReportException(
					_identity,
					ex,
					$"Set CVar '{localName}'"
				);

				return false;
			}
		}

		public IReadOnlyCollection<IModCVar> GetRegisteredCVars()
		{
			ThrowIfDisposed();
			return _ownedCVars.Values.ToArray();
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			foreach ( IModCVar cvar in _ownedCVars.Values ) {
				ICVar? coreCVar = _cvarSystem.GetCVar( cvar.QualifiedName );

				if ( coreCVar != null ) {
					_cvarSystem.Unregister( coreCVar );
				}
			}

			_ownedCVars.Clear();
			_isDisposed = true;

			GC.SuppressFinalize( this );
		}

		private CVarFlags BuildFlags<T>(
			ModCVarCreateInfo<T> createInfo
		)
		{
			CVarFlags flags = CVarFlags.None;

			if ( createInfo.Saved ) {
				if ( !_policy.AllowArchivedCVars ) {
					throw CreatePolicyViolation(
						"MOD_CVAR_ARCHIVE_DENIED",
						$"Mod '{_identity.Id}' attempted to create archived CVar '{createInfo.LocalName}', but archived mod CVars are disabled.",
						"Register CVar"
					);
				}

				flags |= CVarFlags.Archive;
			}

			if ( createInfo.Hidden ) {
				if ( !_policy.AllowHiddenCVars ) {
					throw CreatePolicyViolation(
						"MOD_CVAR_HIDDEN_DENIED",
						$"Mod '{_identity.Id}' attempted to create hidden CVar '{createInfo.LocalName}', but hidden mod CVars are disabled.",
						"Register CVar"
					);
				}

				flags |= CVarFlags.Hidden;
			}

			if ( createInfo.ReadOnly ) {
				if ( !_policy.AllowReadOnlyCVars ) {
					throw CreatePolicyViolation(
						"MOD_CVAR_READONLY_DENIED",
						$"Mod '{_identity.Id}' attempted to create read-only CVar '{createInfo.LocalName}', but read-only mod CVars are disabled.",
						"Register CVar"
					);
				}

				flags |= CVarFlags.ReadOnly;
			}

			if ( createInfo.DeveloperOnly ) {
				if ( !_policy.AllowDeveloperCVars ) {
					throw CreatePolicyViolation(
						"MOD_CVAR_DEVELOPER_DENIED",
						$"Mod '{_identity.Id}' attempted to create developer CVar '{createInfo.LocalName}', but developer mod CVars are disabled.",
						"Register CVar"
					);
				}

				flags |= CVarFlags.Developer;
			}

			return flags;
		}

		private Func<T, bool>? WrapValidator<T>(
			string localName,
			Func<T, bool>? validator
		)
		{
			if ( validator == null ) {
				return value => ValidateStringLength( localName, value );
			}

			return value =>
			{
				if ( !ValidateStringLength( localName, value ) ) {
					return false;
				}

				try {
					return validator( value );
				}
				catch ( Exception ex ) {
					_diagnostics.ReportException(
						_identity,
						ex,
						$"Validate CVar '{localName}'"
					);

					return false;
				}
			};
		}

		private bool ValidateStringLength<T>(
			string localName,
			T value
		)
		{
			if ( value is not string text ) {
				return true;
			}

			if ( text.Length <= _policy.MaxStringValueLength ) {
				return true;
			}

			_diagnostics.ReportPolicyViolation(
				_identity,
				"MOD_CVAR_STRING_TOO_LONG",
				$"Mod '{_identity.Id}' attempted to set CVar '{localName}' to a string longer than {_policy.MaxStringValueLength} characters.",
				"Set CVar"
			);

			return false;
		}

		private void ValidateDescription(
			string description
		)
		{
			if ( description.Length > _policy.MaxDescriptionLength ) {
				throw CreatePolicyViolation(
					"MOD_CVAR_DESCRIPTION_TOO_LONG",
					$"Mod '{_identity.Id}' attempted to register a CVar description longer than {_policy.MaxDescriptionLength} characters.",
					"Register CVar"
				);
			}
		}

		private static void ValidateValueType<T>()
		{
			Type type = typeof( T );

			if ( type == typeof( int )
				|| type == typeof( uint )
				|| type == typeof( bool )
				|| type == typeof( float )
				|| type == typeof( string ) ) {
				return;
			}

			throw new NotSupportedException(
				$"Mod CVars only support int, uint, bool, float, and string. Type '{type.FullName}' is not supported."
			);
		}

		private string NormalizeLocalName(
			string localName
		)
		{
			if ( string.IsNullOrWhiteSpace( localName ) ) {
				throw new ArgumentException(
					"CVar local name cannot be null or whitespace.",
					nameof( localName )
				);
			}

			string trimmed = localName.Trim();

			if ( trimmed.Length > _policy.MaxNameLength ) {
				throw CreatePolicyViolation(
					"MOD_CVAR_NAME_TOO_LONG",
					$"Mod CVar local name '{trimmed}' exceeds max length {_policy.MaxNameLength}.",
					"Normalize CVar Name"
				);
			}

			for ( int i = 0; i < trimmed.Length; i++ ) {
				char c = trimmed[i];

				bool valid = char.IsAsciiLetterOrDigit( c )
					|| c == '_'
					|| c == '.'
					|| c == '-';

				if ( !valid ) {
					throw CreatePolicyViolation(
						"MOD_CVAR_INVALID_NAME",
						$"Mod CVar local name '{trimmed}' contains invalid character '{c}'.",
						"Normalize CVar Name"
					);
				}
			}

			if ( trimmed.Contains( "..", StringComparison.Ordinal ) ) {
				throw CreatePolicyViolation(
					"MOD_CVAR_INVALID_NAME",
					$"Mod CVar local name '{trimmed}' cannot contain '..'.",
					"Normalize CVar Name"
				);
			}

			return trimmed.ToLowerInvariant();
		}

		private string GetQualifiedName(
			string normalizedLocalName
		)
		{
			return $"mod_{SanitizeNamePart( _identity.Id )}_{SanitizeNamePart( normalizedLocalName )}";
		}

		private string GetGroupName()
		{
			return $"Mods.{_identity.Id}";
		}

		private string FormatDescription(
			string description
		)
		{
			if ( string.IsNullOrWhiteSpace( description ) ) {
				return $"Mod CVar owned by '{_identity.Name}' ({_identity.Id}).";
			}

			return $"[{_identity.Id}] {description}";
		}

		private static string SanitizeNamePart(
			string value
		)
		{
			var builder = new StringBuilder( value.Length );

			for ( int i = 0; i < value.Length; i++ ) {
				char c = value[i];

				if ( char.IsAsciiLetterOrDigit( c ) ) {
					builder.Append( char.ToLowerInvariant( c ) );
				}
				else if ( c == '_' || c == '-' || c == '.' ) {
					builder.Append( '_' );
				}
			}

			return builder.ToString();
		}

		private void RequireCapability(
			ModCapabilities capability,
			string operation
		)
		{
			if ( ( _identity.Capabilities & capability ) != 0 ) {
				return;
			}

			throw CreatePolicyViolation(
				"MOD_CVAR_CAPABILITY_DENIED",
				$"Mod '{_identity.Id}' attempted CVar operation '{operation}' without '{capability}' capability.",
				operation
			);
		}

		private ModPolicyException CreatePolicyViolation(
			string code,
			string message,
			string operation
		)
		{
			_diagnostics.ReportPolicyViolation(
				_identity,
				code,
				message,
				operation
			);

			return new ModPolicyException( message );
		}

		private void ThrowIfDisposed()
		{
			if ( _isDisposed ) {
				throw new ObjectDisposedException( nameof( ModCVarSystem ) );
			}
		}
	};
};
