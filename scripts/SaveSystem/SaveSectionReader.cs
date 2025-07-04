using System;
using System.Collections.Generic;
using Godot;

namespace SaveSystem {
	public class SaveSectionReader : IDisposable {
		private readonly struct SaveField {
			private readonly FieldType Type;
			private readonly object Value;

			private Godot.Collections.Dictionary LoadDictionaryInternal( System.IO.BinaryReader reader ) {
				int count = reader.ReadInt32();
				Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

				for ( int i = 0; i < count; i++ ) {
					var key = (Variant.Type)reader.ReadUInt32() switch {
						Variant.Type.Int => (Variant)reader.ReadInt32(),
						Variant.Type.Float => (Variant)(float)reader.ReadDouble(),
						Variant.Type.Bool => (Variant)reader.ReadBoolean(),
						Variant.Type.String => (Variant)reader.ReadString(),
						Variant.Type.StringName => (Variant)reader.ReadString(),
						Variant.Type.Array => (Variant)LoadArrayInternal( reader ),
						Variant.Type.Dictionary => (Variant)LoadDictionaryInternal( reader ),
						_ => new Variant(),
					};

					var value = (Variant.Type)reader.ReadUInt32() switch {
						Variant.Type.Int => (Variant)reader.ReadInt32(),
						Variant.Type.Float => (Variant)(float)reader.ReadDouble(),
						Variant.Type.Bool => (Variant)reader.ReadBoolean(),
						Variant.Type.String => (Variant)reader.ReadString(),
						Variant.Type.StringName => (Variant)reader.ReadString(),
						Variant.Type.Array => (Variant)LoadArrayInternal( reader ),
						Variant.Type.Dictionary => (Variant)LoadDictionaryInternal( reader ),
						_ => new Variant(),
					};

					data.TryAdd( key, value );
				}

				return data;
			}
			private Godot.Collections.Array LoadArrayInternal( System.IO.BinaryReader reader ) {
				int count = reader.ReadInt32();
				Godot.Collections.Array value = new Godot.Collections.Array();

				for ( int i = 0; i < count; i++ ) {
					switch ( (Variant.Type)reader.ReadUInt32() ) {
					case Variant.Type.Int:
						value.Add( reader.ReadInt32() );
						break;
					case Variant.Type.Float:
						value.Add( (float)reader.ReadDouble() );
						break;
					case Variant.Type.Bool:
						value.Add( reader.ReadBoolean() );
						break;
					case Variant.Type.String:
						value.Add( reader.ReadString() );
						break;
					case Variant.Type.StringName:
						value.Add( reader.ReadString() );
						break;
					case Variant.Type.Array:
						value.Add( LoadArrayInternal( reader ) );
						break;
					case Variant.Type.Dictionary:
						value.Add( LoadDictionaryInternal( reader ) );
						break;
					};
				}

				return value;
			}

			public SaveField( System.IO.BinaryReader reader ) {
				Type = (FieldType)reader.ReadUInt32();
				switch ( Type ) {
				case FieldType.SByte:
					Value = reader.ReadSByte();
					break;
				case FieldType.Byte:
					Value = reader.ReadByte();
					break;
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
				case FieldType.ByteArray: {
					int count = reader.ReadInt32();
					Value = reader.ReadBytes( count );
					break; }
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
				case FieldType.Array:
					Value = LoadArrayInternal( reader );
					break;
				case FieldType.Dictionary:
					Value = LoadDictionaryInternal( reader );
					break;
				default:
					Console.PrintError( "Unknown save field type " + Type.ToString() + " found in save file!" );
					break;
				};
			}

			public FieldType GetFieldType() => Type;
			public object GetValue() => Value;
		};

		private Dictionary<string, SaveField> FieldList = null;
		private readonly object LockObject = new object();

		public SaveSectionReader() {
			int fieldCount = ArchiveSystem.SaveReader.ReadInt32();
			FieldList = new Dictionary<string, SaveField>( fieldCount );

			Console.PrintLine( string.Format( "Got {0} fields.", fieldCount ) );

			for ( int i = 0; i < fieldCount; i++ ) {
				string name = ArchiveSystem.SaveReader.ReadString();
				FieldList.TryAdd( name, new SaveField( ArchiveSystem.SaveReader ) );
				Console.PrintLine( string.Format( "...loaded field \"{0}\"", name ) );
			}
		}

		public void Dispose() {
			GC.SuppressFinalize( this );
			FieldList.Clear();
		}
		
		public sbyte LoadSByte( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.SByte ) {
						return 0;
					}
					return (sbyte)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return 0;
		}
		public byte LoadByte( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.Byte ) {
						return 0;
					}
					return (byte)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return 0;
		}
		public int LoadInt( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.Int ) {
						return 0;
					}
					return (int)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return 0;
		}
		public uint LoadUInt( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.UInt ) {
						return 0;
					}
					return (uint)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return 0;
		}
		public float LoadFloat( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.Float ) {
						return 0.0f;
					}
					return (float)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return 0.0f;
		}
		public bool LoadBoolean( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.Boolean ) {
						return false;
					}
					return (bool)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return false;
		}
		public string LoadString( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.String ) {
						return new string( "" );
					}
					return (string)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return new string( "" );
		}
		public Godot.Vector2 LoadVector2( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.Vector2 ) {
						return new Godot.Vector2( 0.0f, 0.0f );
					}
					return (Godot.Vector2)field.GetValue();
				}
				Console.PrintError( "Couldn't find field " + name );
			}
			return new Godot.Vector2( 0.0f, 0.0f );
		}
		public List<int> LoadIntList( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.IntList ) {
						return new List<int>();
					}
					return (List<int>)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return new List<int>();
		}
		public List<uint> LoadUIntList( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.UIntList ) {
						return new List<uint>();
					}
					return (List<uint>)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return new List<uint>();
		}
		public List<float> LoadFloatList( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.FloatList ) {
						return new List<float>();
					}
					return (List<float>)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return new List<float>();
		}
		public List<string> LoadStringList( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.StringList ) {
						return new List<string>();
					}
					return (List<string>)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return new List<string>();
		}
		public Godot.Collections.Array LoadArray( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.Array ) {
						return new Godot.Collections.Array();
					}
					return (Godot.Collections.Array)field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return new Godot.Collections.Array();
		}
		public byte[] LoadByteArray( string name ) {
			lock ( LockObject ) {
				if ( FieldList.TryGetValue( name, out SaveField field ) ) {
					if ( field.GetFieldType() != FieldType.ByteArray ) {
						return [];
					}
					return (byte[])field.GetValue();
				}
				Console.PrintError( string.Format( "...couldn't find save field {0}", name ) );
			}
			return [];
		}
	};
};
