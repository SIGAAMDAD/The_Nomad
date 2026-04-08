/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;
using Nomad.UI;

namespace Nomad.Game.Prefabs {
	public partial class DashStatusBarView : EngineImageView, IDashKitHeatBarView {
		private ShaderMaterial _material;

		protected override void OnInit() {
			base.OnInit();

			if ( Material is ShaderMaterial material ) {
				_material = material;
				_material.SetShaderParameter( "progress", 0.0f );
			}
		}


		public void SetColor( System.Numerics.Vector4 color ) {
		}

		public void SetValue( float value ) {
			_material.SetShaderParameter( "progress", value );
		}

	};
};
