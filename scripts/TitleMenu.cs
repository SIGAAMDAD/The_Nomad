using Godot;

public partial class TitleMenu : Control {
	public enum MenuState {
		Main,
		Campaign,
		Extras,
		Settings,
		Help,
		Credits,
		Mods
	};

	private ExtrasMenu ExtrasMenu;
	private SettingsMenu SettingsMenu;
	private StoryMenu StoryMenu;
//	private DemoMenu DemoMenu;
	private MainMenu MainMenu;
	private CreditsMenu CreditsMenu;
	private Button ExitButton;

	private LobbyBrowser LobbyBrowser;
	private LobbyFactory LobbyFactory;

	private Control CurrentMenu;

	private MenuState State = MenuState.Main;

	private void OnExitButtonPressed() {
		UIAudioManager.OnButtonPressed();

		int index = 0;

		switch ( State ) {
		case MenuState.Campaign:
			index = StoryMenu.GetIndex();
			RemoveChild( StoryMenu );
			StoryMenu.QueueFree();
			StoryMenu = null;
			break;
		case MenuState.Extras:
			index = ExtrasMenu.GetIndex();
			RemoveChild( ExtrasMenu );
			ExtrasMenu.QueueFree();
			ExtrasMenu = null;
			break;
		case MenuState.Settings:
			index = SettingsMenu.GetIndex();
			RemoveChild( SettingsMenu );
			SettingsMenu.QueueFree();
			SettingsMenu = null;
			break;
		case MenuState.Credits:
			index = CreditsMenu.GetIndex();
			RemoveChild( CreditsMenu );
			CreditsMenu.QueueFree();
			CreditsMenu = null;
			break;
		default:
			Console.PrintError( "Invalid menu state!" );
			break;
		};

		AddChild( MainMenu );
		MoveChild( MainMenu, index );
		//AddChild( DemoMenu );
		//MoveChild( DemoMenu, index );

		ExitButton.Hide();
		MainMenu.Show();
		//DemoMenu.Show();
		State = MenuState.Main;
	}

	private void OnMainMenuStoryMenu() {
		StoryMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/story_menu.tscn" ).Instantiate<StoryMenu>();

		int index = MainMenu.GetIndex();
		RemoveChild( MainMenu );
		//int index = DemoMenu.GetIndex();
		//RemoveChild( DemoMenu );
		AddChild( StoryMenu );
		MoveChild( StoryMenu, index );

		ExitButton.Show();
		State = MenuState.Campaign;
	}
	private void OnMainMenuExtrasMenu() {
		ExtrasMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/extras_menu.tscn" ).Instantiate<ExtrasMenu>();

		int index = MainMenu.GetIndex();
		RemoveChild( MainMenu );
		//int index = DemoMenu.GetIndex();
		//RemoveChild( DemoMenu );
		AddChild( ExtrasMenu );
		MoveChild( ExtrasMenu, index );

		ExitButton.Show();
		State = MenuState.Extras;
	}
	private void OnMainMenuSettingsMenu() {
		SettingsMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/settings_menu.tscn" ).Instantiate<SettingsMenu>();

		int index = MainMenu.GetIndex();
		RemoveChild( MainMenu );
		//int index = DemoMenu.GetIndex();
		//RemoveChild( DemoMenu );
		AddChild( SettingsMenu );
		MoveChild( SettingsMenu, index );

		ExitButton.Show();
		State = MenuState.Settings;
	}
	private void OnMainMenuCreditsMenu() {
		CreditsMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/credits_menu.tscn" ).Instantiate<CreditsMenu>();

		int index = MainMenu.GetIndex();
		RemoveChild( MainMenu );
//		int index = DemoMenu.GetIndex();
//		RemoveChild( DemoMenu );
		AddChild( CreditsMenu );
		MoveChild( CreditsMenu, index );

		ExitButton.Show();
		State = MenuState.Credits;
	}

	private void ReleaseAll() {
		ExtrasMenu?.Free();
		SettingsMenu?.Free();
	}

	private void OnKonamiCodeActivated() {
		Console.PrintLine( "========== Meme Mode Activated ==========" );
		GameConfiguration.MemeMode = true;
		UIAudioManager.PlayCustomSound( ResourceLoader.Load<AudioStream>( "res://sounds/ui/meme_mode_activated.ogg" ) );
		SteamAchievements.ActivateAchievement( "ACH_DNA_OF_THE_SOUL" );
	}

	public override void _Ready() {
		base._Ready();

		PhysicsServer2D.SetActive( false );

		Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://cursor_n.png" ) );

		AddChild( ResourceLoader.Load<PackedScene>( "res://scenes/menus/menu_background.tscn" ).Instantiate<Control>() );

		Node KonamiCode = GetNode( "KonamiCode" );
		KonamiCode.Connect( "success", Callable.From( OnKonamiCodeActivated ) );

		/*
		DemoMenu = GetNode<DemoMenu>( "DemoMenu" );
		DemoMenu.SetProcess( true );
		DemoMenu.SetProcessInternal( true );
		DemoMenu.SetProcessUnhandledInput( true );
		DemoMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		DemoMenu.Connect( "CreditsMenu", Callable.From( OnMainMenuCreditsMenu ) );
		DemoMenu.BeginGame += ReleaseAll;
		*/

		MainMenu = GetNode<MainMenu>( "MainMenu" );
		MainMenu.SetProcess( true );
		MainMenu.SetProcessInternal( true );
		MainMenu.SetProcessUnhandledInput( true );
		MainMenu.Connect( "StoryMenu", Callable.From( OnMainMenuStoryMenu ) );
		MainMenu.Connect( "ExtrasMenu", Callable.From( OnMainMenuExtrasMenu ) );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.Connect( "CreditsMenu", Callable.From( OnMainMenuCreditsMenu ) );

		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "focus_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		ExitButton.Connect( "mouse_entered", Callable.From( UIAudioManager.OnButtonFocused ) );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		GetTree().CurrentScene = this;

		SetProcess( false );
		SetProcessInternal( false );
	}
};
