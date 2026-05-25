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

namespace Nomad.Game.Prefabs
{
	internal sealed partial class RageBarView : ColorRect, IRageBarView
	{
		private readonly HudComponentView _impl;
		private ShaderMaterial _material;

		public RageBarView()
		{
			_impl = new HudComponentView( this );
		}

		public void SetColor( System.Numerics.Vector4 color )
		{
			_impl.SetColor( color );
		}

		public void SetValue( float value )
		{
			SetRage( value );
		}

		public void SetVisibility( bool visible )
		{
			_impl.Visible = visible;
		}

		public void SetSizeParameters()
		{
			Vector2 size = CustomMinimumSize;
			_material.SetShaderParameter( "width", size.X );
			_material.SetShaderParameter( "height", size.Y );
		}

		public float GetRage()
		{
			return _material.GetShaderParameter( "health" ).AsSingle();
		}

		public float GetTrail()
		{
			return _material.GetShaderParameter( "trail" ).AsSingle();
		}

		public void SetRage( float value )
		{
			_material.SetShaderParameter( "health", value );
		}

		public void SetTrail( float value )
		{
			_material.SetShaderParameter( "trail", value );
		}

		public override void _Ready()
		{
			base._Ready();

			_material = (ShaderMaterial)Material;
		}
	};
};
