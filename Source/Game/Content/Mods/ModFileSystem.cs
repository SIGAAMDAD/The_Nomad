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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nomad.Core.FileSystem;
using Nomad.Core.Memory.Buffers;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding.FileSystem;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModFileSystem : IModFileSystem
	{
		private readonly IFileSystem _fileSystem;
		private readonly IModDiagnostics _diagnostics;
		private readonly ModuleManifest _identity;
		private readonly ModFileSystemRoots _roots;
		private readonly ModFileSystemPolicy _policy;

		public ModFileSystem(
			IFileSystem fileSystem,
			IModDiagnostics diagnostics,
			ModuleManifest identity,
			ModFileSystemRoots roots,
			ModFileSystemPolicy? policy = null
		)
		{
			_fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );
			_diagnostics = diagnostics ?? throw new ArgumentNullException( nameof( diagnostics ) );
			_identity = identity;
			_roots = roots ?? throw new ArgumentNullException( nameof( roots ) );
			_policy = policy ?? new ModFileSystemPolicy();
		}

		public bool ContentFileExists( string relativePath )
		{
			Require( ModCapabilities.FilesRead, "ContentFileExists" );

			string path = ResolvePath( relativePath, ModFileAccess.ContentRead );
			return _fileSystem.FileExists( path );
		}

		public bool DataFileExists( string relativePath )
		{
			Require( ModCapabilities.FilesRead, "DataFileExists" );

			string path = ResolvePath( relativePath, ModFileAccess.DataRead );
			return _fileSystem.FileExists( path );
		}

		public bool DataDirectoryExists( string relativePath )
		{
			Require( ModCapabilities.FilesRead, "DataDirectoryExists" );

			string path = ResolvePath( relativePath, ModFileAccess.DataRead );
			return _fileSystem.DirectoryExists( path );
		}

		public void CreateDataDirectory( string relativePath )
		{
			Require( ModCapabilities.FilesWrite, "CreateDataDirectory" );

			string path = ResolvePath( relativePath, ModFileAccess.DataWrite );

			if ( !_fileSystem.DirectoryExists( path ) ) {
				_fileSystem.CreateDirectory( path );
			}
		}

		public IReadOnlyList<string> EnumerateContentFiles(
			string relativeDirectory,
			string searchPattern = "*",
			bool recursive = false
		)
		{
			Require( ModCapabilities.FilesRead, "EnumerateContentFiles" );
			ValidateEnumeration( recursive );

			string path = ResolvePath( relativeDirectory, ModFileAccess.ContentRead );
			return LimitEnumeration( _fileSystem.GetFiles( path, searchPattern, recursive ) );
		}

		public IReadOnlyList<string> EnumerateDataFiles(
			string relativeDirectory,
			string searchPattern = "*",
			bool recursive = false
		)
		{
			Require( ModCapabilities.FilesRead, "EnumerateDataFiles" );
			ValidateEnumeration( recursive );

			string path = ResolvePath( relativeDirectory, ModFileAccess.DataRead );
			return LimitEnumeration( _fileSystem.GetFiles( path, searchPattern, recursive ) );
		}

		public byte[] ReadContentBytes( string relativePath )
		{
			Require( ModCapabilities.FilesRead, "ReadContentBytes" );

			string path = ResolvePath( relativePath, ModFileAccess.ContentRead );
			return ReadBytes( path, relativePath );
		}

		public byte[] ReadDataBytes( string relativePath )
		{
			Require( ModCapabilities.FilesRead, "ReadDataBytes" );

			string path = ResolvePath( relativePath, ModFileAccess.DataRead );
			return ReadBytes( path, relativePath );
		}

		public string ReadContentText( string relativePath )
		{
			return Encoding.UTF8.GetString( ReadContentBytes( relativePath ) );
		}

		public string ReadDataText( string relativePath )
		{
			return Encoding.UTF8.GetString( ReadDataBytes( relativePath ) );
		}

		public T ReadContentJson<T>( string relativePath )
		{
			string json = ReadContentText( relativePath );

			T? value = JsonSerializer.Deserialize<T>(
				json,
				_policy.JsonOptions
			);

			if ( value == null ) {
				throw new Exception(
					$"Mod '{_identity.Id}' could not deserialize content JSON '{relativePath}' as '{typeof( T ).Name}'."
				);
			}

			return value;
		}

		public T ReadDataJson<T>( string relativePath )
		{
			string json = ReadDataText( relativePath );

			T? value = JsonSerializer.Deserialize<T>(
				json,
				_policy.JsonOptions
			);

			if ( value == null ) {
				throw new Exception(
					$"Mod '{_identity.Id}' could not deserialize data JSON '{relativePath}' as '{typeof( T ).Name}'."
				);
			}

			return value;
		}

		public bool TryReadContentText( string relativePath, out string text )
		{
			text = string.Empty;

			try {
				if ( !ContentFileExists( relativePath ) ) {
					return false;
				}

				text = ReadContentText( relativePath );
				return true;
			} catch ( Exception ex ) {
				_diagnostics.ReportException(
					_identity,
					ex,
					$"Mod content text read '{relativePath}'"
				);

				return false;
			}
		}

		public bool TryReadDataText( string relativePath, out string text )
		{
			text = string.Empty;

			try {
				if ( !DataFileExists( relativePath ) ) {
					return false;
				}

				text = ReadDataText( relativePath );
				return true;
			} catch ( Exception ex ) {
				_diagnostics.ReportException(
					_identity,
					ex,
					$"Mod data text read '{relativePath}'"
				);

				return false;
			}
		}

		public void WriteDataBytes( string relativePath, ReadOnlySpan<byte> bytes )
		{
			Require( ModCapabilities.FilesWrite, "WriteDataBytes" );
			ValidateWriteSize( bytes.Length, relativePath );

			string path = ResolvePath( relativePath, ModFileAccess.DataWrite );
			EnsureParentDirectory( path );

			byte[] buffer = bytes.ToArray();
			_fileSystem.WriteFile( path, buffer, 0, buffer.Length );
		}

		public void WriteDataText( string relativePath, string text )
		{
			ArgumentNullException.ThrowIfNull( text );

			WriteDataBytes(
				relativePath,
				Encoding.UTF8.GetBytes( text )
			);
		}

		public void WriteDataJson<T>( string relativePath, T value )
		{
			string json = JsonSerializer.Serialize(
				value,
				_policy.JsonOptions
			);

			WriteDataText( relativePath, json );
		}

		public ValueTask WriteDataBytesAsync(
			string relativePath,
			ReadOnlyMemory<byte> bytes,
			CancellationToken ct = default
		)
		{
			Require( ModCapabilities.FilesWrite, "WriteDataBytesAsync" );
			ValidateWriteSize( bytes.Length, relativePath );

			string path = ResolvePath( relativePath, ModFileAccess.DataWrite );
			EnsureParentDirectory( path );

			return _fileSystem.WriteFileAsync(
				path,
				bytes,
				0,
				bytes.Length,
				ct
			);
		}

		public void DeleteDataFile( string relativePath )
		{
			Require( ModCapabilities.FilesWrite, "DeleteDataFile" );

			string path = ResolvePath( relativePath, ModFileAccess.DataWrite );

			if ( _fileSystem.FileExists( path ) ) {
				_fileSystem.DeleteFile( path );
			}
		}

		private byte[] ReadBytes( string resolvedPath, string originalRelativePath )
		{
			if ( !_fileSystem.FileExists( resolvedPath ) ) {
				throw new Exception(
					$"Mod '{_identity.Id}' attempted to read missing file '{originalRelativePath}'."
				);
			}

			long fileSize = _fileSystem.GetFileSize( resolvedPath );
			if ( fileSize > _policy.MaxReadBytes ) {
				throw new Exception(
					$"Mod '{_identity.Id}' attempted to read '{originalRelativePath}', " +
					$"but its size {fileSize} exceeds the mod read limit {_policy.MaxReadBytes}."
				);
			}

			IBufferHandle? buffer = _fileSystem.LoadFile( resolvedPath );

			if ( buffer == null ) {
				throw new Exception(
					$"Mod '{_identity.Id}' failed to load file '{originalRelativePath}'."
				);
			}

			try {
				return buffer.ToArray();
			} finally {
				buffer.Dispose();
			}
		}

		private string ResolvePath(
			string relativePath,
			ModFileAccess access
		)
		{
			string safeRelativePath = ModPathSanitizer.NormalizeRelativePath( relativePath );

			return access switch {
				ModFileAccess.ContentRead => $"{_roots.ContentRoot}/{safeRelativePath}",
				ModFileAccess.DataRead => $"{_roots.DataRoot}/{safeRelativePath}",
				ModFileAccess.DataWrite => $"{_roots.DataRoot}/{safeRelativePath}",
				_ => throw new ArgumentOutOfRangeException( nameof( access ), access, null )
			};
		}

		private void Require(
			ModCapabilities capability,
			string operation
		)
		{
			if ( (_identity.Capabilities & capability) == capability ) {
				return;
			}

			string message =
				$"Mod '{_identity.Id}' attempted '{operation}' without capability '{capability}'.";

			_diagnostics.ReportPolicyViolation(
				_identity,
				"MOD_FILE_CAPABILITY_DENIED",
				message,
				operation
			);

			throw new ModPolicyException( message );
		}

		private void ValidateWriteSize(
			int byteCount,
			string relativePath
		)
		{
			if ( byteCount <= _policy.MaxWriteBytes ) {
				return;
			}

			throw new Exception(
				$"Mod '{_identity.Id}' attempted to write '{relativePath}' with {byteCount} bytes, " +
				$"which exceeds the mod write limit {_policy.MaxWriteBytes}."
			);
		}

		private void ValidateEnumeration( bool recursive )
		{
			if ( !recursive || _policy.AllowRecursiveEnumeration ) {
				return;
			}

			throw new ModPolicyException(
				$"Mod '{_identity.Id}' attempted recursive file enumeration, which is disabled."
			);
		}

		private IReadOnlyList<string> LimitEnumeration( IReadOnlyList<string> files )
		{
			if ( files.Count <= _policy.MaxEnumeratedFiles ) {
				return files;
			}

			var limited = new string[_policy.MaxEnumeratedFiles];

			for ( int i = 0; i < limited.Length; i++ ) {
				limited[i] = files[i];
			}

			_diagnostics.ReportWarning(
				_identity,
				"MOD_FILE_ENUMERATION_TRUNCATED",
				$"Mod '{_identity.Id}' file enumeration returned {files.Count} files; truncated to {_policy.MaxEnumeratedFiles}.",
				"ModFileSystem enumeration"
			);

			return limited;
		}

		private void EnsureParentDirectory( string resolvedPath )
		{
			if ( !_policy.CreateWriteDirectories ) {
				return;
			}

			int slashIndex = resolvedPath.LastIndexOf( '/' );

			if ( slashIndex <= 0 ) {
				return;
			}

			string directory = resolvedPath.Substring( 0, slashIndex );

			if ( !_fileSystem.DirectoryExists( directory ) ) {
				_fileSystem.CreateDirectory( directory );
			}
		}
	};
};
