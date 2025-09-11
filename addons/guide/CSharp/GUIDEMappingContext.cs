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
using System.Runtime.CompilerServices;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDEMappingContext
	
	===================================================================================
	*/

	[Tool]
	[Icon( "res://addons/guide/guide_mapping_context.svg" )]
	public partial class GUIDEMappingContext : Resource {
		/// <summary>
		/// The display name for this mapping context during action remapping
		/// </summary>
		[Export]
		public string? DisplayName {
			get => _displayName;
			set {
				if ( _displayName == value ) {
					return;
				}
				_displayName = value;
				EmitChanged();
			}
		}

		/// <summary>
		/// The mappings. Do yourself a favor and use the G.U.I.D.E. panel
		/// to edit these.
		/// </summary>
		[Export]
		public GUIDEActionMapping[]? Mappings {
			get => _mappings;
			set {
				if ( _mappings == value ) {
					return;
				}
				_mappings = value;
				EmitChanged();
			}
		}

		private string? _displayName;
		private GUIDEActionMapping[]? _mappings;

		/*
		===============
		EditorName
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string EditorName() {
			return _displayName.Length == 0 ? ResourcePath.GetFile() : _displayName;
		}
	};
};