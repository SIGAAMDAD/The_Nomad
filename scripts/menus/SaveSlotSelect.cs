using Godot;
using System;

public partial class SaveSlotSelect : Control {
	[Signal]
	public delegate void SetDifficultySelectMenuEventHandler();
	[Signal]
	public delegate void LoadSaveSlotEventHandler( int nSlot );

	/*
	private void NextMenu( int nSlot ) {
		ArchiveSystem.Instance.SetSlot( nSlot );
		if ( ArchiveSystem.Instance.SlotExists( nSlot ) ) {
			EmitSignal( "LoadSaveSlot", nSlot );
		} else {
			EmitSignal( "SetDifficultySelectMenu" );
		}
	}

	private void OnSaveSlot0ButtonPressed() {
		NextMenu( 0 );
	}
	private void OnSaveSlot1ButtonPressed() {
		NextMenu( 1 );
	}
	private void OnSaveSlot2ButtonPressed() {
		NextMenu( 2 );
	}

	public override void _Ready() {
		Button Slot0Button = GetNode<Button>( "VBoxContainer/SaveSlot0/SaveSlot0Button" );
		Slot0Button.Connect( "pressed", Callable.From( OnSaveSlot0ButtonPressed ) );

		Button Slot1Button = GetNode<Button>( "VBoxContainer/SaveSlot1/SaveSlot1Button" );
		Slot1Button.Connect( "pressed", Callable.From( OnSaveSlot1ButtonPressed ) );

		Button Slot2Button = GetNode<Button>( "VBoxContainer/SaveSlot2/SaveSlot2Button" );
		Slot2Button.Connect( "pressed", Callable.From( OnSaveSlot2ButtonPressed ) );
	}
	*/
};
