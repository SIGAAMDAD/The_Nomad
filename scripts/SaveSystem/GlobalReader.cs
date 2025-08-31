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

using System.Runtime.CompilerServices;

namespace SaveSystem {
	/*
	===================================================================================
	
	GlobalReader
	
	===================================================================================
	*/
	
	public static class GlobalReader {
		/*
		===============
		LoadByte
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static byte LoadByte() {
			try {
				return ArchiveSystem.SaveReader.ReadByte();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadByte: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadUShort
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static ushort LoadUShort() {
			try {
				return ArchiveSystem.SaveReader.ReadUInt16();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadUInt16: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadUInt
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static uint LoadUInt() {
			try {
				return ArchiveSystem.SaveReader.ReadUInt32();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadUInt32: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadULong
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static ulong LoadULong() {
			try {
				return ArchiveSystem.SaveReader.ReadUInt64();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadUInt64: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadSByte
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static sbyte LoadSByte() {
			try {
				return ArchiveSystem.SaveReader.ReadSByte();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadSByte: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadShort
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static short LoadShort() {
			try {
				return ArchiveSystem.SaveReader.ReadInt16();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadShort: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadInt
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int LoadInt() {
			try {
				return ArchiveSystem.SaveReader.ReadInt32();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadInt: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadLong
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static long LoadLong() {
			try {
				return ArchiveSystem.SaveReader.ReadInt64();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadLong: exception thrown while reading from save file\n{e}" );
				return 0;
			}
		}

		/*
		===============
		LoadFloat
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float LoadFloat() {
			try {
				return ArchiveSystem.SaveReader.ReadSingle();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadFloat: exception thrown while reading from save file\n{e}" );
				return 0.0f;
			}
		}

		/*
		===============
		LoadString
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static string LoadString() {
			try {
				return ArchiveSystem.SaveReader.ReadString();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadString: exception thrown while reading from save file\n{e}" );
				return ""; // would null be better here?
			}
		}

		/*
		===============
		LoadVector2
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Godot.Vector2 LoadVector2() {
			Godot.Vector2 value = Godot.Vector2.Zero;

			// exceptions are handled already in LoadFloat,
			// so we'll just get a Vector2 if the read failss
			value.X = LoadFloat();
			value.Y = LoadFloat();

			return value;
		}

		/*
		===============
		LoadBoolean
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool LoadBoolean() {
			try {
				return ArchiveSystem.SaveReader.ReadBoolean();
			} catch ( System.Exception e ) {
				Console.PrintError( $"ArchiveSystem.LoadBoolean: exception thrown while reading from save file\n{e}" );
				return false;
			}
		}
	};
};