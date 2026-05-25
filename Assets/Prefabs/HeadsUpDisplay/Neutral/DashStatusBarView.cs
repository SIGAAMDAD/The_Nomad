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
	/*
	===================================================================================

	DashStatusBarView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class DashStatusBarView : TextureRect, IDashKitHeatBarView
	{
		private readonly HudComponentView _impl;
		private ShaderMaterial _material;
		private TextureRect _overlay;

		/*
		===============
		DashStatusBarView
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public DashStatusBarView()
		{
			_impl = new HudComponentView( this );
		}

		/*
		===============
		ShowOverlayVisibility
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="visible"></param>
		public void ShowOverlayVisibility( bool visible )
		{
			_overlay.Visible = visible;
		}

		/*
		===============
		SetColor
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( System.Numerics.Vector4 color )
		{
			_impl.SetColor( color );
		}

		/*
		===============
		SetValue
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="value"></param>
		public void SetValue( float value )
		{
			_material.SetShaderParameter( "progress", value );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready()
		{
			base._Ready();

			if ( Material is ShaderMaterial material ) {
				_material = material;
				_material.SetShaderParameter( "progress", 0.0f );
			}

			_overlay = GetNode<TextureRect>( "Overlay/ImageView" );
		}
	};
};
