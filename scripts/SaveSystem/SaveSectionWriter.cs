using System;
using System.ComponentModel;

namespace SaveSystem {
	public class SaveSectionWriter : IDisposable {
		private long BeginPosition = 0;
		private int FieldCount = 0;

		public SaveSectionWriter( string name ) {
			ArchiveSystem.SaveWriter.Write( name );
			BeginPosition = ArchiveSystem.SaveWriter.BaseStream.Position;
			ArchiveSystem.SaveWriter.Write( FieldCount );
			ArchiveSystem.SectionCount++;
		}

		public void Dispose() {
			Flush();
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
			case FieldType.SByte:
				ArchiveSystem.SaveWriter.Write( (sbyte)value );
				break;
			case FieldType.Byte:
				ArchiveSystem.SaveWriter.Write( (byte)value );
				break;
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

		public void SaveSByte( string name, sbyte value ) => SaveValue( name, FieldType.SByte, value );
		public void SaveByte( string name, byte value ) => SaveValue( name, FieldType.Byte, value );
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

		private void SaveDictionaryInternal( Godot.Collections.Dictionary value ) {
			ArchiveSystem.SaveWriter.Write( value.Count );
			foreach ( var it in value ) {
				ArchiveSystem.SaveWriter.Write( (uint)it.Key.VariantType );
				switch ( it.Key.VariantType ) {
				case Godot.Variant.Type.Bool:
					ArchiveSystem.SaveWriter.Write( it.Key.AsBool() );
					break;
				case Godot.Variant.Type.Int:
					ArchiveSystem.SaveWriter.Write( it.Key.AsInt32() );
					break;
				case Godot.Variant.Type.Float:
					ArchiveSystem.SaveWriter.Write( it.Key.AsDouble() );
					break;
				case Godot.Variant.Type.String:
					ArchiveSystem.SaveWriter.Write( it.Key.AsString() );
					break;
				case Godot.Variant.Type.StringName:
					ArchiveSystem.SaveWriter.Write( it.Key.AsStringName() );
					break;
				case Godot.Variant.Type.Vector2:
					ArchiveSystem.SaveWriter.Write( (double)it.Key.AsVector2().X );
					ArchiveSystem.SaveWriter.Write( (double)it.Key.AsVector2().Y );
					break;
				case Godot.Variant.Type.Array:
					SaveArrayInternal( it.Key.AsGodotArray() );
					break;
				case Godot.Variant.Type.Dictionary:
					SaveDictionaryInternal( it.Key.AsGodotDictionary() );
					break;
				default:
					Console.PrintError( string.Format( "SaveSectionWriter.SaveDictionaryInternal: unknown Godot.VariantType (key) {0}", it.Key.VariantType ) );
					break;
				};

				ArchiveSystem.SaveWriter.Write( (uint)it.Value.VariantType );
				switch ( it.Value.VariantType ) {
				case Godot.Variant.Type.Bool:
					ArchiveSystem.SaveWriter.Write( it.Value.AsBool() );
					break;
				case Godot.Variant.Type.Int:
					ArchiveSystem.SaveWriter.Write( it.Value.AsInt32() );
					break;
				case Godot.Variant.Type.Float:
					ArchiveSystem.SaveWriter.Write( it.Value.AsDouble() );
					break;
				case Godot.Variant.Type.String:
					ArchiveSystem.SaveWriter.Write( it.Value.AsString() );
					break;
				case Godot.Variant.Type.StringName:
					ArchiveSystem.SaveWriter.Write( it.Value.AsStringName() );
					break;
				case Godot.Variant.Type.Vector2:
					ArchiveSystem.SaveWriter.Write( (double)it.Value.AsVector2().X );
					ArchiveSystem.SaveWriter.Write( (double)it.Value.AsVector2().Y );
					break;
				case Godot.Variant.Type.Array:
					SaveArrayInternal( it.Value.AsGodotArray() );
					break;
				case Godot.Variant.Type.Dictionary:
					SaveDictionaryInternal( it.Value.AsGodotDictionary() );
					break;
				default:
					Console.PrintError( string.Format( "SaveSectionWriter.SaveDictionaryInternal: unknown Godot.VariantType (value) {0}", it.Value.VariantType ) );
					break;
				};
			}
		}
		private void SaveArrayInternal( Godot.Collections.Array value ) {
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( (uint)value[i].VariantType );
				switch ( value[i].VariantType ) {
				case Godot.Variant.Type.Bool:
					ArchiveSystem.SaveWriter.Write( value[i].AsBool() );
					break;
				case Godot.Variant.Type.Int:
					ArchiveSystem.SaveWriter.Write( value[i].AsInt32() );
					break;
				case Godot.Variant.Type.Float:
					ArchiveSystem.SaveWriter.Write( value[i].AsDouble() );
					break;
				case Godot.Variant.Type.String:
					ArchiveSystem.SaveWriter.Write( value[i].AsString() );
					break;
				case Godot.Variant.Type.StringName:
					ArchiveSystem.SaveWriter.Write( value[i].AsStringName() );
					break;
				case Godot.Variant.Type.Vector2:
					ArchiveSystem.SaveWriter.Write( (double)value[i].AsVector2().X );
					ArchiveSystem.SaveWriter.Write( (double)value[i].AsVector2().Y );
					break;
				case Godot.Variant.Type.Array:
					SaveArrayInternal( value[i].AsGodotArray() );
					break;
				case Godot.Variant.Type.Dictionary:
					SaveDictionaryInternal( value[i].AsGodotDictionary() );
					break;
				default:
					Console.PrintError( string.Format( "SaveSectionWriter.SaveArrayInternal: unknown Godot.VariantType {0}", value[i].VariantType ) );
					break;
				};
			}
		}
		public void SaveArray( string name, Godot.Collections.Array value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.Array );
			ArchiveSystem.SaveWriter.Write( value.Count );
			for ( int i = 0; i < value.Count; i++ ) {
				ArchiveSystem.SaveWriter.Write( (uint)value[i].VariantType );
				switch ( value[i].VariantType ) {
				case Godot.Variant.Type.Bool:
					ArchiveSystem.SaveWriter.Write( value[i].AsBool() );
					break;
				case Godot.Variant.Type.Int:
					ArchiveSystem.SaveWriter.Write( value[i].AsInt32() );
					break;
				case Godot.Variant.Type.Float:
					ArchiveSystem.SaveWriter.Write( value[i].AsDouble() );
					break;
				case Godot.Variant.Type.String:
					ArchiveSystem.SaveWriter.Write( value[i].AsString() );
					break;
				case Godot.Variant.Type.StringName:
					ArchiveSystem.SaveWriter.Write( value[i].AsStringName() );
					break;
				case Godot.Variant.Type.Vector2:
					ArchiveSystem.SaveWriter.Write( (double)value[i].AsVector2().X );
					ArchiveSystem.SaveWriter.Write( (double)value[i].AsVector2().Y );
					break;
				case Godot.Variant.Type.Array:
					SaveArrayInternal( value[i].AsGodotArray() );
					break;
				case Godot.Variant.Type.Dictionary:
					SaveDictionaryInternal( value[i].AsGodotDictionary() );
					break;
				default:
					Console.PrintError( string.Format( "SaveSectionWriter.SaveArray: unknown Godot.VariantType {0}", value[i].VariantType ) );
					break;
				};
			}
			FieldCount++;
		}
		public void SaveByteArray( string name, byte[] value ) {
			ArchiveSystem.SaveWriter.Write( name );
			ArchiveSystem.SaveWriter.Write( (uint)FieldType.ByteArray );
			ArchiveSystem.SaveWriter.Write( value.Length );
			ArchiveSystem.SaveWriter.Write( value );
			FieldCount++;
		}
	};
};
