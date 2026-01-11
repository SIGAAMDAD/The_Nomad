using Game.Infrastructure.UI.NomadUI.SelectionNodes.NomadButton;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionSlider;
using Godot;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenuView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class SettingsMenuView( SettingsMenu owner ) {
		public SettingsMenu Owner => owner;

		public NomadButtonView QuitButton => owner.GetNode<NomadButtonNode>( "%BackButton" ).View;
		public NomadButtonView SaveButton => owner.GetNode<NomadButtonNode>( "%SaveButton" ).View;
		public NomadButtonView ResetButton => owner.GetNode<NomadButtonNode>( "%ResetButton" ).View;

		public Control DisplayOptionsContainer => owner.GetNode<Control>( "%Display" );
		public Control GraphicsOptionsContainer => owner.GetNode<Control>( "%Graphics" );
		public Control AudioOptionsContainer =>  owner.GetNode<Control>( "%Audio" );
		public Control AccessibilityOptionsContainer => owner.GetNode<Control>( "%Accessibility" );
		public Control ControlsOptionContainer => owner.GetNode<Control>( "%Controls" );

		public OptionListView WindowResolutionList => _windowResolutionList;
		private readonly OptionListView _windowResolutionList = owner.GetNode<OptionList>( "%WindowResolutionList" ).View;

		public OptionListView VSyncList => _vsyncList;
		private readonly OptionListView _vsyncList = owner.GetNode<OptionList>( "%VSyncList" ).View;

		public OptionListView WindowModeList => _windowModeList;
		private readonly OptionListView _windowModeList = owner.GetNode<OptionList>( "%WindowModeList" ).View;

		public OptionListView AspectRatioList => _aspectRatioList;
		private readonly OptionListView _aspectRatioList = owner.GetNode<OptionList>( "%AspectRatioList" ).View;

		public OptionListView AntiAliasingList => _antiAliasingList;
		private readonly OptionListView _antiAliasingList = owner.GetNode<OptionList>( "%AntiAliasingList" ).View;

		public OptionListView MaxFpsList => _maxFpsList;
		private readonly OptionListView _maxFpsList = owner.GetNode<OptionList>( "%MaxFpsList" ).View;

		public OptionListView MonitorList => _monitorList;
		private readonly OptionListView _monitorList = owner.GetNode<OptionList>( "%MonitorList" ).View;

		public OptionSliderView BrightnessSlider => _brightnessSlider;
		private readonly OptionSliderView _brightnessSlider = owner.GetNode<OptionSlider>( "%BrightnessSlider" ).View;

		public OptionListView OutputDeviceList => _outputDeviceList;
		private readonly OptionListView _outputDeviceList = owner.GetNode<OptionList>( "%OutputDeviceList" ).View;

		public OptionListView AudioDriverList => _audioDriverList;
		private readonly OptionListView _audioDriverList = owner.GetNode<OptionList>( "%DriverAPIList" ).View;

		public OptionCheckboxView EffectsOnToggle => _effectsOnToggle;
		private readonly OptionCheckboxView _effectsOnToggle = owner.GetNode<OptionCheckbox>( "%EffectsOnCheckbox" ).View;

		public OptionSliderView EffectsVolumeSlider => _effectsVolumeSlider;
		private readonly OptionSliderView _effectsVolumeSlider = owner.GetNode<OptionSlider>( "%EffectsVolumeSlider" ).View;

		public OptionCheckboxView MusicOnToggle => _musicOnToggle;
		private readonly OptionCheckboxView _musicOnToggle = owner.GetNode<OptionCheckbox>( "%MusicOnCheckbox" ).View;

		public OptionSliderView MusicVolumeSlider => _musicVolumeSlider;
		private readonly OptionSliderView _musicVolumeSlider = owner.GetNode<OptionSlider>( "%MusicVolumeSlider" ).View;

		public OptionSliderView MasterVolumeSlider => _masterVolumeSlider;
		private readonly OptionSliderView _masterVolumeSlider = owner.GetNode<OptionSlider>( "%MasterVolumeSlider" ).View;

		/*
		public OptionCheckboxView DynamicLighting => _dynamicLighting;
		private readonly OptionCheckboxView _dynamicLighting = owner.GetNode<OptionCheckbox>( "%DynamicLightingToggle" ).View;

		public OptionListView GraphicsPresetList => _graphicsPresetList;
		private readonly OptionListView _graphicsPresetList = owner.GetNode<OptionList>( "%GraphicsPresetList" ).View;

		public OptionListView MaterialDetailsList => _materialDetailsList;
		private readonly OptionListView _materialDetailsList = owner.GetNode<OptionList>( "%MaterialDetailsList" ).View;

		public OptionListView ShadowQualityList => _shadowQualityList;
		private readonly OptionListView _shadowQualityList = owner.GetNode<OptionList>( "%ShadowQualityList" ).View;
		*/
	};
};