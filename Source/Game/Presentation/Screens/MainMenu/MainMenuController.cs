using Game.Application.UI;
using Game.Application.UI.Menus.Events;
using Game.Domain.Events.UI;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Events.Global;
using System;

namespace Game.Presentation.Screens.MainMenu {
	/*
	===================================================================================
	
	MainMenuController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class MainMenuController : IDisposable {
		private readonly MainMenuView _view;

		private readonly ISubscriptionHandle _clicked;

		private bool _isDisposed = false;

		/*
		===============
		MainMenuController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="locator"></param>
		/// <param name="view"></param>
		public MainMenuController( MainMenuView view ) {
			_view = view;

			var audioDevice = ServiceLocator.GetService<IAudioDevice>();
			audioDevice.LoadBank( "res://Assets/Audio/Banks/Desktop/ui.bank" );
			audioDevice.LoadBank( "res://Assets/Audio/Banks/Desktop/music.bank" );

			var musicService = ServiceLocator.GetService<IMusicService>();
			musicService.PlayTheme( "event:/Music/UserInterface/MainMenuTheme" );

			_clicked = GameEventRegistry.GetEvent<ButtonClickedEventArgs>( UIConstants.BUTTON_CLICKED_EVENT, UIConstants.NAMESPACE ).Subscribe( OnButtonClicked );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if ( !_isDisposed ) {
				_clicked?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnButtonClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="Exception"></exception>
		private void OnButtonClicked( in ButtonClickedEventArgs args ) {
			if ( args.ButtonId == _view.NewGameButton.ButtonId ) {
			} else if ( args.ButtonId == _view.CreditsButton.ButtonId ) {				
			} else if ( args.ButtonId == _view.ExtrasButton.ButtonId ) {
				GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
			} else if ( args.ButtonId == _view.SettingsButton.ButtonId ) {
				GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
			} else if ( args.ButtonId == _view.QuitGameButton.ButtonId ) {
				System.Environment.Exit( 0 );
			} else {
				throw new Exception( $"Invalid ButtonId for MainMenu - {args.ButtonId}" );
			}
		}
	};
};
