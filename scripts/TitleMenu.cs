using Godot;

public partial class TitleMenu : Control {
	public enum MenuState {
		Main,
		Campaign,
		Extras,
		Settings,
		Help,
		Mods
	};

	private AudioStreamPlayer UIChannel;
	private AudioStreamPlayer MusicTheme;

	private ExtrasMenu ExtrasMenu;
	private SettingsMenu SettingsMenu;
	private MainMenu MainMenu;
	private Button ExitButton;

	private LobbyBrowser LobbyBrowser;
	private LobbyFactory LobbyFactory;

	private AudioStream LoopingTheme;

	private Control CurrentMenu;

	private MenuState State = MenuState.Main;

    private void OnExitButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		int index = 0;

		switch ( State ) {
		case MenuState.Extras:
			index = ExtrasMenu.GetIndex();
			RemoveChild( ExtrasMenu );
			break;
		case MenuState.Settings:
			index = SettingsMenu.GetIndex();
			RemoveChild( SettingsMenu );
			break;
		default:
			Console.PrintError( "Invalid menu state!" );
			break;
		};

		AddChild( MainMenu );
		MoveChild( MainMenu, index );

		ExitButton.Hide();
		MainMenu.Show();
		State = MenuState.Main;
	}
	private void OnMainMenuExtrasMenu() {
		ExtrasMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/extras_menu.tscn" ).Instantiate<ExtrasMenu>();

		int index = MainMenu.GetIndex();
		RemoveChild( MainMenu );
		AddChild( ExtrasMenu );
		MoveChild( ExtrasMenu, index );

		ExitButton.Show();
		State = MenuState.Extras;
	}
	private void OnMainMenuSettingsMenu() {
		SettingsMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/settings_menu.tscn" ).Instantiate<SettingsMenu>();

		int index = MainMenu.GetIndex();
		RemoveChild( MainMenu );
		AddChild( SettingsMenu );
		MoveChild( SettingsMenu, index );

		ExitButton.Show();
		State = MenuState.Settings;
	}

	private void ReleaseAll() {
		ExtrasMenu?.Free();
		SettingsMenu?.Free();
	}

	public override void _Ready() {
		PhysicsServer2D.SetActive( false );

		Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://cursor_n.png" ) );

		Control Background = GetNode<Control>( "MenuBackground" );
		Background.SetProcess( false );
		Background.SetProcessInternal( false );

		MainMenu = GetNode<MainMenu>( "MainMenu" );
		MainMenu.SetProcess( true );
		MainMenu.SetProcessInternal( true );
		MainMenu.SetProcessUnhandledInput( true );
		MainMenu.Connect( "ExtrasMenu", Callable.From( OnMainMenuExtrasMenu ) );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.BeginGame += ReleaseAll;

		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );
		UIChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		MusicTheme = GetNode<AudioStreamPlayer>( "Theme" );
		MusicTheme.VolumeDb = SettingsData.GetMusicVolumeLinear();
		MusicTheme.Connect( "finished", Callable.From( OnThemeIntroFinished ) );

		SettingsData.Instance.MusicVolumeChanged += () => { MusicTheme.VolumeDb = SettingsData.GetMusicVolumeLinear(); };
		SettingsData.Instance.EffectsVolumeChanged += () => { UIChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear(); };

		LoopingTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" );

		GetTree().CurrentScene = this;

		SetProcess( false );
		SetProcessInternal( false );
	}

	private void OnThemeIntroFinished() {
		Console.PrintLine( "Menu intro theme finished, moving to loop..." );

		MusicTheme.Stream = LoopingTheme;
		MusicTheme.Play();
		MusicTheme.Set( "parameters/looping", true );
		MusicTheme.SetProcess( false );
		MusicTheme.SetProcessInternal( false );
		MusicTheme.Disconnect( "finished", Callable.From( OnThemeIntroFinished ) );
	}
}
