using Godot;

public partial class StoryMenu : Control {
	private Label LocationLabel;
	private Label TimeLabel;

	public override void _Ready() {
		base._Ready();

		LocationLabel = GetNode<Label>( "MarginContainer/MainContainer/HBoxContainer/SaveDataContainer/LocationLabel" );
		TimeLabel = GetNode<Label>( "MarginContainer/MainContainer/HBoxContainer/SaveDataContainer/TimeLabel" );
	}
};