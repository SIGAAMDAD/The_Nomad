using Godot;

namespace SaveSystem {
	public class SaveSection {
		public SaveSection( string name, System.IO.BinaryWriter file ) {
			file.Write( name );
			file.Write( (uint)0 );
			Name = name;
			Saving = true;
			SavePosition = file.BaseStream.Position;
			SaveFile = file;
		}
		~SaveSection() {
			if ( !Saving ) {
				return;
			}

			long Now = SaveFile.BaseStream.Position;
			SaveFile.Seek( (int)SavePosition, System.IO.SeekOrigin.Begin );
			SaveFile.Write( FieldCount );
			SaveFile.Seek( (int)Now, System.IO.SeekOrigin.Begin );
		}

		public SaveSection( System.IO.BinaryReader file ) {
			uint FieldCount = file.ReadUInt32();

			Name = file.ReadString();
			GD.Print( "Loading save section " + Name + "..." );

			FieldList = new System.Collections.Generic.Dictionary<string, SaveField>();
			for ( uint i = 0; i < FieldCount; i++ ) {
				string name = file.ReadString();
				FieldList.Add( name, new SaveField( name, file ) );
			}
		}

		public void SaveInt( string name, int value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.Int );
			SaveFile.Write( value );

			FieldCount++;
		}
		public void SaveInt( string name, uint value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.UInt );
			SaveFile.Write( value );

			FieldCount++;
		}
		public void SaveFloat( string name, float value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.Float );
			SaveFile.Write( value );

			FieldCount++;
		}
		public void SaveString( string name, string value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.String );
			SaveFile.Write( value );

			FieldCount++;
		}
		public void SaveVec2( string name, Godot.Vector2 value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.Vec2 );
			SaveFile.Write( (double)value.X );
			SaveFile.Write( (double)value.Y );

			FieldCount++;
		}
		public void SaveVec3( string name, Godot.Vector3 value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.Vec3 );
			SaveFile.Write( (double)value.X );
			SaveFile.Write( (double)value.Y );
			SaveFile.Write( (double)value.Z );

			FieldCount++;
		}
		public void SaveBoolean( string name, bool value ) {
			SaveFile.Write( name );
			SaveFile.Write( (uint)SaveField.FieldType.Boolean );
			SaveFile.Write( value );
		}

		private SaveField FindField( string name, SaveField.FieldType type ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != type ) {
					GD.PushError( "Save field type isn't correct!" );
					return null;
				}
				return field;
			}
			return null;
		}
		public int LoadInt( string name ) {
			SaveField field = FindField( name, SaveField.FieldType.Int );
			if ( field == null ) {
				GD.PushError( "Couldn't load field " + name + "!" );
				return 0;
			}
			return (int)field.GetValue();
		}
		public uint LoadUInt( string name ) {
			SaveField field = FindField( name, SaveField.FieldType.UInt );
			if ( field == null ) {
				GD.PushError( "Couldn't load field " + name + "!" );
				return 0;
			}
			return (uint)field.GetValue();
		}
		public float LoadFloat( string name ) {
			SaveField field = FindField( name, SaveField.FieldType.Float );
			if ( field == null ) {
				GD.PushError( "Couldn't load field " + name + "!" );
				return 0;
			}
			return (float)field.GetValue();
		}
		public Vector2 LoadVec2( string name ) {
			SaveField field = FindField( name, SaveField.FieldType.Vec2 );
			if ( field == null ) {
				GD.PushError( "Couldn't load field " + name + "!" );
				return Godot.Vector2.Zero;
			}
			return (Godot.Vector2)field.GetValue();
		}
		public string LoadString( string name ) {
			SaveField field = FindField( name, SaveField.FieldType.String );
			if ( field == null ) {
				GD.PushError( "Couldn't load field " + name + "!" );
				return null;
			}
			return (string)field.GetValue();
		}
		public bool LoadBoolean( string name ) {
			SaveField field = FindField( name, SaveField.FieldType.Boolean );
			if ( field == null ) {
				GD.PushError( "Couldn't load field " + name + "!" );
				return false;
			}
			return (bool)field.GetValue();
		}

		public string GetName() {
			return Name;
		}

		private string Name;

		// meant for saving
		private bool Saving = false;
		private long SavePosition = 0;
		private uint FieldCount = 0;
		private System.IO.BinaryWriter SaveFile = null;

		private System.Collections.Generic.Dictionary<string, SaveField> FieldList;
	};
};