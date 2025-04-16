using Godot;

public partial class BugReporter : Control {
	private TextEdit EmailTextEdit;
	private TextEdit TitleTextEdit;
	private TextEdit DescriptionTextEdit;

	[Export]
	private string ConfigPath = "res://bugreport_webhook.cfg";
	[Export]
	private bool HideAfterSend = true;
	[Export]
	private bool ClearAfterSend = true;

	private TextEdit MessageText;
	private LineEdit MailLineEdit;
	private OptionButton Options;
	private Button ScreenshotCheck;
	private TextureRect Screenshot;
	private Button Analytics;
	private Label TextLimit;

	private void OnSubmitButtonPressed() {
		
	}

	private void ReloadConfig() {
		ConfigFile config = new ConfigFile();
		Error err = config.Load( ConfigPath );
	}

	public override void _Ready() {
		base._Ready();
		
		Button SubmitButton = GetNode<Button>( "MarginContainer/VBoxContainer/SubmitButton" );
		SubmitButton.Connect( "pressed", Callable.From( OnSubmitButtonPressed ) );
	}
};
