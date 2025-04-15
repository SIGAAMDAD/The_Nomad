using Godot;
using System;

public partial class BugReporter : Control {
	private TextEdit EmailTextEdit;
	private TextEdit TitleTextEdit;
	private TextEdit DescriptionTextEdit;

	/*
	private void OnSubmitButtonPressed() {
		string body = string.Format( "email: {0}\n\ndescription: {1}", EmailTextEdit.Text, DescriptionTextEdit.Text );
		string url = string.Format( "https://github.com/SIGAAMDAD/the-nomad/issues/new?title={0}&body={1}", TitleTextEdit.Text.URIEncode(), body.URIEncode() );

		Console.PrintLine( string.Format( "Sending bug report, url: {0}", url ) );

		OS.Shell( url );
	}
	*/

	public override void _Ready() {
		base._Ready();
		
		Button SubmitButton = GetNode<Button>( "MarginContainer/VBoxContainer/SubmitButton" );
	//	SubmitButton.Connect( "pressed", Callable.From( OnSubmitButtonPressed ) );
	}
};
