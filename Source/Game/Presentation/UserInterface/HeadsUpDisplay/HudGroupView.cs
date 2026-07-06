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

namespace Nomad.Game.Prefabs
{
	internal sealed partial class HudGroupView : Control
	{
		private readonly HudComponentFader _fader;

		public HudGroupView()
		{
			_fader = new HudComponentFader( this );
		}

		public void FadeOut( float fadeOutSpeedSeconds = 1.5f )
		{
			_fader.FadeOut( fadeOutSpeedSeconds );
		}

		public void Activate()
		{
			Modulate = Colors.White;
		}
	};
};
