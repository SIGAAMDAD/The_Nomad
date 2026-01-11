using Game.Application.UI;
using Game.Application.UI.Menus.Events;
using Game.Domain.Events.UI;
using Game.Infrastructure;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Interfaces;
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
		public MainMenuController( IServiceLocator locator, MainMenuView view ) {
			_view = view;

			var audioDevice = locator.GetService<IAudioDevice>();
			audioDevice.LoadBank( "res://Assets/Audio/Banks/Desktop/ui.bank" );
			audioDevice.LoadBank( "res://Assets/Audio/Banks/Desktop/music.bank" );

			var musicService = locator.GetService<IMusicService>();
			musicService.PlayTheme( "event:/Music/UserInterface/MainMenuTheme" );

			var eventFactory = locator.GetService<IGameEventRegistryService>();
			UIEventHelper.SubscribeToUIEvent<ButtonClickedEventArgs>( eventFactory, this, UIConstants.BUTTON_CLICKED_EVENT, OnButtonClicked );
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
			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.UnsubscribeFromUIEvent<ButtonClickedEventArgs>( eventFactory, this, UIConstants.BUTTON_CLICKED_EVENT, OnButtonClicked );
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
			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			if ( args.ButtonId == _view.NewGameButton.ButtonId ) {
			//	UIEventHelper.PublishUIEvent( eventFactory, UIConstants.MENU_TRANSITION_REQUESTED_EVENT, new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.CharacterCreation ) );
			} else if ( args.ButtonId == _view.CreditsButton.ButtonId ) {
				
			} else if ( args.ButtonId == _view.ExtrasButton.ButtonId ) {
				UIEventHelper.PublishUIEvent( eventFactory, UIConstants.MENU_TRANSITION_REQUESTED_EVENT, new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
			} else if ( args.ButtonId == _view.SettingsButton.ButtonId ) {
				UIEventHelper.PublishUIEvent( eventFactory, UIConstants.MENU_TRANSITION_REQUESTED_EVENT, new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
			} else if ( args.ButtonId == _view.QuitGameButton.ButtonId ) {
				System.Environment.Exit( 0 );
			} else {
				throw new Exception( $"Invalid ButtonId for MainMenu - {args.ButtonId}" );
			}
		}
	};
};