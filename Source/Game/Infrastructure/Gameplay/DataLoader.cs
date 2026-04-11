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
using Nomad.Core.FileSystem;

namespace Nomad.Game.Infrastructure.Gameplay {
	/*
	===================================================================================
	
	DataLoader
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal abstract class DataLoader {
		protected abstract string dataPath { get; }
		protected abstract string extensionPattern { get; }

		protected readonly IFileSystem fileSystem;

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
		protected DataLoader( IFileSystem fileSystem ) {
			this.fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );

			var files = this.fileSystem.GetFiles( dataPath, extensionPattern, true );
			for ( int i = 0; i < files.Count; i++ ) {
				LoadDefinition( files[i] );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		protected abstract bool LoadDefinition( string filePath );
	};
};