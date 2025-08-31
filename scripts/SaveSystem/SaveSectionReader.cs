/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

namespace SaveSystem {
	/*
	===================================================================================
	
	SaveSectionReader

	Be wary of switch statement hell
	
	===================================================================================
	*/

	/// <summary>
	/// <para>The dedicated class for reading dictionary style "sections" froms the current savefile</para>
	/// 
	/// <para>NOTE: reading from loaded save section can be multithreaded since we are loading all values
	/// into RAM from disk before applying gamestate.</para>
	/// 
	/// <para>exceptions are caught in ArchiveSystem.LoadGame</para>
	/// 
	/// <para>the default stream is ArchiveSystem.SaveReader, but for the sake of testing, it uses
	/// dependency injection in the constructor</para>
	/// </summary>
	public sealed class SaveSectionReader : IDisposable {
		private readonly struct SaveField {
			public readonly FieldType Type { get; }
			public readonly object Value { get; }

			/*
			===============
			LoadVariant
			===============
			*/
			/// <summary>
			/// Loads a Godot.Variant from the stream
			/// </summary>
			/// <param name="reader">The stream to read from</param>
			/// <returns>The loaded variant</returns>
			/// <exception cref="IndexOutOfRangeException">Thrown if the variant type isn't supported</exception>
			private Variant LoadVariant( System.IO.BinaryReader reader ) {
				Variant.Type type = (Variant.Type)reader.ReadUInt32();
				switch ( type ) {
					case Variant.Type.Bool:
						return Variant.From( reader.ReadBoolean() );
					case Variant.Type.Int:
						return Variant.From( reader.ReadInt32() );
					case Variant.Type.Float:
						return Variant.From( reader.ReadSingle() );
					case Variant.Type.String:
						return Variant.From( reader.ReadString() );
					case Variant.Type.StringName:
						return Variant.From( (StringName)reader.ReadString() );
					case Variant.Type.NodePath:
						return Variant.From( (NodePath)reader.ReadString() );
					case Variant.Type.Color:
						return new Color(
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle()
						);
					case Variant.Type.Vector2:
						return new Vector2(
							reader.ReadSingle(),
							reader.ReadSingle()
						);
					case Variant.Type.Vector2I:
						return new Vector2I(
							reader.ReadInt32(),
							reader.ReadInt32()
						);
					case Variant.Type.PackedColorArray: {
							Color[] arr = new Color[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ].R = reader.ReadSingle();
								arr[ i ].G = reader.ReadSingle();
								arr[ i ].B = reader.ReadSingle();
								arr[ i ].A = reader.ReadSingle();
							}
							return arr;
						}
					case Variant.Type.PackedByteArray: {
							// more efficient to do reader.ReadBytes( reader.ReadInt32() )?
							int count = reader.ReadInt32();
							return reader.ReadBytes( count );
						}
					case Variant.Type.PackedInt32Array: {
							int[] arr = new int[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadInt32();
							}
							return arr;
						}
					case Variant.Type.PackedInt64Array: {
							long[] arr = new long[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadInt64();
							}
							return arr;
						}
					case Variant.Type.PackedFloat32Array: {
							float[] arr = new float[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadSingle();
							}
							return arr;
						}
					case Variant.Type.PackedStringArray: {
							string[] arr = new string[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadString();
							}
							return arr;
						}
					case Variant.Type.PackedVector2Array: {
							Vector2[] arr = new Vector2[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ].X = reader.ReadSingle();
								arr[ i ].Y = reader.ReadSingle();
							}
							return arr;
						}
					case Variant.Type.Array:
						return LoadArrayInternal( reader );
					case Variant.Type.Dictionary:
						return LoadDictionaryInternal( reader );
					default:
						throw new IndexOutOfRangeException( $"invalid godot variant type found in savefile - {type}" );
				}
			}

			/*
			===============
			LoadDictionaryInternal
			===============
			*/
			/// <summary>
			/// Loads a Godot.Collections.Dictionary from a savefiles
			/// </summary>
			/// <param name="reader">The stream to read from</param>
			/// <returns>The loaded dictionary</returns>
			private Godot.Collections.Dictionary LoadDictionaryInternal( System.IO.BinaryReader reader ) {
				int count = reader.ReadInt32();
				Godot.Collections.Dictionary data = new Godot.Collections.Dictionary();

				for ( int i = 0; i < count; i++ ) {
					var key = LoadVariant( reader );
					var value = LoadVariant( reader );

					data.TryAdd( key, value );
				}

				return data;
			}

			/*
			===============
			LoadArrayInternal
			===============
			*/
			/// <summary>
			/// Loads a Godot.Collections.Array from a savefiles
			/// </summary>
			/// <param name="reader">The stream to read from</param>
			/// <returns>The loaded array</returns>
			private Godot.Collections.Array LoadArrayInternal( System.IO.BinaryReader reader ) {
				int count = reader.ReadInt32();
				Godot.Collections.Array value = new Godot.Collections.Array();
				value.Resize( count );

				for ( int i = 0; i < count; i++ ) {
					value[ i ] = LoadVariant( reader );
				}

				return value;
			}

			/*
			===============
			SaveField.SaveField
			===============
			*/
			/// <summary>
			/// Should only be called from within this class
			/// </summary>
			/// <param name="reader">The stream to read from</param>
			/// <exception cref="IndexOutOfRangeException"></exception>
			public SaveField( System.IO.BinaryReader? reader ) {
				ArgumentNullException.ThrowIfNull( reader, nameof( reader ) );

				Type = (FieldType)reader.ReadUInt32();
				switch ( Type ) {
					case FieldType.Int8:
						Value = reader.ReadSByte();
						break;
					case FieldType.Int16:
						Value = reader.ReadInt16();
						break;
					case FieldType.Int32:
						Value = reader.ReadInt32();
						break;
					case FieldType.Int64:
						Value = reader.ReadInt64();
						break;
					case FieldType.UInt8:
						Value = reader.ReadByte();
						break;
					case FieldType.UInt16:
						Value = reader.ReadUInt16();
						break;
					case FieldType.UInt32:
						Value = reader.ReadUInt32();
						break;
					case FieldType.UInt64:
						Value = reader.ReadUInt64();
						break;
					case FieldType.Float:
						Value = reader.ReadSingle();
						break;
					case FieldType.Double:
						Value = reader.ReadDouble();
						break;
					case FieldType.Boolean:
						Value = reader.ReadBoolean();
						break;
					case FieldType.String:
						Value = reader.ReadString();
						break;
					case FieldType.Vector2:
						Value = new Vector2(
							reader.ReadSingle(),
							reader.ReadSingle()
						);
						break;
					case FieldType.Vector2I:
						Value = new Vector2I(
							reader.ReadInt32(),
							reader.ReadInt32()
						);
						break;
					case FieldType.IntList: {
							int[] arr = new int[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadInt32();
							}
							Value = arr;
							break;
						}
					case FieldType.UIntList: {
							uint[] arr = new uint[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadUInt32();
							}
							Value = arr;
							break;
						}
					case FieldType.FloatList: {
							float[] arr = new float[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadSingle();
							}
							Value = arr;
							break;
						}
					case FieldType.StringList: {
							string[] arr = new string[ reader.ReadInt32() ];
							for ( int i = 0; i < arr.Length; i++ ) {
								arr[ i ] = reader.ReadString();
							}
							Value = arr;
							break;
						}
					case FieldType.ByteArray: {
							int count = reader.ReadInt32();
							Value = reader.ReadBytes( count );
							break;
						}
					case FieldType.Array:
						Value = LoadArrayInternal( reader );
						break;
					case FieldType.Dictionary:
						Value = LoadDictionaryInternal( reader );
						break;
					default:
						throw new IndexOutOfRangeException( $"invalid FieldType in savefile - {Type}" );
				}
			}

			/*
			===============
			SaveField
			===============
			*/
			/// <summary>
			/// only called when the field isn't found, so we provide a default value of 0/empty
			/// </summary>
			/// <param name="type">Type of the field</param>
			public SaveField( FieldType type ) {
				Type = type;
				switch ( Type ) {
					case FieldType.Int8:
						Value = (sbyte)0;
						break;
					case FieldType.Int16:
						Value = (short)0;
						break;
					case FieldType.Int32:
						Value = (int)0;
						break;
					case FieldType.Int64:
						Value = (long)0;
						break;
					case FieldType.UInt8:
						Value = (byte)0;
						break;
					case FieldType.UInt16:
						Value = (ushort)0;
						break;
					case FieldType.UInt32:
						Value = (uint)0;
						break;
					case FieldType.UInt64:
						Value = (ulong)0;
						break;
					case FieldType.Boolean:
						Value = false;
						break;
					case FieldType.Vector2:
						Value = Vector2.Zero;
						break;
					case FieldType.Vector2I:
						Value = Vector2I.Zero;
						break;
					case FieldType.Float:
						Value = (float)0.0f;
						break;
					case FieldType.Double:
						Value = (double)0.0f;
						break;
					case FieldType.String:
						Value = "";
						break;
					case FieldType.IntList:
						Value = new int[ 0 ];
						break;
					case FieldType.UIntList:
						Value = new uint[ 0 ];
						break;
					case FieldType.FloatList:
						Value = new float[ 0 ];
						break;
					case FieldType.StringList:
						Value = new string[ 0 ];
						break;
					case FieldType.ByteArray:
						Value = new byte[ 0 ];
						break;
					case FieldType.Array:
						Value = new Godot.Collections.Array();
						break;
				}
			}
		};

		private readonly System.IO.BinaryReader? Reader = null;
		private ConcurrentDictionary<string, SaveField>? FieldList = null;

		/*
		===============
		SaveSectionReader
		===============
		*/
		public SaveSectionReader( System.IO.BinaryReader? reader ) {
			if ( ArchiveSystem.SaveReader == null ) {
				throw new InvalidOperationException( "a SaveSectionReader object shouldn't be created outside of archive context" );
			}

			int fieldCount = ArchiveSystem.SaveReader.ReadInt32();
			FieldList = new ConcurrentDictionary<string, SaveField>( 1, fieldCount );
			Reader = reader ?? ArchiveSystem.SaveReader;

			Console.PrintLine( $"Got {fieldCount} fields." );

			for ( int i = 0; i < fieldCount; i++ ) {
				string name = Reader.ReadString();
				if ( !FieldList.TryAdd( name, new SaveField( Reader ) ) ) {
					throw new InvalidOperationException( $"A duplicate of SaveField {name} was found in savefile, aborting" );
				}
				Console.PrintLine( $"...loaded field \"{name}\"" );
			}
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose() {
			FieldList?.Clear();
		}

		/*
		===============
		CheckName

		checks if the given name is valid
		===============
		*/
		private static void CheckName( string name ) {
			if ( name == null || name.Length == 0 ) {
				throw new ArgumentException( $"name is invalid (null or empty)" );
			}
		}

		/*
		===============
		LoadField

		helper function to reduce boilerplate
		===============
		*/
		private SaveField LoadField( string name, FieldType type ) {
			CheckName( name );

			if ( FieldList.TryGetValue( name, out SaveField field ) ) {
				if ( field.Type != type ) {
					throw new InvalidCastException( $"SaveField {name} from savefile isn't the same type as {type}" );
				}
				return field;
			}

			// not the end of the world, just a missing field, so apply the default value
			Console.PrintError( $"...couldn't find save field {name}" );
			return new SaveField();
		}

		/*
		===============
		LoadSByte

		loads an sbyte from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public sbyte LoadSByte( string name ) {
			return (sbyte)LoadField( name, FieldType.Int8 ).Value;
		}

		/*
		===============
		LoadShort

		loads a short from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public short LoadShort( string name ) {
			return (short)LoadField( name, FieldType.Int16 ).Value;
		}

		/*
		===============
		LoadInt

		loads an int from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int LoadInt( string name ) {
			return (int)LoadField( name, FieldType.Int32 ).Value;
		}

		/*
		===============
		LoadLong

		loads a long from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public long LoadLong( string name ) {
			return (long)LoadField( name, FieldType.Int64 ).Value;
		}

		/*
		===============
		LoadByte

		loads a byte from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public byte LoadByte( string name ) {
			return (byte)LoadField( name, FieldType.UInt8 ).Value;
		}

		/*
		===============
		LoadUShort

		loads an sbyte from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ushort LoadUShort( string name ) {
			return (ushort)LoadField( name, FieldType.UInt16 ).Value;
		}

		/*
		===============
		LoadUInt

		loads a uint from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public uint LoadUInt( string name ) {
			return (uint)LoadField( name, FieldType.UInt32 ).Value;
		}

		/*
		===============
		LoadULong

		loads an sbyte from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ulong LoadULong( string name ) {
			return (ulong)LoadField( name, FieldType.UInt64 ).Value;
		}

		/*
		===============
		LoadFloat

		loads a float from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float LoadFloat( string name ) {
			return (float)LoadField( name, FieldType.Float ).Value;
		}

		/*
		===============
		LoadDouble

		loads a double from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public double LoadDouble( string name ) {
			return (double)LoadField( name, FieldType.Double ).Value;
		}

		/*
		===============
		LoadString

		loads a string from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string LoadString( string name ) {
			return (string)LoadField( name, FieldType.String ).Value;
		}

		/*
		===============
		LoadBoolean

		loads a boolean from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool LoadBoolean( string name ) {
			return (bool)LoadField( name, FieldType.Boolean ).Value;
		}

		/*
		===============
		LoadVector2

		loads a Godot.Vector2 from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector2 LoadVector2( string name ) {
			return (Vector2)LoadField( name, FieldType.Vector2 ).Value;
		}

		/*
		===============
		LoadVector2I

		loads a Godot.Vector2I from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector2I LoadVector2I( string name ) {
			return (Vector2I)LoadField( name, FieldType.Vector2I ).Value;
		}

		/*
		===============
		LoadIntList

		loads an int[] from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int[] LoadIntList( string name ) {
			return (int[])LoadField( name, FieldType.IntList ).Value;
		}

		/*
		===============
		LoadUIntList

		loads a uint[] from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public uint[] LoadList( string name ) {
			return (uint[])LoadField( name, FieldType.UIntList ).Value;
		}

		/*
		===============
		LoadFloatList

		loads a float[] from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float[] LoadFloatList( string name ) {
			return (float[])LoadField( name, FieldType.FloatList ).Value;
		}

		/*
		===============
		LoadStringList

		loads a string[] from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string[] LoadStringList( string name ) {
			return (string[])LoadField( name, FieldType.StringList ).Value;
		}

		/*
		===============
		LoadByteArray

		loads a byte[] from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public byte[] LoadByteArray( string name ) {
			return (byte[])LoadField( name, FieldType.ByteArray ).Value;
		}

		/*
		===============
		LoadArray

		loads a Godot.Collections.Array from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Godot.Collections.Array LoadArray( string name ) {
			return (Godot.Collections.Array)LoadField( name, FieldType.Array ).Value;
		}

		/*
		===============
		LoadDictionary

		loads a Godot.Collections.Dictionary from the loaded save section
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Godot.Collections.Dictionary LoadDictionary( string name ) {
			return (Godot.Collections.Dictionary)LoadField( name, FieldType.Dictionary ).Value;
		}
	};
};
