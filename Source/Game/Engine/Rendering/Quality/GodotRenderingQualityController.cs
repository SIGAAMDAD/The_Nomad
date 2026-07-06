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

using System;
using System.Text;
using Godot;
using Nomad.Core.CVars;
using Nomad.Game.Content.Caching;

namespace Nomad.Game.Engine.Rendering.Quality
{
	internal sealed class GodotRenderingQualityController
	{
		private enum TextureMetaSlot : byte
		{
			Albedo,
			Normal,
			ORM
		};

		private const string BASE_SURFACE_MATERIAL_DIRECTORY = "res://Assets/Textures/Environment/Surfaces/";

		private static readonly StringName QualityVeryLow = "very_low";
		private static readonly StringName QualityLow = "low";
		private static readonly StringName QualityNormal = "normal";
		private static readonly StringName QualityHigh = "high";
		private static readonly StringName QualityVeryHigh = "very_high";
		private static readonly StringName QualityUltra = "ultra";

		private static readonly StringName AlbedoTexture = "albedo_texture";
		private static readonly StringName OrmTexture = "orm_texture";
		private static readonly StringName NormalTexture = "normal_texture";
		private static readonly StringName HeightMapEnabled = "heightmap_enabled";
		private static readonly StringName HeightMapTexture = "heightmap_texture";

		private readonly Viewport _rootViewport;

		public GodotRenderingQualityController( Viewport rootViewport )
		{
			_rootViewport = rootViewport ?? throw new ArgumentNullException( nameof( rootViewport ) );
		}

		private void OnTextureFilteringQualityChanged( in CVarValueChangedEventArgs<TextureFilteringQuality> args )
		{
			switch ( args.NewValue ) {
				case TextureFilteringQuality.Bilinear:
					_rootViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.NearestWithMipmaps;
					break;
				case TextureFilteringQuality.Trilinear:
					_rootViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					break;
				case TextureFilteringQuality.Anisotropy2X:
					_rootViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_rootViewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy2X;
					break;
				case TextureFilteringQuality.Anisotropy4X:
					_rootViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_rootViewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy2X;
					break;
				case TextureFilteringQuality.Anisotropy8X:
					_rootViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_rootViewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy2X;
					break;
				case TextureFilteringQuality.Anisotropy16X:
					_rootViewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_rootViewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy2X;
					break;
			}
		}

		private void OnTextureQualityChanged( in CVarValueChangedEventArgs<TextureQuality> args )
		{
			int textureSize = args.NewValue switch {
				TextureQuality.VeryLow => 512,
				TextureQuality.Low => 512,
				TextureQuality.Normal => 512,
				TextureQuality.High => 1024,
				TextureQuality.VeryHigh => 2048,
				TextureQuality.Ultra => 4096,
				_ => throw new ArgumentOutOfRangeException( nameof( args ) )
			};

			StringBuilder texturePath = new StringBuilder( 512 );

			foreach ( var material in MaterialCache.GetMaterials() ) {
				texturePath.Clear();
			}
		}
	};
};
