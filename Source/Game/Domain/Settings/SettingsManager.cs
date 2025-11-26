/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using System;

namespace Game.Domain.Settings {
	/*
	===================================================================================
	
	SettingsManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class SettingsManager {
		public const int CUSTOM_SETTING_VALUE = -1;

		private readonly BindingManager BindingManager;
		private readonly ILoggerService? Logger = ServiceRegistry.Get<ILoggerService>();

		/*
		===============
		SettingsManager
		===============
		*/
		public SettingsManager( string? configurationFile ) {
			ArgumentException.ThrowIfNullOrEmpty( configurationFile );

			var cvarSystem = ServiceRegistry.Get<ICVarSystemService>();
			ArgumentNullException.ThrowIfNull( cvarSystem );

			BindingManager = new BindingManager();

			Logger?.PrintLine( "SettingsManager: initializing global configuration..." );
			cvarSystem.Load( configurationFile );

			ServiceRegistry.Register( new AudioConfigService( cvarSystem ) );
			ServiceRegistry.Register( new AccessibilityConfigService( cvarSystem ) );
			ServiceRegistry.Register( new DisplayConfigService( cvarSystem ) );
		}
	};
};