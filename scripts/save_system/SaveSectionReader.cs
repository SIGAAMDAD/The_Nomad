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
					System.Collections.Generic.List<int> value = new System.Collections.Generic.List<int>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadInt32() );
					}
					Value = value;
					break; }
				case FieldType.UIntList: {
					System.Collections.Generic.List<uint> value = new System.Collections.Generic.List<uint>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( reader.ReadUInt32() );
					}
					Value = value;
					break; }
				case FieldType.FloatList: {
					System.Collections.Generic.List<float> value = new System.Collections.Generic.List<float>();
					int count = reader.ReadInt32();
					for ( int i = 0; i < count; i++ ) {
						value.Add( (float)reader.ReadDouble() );
					}
					Value = value;
					break; }
				case FieldType.StringList: {
					System.Collections.Generic.List<string> value = new System.Collections.Generic.List<string>();
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

		private System.Collections.Generic.Dictionary<string, SaveField> FieldList;

		public SaveSectionReader() {
			FieldList = new System.Collections.Generic.Dictionary<string, SaveField>();

			int fieldCount = ArchiveSystem.SaveReader.ReadInt32();
			for ( int i = 0; i < fieldCount; i++ ) {
				string name = ArchiveSystem.SaveReader.ReadString();
				FieldList.Add( name, new SaveField( ArchiveSystem.SaveReader ) );
			}
		}

		public int LoadInt( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.Int ) {
					return 0;
				}
				return (int)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return 0;
		}
		public uint LoadUInt( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.UInt ) {
					return 0;
				}
				return (uint)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return 0;
		}
		public float LoadFloat( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.Float ) {
					return 0;
				}
				return (float)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return 0.0f;
		}
		public bool LoadBoolean( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.Boolean ) {
					return false;
				}
				return (bool)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return false;
		}
		public string LoadString( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.Int ) {
					return null;
				}
				return (string)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return null;
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
		public System.Collections.Generic.List<int> LoadIntList( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.IntList ) {
					return null;
				}
				return (System.Collections.Generic.List<int>)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return null;
		}
		public System.Collections.Generic.List<uint> LoadUIntList( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.UIntList ) {
					return null;
				}
				return (System.Collections.Generic.List<uint>)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return null;
		}
		public System.Collections.Generic.List<float> LoadFloatList( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.FloatList ) {
					return null;
				}
				return (System.Collections.Generic.List<float>)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return null;
		}
		public System.Collections.Generic.List<string> LoadStringList( string name ) {
			SaveField field = null;
			if ( FieldList.TryGetValue( name, out field ) ) {
				if ( field.GetFieldType() != FieldType.StringList ) {
					return null;
				}
				return (System.Collections.Generic.List<string>)FieldList[ name ].GetValue();
			}
			GD.PushError( "Couldn't find field " + name );
			return null;
		}
	};
};