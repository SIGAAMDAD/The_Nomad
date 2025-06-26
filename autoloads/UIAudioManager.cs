using Godot;

public partial class UIAudioManager : Node {
	private static AudioStreamPlayer UISfxStream;
	private static AudioStreamPlayer UIMusicStream;

	private static AudioStream ButtonPressed;
	private static AudioStream ButtonFocused;
	private static AudioStream Activate;
	private static AudioStream LoopingTheme;
	private static AudioStream IntroTheme;

	private void OnSettingsChanged() {
		UISfxStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
	}

	public override void _Ready() {
		base._Ready();

		ButtonPressed = ResourceLoader.Load<AudioStream>( "res://sounds/ui/button_pressed.ogg" );
		ButtonFocused = ResourceLoader.Load<AudioStream>( "res://sounds/ui/button_focused.ogg" );
		Activate = ResourceLoader.Load<AudioStream>( "res://sounds/ui/begin_game.ogg" );

		IntroTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_intro.ogg" );
		LoopingTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" );

		UISfxStream = new AudioStreamPlayer();
		UISfxStream.Name = "UISfxStream";
		UISfxStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( UISfxStream );

		UIMusicStream = new AudioStreamPlayer();
		UIMusicStream.Name = "UIMusicStream";
		AddChild( UIMusicStream );

		SettingsData.Instance.SettingsChanged += OnSettingsChanged;
	}
	private static void OnMenuIntroThemeFinished() {
		UIMusicStream.Stream = LoopingTheme;
		UIMusicStream.Set( "parameters/looping", true );
		UIMusicStream.Disconnect( "finished", Callable.From( OnMenuIntroThemeFinished ) );
		UIMusicStream.Play();
	}

	public static void OnButtonFocused() {
		UISfxStream.Stream = ButtonFocused;
		UISfxStream.Play();
	}
	public static void OnButtonPressed() {
		UISfxStream.Stream = ButtonPressed;
		UISfxStream.Play();
	}
	public static void OnActivate() {
		UISfxStream.Stream = Activate;
		UISfxStream.Play();
	}
	public static void PlayCustomSound( AudioStream stream ) {
		UISfxStream.Stream = stream;
		UISfxStream.Play();
	}
	public static void PlayTheme() {
		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
		UIMusicStream.Stream = IntroTheme;
		if ( !UIMusicStream.IsConnected( "finished", Callable.From( OnMenuIntroThemeFinished ) ) ) {
			UIMusicStream.Connect( "finished", Callable.From( OnMenuIntroThemeFinished ) );
		}
		UIMusicStream.Play();
	}
	private static void OnAudioFadeFinished() {
		UIMusicStream.Stop();
	}
	public static void FadeMusic() {
		Node Root = (Node)Engine.GetMainLoop().Get( "root" );

		Tween AudioFade = Root.GetTree().CreateTween();
		AudioFade.TweenProperty( UIMusicStream, "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );
	}
};