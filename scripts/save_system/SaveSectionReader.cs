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
					List<int> value = new List<int>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadInt32() );
					}
					Value = value;
					break; }
				case FieldType.UIntList: {
					List<uint> value = new List<uint>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadUInt32() );
					}
					Value = value;
					break; }
				case FieldType.FloatList: {
					List<float> value = new List<float>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( (float)reader.ReadDouble() );
					}
					Value = value;
					break; }
				case FieldType.StringList: {
					List<string> value = new List<string>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadString() );
					}
					Value = value;
					break; }
				default:
					GD.PushError( "Unknown save field type " + Type.ToString() + " found in save file!" );
					break;
				};
			}

			public FieldType GetFieldType() {
				return Type;
			}
			public object GetValue() {
				return Value;
			}
		};

		private Dictionary<string, SaveField> FieldList;

		public SaveSectionReader() {
			FieldList = new Dictionary<string, SaveField>();

			int fieldCount = ArchiveSystem.SaveReader.ReadInt32();
			GD.Print( "Got " + fieldCount + " fields..." );
			for ( int i = 0; i < fieldCount; i++ ) {
				string name = ArchiveSystem.SaveReader.ReadString();
				FieldList.Add( name, new SaveField( ArchiveSystem.SaveReader ) );
				GD.Print( "...loaded field \"" + name + "\"" );
			}
		}

		public int LoadInt( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.Int ) {
					return 0;
				}
				return (int)FieldList[ name ].GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return 0;
		}
		public uint LoadUInt( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.Int ) {
					return 0;
				}
				return (uint)FieldList[ name ].GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return 0;
		}
		public float LoadFloat( string name ) {
			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.GetFieldType() != FieldType.Float ) {
					return 0;
				}
				return (float)field.GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return 0;
		}
		public bool LoadBoolean( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.Boolean ) {
					return false;
				}
				return (bool)FieldList[ name ].GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return false;
		}
		public string LoadString( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.String ) {
					return new string( "" );
				}
				return (string)FieldList[ name ].GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new string( "" );
		}
		public Godot.Vector2 LoadVector2( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.Vector2 ) {
					return new Godot.Vector2( 0.0f, 0.0f );
				}
				return (Godot.Vector2)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return new Godot.Vector2( 0.0f, 0.0f );
		}
		public List<int> LoadIntList( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.IntList ) {
					return new List<int>();
				}
				return (List<int>)FieldList[ name ].GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new List<int>();
		}
		public List<uint> LoadUIntList( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.UIntList ) {
					return new List<uint>();
				}
				return (List<uint>)FieldList[ name ].GetValue();
			}
			Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			return new List<uint>();
		}
		public List<float> LoadFloatList( string name ) {
			if ( FieldList.ContainsKey( name ) ) {
				SaveField field = FieldList[ name ];
				if ( field.GetFieldType() != FieldType.FloatList ) {
					return new List<float>();
				}
				return (List<float>)FieldList[ name ].GetValue();
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