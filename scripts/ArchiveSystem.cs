/*
using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class ArchiveSystem : Node {
	private int SaveSlot = 0;
	private int CurrentChapter = 0;
	private int CurrentPart = 0;
	private const int MaxSaveSlots = 3; // may change in the future

	private List<SaveSlot> Slots;

	public void SetSlot( int nSlot ) {
		SaveSlot = nSlot;
	}

	public int GetSlot() {
		return SaveSlot;
	}
	public int GetChapter() {
		return CurrentChapter;
	}
	public int GetPart() {
		return CurrentPart;
	}
	public bool SlotExists( int nSlot ) {
		return Slots[ nSlot ] != null;
	}

	public SaveSection GetSection( string name ) {
		return Slots[ SaveSlot ].GetSection( name );
	}

	public void Save() {
		Array<Node> SaveNodes = GetTree().GetNodesInGroup( "Archive" );

		for ( int i = 0; i < SaveNodes.Count; i++ ) {
			if ( SaveNodes[i].HasMethod( "Save" ) ) {
				SaveNodes[i].Call( "Save" );
			}
		}
	}

	public override void _Ready() {
		Slots = new List<SaveSlot>();
		for ( int i = 0; i < MaxSaveSlots; i++ ) {
			Slots.Add( new SaveSlot( i ) );
		}
	}
	public override void _Process( double delta ) {
	}
};
*/