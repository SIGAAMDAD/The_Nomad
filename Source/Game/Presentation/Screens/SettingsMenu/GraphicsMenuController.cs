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

using Game.Application.Common;
using Game.Application.Common.Interfaces;
using Game.Application.Common.Models;
using Game.Application.Configuration.Enums;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionSlider;
using Game.Presentation.Screens.SettingsMenu.Interfaces;
using NomadCore.Abstractions.Services;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	GraphicsMenuController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class GraphicsMenuController : IGraphicsMenuController {
		private readonly ICVarSystemService _cvarSystem;
		private readonly IGraphicsSettingsService _graphicsService;

		private readonly IOptionListController _effectsQuality;
		private readonly IOptionListController _lightingQuality;
		private readonly IOptionListController _preset;

		private readonly IOptionCheckboxController _bakedLights;
		private readonly IOptionCheckboxController _bloomEnabled;
		private readonly IOptionCheckboxController _physicallyBasedRendering;
		private readonly IOptionCheckboxController _forceVertexShading;
		private readonly IOptionListController _shadowFilterQuality;
		private readonly IOptionSliderController _shadowFilterSmooth;
		private readonly IOptionListController _shadowAtlasSize;

		/*
		===============
		GraphicsMenuController
		===============
		*/
		public GraphicsMenuController( ICVarSystemService cvarSystem, IGraphicsMenuView view, IGraphicsSettingsService graphicsService ) {
			_cvarSystem = cvarSystem;
			_graphicsService = graphicsService;

			GraphicsConfig config = _graphicsService.GetCurrentConfig();

			_effectsQuality = new OptionListController(
				view.EffectsQuality,
				[
					TranslationKeys.Graphics.QualityLow,
					TranslationKeys.Graphics.QualityNormal,
					TranslationKeys.Graphics.QualityHigh,
					TranslationKeys.Graphics.QualityUltra
				],
				(int)config.Effects
			);
			_lightingQuality = new OptionListController(
				view.LightingQuality,
				[
					TranslationKeys.Graphics.QualityLow,
					TranslationKeys.Graphics.QualityNormal,
					TranslationKeys.Graphics.QualityHigh
				],
				(int)config.LightingQuality
			);
			_preset = new OptionListController(
				view.Preset,
				[
					TranslationKeys.Graphics.QualityLow,
					TranslationKeys.Graphics.QualityNormal,
					TranslationKeys.Graphics.QualityHigh,
				],
				(int)_graphicsService.GetPreset()
			);

			_bakedLights = new OptionCheckboxController( view.BakedLights, config.Lighting.BakedLights );
			_bloomEnabled = new OptionCheckboxController( view.BloomEnabled, config.Lighting.BloomEnabled );
			_forceVertexShading = new OptionCheckboxController( view.ForceVertexShading, config.Lighting.ForceVertexShading );
			_physicallyBasedRendering = new OptionCheckboxController( view.PhysicallyBasedRendering, config.Lighting.PhysicallyBasedRendering );
			_shadowAtlasSize = new OptionListController(
				view.ShadowAtlasSize,
				_graphicsService.GetShadowAtlasSizes(),
				(int)config.Lighting.ShadowAtlasSize
			);
			_shadowFilterQuality = new OptionListController(
				view.ShadowFilterQuality,
				_graphicsService.Get
			);
		}

		/*
		===============
		UpdateUI
		===============
		*/
		public void UpdateUI() {
			GraphicsConfig config = _graphicsService.GetCurrentConfig();
		}
	};
};