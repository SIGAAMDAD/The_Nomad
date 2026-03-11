using Godot;
using Nomad.EngineUtils.UserInterface;

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
		private MainMenuView _view;
		private MainMenuController _controller;

		/*
		===============
		OnInit
		===============
		*/
		protected override void OnInit() {
			_view = new MainMenuView( this );
			_controller = new MainMenuController(
				_view
			);
		}

		/*
		===============
		OnShutdown
		===============
		*/
		protected override void OnShutdown() {
			_controller.Dispose();
		}
	};
};
