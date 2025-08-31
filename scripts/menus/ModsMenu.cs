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

using System.Collections.Generic;
using Godot;

namespace Menus {
	/*
	===================================================================================
	
	ModsMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class ModsMenu : Control {
		private Dictionary<StringName, ModMetadata> ModList;
		private AudioStreamPlayer ThemeChannel;

		private HBoxContainer Cloner;

		/*
		===============
		GetModList
		===============
		*/
		private List<string> GetModList( string directory ) {
			List<string> modList = new List<string>();

			DirAccess dir = DirAccess.Open( directory );
			if ( dir != null ) {
				dir.ListDirBegin();
				string fileName = dir.GetNext();
				while ( fileName.Length > 0 ) {
					if ( fileName.GetExtension() != "mod" ) {
						fileName = dir.GetNext();
						continue;
					}
					modList.Add( dir.GetCurrentDir() + "/" + fileName );
					fileName = dir.GetNext();
				}
			} else {
				Console.PrintError( string.Format( "An error occurred when trying to access path \"{0}\"", directory ) );
			}

			return modList;
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			Cloner = GetNode<HBoxContainer>( "MarginContainer/VScrollBar/Cloner" );

			ThemeChannel = GetNode<AudioStreamPlayer>( "Theme" );

			Console.PrintLine( "Loading mods..." );

			List<string> modList = GetModList( "user://Mods" );
			modList.Sort();

			ModList = new Dictionary<StringName, ModMetadata>( modList.Count );
			for ( int i = 0; i < modList.Count; i++ ) {
				ModMetadata mod = ResourceLoader.Load<ModMetadata>( modList[ i ] );
				mod.Load();
				ModList.Add( mod.Name, mod );

				HBoxContainer container = Cloner.Duplicate() as HBoxContainer;
				( container.GetChild( 0 ) as RichTextLabel ).ParseBbcode( mod.Name );
				( container.GetChild( 1 ) as Label ).Text = mod.Version;
				container.Show();

				Console.PrintLine( string.Format( "...loaded mod {0}", mod.Name ) );
			}
		}
	};
};