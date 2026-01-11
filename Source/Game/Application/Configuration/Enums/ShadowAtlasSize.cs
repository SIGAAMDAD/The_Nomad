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

using Nomad.Core.Util;
using System;

namespace Game.Application.Configuration.Enums {
	public enum ShadowAtlasSize : uint {
		Size1024,
		Size2048,
		Size4096,
		Size8192,

		Count,

		Default = Size2048
	};

	public static class ShadowAtlasSizeExtensions {
		private const string SHADOW_ATLAS_SIZE_1024 = "1024";
		private const string SHADOW_ATLAS_SIZE_2048 = "2048";
		private const string SHADOW_ATLAS_SIZE_4096 = "4096";
		private const string SHADOW_ATLAS_SIZE_8192 = "8192";

		/*
		===============
		ToDisplayString
		===============
		*/
		public static InternString ToDisplayString( this ShadowAtlasSize atlasSize ) => atlasSize switch {
			ShadowAtlasSize.Size1024 => new( SHADOW_ATLAS_SIZE_1024 ),
			ShadowAtlasSize.Size2048 => new( SHADOW_ATLAS_SIZE_2048 ),
			ShadowAtlasSize.Size4096 => new( SHADOW_ATLAS_SIZE_4096 ),
			ShadowAtlasSize.Size8192 => new( SHADOW_ATLAS_SIZE_8192 ),
			_ => throw new ArgumentOutOfRangeException( nameof( atlasSize ) )
		};
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( InternString atlasSizeString, out ShadowAtlasSize atlasSize ) {
			switch ( (string)atlasSizeString ) {
				case SHADOW_ATLAS_SIZE_1024:
					atlasSize = ShadowAtlasSize.Size1024;
					break;
				case SHADOW_ATLAS_SIZE_2048:
					atlasSize = ShadowAtlasSize.Size2048;
					break;
				case SHADOW_ATLAS_SIZE_4096:
					atlasSize = ShadowAtlasSize.Size4096;
					break;
				case SHADOW_ATLAS_SIZE_8192:
					atlasSize = ShadowAtlasSize.Size8192;
					break;
				default:
					atlasSize = ShadowAtlasSize.Default;
					return false;
			}
			return true;
		}
	};
};