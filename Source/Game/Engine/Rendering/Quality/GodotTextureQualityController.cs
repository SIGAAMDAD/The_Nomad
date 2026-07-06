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
using Godot;
using Nomad.Core.CVars;
using Nomad.Game.Content.Caching;

namespace Nomad.Game.Engine.Rendering.Quality
{
	internal sealed class GodotTextureQualityController
	{
		private readonly Viewport _viewport;
		private readonly MaterialCache _cache;

		public GodotTextureQualityController( Viewport viewport )
		{
			_viewport = viewport ?? throw new ArgumentNullException( nameof( viewport ) );
		}

		private void OnTextureQualityChanged( in CVarValueChangedEventArgs<RenderingPreset> args )
		{
			switch ( args.NewValue ) {
				case RenderingPreset.VeryLow:
					break;

				case RenderingPreset.Low:
					break;

				case RenderingPreset.Normal:
					break;

				case RenderingPreset.High:
					break;

				case RenderingPreset.VeryHigh:
					break;

				case RenderingPreset.Ultra:
					break;
			}
		}

		private void OnTextureFilteringQualityChanged( in CVarValueChangedEventArgs<TextureFilteringQuality> args )
		{
			switch ( args.NewValue ) {
				case TextureFilteringQuality.Bilinear:
					_viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.NearestWithMipmaps;
					break;

				case TextureFilteringQuality.Trilinear:
					_viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					break;

				case TextureFilteringQuality.Anisotropy2X:
					_viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_viewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy2X;
					break;

				case TextureFilteringQuality.Anisotropy4X:
					_viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_viewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy4X;
					break;

				case TextureFilteringQuality.Anisotropy8X:
					_viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_viewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy8X;
					break;

				case TextureFilteringQuality.Anisotropy16X:
					_viewport.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps;
					_viewport.AnisotropicFilteringLevel = Viewport.AnisotropicFiltering.Anisotropy16X;
					break;
			}
		}
	};
};
