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
using System.Runtime.CompilerServices;
using Godot;

namespace Nomad.Game.Presentation.Widgets.NomadButton
{
	internal sealed class ButtonAnimator : IDisposable
	{
		private readonly NomadButton _button;

		private bool _animating;
		private float _animationTime;
		private float _animationDuration;
		private Vector2 _startScale;
		private Vector2 _endScale;

		private bool _isDisposed = false;

		public ButtonAnimator( NomadButton button )
		{
			_button = button ?? throw new ArgumentNullException( nameof( button ) );
			_button.GetTree().ProcessFrame += OnProcess;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_button.GetTree().ProcessFrame -= OnProcess;

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void Animate( Vector2 scale, float duration = 1.0f )
		{
			_animationDuration = MathF.Max( duration, 0.0001f );
			_animationTime = 0.0f;
			_startScale = _button.Scale;
			_endScale = scale;

			_animating = true;
		}

		private void OnProcess()
		{
			if ( !_animating ) {
				return;
			}

			_animationTime += (float)_button.GetProcessDeltaTime();

			float t = _animationDuration <= 0.0f
				? 1.0f
				: Math.Clamp( _animationTime / _animationDuration, 0.0f, 1.0f );

			float eased = EaseOutCubic( t );

			_button.Scale = _startScale.Lerp( _endScale, eased );

			if ( t >= 1.0f ) {
				_button.Scale = _endScale;
				_animating = false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float EaseOutCubic( float t )
		{
			float p = 1.0f - t;
			return 1.0f - p * p * p;
		}
	};
};
