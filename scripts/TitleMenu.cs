/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using ResourceCache;
using System;

namespace Menus {
	/*
	===================================================================================

	TitleMenu

	===================================================================================
	*/

	public partial class TitleMenu : Control {
		private enum MenuState {
			Main,
			SaveSlots,
			Extras,
			Settings,
			Help,
			Credits,
			Mods
		};

		private static readonly StringName @ExitMenuSignalName = "ExitMenu";

		private ExtrasMenu ExtrasMenu;
		private SettingsMenu SettingsMenu;
		private SaveSlotsMenu StoryMenu;
		//	private DemoMenu DemoMenu;
		private MainMenu MainMenu;
		private CreditsMenu CreditsMenu;

		private LobbyBrowser LobbyBrowser;
		private LobbyFactory LobbyFactory;

		private Control CurrentMenu;

		private MenuState State = MenuState.Main;

		/*
		===============
		InitMainMenu
		===============
		*/
		/// <summary>
		/// Connects relevant signals to the main menu
		/// </summary>
		private void InitMainMenu() {
			MainMenu.SetProcess( true );
			MainMenu.SetProcessInternal( true );
			MainMenu.SetProcessUnhandledInput( true );

			CurrentMenu ??= MainMenu;

			GameEventBus.ConnectSignal( MainMenu, MainMenu.SignalName.SaveSlotsMenu, this, OnMainMenuSaveSlotsMenu );
			GameEventBus.ConnectSignal( MainMenu, MainMenu.SignalName.ExtrasMenu, this, OnMainMenuExtrasMenu );
			GameEventBus.ConnectSignal( MainMenu, MainMenu.SignalName.SettingsMenu, this, OnMainMenuSettingsMenu );
			GameEventBus.ConnectSignal( MainMenu, MainMenu.SignalName.CreditsMenu, this, OnMainMenuCreditsMenu );
		}

		private void SetMenu<T>( ref T newMenu, MenuState newState, string scenePath ) where T : Control {
			int index = CurrentMenu.GetIndex();
			RemoveChild( CurrentMenu );
			CurrentMenu.QueueFree();

			newMenu = SceneCache.GetScene( scenePath ).Instantiate<T>();
			AddChild( newMenu );
			MoveChild( newMenu, index );

			CurrentMenu = newMenu;
			State = newState;

			if ( newMenu != MainMenu ) {
				GameEventBus.ConnectSignal( newMenu, ExitMenuSignalName, this, Callable.From( OnExitButtonPressed ) );
			}
			newMenu.Show();
		}

		/*
		===============
		OnExitButtonPressed
		===============
		*/
		private void OnExitButtonPressed() {
			UIAudioManager.OnButtonPressed();
			SetMenu( ref MainMenu, MenuState.Main, "res://scenes/menus/main_menu.tscn" );
			InitMainMenu();
		}

		/*
		===============
		OnMainMenuSaveSlotsMenu
		===============
		*/
		private void OnMainMenuSaveSlotsMenu() {
			SetMenu( ref StoryMenu, MenuState.SaveSlots, "res://scenes/menus/save_slots_menu.tscn" );
		}

		/*
		===============
		OnMainMenuExtrasMenu
		===============
		*/
		private void OnMainMenuExtrasMenu() {
			SetMenu( ref ExtrasMenu, MenuState.Extras, "res://scenes/menus/extras_menu.tscn" );
		}

		/*
		===============
		OnMainMenuSettingsMenu
		===============
		*/
		private void OnMainMenuSettingsMenu() {
			SetMenu( ref SettingsMenu, MenuState.Settings, "res://scenes/menus/settings_menu.tscn" );
		}

		/*
		===============
		OnMainMenuCreditsMenu
		===============
		*/
		private void OnMainMenuCreditsMenu() {
			SetMenu( ref CreditsMenu, MenuState.Credits, "res://scenes/menus/credits_menu.tscn" );
		}

		private void ReleaseAll() {
			ExtrasMenu?.QueueFree();
			SettingsMenu?.QueueFree();
			StoryMenu?.QueueFree();
		}

		/*
		===============
		OnKonamiCodeActivated
		===============
		*/
		private void OnKonamiCodeActivated() {
			Console.PrintLine( "========== Meme Mode Activated ==========" );
			GameConfiguration.MemeMode = true;
			UIAudioManager.PlayCustomSound( AudioCache.GetStream( "res://sounds/ui/meme_mode_activated.ogg" ) );
			Steam.SteamAchievements.ActivateAchievement( "ACH_DNA_OF_THE_SOUL" );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// Loads and initializes the menu system
		/// </summary>
		public override void _Ready() {
			base._Ready();

			PhysicsServer2D.SetActive( false );
			UIAudioManager.PlayTheme();
			Input.SetCustomMouseCursor( TextureCache.GetTexture( "res://cursor_n.png" ) );

			AddChild( SceneCache.GetScene( "res://scenes/menus/menu_background.tscn" ).Instantiate<Control>() );

			GameEventBus.ConnectSignal( GetNode( "KonamiCode" ), "success", this, OnKonamiCodeActivated );

			MainMenu = GetNode<MainMenu>( "MainMenu" );
			InitMainMenu();

			GetTree().CurrentScene = this;
		}
	};
};