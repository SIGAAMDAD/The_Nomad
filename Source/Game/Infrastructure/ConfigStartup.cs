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

using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Core.FileSystem;
using Nomad.Core.ServiceRegistry.Interfaces;

namespace Nomad.Game.Infrastructure.Godot
{
	/*
	===================================================================================

	ConfigStartup

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal static class ConfigStartup
	{
		/*
		===============
		Configure
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fileSystem"></param>
		/// <param name="cvarSystem"></param>
		public static void Configure( IServiceLocator locator )
		{
			ArgumentGuard.ThrowIfNull( locator, nameof( locator ) );

			var cvarSystem = locator.GetService<ICVarSystemService>();
			var fileSystem = locator.GetService<IFileSystem>();

			ICVar<string> configFile = cvarSystem.Register(
				new CVarCreateInfo<string> {
					Name = "game.ConfigPath",
					DefaultValue = $"{fileSystem.GetUserDataPath()}/UserConfig.ini",
					Description = "The path to the configuration file.",
					Flags = CVarFlags.Init | CVarFlags.ReadOnly
				}
			);

			if ( fileSystem.FileExists( configFile.Value ) ) {
				cvarSystem.Load( fileSystem, configFile.Value );
			}
		}
	};
};
