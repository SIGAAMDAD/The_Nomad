namespace SaveSystem {
	public class SaveSectionWriter {
		private long BeginPosition = 0;
		private uint FieldCount = 0;

		public SaveSectionWriter( string name ) {
			ArchiveSystem.SaveWriter.Write( name );
			BeginPosition = ArchiveSystem.SaveWriter.BaseStream.Position;
			ArchiveSystem.SaveWriter.Write( (long)0 );
			ArchiveSystem.SectionCount++;
		}
		~SaveSectionWriter() {
			long position = ArchiveSystem.SaveWriter.BaseStream.Position;

			ArchiveSystem.SaveWriter.BaseStream.Seek( BeginPosition, System.IO.SeekOrigin.Begin );
			ArchiveSystem.SaveWriter.Write( position );
			ArchiveSystem.SaveWriter.BaseStream.Seek( position, System.IO.SeekOrigin.Begin );
		}

		public void SaveInt( string name, int value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.Int );
			ArchiveSystem.SaveWriter.Write( value );
		}
		public void SaveUInt( string name, uint value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.UInt );
			ArchiveSystem.SaveWriter.Write( value );
		}
		public void SaveFloat( string name, float value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.Float );
			ArchiveSystem.SaveWriter.Write( value );
		}
		public void SaveString( string name, string value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.String );
			ArchiveSystem.SaveWriter.Write( value );
		}
		public void SaveBool( string name, bool value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.Boolean );
			ArchiveSystem.SaveWriter.Write( value );
		}
		public void SaveVector2( string name, Godot.Vector2 value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.Vector2 );
			ArchiveSystem.SaveWriter.Write( value.X );
			ArchiveSystem.SaveWriter.Write( value.Y );
		}
		public void SaveIntList( string name, System.Collections.Generic.List<int> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.IntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
		public void SaveUIntList( string name, System.Collections.Generic.List<uint> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.UIntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
		public void SaveFloatList( string name, System.Collections.Generic.List<float> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.FloatList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
		public void SaveStringList( string name, System.Collections.Generic.List<string> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.StringList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
		public void SaveIntList( string name, Godot.Collections.Array<int> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.IntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
		public void SaveUIntList( string name, Godot.Collections.Array<uint> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.UIntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
		public void SaveFloatList( string name, Godot.Collections.Array<float> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.FloatList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( (double)value[i] );
			}
		}
		public void SaveStringList( string name, Godot.Collections.Array<string> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.StringList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
		}
	};
};