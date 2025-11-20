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

using System;
using CVars;
using EventSystem;

namespace Settings {
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

		public static float EffectsVolumeDb { get; private set; } = 0.0f;
		public static float MusicVolumeDb { get; private set; } = 0.0f;

		private readonly string? ConfigurationFile;
		private readonly BindingManager BindingManager;

		private readonly CVarGroup DisplayGroup = new CVarGroup( "Display" );
		private readonly CVarGroup GraphicsGroup = new CVarGroup( "Graphics" );
		private readonly CVarGroup AudioGroup = new CVarGroup( "Audio" );
		private readonly CVarGroup AccessibilityGroup = new CVarGroup( "Accessibility" );
		private readonly CVarGroup NetworkingGroup = new CVarGroup( "Networking" );
		private readonly CVarGroup GameplayGroup = new CVarGroup( "Gameplay" );

		public static readonly GameEvent Save = new GameEvent( nameof( Save ) );
		public static readonly GameEvent Changed = new GameEvent( nameof( Changed ) );

		/*
		===============
		SettingsManager
		===============
		*/
		public SettingsManager( string? configFile ) {
			ArgumentException.ThrowIfNullOrEmpty( configFile );

			ConfigurationFile = configFile;
			BindingManager = new BindingManager();
		}
	};
};