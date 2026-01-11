using Game.Infrastructure;
using Godot;

namespace Game.Presentation.Screens.MainMenu {
	/*
	===================================================================================
	
	MainMenu
	
	===================================================================================
	*/
	/// <summary>
	/// Handles the main menu's creation.
	/// </summary>
	
	public sealed partial class MainMenu : Control {
		private MainMenuView _view;
		private MainMenuController _controller;

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var bootstrapper = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" );

			_view = new MainMenuView( this );
			_controller = new MainMenuController(
				bootstrapper.ServiceLocator,
				_view
			);
		}

		/*
		===============
		_ExitTree
		===============
		*/
		public override void _ExitTree() {
			base._ExitTree();

			_controller.Dispose();
		}
	};
};