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

namespace Nomad.Game.Prefabs
{
	internal sealed class HudComponentFader : IDisposable
	{
		private readonly CanvasItem _component;

		private bool _fading;
		private float _fadeTime;
		private float _fadeDuration;
		private float _startAlpha;

		private bool _isDisposed = false;

		public HudComponentFader( CanvasItem component )
		{
			_component = component ?? throw new ArgumentNullException( nameof( component ) );
			_component.GetTree().ProcessFrame += OnProcess;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_component.GetTree().ProcessFrame -= OnProcess;

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void FadeOut( float duration = 1.0f )
		{
			_fadeDuration = MathF.Max( duration, 0.0001f );
			_fadeTime = duration;
			_startAlpha = _component.Modulate.A;

			_fading = true;
		}

		private void OnProcess()
		{
			if ( !_fading ) {
				return;
			}

			_fadeTime += (float)_component.GetProcessDeltaTime();

			float t = Math.Clamp( _fadeTime / _fadeDuration, 0.0f, 1.0f );

			Color c = _component.Modulate;
			c.A = Mathf.Lerp( _startAlpha, 0.0f, t );
			_component.Modulate = c;

			if ( t >= 1.0f ) {
				_fading = false;
			}
		}
	};
};
