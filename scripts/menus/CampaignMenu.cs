using Godot;
using System;

public partial class CampaignMenu : Control {
	private Control SaveSlotSelect;
	private Control DifficultySelect;

	private void OnSetDifficultySelectMenu() {
		SaveSlotSelect.Hide();
		DifficultySelect.Call( "SetMemeModeName" );
		DifficultySelect.Show();
	}

	public override void _Ready() {
		SaveSlotSelect = GetNode<Control>( "SaveSlotSelect" );
		SaveSlotSelect.Connect( "SetDifficultySelectMenu", Callable.From( OnSetDifficultySelectMenu ) );

		DifficultySelect = GetNode<Control>( "DifficultySelect" );
	}
};
