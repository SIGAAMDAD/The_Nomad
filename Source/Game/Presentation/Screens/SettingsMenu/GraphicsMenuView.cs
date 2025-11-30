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

using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList;
using Game.Presentation.Screens.SettingsMenu.Interfaces;
using Godot;

namespace Game.Presentation.Screens.SettingsMenu {
	public class GraphicsMenuView : IGraphicsMenuView {
		public IOptionListView Preset => throw new System.NotImplementedException();
		public IOptionListView LightingQuality => throw new System.NotImplementedException();

		public IOptionListView EffectsQuality => _effectsQuality;
		private readonly IOptionListView _effectsQuality;

		public IOptionListView ShadowFilterQuality => _shadowFilterQuality;
		private readonly IOptionListView _shadowFilterQuality;

		public IOptionSliderView ShadowFilterSmooth => _shadowFilterSmooth;

		public IOptionListView AnimationQuality => throw new System.NotImplementedException();

		public IOptionListView ParticleQuality => throw new System.NotImplementedException();

		public IOptionListView ShadowAtlasSize => throw new System.NotImplementedException();

		public IOptionCheckboxView BakedLights => throw new System.NotImplementedException();

		public IOptionCheckboxView BloomEnabled => throw new System.NotImplementedException();

		public IOptionCheckboxView ForceVertexShading => throw new System.NotImplementedException();

		public IOptionCheckboxView PhysicallyBasedRendering => throw new System.NotImplementedException();

		private readonly IOptionSliderView _shadowFilterSmooth;

		public GraphicsMenuView( Control owner ) {
			var effectsQuality = owner.GetNode<OptionList>( "%EffectsQualityList" );
			_effectsQuality = (IOptionListView)effectsQuality.View;

			var shadowFilterQuality = owner.GetNode<OptionList>( "%ShadowFilterQualityList" );
			_shadowFilterQuality = (IOptionListView)shadowFilterQuality.View;
		}
	};
};