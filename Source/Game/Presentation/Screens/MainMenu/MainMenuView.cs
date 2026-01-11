using Godot;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.NomadButton;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;

namespace Game.Presentation.Screens.MainMenu {
	/*
	===================================================================================

	MainMenuView

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class MainMenuView( Control owner ) {
		public Control Owner => owner;

		public NomadButtonView NewGameButton => _newGameButton;
		private readonly NomadButtonView _newGameButton = owner.GetNode<NomadButtonNode>( "%NewGameButton" ).View;

		public INomadButtonView LoadGameButton => _loadGameButton;
		private readonly NomadButtonView _loadGameButton = owner.GetNode<NomadButtonNode>( "%LoadGameButton" ).View;

		public NomadButtonView ExtrasButton => _extrasButton;
		private readonly NomadButtonView _extrasButton = owner.GetNode<NomadButtonNode>( "%ExtrasButton" ).View;

		public NomadButtonView SettingsButton => _settingsButton;
		private readonly NomadButtonView _settingsButton = owner.GetNode<NomadButtonNode>( "%SettingsButton" ).View;

		public NomadButtonView CreditsButton => _creditsButton;
		private readonly NomadButtonView _creditsButton = owner.GetNode<NomadButtonNode>( "%CreditsButton" ).View;

		public NomadButtonView QuitGameButton => _quitGameButton;
		private readonly NomadButtonView _quitGameButton = owner.GetNode<NomadButtonNode>( "%QuitGameButton" ).View;
	};
};