/*
using Godot;
using System;
using System.Collections.Generic;

public class SaveSlot {
	public SaveSlot( int nSlot ) {
		Slot = nSlot;
		Load();
	}

	public void Load() {
		GD.Print( "Loading save slot " + Slot + ", please do not exit the game..." );

		string fileName = "user://the-nomad/SLOT_" + Convert.ToString( Slot ) + ".ngd";

		System.IO.FileStream stream = new System.IO.FileStream(
			OS.GetDataDir() + "godot/app_userdata/TheNomad/" + fileName,
			System.IO.FileMode.Open,
			System.IO.FileAccess.Read
		);
		System.IO.BinaryReader file = new System.IO.BinaryReader( stream );

		uint SectionCount = 0;
		SectionCount = file.ReadUInt32();

		Sections = new Dictionary<string, SaveSection>();
		for ( uint i = 0; i < SectionCount; i++ ) {
			SaveSection Section = new SaveSection( file );
			Sections.Add( Section.GetName(), Section );
		}
	}
	public void Save() {

	}

	public int GetSlot() {
		return Slot;
	}
	public SaveSection GetSection( string name ) {
		return Sections[ name ];
	}

	private int Slot = 0;
	private Dictionary<string, SaveSection> Sections;
};
*/