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

using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.FileSystem;
using Godot;

namespace Nomad.Game.Presentation.Screens.DeveloperCommentaryMenu
{
	/*
	===================================================================================

	DeveloperCommentaryMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class DeveloperCommentaryMenuView : Control
	{
		public override void _Ready()
		{
			base._Ready();

			var fileSystem = ServiceLocator.GetService<IFileSystem>();

			fileSystem.AddSearchDirectory( "Assets/DeveloperCommentary" );
			var files = fileSystem.GetFiles( "Assets/DeveloperCommentary", "*.json", true );
			for ( int i = 0; i < files.Count; i++ ) {

			}
		}
	};
};
