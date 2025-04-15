using Godot;
using System;

public partial class BugReporter : Control {
	private TextEdit EmailTextEdit;
	private TextEdit TitleTextEdit;
	private TextEdit DescriptionTextEdit;

	private void OnSubmitButtonPressed() {
		
	}

	public override void _Ready() {
		base._Ready();
		
		Button SubmitButton = GetNode<Button>( "MarginContainer/VBoxContainer/SubmitButton" );
		SubmitButton.Connect( "pressed", Callable.From( OnSubmitButtonPressed ) );
	}
};
