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
using System.IO;
using System.Text.Json;
using Nomad.Core.FileSystem;
using Nomad.Core.Util;

namespace Nomad.Game.Infrastructure.Gameplay
{
	/*
	===================================================================================
	
	DataLoader
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal abstract class DataLoader<TData>
		where TData : class
	{
		protected readonly IFileSystem fileSystem;
		protected readonly ConcurrentDictionary<Guid, TData> dataCache = new();
		protected readonly ConcurrentDictionary<string, Guid> nameToGuid = new();

		/*
		===============
		DataLoader
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileSystem"></param>
		/// <exception cref="ArgumentNullException"></exception>
		protected DataLoader( IFileSystem fileSystem )
		{
			this.fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );
		}

		/*
		===============
		Get
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public TData? Get( Guid itemId )
		{
			return dataCache.TryGetValue( itemId, out var item ) ? item : null;
		}

		/*
		===============
		Get
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public bool TryGet( Guid itemId, out TData? item )
		{
			return dataCache.TryGetValue( itemId, out item );
		}

		/*
		===============
		GuidFromName
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemName"></param>
		/// <returns></returns>
		public Guid GuidFromName( string itemName )
		{
			return nameToGuid.TryGetValue( itemName, out var guid ) ? guid : Guid.Empty;
		}

		/*
		===============
		ScanDirectory
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataPath"></param>
		/// <param name="extensionPattern"></param>
		protected void ScanDirectory( string dataPath, string extensionPattern )
		{
			var files = fileSystem.GetFiles( dataPath, extensionPattern, true );
			for ( int i = 0; i < files.Count; i++ ) {
				using var fileBuffer = fileSystem.LoadFile( files[i] );
				if ( fileBuffer == null ) {
					return;
				}
				using var json = JsonLoader.Parse( fileBuffer.AsStream() );
				if ( TryLoadDefinition( json.RootElement, out var definition ) ) {
					var dataId = Path.GetFileNameWithoutExtension( files[i] );
					if ( !nameToGuid.TryGetValue( dataId, out var guid ) ) {
						guid = Guid.NewGuid();
						nameToGuid[dataId] = guid;
					}
					dataCache[guid] = definition;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="json"></param>
		/// <param name="definition"></param>
		/// <returns></returns>
		protected abstract bool TryLoadDefinition( JsonElement json, out TData definition );
	};
};
