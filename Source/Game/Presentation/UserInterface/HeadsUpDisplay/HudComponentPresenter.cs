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
using System.Drawing;
using Nomad.Game.Application.Configuration.Enums;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay
{
	internal abstract class HudComponentPresenter<TView> : IHudComponentPresenter
		where TView : IHudComponentView
	{
		protected readonly TView view;

		private bool _isDisposed = false;

		public HUDPreset Preset => _preset;
		private HUDPreset _preset;

		public Color Color => _color;
		private Color _color;

		public bool Visible => view.Visible;

		public HudComponentPresenter( TView view )
		{
			this.view = view;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			Dispose( true );

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		protected virtual void Dispose( bool disposing )
		{
		}

		public abstract void Render( float delta );

		public void Show()
		{
			view.Show();
		}

		public void Hide()
		{
			view.Hide();
		}
	};
};
