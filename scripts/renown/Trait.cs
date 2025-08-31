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
using ResourceCache;
using System;
using System.Collections.Generic;

namespace Renown {
	/*
	===================================================================================
	
	Trait
	
	===================================================================================
	*/

	public partial class Trait : Resource {
		[Export]
		public StringName Name { get; private set; }
		[Export]
		public Godot.Collections.Dictionary<Trait, bool> ConflictingTraits { get; private set; }
		[Export]
		public Godot.Collections.Dictionary<Trait, bool> AgreeingTraits { get; private set; }

		/*
		===============
		Trait
		===============
		*/
		public Trait() {
			ArgumentException.ThrowIfNullOrEmpty( Name );
			ArgumentNullException.ThrowIfNull( ConflictingTraits );
			ArgumentNullException.ThrowIfNull( AgreeingTraits );
		}

		/*
		===============
		Create
		===============
		*/
		/// <summary>
		/// Instantiates a Trait from the provided <paramref name="resourcePath"/>
		/// </summary>
		/// <param name="resourcePath">The path to the .tres trait file</param>
		/// <returns>The loaded trait class</returns>
		/// <exception cref="InvalidCastException">Thrown if the resource file does not have a valid Trait C# script attached</exception>
		public static Trait? Create( string resourcePath ) {
			try {
				return (Trait)PreLoader.GetResource( resourcePath );
			} catch ( InvalidCastException ) {
				throw new InvalidCastException( $"A trait resource must have a valid Trait C# class file script attached!" );
			}
		}

		/*
		===============
		Conflicts
		===============
		*/
		public bool Conflicts( Trait trait ) {
			if ( ConflictingTraits.TryGetValue( trait, out bool conflicts ) ) {
				return conflicts;
			}
			return false;
		}

		/*
		===============
		Agrees
		===============
		*/
		public bool Agrees( Trait trait ) {
			if ( AgreeingTraits.TryGetValue( trait, out bool agrees ) ) {
				return agrees;
			}
			return false;
		}
	};
};