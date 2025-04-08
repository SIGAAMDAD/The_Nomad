using Godot;

namespace SaveSystem {
	public class SaveSectionWriter {
		private long BeginPosition = 0;
		private int FieldCount = 0;

		public SaveSectionWriter( string name ) {
			ArchiveSystem.SaveWriter.Write( name );
			BeginPosition = ArchiveSystem.SaveWriter.BaseStream.Position;
			ArchiveSystem.SaveWriter.Write( FieldCount );
			ArchiveSystem.SectionCount++;
		}

		public void Flush() {
			long position = ArchiveSystem.SaveWriter.BaseStream.Position;

			ArchiveSystem.SaveWriter.BaseStream.Seek( BeginPosition, System.IO.SeekOrigin.Begin );
			ArchiveSystem.SaveWriter.Write( FieldCount );
			ArchiveSystem.SaveWriter.BaseStream.Seek( position, System.IO.SeekOrigin.Begin );
		}
		
		private void SaveValue( string name, FieldType type, object value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)type );
			switch ( type ) {
			case FieldType.Int:
				ArchiveSystem.SaveWriter.Write( (int)value );
				break;
			case FieldType.UInt:
				ArchiveSystem.SaveWriter.Write( (uint)value );
				break;
			case FieldType.Float:
				ArchiveSystem.SaveWriter.Write( (double)(float)value );
				break;
			case FieldType.Boolean:
				ArchiveSystem.SaveWriter.Write( (bool)value );
				break;
			case FieldType.String:
				ArchiveSystem.SaveWriter.Write( (string)value );
				break;
			};
			FieldCount++;
		}

		public void SaveInt( string name, int value ) => SaveValue( name, FieldType.Int, value );
		public void SaveUInt( string name, uint value ) => SaveValue( name, FieldType.UInt, value );
		public void SaveFloat( string name, float value ) => SaveValue( name, FieldType.Float, value );
		public void SaveString( string name, string value ) => SaveValue( name, FieldType.String, value );
		public void SaveBool( string name, bool value ) => SaveValue( name, FieldType.Boolean, value );
		public void SaveVector2( string name, Godot.Vector2 value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.Vector2 );
			ArchiveSystem.SaveWriter.Write( (double)value.X );
			ArchiveSystem.SaveWriter.Write( (double)value.Y );
			FieldCount++;
		}
		public void SaveIntList( string name, System.Collections.Generic.List<int> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.IntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
		public void SaveUIntList( string name, System.Collections.Generic.List<uint> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.UIntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
		public void SaveFloatList( string name, System.Collections.Generic.List<float> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.FloatList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
		public void SaveStringList( string name, System.Collections.Generic.List<string> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.StringList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
		public void SaveIntList( string name, Godot.Collections.Array<int> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.IntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
		public void SaveUIntList( string name, Godot.Collections.Array<uint> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.UIntList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
		public void SaveFloatList( string name, Godot.Collections.Array<float> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.FloatList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( (double)value[i] );
			}
			FieldCount++;
		}
		public void SaveStringList( string name, Godot.Collections.Array<string> value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.StringList );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( value[i] );
			}
			FieldCount++;
		}
	};
};
