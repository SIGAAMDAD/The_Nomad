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
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay;
using Nomad.UI;

namespace Nomad.Game.Prefabs {
	internal sealed partial class HealthBarView : EnginePanel, IHealthBarView {
		private readonly HudComponentView _impl;
		private ShaderMaterial _material;

		public HealthBarView() {
			_impl = new HudComponentView( this );
		}

		public void SetColor( System.Numerics.Vector4 color ) {
			_impl.SetColor( color );
		}

		public void SetValue( float value ) {
			SetHealth( value );
		}

		public void SetSizeParameters() {
			Vector2 size = Size;
			_material.SetShaderParameter( "width", size.X );
			_material.SetShaderParameter( "height", size.Y );
		}

		public float GetHealth() {
			return _material.GetShaderParameter( "health" ).AsSingle();
		}

		public float GetTrail() {
			return _material.GetShaderParameter( "trail" ).AsSingle();
		}

		public void SetHealth( float value ) {
			_material.SetShaderParameter( "health", value );
		}

		public void SetTrail( float value ) {
			_material.SetShaderParameter( "trail", value );
		}

		protected override void OnInit() {
			base.OnInit();

			_material = (ShaderMaterial)Material;
		}
	};
};
