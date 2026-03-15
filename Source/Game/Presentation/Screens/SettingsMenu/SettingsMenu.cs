using Game.Application.UI;
using Game.Application.UI.Menus.Events;
using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.EngineUtils.UserInterface;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class SettingsMenu : EnginePanel {
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
			_buttonGroup = GameEventRegistry.GetGroup( "SettingsMenuButtons" );
			_buttonGroup.Add( FindChild<EngineButton>( "BottomContainer/ButtonContainer/BackButton" ).Clicked, OnBackPressed );
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
		OnBackPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBackPressed( in EmptyEventArgs args ) {
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Settings, MenuState.Main ) );
		}
	};
};
