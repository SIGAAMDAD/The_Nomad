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
using System.Collections.Generic;
using System.Timers;
using Nomad.Game.Sdk.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay
{
	/*
	===================================================================================

	ComponentGroup

	===================================================================================
	*/
	/// <summary>
	/// Controls a unique HUD layer such as Combat or Exploration.
	/// </summary>

	internal sealed class ComponentGroup : IDisposable
	{
		private readonly List<IHudComponentPresenter> _components;
		private readonly Timer _fadeTimer;

		private bool _active = false;

		private bool _isDisposed = false;

		public ComponentGroup( List<IHudComponentPresenter> components, float fadeTimeout = 1.0f )
		{
			_components = components;

			_fadeTimer = new Timer() {
				Interval = fadeTimeout,
				Enabled = true,
				AutoReset = false
			};
			_fadeTimer.Elapsed += OnFadeout;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void Activate()
		{
			if ( !_active ) {
				_fadeTimer.Start();
			}
			_active = true;
		}

		public void Render( float delta )
		{
			for ( int i = 0; i < _components.Count; i++ ) {
				_components[ i ].Render( delta );
			}
		}

		private void OnFadeout( object? sender, ElapsedEventArgs e )
		{
		}
	};
};
