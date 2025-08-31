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

namespace SaveSystem {
	/*
	===================================================================================
	
	SaveField
	
	===================================================================================
	*/
	
	public sealed class SaveField {
		public enum FieldType : uint {
			Byte,
			UInt16,
			UInt,
			Int,
			Vec2,
			Vec3,
			String,
			Float,
			Boolean,

			Count
		};

		private string Name;
		private FieldType Type;
		private object Value;

		public SaveField( string name, uint value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.UInt );
			file.Write( value );
		}
		public SaveField( string name, int value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.Int );
			file.Write( value );
		}
		public SaveField( string name, float value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.Float );
			file.Write( value );
		}
		public SaveField( string name, Godot.Vector2 value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.Vec2 );
			file.Write( (double)value.X );
			file.Write( (double)value.Y );
		}
		public SaveField( string name, Godot.Vector3 value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.Vec3 );
			file.Write( (double)value.X );
			file.Write( (double)value.Y );
			file.Write( (double)value.Z );
		}
		public SaveField( string name, string value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.String );
			file.Write( value );
		}
		public SaveField( string name, bool value, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)FieldType.Boolean );
			file.Write( value );
		}

		public SaveField( string name, System.IO.BinaryReader file ) {
			Name = name;
			Type = (FieldType)file.ReadUInt32();

			switch ( Type ) {
				case FieldType.Int:
					Value = file.ReadInt32();
					break;
				case FieldType.UInt:
					Value = file.ReadUInt32();
					break;
				case FieldType.Boolean:
					Value = file.ReadBoolean();
					break;
				case FieldType.Vec2: {
						Godot.Vector2 value;
						value.X = (float)file.ReadDouble();
						value.Y = (float)file.ReadDouble();
						Value = value;
						break;
					}
				case FieldType.Vec3: {
						Godot.Vector3 value;
						value.X = (float)file.ReadDouble();
						value.Y = (float)file.ReadDouble();
						value.Z = (float)file.ReadDouble();
						Value = value;
						break;
					}
				case FieldType.String:
					Value = file.ReadString();
					break;
				case FieldType.Float:
					Value = (float)file.ReadDouble();
					break;
				default:
					Console.PrintError( string.Format( "Invalid save field type {0}, corruption or incompatible version?", Type ) );
					break;
			}
			;
		}

		public object GetValue() {
			return Value;
		}
		public string GetName() {
			return Name;
		}
		public FieldType GetFieldType() {
			return Type;
		}
	};
};