using System.Collections.Generic;
using Godot;

/*
public class SaveSection {
	public SaveSection( string name, System.IO.BinaryWriter file ) {
		name = file.ReadString();
		file.Store32( 0 );
		Name = name;
		Saving = true;
		SavePosition = file.GetPosition();
		SaveFile = file;
	}
	~SaveSection() {
		if ( !Saving ) {
			return;
		}

		ulong Now = SaveFile.GetPosition();
		SaveFile.Seek( SavePosition );
		SaveFile.Store32( FieldCount );
		SaveFile.Seek( Now );
	}

	public SaveSection( System.IO.BinaryReader file ) {
		uint FieldCount = file.ReadUInt32();

		Name = file.ReadString();
		GD.Print( "Loading save section " + Name + "..." );

		FieldList = new List<SaveField>();
		for ( uint i = 0; i < FieldCount; i++ ) {
			FieldList.Add( new SaveField( file ) );
		}
	}

	public void SaveInt( string name, int value ) {
		SaveFile.Write( name );
		SaveFile.Write( value );

		FieldCount++;
	}
	public void SaveFloat( string name, float value ) {
		SaveFile.StorePascalString( name );
		SaveFile.StoreFloat( value );

		FieldCount++;
	}
	public void SaveString( string name, string value ) {
		SaveFile.StorePascalString( name );
		SaveFile.StorePascalString( value );

		FieldCount++;
	}
	public void SaveVec2( string name, Vector2 value ) {
		SaveFile.StorePascalString( name );
		SaveFile.StoreFloat( value.X );
		SaveFile.StoreFloat( value.Y );

		FieldCount++;
	}
	public void SaveVec3( string name, Vector3 value ) {
		SaveFile.StorePascalString( name );
		SaveFile.StoreFloat( value.X );
		SaveFile.StoreFloat( value.Y );
		SaveFile.StoreFloat( value.Z );

		FieldCount++;
	}

	private SaveField FindField( string name, SaveField.FieldType type ) {
		for ( int i = 0; i < FieldList.Count; i++ ) {
			if ( FieldList[i].GetName() == name && FieldList[i].GetType() == type ) {
				return FieldList[i];
			}
		}
		return null;
	}
	public int LoadInt( string name ) {
		SaveField Field = FindField( name, SaveField.FieldType.Int );

		if ( Field == null ) {
			GD.PushError( "Couldn't load field " + name + "!" );
			return 0;
		}

		return (int)Field.GetValue();
	}
	public float LoadFloat( string name ) {
		SaveField Field = FindField( name, SaveField.FieldType.Float );

		if ( Field == null ) {
			GD.PushError( "Couldn't load field " + name + "!" );
			return 0;
		}

		return (float)Field.GetValue();
	}
	public Vector2 LoadVec2( string name ) {
		SaveField Field = FindField( name, SaveField.FieldType.Vec2 );

		if ( Field == null ) {
			GD.PushError( "Couldn't load field " + name + "!" );
			return new Vector2( 0, 0 );
		}

		return (Vector2)Field.GetValue();
	}

	public string GetName() {
		return Name;
	}

	private string Name;
	
	// meant for saving
	private bool Saving = false;
	private ulong SavePosition = 0;
	private uint FieldCount = 0;
	private System.IO.BinaryWriter SaveFile = null;

	private List<SaveField> FieldList;
};

*/