/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using EventSystem;
using Menus.SelectionNodes;
using System;
using Settings;
using Menus.Settings.Updaters;

namespace Menus.Settings.Options {
	/*
	===================================================================================
	
	DisplayOptionManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class DisplayOptionManager : OptionContainer {
		private const int WindowMode_Windowed = 0;
		private const int WindowMode_Fullscreen = 1;
		private const WindowMode WindowMode_WindowedMapping = WindowMode.BorderlessWindowed;
		private const WindowMode WindowMode_FullscreenMapping = WindowMode.ExclusiveFullscreen;

		private const int AntiAliasing_None = 0;
		private const int AntiAliasing_EdgeAA = 1;
		private const int AntiAliasing_ScreenSpace = 2;
		private const AntiAliasing AntiAliasing_NoneMapping = AntiAliasing.None;
		private const AntiAliasing AntiAliasing_EdgeAAMapping = AntiAliasing.MSAA_2x;
		private const AntiAliasing AntiAliasing_ScreenSpaceMapping = AntiAliasing.FXAA;

		private const int VSync_Off = 0;
		private const int VSync_On = 1;
		private const VSyncMode VSync_OffMapping = VSyncMode.Off;
		private const VSyncMode VSync_OnMapping = VSyncMode.On;

		private DisplayConfig? Temp;

		private OptionList? ResolutionOption;
		private OptionList? VSync_Simple;
		private OptionList? WindowModeOption_Simple;
		private OptionList? AntiAliasingOption_Simple;
		private OptionList? VSync;
		private OptionList? WindowModeOption;
		private OptionList? AntiAliasingOption;
		private OptionList? MaxFps;
		private OptionList? PerformanceOverlay;

		/*
		===============
		SetConfig
		===============
		*/
		/// <summary>
		/// Initializes the display configuration values.
		/// </summary>
		/// <param name="config"></param>
		public override void SetConfig( in ConfigHandler config ) {
			Temp = config as DisplayConfig;
		}

		/*
		===============
		OnVSyncSimpleChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnVSyncSimpleChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				ArgumentNullException.ThrowIfNull( VSync );

				Temp[ "VSyncMode" ] = valueChanged.Value switch {
					VSync_On => VSync_OnMapping,
					VSync_Off => VSync_OffMapping,
					_ => VSync_OffMapping
				};
				VSync.SetValue( (VSyncMode)Temp[ "VSyncMode" ] );
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnVSyncAdvancedChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnVSyncAdvancedChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( VSync_Simple );
				ArgumentNullException.ThrowIfNull( Temp );

				Temp[ "VSyncMode" ] = (VSyncMode)valueChanged.Value;
				VSync_Simple.SetValue(
					Temp[ "VSyncMode" ] switch {
						VSync_OffMapping => VSync_Off,
						VSync_OnMapping => VSync_On,
						_ => SettingsManager.CUSTOM_SETTING_VALUE
					}
				);
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnWindowModeSimpleChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnWindowModeSimpleChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( WindowModeOption );
				ArgumentNullException.ThrowIfNull( Temp );

				Temp[ "WindowMode" ] = valueChanged.Value switch {
					WindowMode_Windowed => WindowMode_WindowedMapping,
					WindowMode_Fullscreen => WindowMode_FullscreenMapping,
					_ => WindowMode_FullscreenMapping
				};
				WindowModeOption.SetValue( (WindowMode)Temp[ "WindowMode" ] );
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnWindowModeAdvancedChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnWindowModeAdvancedChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( WindowModeOption_Simple );
				ArgumentNullException.ThrowIfNull( Temp );

				Temp[ "WindowMode" ] = (WindowMode)valueChanged.Value;
				WindowModeOption_Simple.SetValue(
					Temp[ "WindowMode" ] switch {
						WindowMode_WindowedMapping => WindowMode_Windowed,
						WindowMode_FullscreenMapping => WindowMode_Fullscreen,
						_ => SettingsManager.CUSTOM_SETTING_VALUE
					}
				);
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnAntiAliasingSimpleChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnAntiAliasingSimpleChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( AntiAliasingOption );
				ArgumentNullException.ThrowIfNull( Temp );

				Temp[ "AntiAliasing" ] = valueChanged.Value switch {
					AntiAliasing_None => AntiAliasing_NoneMapping,
					AntiAliasing_EdgeAA => AntiAliasing_EdgeAAMapping,
					AntiAliasing_ScreenSpace => AntiAliasing_ScreenSpaceMapping,
					_ => AntiAliasing_EdgeAAMapping
				};
				AntiAliasingOption.SetValue( (AntiAliasing)Temp[ "AntiAliasing" ] );
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnAntiAliasingAdvancedChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnAntiAliasingAdvancedChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( AntiAliasingOption_Simple );
				ArgumentNullException.ThrowIfNull( Temp );

				Temp[ "AntiAliasing" ] = (AntiAliasing)valueChanged.Value;
				AntiAliasingOption_Simple.SetValue(
					Temp[ "AntiAliasing" ] switch {
						AntiAliasing_NoneMapping => AntiAliasing_None,
						AntiAliasing_ScreenSpaceMapping => AntiAliasing_ScreenSpace,
						AntiAliasing_EdgeAAMapping => AntiAliasing_EdgeAA,
						_ => SettingsManager.CUSTOM_SETTING_VALUE
					}
				);
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnWindowResolutionChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnWindowResolutionChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {

			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnMaxFpsValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnMaxFpsValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {

			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnPerformanceOverlayValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		/// <exception cref="InvalidCastException"></exception>
		private void OnPerformanceOverlayValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( PerformanceOverlay );

				bool visibility;
				ProcessModeEnum processMode;
				int style;

				switch ( (PerformanceOverlayPreset)valueChanged.Value ) {
					case PerformanceOverlayPreset.Hidden:
						processMode = ProcessModeEnum.Disabled;
						visibility = false;
						style = 0;
						break;
					case PerformanceOverlayPreset.FpsOnly:
						processMode = ProcessModeEnum.Always;
						visibility = true;
						style = 0;
						break;
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		LinkBasicDisplaySettings
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void LinkBasicDisplaySettings() {
			VSync_Simple = GetNode<OptionList>( "OptionsContainer/BasicContainer/VSyncList" );
			VSync_Simple.ValueChanged.Subscribe( this, OnVSyncSimpleChanged );

			WindowModeOption_Simple = GetNode<OptionList>( "OptionsContainer/BasicContainer/WindowModeList" );
			WindowModeOption_Simple.ValueChanged.Subscribe( this, OnWindowModeSimpleChanged );

			AntiAliasingOption_Simple = GetNode<OptionList>( "OptionsContainer/BasicContainer/AntiAliasingList" );
			AntiAliasingOption_Simple.ValueChanged.Subscribe( this, OnAntiAliasingSimpleChanged );

			ResolutionOption = GetNode<OptionList>( "OptionsContainer/BasicContainer/ResolutionList" );
			ResolutionOption.ValueChanged.Subscribe( this, OnWindowResolutionChanged );

			MaxFps = GetNode<OptionList>( "OptionsContainer/BasicContainer/MaxFpsList" );
			MaxFps.ValueChanged.Subscribe( this, OnMaxFpsValueChanged );
		}

		/*
		===============
		LinkAdvancedDisplaySettings
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void LinkAdvancedDisplaySettings() {
			WindowModeOption = GetNode<OptionList>( "OptionsContainer/AdvancedContainer/WindowModeList" );
			WindowModeOption.ValueChanged.Subscribe( this, OnWindowModeAdvancedChanged );

			VSync = GetNode<OptionList>( "OptionsContainer/AdvancedContainer/VSyncList" );
			VSync.ValueChanged.Subscribe( this, OnVSyncAdvancedChanged );

			AntiAliasingOption = GetNode<OptionList>( "OptionsContainer/AdvancedContainer/AntiAliasingList" );
			AntiAliasingOption.ValueChanged.Subscribe( this, OnAntiAliasingAdvancedChanged );

			PerformanceOverlay = GetNode<OptionList>( "OptionsContainer/AdvancedContainer/PerformanceOverlayList" );
			PerformanceOverlay.ValueChanged.Subscribe( this, OnPerformanceOverlayValueChanged );
		}

		/*
		===============
		LinkVideoNodes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void LinkNodes() {
			LinkBasicDisplaySettings();
			LinkAdvancedDisplaySettings();
		}

		/*
		===============
		OnVisibilityChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnVisibilityChanged() {
		}
	};
};