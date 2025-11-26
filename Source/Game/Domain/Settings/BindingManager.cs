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

using Godot;

namespace Game.Domain.Settings {
	/*
	===================================================================================
	
	BindingManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class BindingManager {
		public readonly RefCounted Remapper;
		public readonly Resource RemappingConfig;
		public readonly Godot.Collections.Array<RefCounted> RemappableItems;
		public readonly RefCounted MappingFormatter;

		/*
		===============
		BindingManager
		===============
		*/
		public BindingManager() {
			using RefCounted bindingSetup = (RefCounted)ResourceLoader.Load<GDScript>( "res://scripts/menus/settings_bindings.gd" ).New();

			Remapper = (RefCounted)bindingSetup.Get( "_remapper" );
			RemappingConfig = (Resource)bindingSetup.Get( "_remapping_config" );
			RemappableItems = (Godot.Collections.Array<RefCounted>)bindingSetup.Get( "_remappable_items" );
			MappingFormatter = (RefCounted)bindingSetup.Get( "_mapping_formatter" );
		}
	};
};