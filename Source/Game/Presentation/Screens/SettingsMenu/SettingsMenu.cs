using Game.Application.UI;
using Game.Application.UI.Menus.Events;
using Nomad.Core.Events;
using Nomad.EngineUtils.UserInterface;
using Nomad.Events.Globals;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	
	public partial class SettingsMenu : EnginePanel {
		private ISubscriptionGroup _buttonGroup;

		public SettingsMenu() {
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_buttonGroup = GameEventRegistry.CreateGroup( "SettingsMenuButtons" );
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

		private void OnBackPressed( in EmptyEventArgs args ) {
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Settings, MenuState.Main ) );
		}
	};
};
