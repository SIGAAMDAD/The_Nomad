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
using Godot;
using Menus.SelectionNodes;
using System;
using Settings;
using Settings.Config;

namespace Menus.Settings.Options {
	/*
	===================================================================================
	
	GraphicsOptionManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class GraphicsOptionsManager : OptionContainer {
		private GraphicsConfig Temp;

		private GraphicsConfig? CurrentGraphicsPreset;
		private LightingConfig? CurrentLightingPreset;

		private OptionList? LightingQuality;
		private OptionList? EffectsQuality;
		private OptionList? Preset;
		private OptionCheckbox? BakedLights;
		private OptionList? ParticleQuality;
		private OptionSlider? ShadowFilterSmooth;
		private OptionList? ShadowFilterQuality;
		private OptionList? AnimationQuality;
		private OptionCheckbox? ForceVertexShading;
		private OptionCheckbox? PhysicallyBasedRendering;

		/*
		===============
		GraphicsOptionsManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tmp"></param>
		public GraphicsOptionsManager( GraphicsConfig? tmp ) {
			ArgumentNullException.ThrowIfNull( tmp );

			Temp = tmp;
		}

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
			Temp = config as GraphicsConfig;
		}

		/*
		===============
		OnPresetValueChanged
		===============
		*/
		private void OnPresetValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				Temp[ Graphics.Preset.Name ] = (QualitySetting)valueChanged.Value;
				CurrentGraphicsPreset = Temp[ Graphics.Preset.Name ] switch {
					QualitySetting.Low => GraphicsConfig.Presets.Low,
					QualitySetting.Normal => GraphicsConfig.Presets.Medium,
					QualitySetting.High => GraphicsConfig.Presets.High,
					_ => throw new ArgumentOutOfRangeException( Graphics.Preset.Name )
				};
				MapGraphicsConfigToMenu();
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnShadowFilterTypeValueChanged
		===============
		*/
		private void OnShadowFilterTypeValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( CurrentLightingPreset );
				ArgumentNullException.ThrowIfNull( LightingQuality );

				Temp[ Graphics.ShadowFilterType.Name ] = (Light2D.ShadowFilterEnum)valueChanged.Value;
				if ( Temp[ Graphics.ShadowFilterType.Name ] != CurrentLightingPreset[ Graphics.ShadowFilterType.Name ] ) {
					LightingQuality.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnAnimationQualityValueChanged
		===============
		*/
		private void OnAnimationQualityValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( CurrentGraphicsPreset );
				ArgumentNullException.ThrowIfNull( EffectsQuality );

				Temp.AnimationQuality = (AnimationQuality)valueChanged.Value;
				if ( Temp.AnimationQuality != CurrentGraphicsPreset.AnimationQuality ) {
					EffectsQuality.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnParticleQualityValueChanged
		===============
		*/
		private void OnParticleQualityValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( CurrentGraphicsPreset );
				ArgumentNullException.ThrowIfNull( EffectsQuality );

				Temp.ParticleQuality = (ParticleQuality)valueChanged.Value;
				if ( Temp.ParticleQuality != CurrentGraphicsPreset.ParticleQuality ) {
					EffectsQuality.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnForceVertexShadingToggled
		===============
		*/
		private void OnForceVertexShadingToggled( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( CurrentLightingPreset );
				ArgumentNullException.ThrowIfNull( LightingQuality );
				ArgumentNullException.ThrowIfNull( CurrentGraphicsPreset );
				ArgumentNullException.ThrowIfNull( Preset );

				Temp.ForceVertexShading = valueChanged.Value;
				if ( Temp.ForceVertexShading != CurrentLightingPreset.ForceVertexShading ) {
					LightingQuality.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
				if ( Temp.ForceVertexShading != CurrentGraphicsPreset.ForceVertexShading ) {
					Preset.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnPhysicallyBasedRenderingToggled
		===============
		*/
		private void OnPhysicallyBasedRenderingToggled( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( CurrentLightingPreset );
				ArgumentNullException.ThrowIfNull( LightingQuality );
				ArgumentNullException.ThrowIfNull( CurrentGraphicsPreset );
				ArgumentNullException.ThrowIfNull( Preset );

				Temp.PhysicallyBasedRendering = valueChanged.Value;
				if ( Temp.PhysicallyBasedRendering != CurrentLightingPreset.PhysicallyBasedRendering ) {
					LightingQuality.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
				if ( Temp.PhysicallyBasedRendering != CurrentGraphicsPreset.PhysicallyBasedRendering ) {
					Preset.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnBakedLightsToggled
		===============
		*/
		private void OnBakedLightsToggled( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( CurrentLightingPreset );
				ArgumentNullException.ThrowIfNull( LightingQuality );
				ArgumentNullException.ThrowIfNull( CurrentGraphicsPreset );
				ArgumentNullException.ThrowIfNull( Preset );

				Temp[ "BakedLights" ] = valueChanged.Value;
				if ( Temp.BakedLights != CurrentLightingPreset.BakedLights ) {
					LightingQuality.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
				if ( Temp.BakedLights != CurrentGraphicsPreset.BakedLights ) {
					Preset.SetValue( SettingsManager.CUSTOM_SETTING_VALUE );
				}
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		LinkAdvancedGraphicsSettings
		===============
		*/
		private void LinkAdvancedGraphicsSettings() {
			ShadowFilterQuality = GetNode<OptionList>( "OptionsContainer/AdvancedContainer/ShadowFilterQualityList" );
			ShadowFilterQuality.ValueChanged.Subscribe( this, OnShadowFilterTypeValueChanged );

			AnimationQuality = GetNode<OptionList>( "OptionsContainer/AdvancedContainer/AnimationQualityList" );
			AnimationQuality.ValueChanged.Subscribe( this, OnAnimationQualityValueChanged );

			ParticleQuality = GetNode<OptionList>( "Graphics/OptionsContainer/AdvancedContainer/ParticleQualityList" );
			ParticleQuality.ValueChanged.Subscribe( this, OnParticleQualityValueChanged );

			ForceVertexShading = GetNode<OptionCheckbox>( "Graphics/OptionsContainer/AdvancedContainer/ForceVertexShadingButton" );
			ForceVertexShading.ValueChanged.Subscribe( this, OnForceVertexShadingToggled );

			PhysicallyBasedRendering = GetNode<OptionCheckbox>( "Graphics/OptionsContainer/AdvancedContainer/PhysicallyBasedRenderingButton" );
			PhysicallyBasedRendering.ValueChanged.Subscribe( this, OnPhysicallyBasedRenderingToggled );

			BakedLights = GetNode<OptionCheckbox>( "Graphics/OptionsContainer/AdvancedContainer/BakedLightsButton" );
			BakedLights.ValueChanged.Subscribe( this, OnBakedLightsToggled );
		}

		/*
		===============
		OnLightingQualityValueChanged
		===============
		*/
		private void OnLightingQualityValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				Temp.LightingQuality = (LightingQuality)valueChanged.Value;
				MapLightingConfigToAdvanced();
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnEffectsQualityValueChanged
		===============
		*/
		private void OnEffectsQualityValueChanged( in IGameEvent eventData, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				Temp.EffectsQuality = (QualitySetting)valueChanged.Value;
				MapEffectsConfigToAdvanced();
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		LinkBasicGraphicsSettings
		===============
		*/
		private void LinkBasicGraphicsSettings() {
			LightingQuality = GetNode<OptionList>( "Graphics/OptionsContainer/BasicContainer/LightingQualityList" );
			LightingQuality.ValueChanged.Subscribe( this, OnLightingQualityValueChanged );

			EffectsQuality = GetNode<OptionList>( "Graphics/OptionsContainer/BasicContainer/EffectsQualityList" );
			EffectsQuality.ValueChanged.Subscribe( this, OnEffectsQualityValueChanged );

			Preset = GetNode<OptionList>( "Graphics/OptionsContainer/BasicContainer/PresetList" );
			Preset.ValueChanged.Subscribe( this, OnPresetValueChanged );
		}

		/*
		===============
		LinkGraphicsNodes
		===============
		*/
		private void LinkGraphicsNodes() {
			LinkBasicGraphicsSettings();
			LinkAdvancedGraphicsSettings();
		}

		/*
		===============
		MapLightingConfigToAdvanced
		===============
		*/
		/// <summary>
		/// Maps lighting presets to more advanced graphical settings
		/// </summary>
		/// <remarks>
		/// Effects: <see cref="SettingsManager.ShadowAtlasSize"/>, <see cref="SettingsManager.ShadowFilterSmooth"/>, <see cref="SettingsManager.ShadowFilterType"/>, <see cref="SettingsManager.BakedLights"/>,
		/// <see cref="SettingsManager.ForceVertexShading"/>, and <see cref="SettingsManager.PhysicallyBasedRendering"/>
		/// </remarks>
		private void MapLightingConfigToAdvanced() {
			ArgumentNullException.ThrowIfNull( LightingQuality );

			bool bakedLights, vertexShading, physicallyBasedRendering;
			Light2D.ShadowFilterEnum filterType;
			float filterSmooth;
			int atlasSize;

			switch ( (global::LightingQuality)LightingQuality.Value ) {
				case global::LightingQuality.VeryLow:
					bakedLights = true;
					vertexShading = true;
					physicallyBasedRendering = false;
					filterType = Light2D.ShadowFilterEnum.None;
					filterSmooth = 0.0f;
					atlasSize = 2048;
					break;
				case global::LightingQuality.Low:
					bakedLights = false;
					vertexShading = true;
					physicallyBasedRendering = false;
					filterType = Light2D.ShadowFilterEnum.None;
					filterSmooth = 0.0f;
					atlasSize = 4096;
					break;
				case global::LightingQuality.Normal:
					bakedLights = false;
					vertexShading = true;
					physicallyBasedRendering = false;
					filterType = Light2D.ShadowFilterEnum.Pcf5;
					filterSmooth = 0.05f;
					atlasSize = 8192;
					break;
				case global::LightingQuality.High:
					bakedLights = false;
					vertexShading = false;
					physicallyBasedRendering = true;
					filterType = Light2D.ShadowFilterEnum.Pcf13;
					filterSmooth = 0.10f;
					atlasSize = 12288;
					break;
				default:
					// custom config
					return;
			}

			ProjectSettings.SetSetting( "rendering/shading/overrides/force_vertex_shading", vertexShading );
			ProjectSettings.SetSetting( "rendering/shading/overrides/force_lambert_over_burley", !physicallyBasedRendering );
			RenderingServer.CanvasSetShadowTextureSize( atlasSize );

			Config.Graphics.BakedLights.Set( bakedLights );
			Config.Graphics.ForceVertexShading.Set( vertexShading );
			Config.Graphics.PhysicallyBasedRendering.Set( physicallyBasedRendering );
			Config.Graphics.ShadowFilterSmooth.Set( filterSmooth );
			Config.Graphics.ShadowFilterType.Set( (int)filterType );
			Config.Graphics.ShadowAtlasSize.Set( atlasSize );
		}

		/*
		===============
		MapEffectsConfigToAdvanced
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void MapEffectsConfigToAdvanced() {
			ArgumentNullException.ThrowIfNull( EffectsQuality );
			ArgumentNullException.ThrowIfNull( AnimationQuality );
			ArgumentNullException.ThrowIfNull( ParticleQuality );

			switch ( (QualitySetting)EffectsQuality.Value ) {
				case QualitySetting.Low:
					AnimationQuality.SetValue( (int)global::AnimationQuality.Low );
					ParticleQuality.SetValue( (int)global::ParticleQuality.Low );
					break;
				case QualitySetting.Normal:
					AnimationQuality.SetValue( (int)global::AnimationQuality.Medium );
					ParticleQuality.SetValue( (int)global::ParticleQuality.Low );
					break;
				case QualitySetting.High:
					AnimationQuality.SetValue( (int)global::AnimationQuality.High );
					ParticleQuality.SetValue( (int)global::ParticleQuality.High );
					break;
				default:
					// custom config
					return;
			}
		}

		/*
		===============
		MapGraphicsConfigToMenu
		===============
		*/
		private void MapGraphicsConfigToMenu() {
			ArgumentNullException.ThrowIfNull( Preset );
			ArgumentNullException.ThrowIfNull( LightingQuality );
			ArgumentNullException.ThrowIfNull( EffectsQuality );

			switch ( (QualitySetting)Preset.Value ) {
				case QualitySetting.Low:
					LightingQuality.SetValue( (int)LightingQuality.Low );
					EffectsQuality.SetValue( (int)global::QualitySetting.Low );
					break;
				case QualitySetting.Normal:
					LightingQuality.SetValue( (int)global::LightingQuality.Normal );
					EffectsQuality.SetValue( (int)global::QualitySetting.Normal );
					break;
				case QualitySetting.High:
					LightingQuality.SetValue( (int)global::LightingQuality.High );
					EffectsQuality.SetValue( (int)global::QualitySetting.High );
					break;
			}
		}

		/*
		===============
		LinkNodes
		===============
		*/
		protected override void LinkNodes() {
			LinkBasicGraphicsSettings();
			LinkAdvancedGraphicsSettings();
		}

		/*
		===============
		OnVisibilityChanged
		===============
		*/
		protected override void OnVisibilityChanged() {
		}
	};
};