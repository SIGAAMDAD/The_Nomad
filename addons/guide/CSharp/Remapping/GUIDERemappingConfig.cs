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
using System.Collections.Generic;

namespace GUIDE {
	/*
	===================================================================================
	
	GUIDERemappingConfig
	
	===================================================================================
	*/
	/// <summary>
	/// A remapping configuration. This only holds changes to the context mapping,
	/// so to get the full input map you need to apply this on top of one or more
	/// mapping contexts. The settings from this config take precedence over the
	/// settings from the mapping contexts.
	/// </summary>

	[Icon( "res://addons/guide/guide_interval.svg" )]
	public sealed partial class GUIDERemappingConfig : Resource {
		[Export]
		public Godot.Collections.Dictionary<GUIDEMappingContext, Godot.Collections.Dictionary<GUIDEAction, Godot.Collections.Dictionary<int, GUIDEInput>>> RemappedInputs;
		[Export]
		public Godot.Collections.Dictionary CustomData;

		/*
		===============
		Bind
		===============
		*/
		/// <summary>
		/// Binds the given input to the given action. Index can be given to have 
		/// alternative bindings for the same action.
		/// </summary>
		/// <param name="mappingContext"></param>
		/// <param name="action"></param>
		/// <param name="input"></param>
		/// <param name="index"></param>
		public void Bind( GUIDEMappingContext mappingContext, GUIDEAction action, GUIDEInput input, int index = 0 ) {
			if ( !RemappedInputs.ContainsKey( mappingContext ) ) {
				RemappedInputs[ mappingContext ] = new Godot.Collections.Dictionary<GUIDEAction, Godot.Collections.Dictionary<int, GUIDEInput>>();
			}
			if ( !RemappedInputs[ mappingContext ].ContainsKey( action ) ) {
				RemappedInputs[ mappingContext ][ action ] = new Godot.Collections.Dictionary<int, GUIDEInput>();
			}
			RemappedInputs[ mappingContext ][ action ][ index ] = input;
		}

		/*
		===============
		Unbind
		===============
		*/
		/// <summary>
		/// Unbinds the given input from the given action. This is a deliberate unbind
		/// which means that the action should not be triggerable by the input anymore. It 
		/// its not the same as _clear.	
		/// </summary>
		/// <param name="mappingContext"></param>
		/// <param name="action"></param>
		/// <param name="index"></param>
		public void Unbind( GUIDEMappingContext mappingContext, GUIDEAction action, int index = 0 ) {
			Bind( mappingContext, action, null, index );
		}

		/*
		===============
		Clear
		===============
		*/
		/// <summary>
		/// Removes the given input action binding from this configuration. The action will
		/// now have the default input that it has in the mapping_context. This is not the 
		/// same as _unbind.	
		/// </summary>
		/// <param name="mappingContext"></param>
		/// <param name="action"></param>
		/// <param name="index"></param>
		public void Clear( GUIDEMappingContext mappingContext, GUIDEAction action, int index = 0 ) {
			if ( !RemappedInputs.ContainsKey( mappingContext ) ) {
				return;
			}
			if ( !RemappedInputs[ mappingContext ].ContainsKey( action ) ) {
				return;
			}
			RemappedInputs[ mappingContext ][ action ].Remove( index );

			if ( RemappedInputs[ mappingContext ][ action ].Count == 0 ) {
				RemappedInputs[ mappingContext ].Remove( action );
			}
			if ( RemappedInputs[ mappingContext ].Count == 0 ) {
				RemappedInputs.Remove( mappingContext );
			}
		}

		/*
		===============
		GetBoundInputOrNull
		===============
		*/
		/// <summary>
		/// Returns the bound input for the given action name and index. Returns null
		/// if there is matching binding.
		/// </summary>
		/// <param name="mappingContext"></param>
		/// <param name="action"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public GUIDEInput? GetBoundInputOrNull( GUIDEMappingContext mappingContext, GUIDEAction action, int index = 0 ) {
			return !RemappedInputs.TryGetValue( mappingContext, out Godot.Collections.Dictionary<GUIDEAction, Godot.Collections.Dictionary<int, GUIDEInput>>? value )
				|| !value.TryGetValue( action, out Godot.Collections.Dictionary<int, GUIDEInput>? inputs ) ? null : inputs[ index ];
		}

		/*
		===============
		Has
		===============
		*/
		public bool Has( GUIDEMappingContext mappingContext, GUIDEAction action, int index = 0 ) {
			return RemappedInputs.ContainsKey( mappingContext )
				&& RemappedInputs[ mappingContext ].ContainsKey( action )
				&& RemappedInputs[ mappingContext ][ action ].ContainsKey( index );
		}
	};
};