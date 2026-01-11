using Godot;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================

	SettingsMenu

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class SettingsMenu : Control {
		private SettingsMenuView _view;
		private SettingsMenuController _controller;

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			_view = new SettingsMenuView( this );
			_controller = new SettingsMenuController( _view );
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