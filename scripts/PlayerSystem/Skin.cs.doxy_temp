/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using System.Collections.Generic;
using Godot;

namespace PlayerSystem {
	public class Skin {
		private SpriteFrames BodyAnimations;
		private SpriteFrames ArmAnimations;
		private SpriteFrames LegAnimations;
		private StringName Description;
		private StringName Name;

		private static Dictionary<StringName, Skin> SkinCache = null;
		
		public static void LoadSkins() {
			List<string> skinPaths = new List<string>();

			SkinCache = new Dictionary<StringName, Skin>();
		}

		public Skin( string path ) {
			BodyAnimations = ResourceLoader.Load<SpriteFrames>( path + "/" );
		}
	};
};