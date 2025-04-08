using System.Collections.Generic;
using Godot;

namespace SaveSystem {
	public class SaveSectionReader {
		private class SaveField {
			private FieldType Type;
			private object Value;

			public SaveField( System.IO.BinaryReader reader ) {
				Type = (FieldType)reader.ReadUInt32();
				switch ( Type ) {
				case FieldType.Int:
					Value = reader.ReadInt32();
					break;
				case FieldType.UInt:
					Value = reader.ReadUInt32();
					break;
				case FieldType.Float:
					Value = (float)reader.ReadDouble();
					break;
				case FieldType.Boolean:
					Value = reader.ReadBoolean();
					break;
				case FieldType.String:
					Value = reader.ReadString();
					break;
				case FieldType.Vector2: {
					Godot.Vector2 value = new Godot.Vector2( 0.0f, 0.0f );
					value.X = (float)reader.ReadDouble();
					value.Y = (float)reader.ReadDouble();
					Value = value;
					break; }
				case FieldType.IntList: {
					int count = reader.ReadInt32();
					List<int> value = new List<int>( count );
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadInt32() );
					}
					Value = value;
					break; }
				case FieldType.UIntList: {
					int count = reader.ReadInt32();
					List<uint> value = new List<uint>( count );
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadUInt32() );
					}
					Value = value;
					break; }
				case FieldType.FloatList: {
					int count = reader.ReadInt32();
					List<float> value = new List<float>( count );
					for ( int i = 0; i < count; i++ ) {
						value.Add( (float)reader.ReadDouble() );
					}
					Value = value;
					break; }
				case FieldType.StringList: {
					int count = reader.ReadInt32();
					List<string> value = new List<string>( count );
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadString() );
					}
					Value = value;
					break; }
				default:
					Console.PrintError( "Unknown save field type " + Type.ToString() + " found in save file!" );
					break;
				};
			}

			public FieldType GetFieldType() => Type;
			public object GetValue() => Value;
		};

		private Dictionary<string, SaveField> FieldList = null;

		public SaveSectionReader() {
			int fieldCount = ArchiveSystem.SaveReader.ReadInt32();
			FieldList = new Dictionary<string, SaveField>( fieldCount );
			
			GD.Print( "Got " + fieldCount + " fields..." );
			for ( int i = 0; i < fieldCount; i++ ) {
				string name = ArchiveSystem.SaveReader.ReadString();
				FieldList.Add( name, new SaveField( ArchiveSystem.SaveReader ) );
				GD.Print( "...loaded field \"" + name + "\"" );
			}
		}
		
		public int LoadInt( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.Int ) {
					return 0;
				}
				return (int)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return 0;
		}
		public uint LoadUInt( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.UInt ) {
					return 0;
				}
				return (uint)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return 0;
		}
		public float LoadFloat( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.Float ) {
					return 0.0f;
				}
				return (float)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return 0.0f;
		}
		public bool LoadBoolean( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.Boolean ) {
					return false;
				}
				return (bool)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return false;
		}
		public string LoadString( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.String ) {
					return new string( "" );
				}
				return (string)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new string( "" );
		}
		public Godot.Vector2 LoadVector2( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.Vector2 ) {
					return new Godot.Vector2( 0.0f, 0.0f );
				}
				return (Godot.Vector2)field.GetValue();
			}
			Console.PrintError( "Couldn't find field " + name );
			return new Godot.Vector2( 0.0f, 0.0f );
		}
		public List<int> LoadIntList( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.IntList ) {
					return new List<int>();
				}
				return (List<int>)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new List<int>();
		}
		public List<uint> LoadUIntList( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.UIntList ) {
					return new List<uint>();
				}
				return (List<uint>)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new List<uint>();
		}
		public List<float> LoadFloatList( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.FloatList ) {
					return new List<float>();
				}
				return (List<float>)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new List<float>();
		}
		public List<string> LoadStringList( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.StringList ) {
					return new List<string>();
				}
				return (List<string>)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new List<string>();
		}
	};
};
