/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SaveSystem {
	/*
	===================================================================================
	
	SaveSectionWriter

	the dedicated class for writing dictionary style "sections" into the current savefile

	NOTE: it is a programming error if this is called without explicit ArchiveSystem SaveGame()
	setup or called in a multithreaded context. SO DO NOT MULTITHREAD IT!

	the default stream is ArchiveSystem.SaveWriter, but for the sake of testing, it uses
	dependency injection in the contructor
	
	===================================================================================
	*/
	/// <summary>
	/// Handles writing of savefile "sections" into a binary format
	/// </summary>
	/// <remarks>
	/// Not thread-safe. Should ONLY be used in a dedicated Save() function.
	/// Exceptions are caught in ArchiveSystem.SaveGame
	/// </remarks>
	/// <seealso cref="ArchiveSystem"/>
	/// <seealso cref="SaveSectionReader"/>

	public sealed class SaveSectionWriter : IDisposable {
		/// <summary>
		/// realistically someone shouldn't be writing more than 4 MB of contigious data to disk in a go
		/// </summary>
		private static readonly int MAX_ARRAY_SIZE = 4 * 1024 * 1024;

		/// <summary>
		/// the maximum amount of recursion allowed while writing an array to disk. Meant to avoid stack overflows
		/// </summary>
		private static readonly int MAX_ARCHIVE_DEPTH = 24;

		private readonly long BeginPosition = 0;
		private readonly System.IO.BinaryWriter? Writer = null;

		/// <summary>
		/// only here for possible future proofing and conformity
		/// </summary>
		private readonly object LockObject = new object();

		private int FieldCount = 0;

		/*
		===============
		SaveSectionWriter
		===============
		*/
		/// <summary>
		/// constructs a SaveSectionWriter object
		/// </summary>
		/// <param name="name">name of the section</param>
		/// <param name="writer">the stream to write to, if null, ArchiveSystem.SaveWriter is used by default</param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public SaveSectionWriter( string name, System.IO.BinaryWriter? writer ) {
			if ( ArchiveSystem.SaveWriter == null ) {
				throw new InvalidOperationException( "a SaveSectionWriter object shouldn't be created outside of archive context" );
			}
			if ( name == null || name.Length == 0 ) {
				throw new ArgumentNullException( nameof( name ) );
			}

			Writer = writer ?? ArchiveSystem.SaveWriter;

			lock ( LockObject ) {
				Writer.Write( name );
				BeginPosition = Writer.BaseStream.Position;
				Writer.Write( FieldCount );
				ArchiveSystem.PushSection();
			}
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose() {
			// exceptions are already being caught in Flush()
			Flush();
		}

		/*
		===============
		Flush
		===============
		*/
		/// <summary>
		/// finalizes a save section. Called automatically when the SaveSectionWriter
		/// is disposed
		/// </summary>
		/// <remarks>
		/// There is very little reason to call this explicitly.
		/// </remarks>
		public void Flush() {
			lock ( LockObject ) {
				long position = Writer.BaseStream.Position;

				Writer.BaseStream.Seek( BeginPosition, System.IO.SeekOrigin.Begin );
				Writer.Write( FieldCount );
				Writer.BaseStream.Seek( position, System.IO.SeekOrigin.Begin );
			}
		}

		/*
		===============
		CheckValue
		===============
		*/
		/// <summary>
		/// ensures the value being given to a function is valid
		/// </summary>
		/// <param name="name">Name of the field</param>
		/// <param name="value">Value of the field</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty, or if value is null</exception>
		private static void CheckValue( string name, object value ) {
			if ( name == null || name.Length == 0 ) {
				throw new ArgumentException( $"name is invalid (null or empty)" );
			}
			ArgumentNullException.ThrowIfNull( value );
		}

		/*
		===============
		SaveValue
		===============
		*/
		/// <summary>
		/// Saves a primitive value using FieldType to the section
		/// </summary>
		/// <remarks>
		/// strictly for primitives, supports the following types: sbyte, short, int, long, byte, ushort,
		/// uint, ulong, string, float, double, bool
		/// </remarks>
		/// <param name="name">Name of the field</param>
		/// <param name="type">Type of the field</param>
		/// <param name="value">Value of the field</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty, or if value is null</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <b>type</b> isn't one of the supported types</exception>
		private void SaveValue( string name, FieldType type, object value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)type );
				switch ( type ) {
					case FieldType.Int8:
						Writer.Write( (sbyte)value );
						break;
					case FieldType.Int16:
						Writer.Write( (short)value );
						break;
					case FieldType.Int32:
						Writer.Write( (int)value );
						break;
					case FieldType.Int64:
						Writer.Write( (long)value );
						break;
					case FieldType.UInt8:
						Writer.Write( (byte)value );
						break;
					case FieldType.UInt16:
						Writer.Write( (ushort)value );
						break;
					case FieldType.UInt32:
						Writer.Write( (uint)value );
						break;
					case FieldType.UInt64:
						Writer.Write( (ulong)value );
						break;
					case FieldType.Float:
						Writer.Write( (float)value );
						break;
					case FieldType.Double:
						Writer.Write( (double)value );
						break;
					case FieldType.Boolean:
						Writer.Write( (bool)value );
						break;
					case FieldType.String:
						Writer.Write( (string)value );
						break;

					// complex types with their own custom handling
					case FieldType.Vector2:
					case FieldType.Vector2I:
						throw new ArgumentException( "please use SaveSectionWriter.SaveVector[type] for Godot Vectors" );
					case FieldType.IntList:
					case FieldType.UIntList:
					case FieldType.FloatList:
					case FieldType.StringList:
					case FieldType.ByteArray:
						throw new ArgumentException( "please use SaveSectionWriter.SaveArray for lists" );
					case FieldType.Array:
						throw new ArgumentException( "please use SaveSectionWriter.SaveGodotArray for Godot Arrays" );
					case FieldType.Dictionary:
						throw new ArgumentException( "please use SaveSectionWriter.SaveGodotDictionary for Godot Dictionaries" );
					default:
						throw new ArgumentOutOfRangeException( $"invalid FieldType {type}" );
				}
			}
			FieldCount++;
		}

		/*
		===============
		SaveSByte
		===============
		*/
		/// <summary>
		/// Writes an sbyte (int8) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveSByte( string name, sbyte value ) {
			SaveValue( name, FieldType.Int8, value );
		}

		/*
		===============
		SaveShort
		===============
		*/
		/// <summary>
		/// Writes a short (int16) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveShort( string name, short value ) {
			SaveValue( name, FieldType.Int16, value );
		}

		/*
		===============
		SaveInt
		===============
		*/
		/// <summary>
		/// Writes an int (int32) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveInt( string name, int value ) {
			SaveValue( name, FieldType.Int32, value );
		}

		/*
		===============
		SaveLong
		===============
		*/
		/// <summary>
		/// Writes a long (int64) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveLong( string name, long value ) {
			SaveValue( name, FieldType.Int64, value );
		}

		/*
		===============
		SaveByte
		===============
		*/
		/// <summary>
		/// Writes a byte (uint8) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveByte( string name, byte value ) {
			SaveValue( name, FieldType.UInt8, value );
		}

		/*
		===============
		SaveUShort
		===============
		*/
		/// <summary>
		/// Writes a ushort (uint16) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveUShort( string name, ushort value ) {
			SaveValue( name, FieldType.UInt16, value );
		}

		/*
		===============
		SaveUInt
		===============
		*/
		/// <summary>
		/// Writes a uint (uint32) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveUInt( string name, uint value ) {
			SaveValue( name, FieldType.UInt32, value );
		}

		/*
		===============
		SaveULong
		===============
		*/
		/// <summary>
		/// Writes a ulong (uint64) value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveULong( string name, ulong value ) {
			SaveValue( name, FieldType.UInt64, value );
		}

		/*
		===============
		SaveFloat
		===============
		*/
		/// <summary>
		/// Writes a float value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveFloat( string name, float value ) {
			SaveValue( name, FieldType.Float, value );
		}

		/*
		===============
		SaveDouble
		===============
		*/
		/// <summary>
		/// Writes a double value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveDouble( string name, double value ) {
			SaveValue( name, FieldType.Double, value );
		}

		/*
		===============
		SaveString
		===============
		*/
		/// <summary>
		/// Writes a string value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveString( string name, string value ) {
			SaveValue( name, FieldType.String, value );
		}

		/*
		===============
		SaveBool
		===============
		*/
		/// <summary>
		/// Writes a boolean value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SaveBool( string name, bool value ) {
			SaveValue( name, FieldType.Boolean, value );
		}

		/*
		===============
		SaveVector2
		===============
		*/
		/// <summary>
		/// Writes a Godot.Vector2 value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveVector2( string name, Godot.Vector2 value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.Vector2 );
				Writer.Write( value.X );
				Writer.Write( value.Y );
			}

			FieldCount++;
		}

		/*
		===============
		SaveVector2I
		===============
		*/
		/// <summary>
		/// Writes a Godot.Vector2I value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveVector2I( string name, Godot.Vector2I value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.Vector2I );
				Writer.Write( value.X );
				Writer.Write( value.Y );
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes an int System.Collections.Generic.List value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, List<int> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.IntList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a int[] value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, int[] value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.IntList );
				Writer.Write( value.Length );
				for ( int i = 0; i < value.Length; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes an int Godot.Collections.Array value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, Godot.Collections.Array<int> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.IntList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a uint System.Collections.Generic.List value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, List<uint> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.UIntList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a uint[] value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, uint[] value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.UIntList );
				Writer.Write( value.Length );
				for ( int i = 0; i < value.Length; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a uint Godot.Collections.Array value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, Godot.Collections.Array<uint> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.UIntList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a float System.Collections.Generic.List value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, List<float> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.FloatList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a float[] value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, float[] value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.FloatList );
				Writer.Write( value.Length );
				for ( int i = 0; i < value.Length; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a float Godot.Collections.Array value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, Godot.Collections.Array<float> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.FloatList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a string System.Collections.Generic value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, List<string> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.StringList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a string[] value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, string[] value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.StringList );
				Writer.Write( value.Length );
				for ( int i = 0; i < value.Length; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a string Godot.Collections.Array value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, Godot.Collections.Array<string> value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.StringList );
				Writer.Write( value.Count );
				for ( int i = 0; i < value.Count; i++ ) {
					Writer.Write( value[ i ] );
				}
			}

			FieldCount++;
		}

		/*
		===============
		WriteVariant
		===============
		*/
		/// <summary>
		/// Writes a Godot variant's type and value to the current savefile section.
		/// </summary>
		/// <remarks>
		/// Should only be called to from SaveArray or SaveDictionary, strictly for archiving Godot .NET collections
		/// If you want to archive a primitive, use the provided functions like SaveInt.
		/// this function only supports the following variant types: Boolean, Int, Float, String, StringName, NodePath,
		/// Vector2, Vector2I, Color, PackedColorArray, PackedByteArray, PackedInt32Array, PackedInt64Array, PackedFloat32Array
		/// PackedVector2Array, PackedStringArray
		/// </remarks>
		/// <param name="value">The Variant object</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported VariantType is being saved</exception>
		private void WriteVariant( Variant value ) {
			lock ( LockObject ) {
				Writer.Write( (uint)value.VariantType );
				switch ( value.VariantType ) {
					case Variant.Type.Bool:
						Writer.Write( value.AsBool() );
						break;
					case Variant.Type.Int:
						Writer.Write( value.AsInt32() );
						break;
					case Variant.Type.Float:
						Writer.Write( value.AsSingle() );
						break;
					case Variant.Type.String:
						Writer.Write( value.AsString() );
						break;
					case Variant.Type.StringName:
						Writer.Write( value.AsStringName() );
						break;
					case Variant.Type.NodePath:
						Writer.Write( value.AsNodePath() );
						break;
					case Variant.Type.Color: {
							Color color = value.AsColor();
							Writer.Write( color.R );
							Writer.Write( color.G );
							Writer.Write( color.B );
							Writer.Write( color.A );
							break;
						}
					case Variant.Type.Vector2: {
							Vector2 vector = value.AsVector2();
							Writer.Write( vector.X );
							Writer.Write( vector.Y );
							break;
						}
					case Variant.Type.Vector2I: {
							Vector2I vector = value.AsVector2I();
							Writer.Write( vector.X );
							Writer.Write( vector.Y );
							break;
						}
					case Variant.Type.PackedColorArray: {
							Color[] arr = value.AsColorArray();
							Writer.Write( arr.Length );
							for ( int i = 0; i < arr.Length; i++ ) {
								Writer.Write( arr[ i ].R );
								Writer.Write( arr[ i ].G );
								Writer.Write( arr[ i ].B );
								Writer.Write( arr[ i ].A );
							}
							break;
						}
					case Variant.Type.PackedByteArray: {
							byte[] arr = value.AsByteArray();
							Writer.Write( arr.Length );
							Writer.Write( arr );
							break;
						}
					case Variant.Type.PackedInt32Array: {
							int[] arr = value.AsInt32Array();
							Writer.Write( arr.Length );
							for ( int i = 0; i < arr.Length; i++ ) {
								Writer.Write( arr[ i ] );
							}
							break;
						}
					case Variant.Type.PackedInt64Array: {
							long[] arr = value.AsInt64Array();
							Writer.Write( arr.Length );
							for ( int i = 0; i < arr.Length; i++ ) {
								Writer.Write( arr[ i ] );
							}
							break;
						}
					case Variant.Type.PackedFloat32Array: {
							float[] arr = value.AsFloat32Array();
							Writer.Write( arr.Length );
							for ( int i = 0; i < arr.Length; i++ ) {
								Writer.Write( arr[ i ] );
							}
							break;
						}
					case Variant.Type.PackedStringArray: {
							string[] arr = value.AsStringArray();
							Writer.Write( arr.Length );
							for ( int i = 0; i < arr.Length; i++ ) {
								Writer.Write( arr[ i ] );
							}
							break;
						}
					case Variant.Type.PackedVector2Array: {
							Vector2[] arr = value.AsVector2Array();
							Writer.Write( arr.Length );
							for ( int i = 0; i < arr.Length; i++ ) {
								Writer.Write( arr[ i ].X );
								Writer.Write( arr[ i ].Y );
							}
							break;
						}
					case Variant.Type.Array:
						SaveArrayInternal( value.AsGodotArray() );
						break;
					case Variant.Type.Dictionary:
						SaveDictionaryInternal( value.AsGodotDictionary() );
						break;
					default:
						throw new ArgumentOutOfRangeException( $"invalid Godot.VariantType: {value.VariantType}" );
				}
			}
		}

		/*
		===============
		SaveDictionary
		===============
		*/
		/// <summary>
		/// Writes a Godot.Collections.Dictionary value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveDictionary( string name, Godot.Collections.Dictionary value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.Dictionary );
				Writer.Write( value.Count );
			}
			foreach ( var it in value ) {
				WriteVariant( it.Key );
				WriteVariant( it.Value );
			}

			FieldCount++;
		}

		/*
		===============
		SaveDictionaryInternal
		===============
		*/
		/// <summary>
		/// SaveDictionary, but its for recursive dictionaries found in the base dictionary
		/// given to the section writer.
		/// </summary>
		/// <remarks>
		/// Shouldn't be called outside of this class
		/// </remarks>
		/// <param name="value"></param>
		/// <param name="depth"></param>
		/// <exception cref="InvalidOperationException">Thrown if the depth of a dictionary reference exceeds MAX_ARCHIVE_DEPTH</exception>
		private void SaveDictionaryInternal( Godot.Collections.Dictionary value, int depth = 0 ) {
			if ( depth++ > MAX_ARCHIVE_DEPTH ) {
				throw new InvalidOperationException( $"Dictionary depth exceeded maximum depth of {MAX_ARCHIVE_DEPTH}" );
			}

			lock ( LockObject ) {
				Writer.Write( value.Count );
			}
			foreach ( var it in value ) {
				WriteVariant( it.Key );
				WriteVariant( it.Value );
			}
		}

		/*
		===============
		SaveArray
		===============
		*/
		/// <summary>
		/// Writes a Godot.Collections.Array value to the current savefile section
		/// </summary>
		/// <param name="name">The unique identifier for this field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
		/// <exception cref="InvalidOperationException">Thrown if called outside of a save operation</exception>
		public void SaveArray( string name, Godot.Collections.Array value ) {
			CheckValue( name, value );

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.Array );
				Writer.Write( value.Count );
			}
			for ( int i = 0; i < value.Count; i++ ) {
				WriteVariant( value[ i ] );
			}
			FieldCount++;
		}

		/*
		===============
		SaveArrayInternal
		===============
		*/
		/// <summary>
		/// SaveArray, but its for recursive arrays found in the base array
		/// given to the section writer.
		/// </summary>
		/// <remarks>
		/// Shouldn't be called outside of this class
		/// </remarks>
		/// <param name="value"></param>
		/// <param name="depth"></param>
		/// <exception cref="InvalidOperationException">Thrown if the depth of a array reference exceeds MAX_ARCHIVE_DEPTH</exception>
		private void SaveArrayInternal( Godot.Collections.Array value, int depth = 0 ) {
			if ( depth++ > MAX_ARCHIVE_DEPTH ) {
				throw new InvalidOperationException( $"Array depth exceeded maximum depth of {MAX_ARCHIVE_DEPTH}" );
			}

			lock ( LockObject ) {
				Writer.Write( value.Count );
			}
			for ( int i = 0; i < value.Count; i++ ) {
				WriteVariant( value[ i ] );
			}
		}

		/*
		===============
		SaveByteArray
		===============
		*/
		/// <summary>
		/// Writes a fixed-length byte[] variable to the current savefile section
		/// </summary>
		/// <remarks>
		/// the given array shouldn't exceed 4 MiB, so be mindful of that
		/// </remarks>
		/// <param name="name">The unique identifier of the field</param>
		/// <param name="value">The value to be saved</param>
		/// <exception cref="ArgumentException">Thrown if value.Length exceeds MAX_ARRAY_SIZE</exception>
		public void SaveByteArray( string name, byte[] value ) {
			CheckValue( name, value );
			if ( value.Length > MAX_ARRAY_SIZE ) {
				throw new ArgumentException( "value.Length is greater than 4 MiB" );
			}

			lock ( LockObject ) {
				Writer.Write( name );
				Writer.Write( (uint)FieldType.ByteArray );
				Writer.Write( value.Length );
				Writer.Write( value );
			}

			FieldCount++;
		}
	};
};
