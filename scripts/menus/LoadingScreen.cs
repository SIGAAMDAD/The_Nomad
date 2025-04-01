using System.Diagnostics.Tracing;
using Godot;

public partial class LoadingScreen : CanvasLayer {
	private Label TipLabel;
	private Label ProgressLabel;
	private CanvasLayer TransitionScreen;
	private bool FadingOut = false;
	private Timer ImageChange;
	private System.Random random = new System.Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );

	private readonly System.Collections.Generic.List<string> Tips = new System.Collections.Generic.List<string>{
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
		"You can slice bullets in half, just make sure whatever's behind you can take the hit",
		"You are literally too angry to die",
		"Stop hiding behind cover like a little coward",
		"Don't blame the game for your skill issue"
	};

	private void OnImageChangeTimeout() {
		int tipIndex = random.Next( 0, Tips.Count - 1 );
		string newTip = Tips[ tipIndex ];
		if ( newTip == TipLabel.Text ) {
			if ( tipIndex == Tips.Count - 1 ) {
				tipIndex = 0;
			} else {
				tipIndex++;
			}
		}
		TipLabel.Text = Tips[ tipIndex ];
	}

	private void OnTransitionFinished() {
		Visible = false;
	}
	public void FadeOut() {
		TransitionScreen.Call( "transition" );

		ImageChange.SetProcess( false );
		ImageChange.SetProcessInternal( false );
		SetProcess( false );
		SetProcessInternal( false );
	}
	public void FadeIn() {
		TransitionScreen.Call( "transition" );
		Visible = true;

		ImageChange.SetProcess( true );
		ImageChange.SetProcessInternal( true );
		SetProcess( true );
		SetProcessInternal( true );
	}

	public override void _Ready() {
		base._Ready();

		ImageChange = GetNode<Timer>( "ImageChange" );
		ImageChange.Connect( "timeout", Callable.From( OnImageChangeTimeout ) );

		TipLabel = GetNode<Label>( "Tips/TipLabel" );
		TipLabel.SetProcess( false );
		TipLabel.SetProcessInternal( false );
		TipLabel.Show();

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnTransitionFinished ) );

		SetProcess( false );
		SetProcessInternal( false );
	}
};