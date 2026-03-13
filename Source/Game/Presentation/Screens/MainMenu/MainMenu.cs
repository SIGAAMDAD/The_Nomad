using Game.Application.UI;
using Game.Application.UI.Menus.Events;
using Nomad.Audio.Interfaces;
using Nomad.Core.EngineUtils;
using Nomad.Core.EngineUtils.Globals;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils.UserInterface;
using Nomad.Events.Globals;

namespace Game.Presentation.Screens.MainMenu {
	/*
	===================================================================================
	
	MainMenu
	
	===================================================================================
	*/
	/// <summary>
	/// Handles the main menu's creation.
	/// </summary>
	
	public sealed partial class MainMenu : EnginePanel {
		private ISubscriptionGroup _buttonGroup;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			var audioDevice = ServiceLocator.GetService<IAudioDevice>();
			audioDevice.LoadBank( EngineService.GetStoragePath( "Audio/Banks/Desktop/ui.bank", StorageScope.StreamingAssets ) );
			audioDevice.LoadBank( EngineService.GetStoragePath( "Audio/Banks/Desktop/music.bank", StorageScope.StreamingAssets ) );

			var musicService = ServiceLocator.GetService<IMusicService>();
			musicService.PlayTheme( "event:/Music/UserInterface/MainMenuTheme" );

			_buttonGroup = GameEventRegistry.CreateGroup( "MainMenu" );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/QuitGameButton" ).Clicked, OnQuitGameClicked );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/SettingsButton" ).Clicked, OnSettingsMenuButtonClicked );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown() {
			_buttonGroup?.Dispose();
		}

		/*
		===============
		OnSettingsMenuButtonClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSettingsMenuButtonClicked( in EmptyEventArgs args ) {
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
		}

		/*
		===============
		OnQuitGameClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnQuitGameClicked( in EmptyEventArgs args ) {
			EngineService.Quit();
		}
	};
};
