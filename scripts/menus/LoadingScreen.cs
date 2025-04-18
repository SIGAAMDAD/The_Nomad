using System.Collections.Generic;
using DialogueManagerRuntime;
using Godot;

public partial class LoadingScreen : CanvasLayer {
	private Label TipLabel;
	private Label ProgressLabel;
	private MenuBackground Background;
	private Range Spinner;
	private bool FadingOut = false;
	private Timer ImageChange;
	private System.Random random = new System.Random();

	private readonly string[] Tips = [
		"You can parry anything that's flashing green, including blades, bullets, etc.",
		"Dashing into an enemy will send them flying",
		"Just parry the bullet dude!",
		"Different enemies require different tactics. No more brainless shooting!",
		"You're a 500 pound hunk of muscle and metal, use that to your advantage",
		"The harder you are hit, the harder you hit back",
		"Death follows you everywhere you go",
		"Don't be scared to experiment",
		"Bathe in the blood of your enemies for some health",
		"Remember: warcrimes don't exist anymore!",
		"A mission can count as a stealth mission if there aren't any witnesses",
		"There are tips here, y'know, read 'em",
		"Always keep in mind that STEALTH is optional",
		"ANYTHING and EVERYTHING is a weapon",
		"Parry that you filthy casual",
		"This game won't baby you, so stop acting like a child",
		"You can slice bullets in half, just make sure whatever's behind you can take the hit",
		"You are literally too angry to die",
		"Stop hiding behind cover like a little coward",
		"Don't blame the game for your skill issue"
	];

	private void OnImageChangeTimeout() {
		int tipIndex = random.Next( 0, Tips.Length - 1 );
		string newTip = Tips[ tipIndex ];
		if ( newTip == TipLabel.Text ) {
			if ( tipIndex == Tips.Length - 1 ) {
				tipIndex = 0;
			} else {
				tipIndex++;
			}
		}
		TipLabel.Text = Tips[ tipIndex ];

		ImageChange.Start();
	}

	private void OnFadeOutFinished() {
		Hide();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnFadeOutFinished ) );

		ImageChange.Stop();
		Spinner.SetProcess( false );
	}
	public void FadeOut() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnFadeOutFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
	}
	public void FadeIn() {
		Spinner.SetProcess( true );

		Show();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		ImageChange.Start();
		OnImageChangeTimeout();
	}

	public override void _Ready() {
		base._Ready();

		ImageChange = GetNode<Timer>( "ImageChange" );
		ImageChange.SetProcess( false );
		ImageChange.SetProcessInternal( false );
		ImageChange.Connect( "timeout", Callable.From( OnImageChangeTimeout ) );

		Background = GetNode<MenuBackground>( "MenuBackground" );

		TipLabel = GetNode<Label>( "Tips/TipLabel" );
		TipLabel.SetProcess( false );
		TipLabel.SetProcessInternal( false );
		TipLabel.Show();

		Spinner = GetNode<Range>( "Tips/Spinner" );
		Spinner.SetProcess( false );
		Spinner.SetProcessInternal( false );
		Spinner.Show();

		SetProcess( false );
		SetProcessInternal( false );
	}
};