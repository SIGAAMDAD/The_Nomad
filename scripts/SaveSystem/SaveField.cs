using Godot;

namespace SaveSystem {
	public class SaveField {
		public enum FieldType : uint {
			UInt,
			Int,
			Vec2,
			Vec3,
			String,
			Float,
			Boolean,

			Count
		};

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
				break; }
			case FieldType.Vec3: {
				Godot.Vector3 value;
				value.X = (float)file.ReadDouble();
				value.Y = (float)file.ReadDouble();
				value.Z = (float)file.ReadDouble();
				Value = value;
				break; }
			case FieldType.String:
				Value = file.ReadString();
				break;
			case FieldType.Float:
				Value = (float)file.ReadDouble();
				break;
			default:
				GD.PushError( "Invalid save field type " + Type + ", corruption or incompatible version?" );
				break;
			};
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

		private string Name;
		private FieldType Type;
		private object Value;
	};
};