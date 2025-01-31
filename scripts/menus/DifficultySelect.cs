using Godot;
using System;

public partial class DifficultySelect : Control {
	private Label EasyModeDescription = null;
	private Label NormalModeDecription = null;
	private Label HardModeDescription = null;
	private Label VeryHardModeDescription = null;
	private Label InsaneModeDescription = null;
	private Label MemeModeDescription = null;

	private int HoveredDifficulty;

	[Signal]
	public delegate void EasyModeDescriptionHoverEventHandler();

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		VBoxContainer ModeDescription = GetNode<VBoxContainer>( "ModeDescription" );
		EasyModeDescription = ModeDescription.GetNode<Label>( "EasyModeDescription" );
		NormalModeDecription = ModeDescription.GetNode<Label>( "NormalModeDescription" );
		HardModeDescription = ModeDescription.GetNode<Label>( "HardModeDescription" );
		VeryHardModeDescription = ModeDescription.GetNode<Label>( "VeryHardModeDescription" );
		InsaneModeDescription = ModeDescription.GetNode<Label>( "InsaneModeDescription" );
		MemeModeDescription = ModeDescription.GetNode<Label>( "MemeModeDescription" );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta ) {
	}
};