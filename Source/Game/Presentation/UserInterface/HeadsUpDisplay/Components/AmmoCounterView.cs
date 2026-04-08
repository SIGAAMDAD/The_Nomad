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

using System.Drawing;
using System.Numerics;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;
using Nomad.UI;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components {
	internal sealed partial class AmmoCounterView : EngineText, IAmmoCounterView {
		public void SetColor( Vector4 color ) {
			Color = Color.FromArgb( (int)color.W, (int)color.X, (int)color.Y, (int)color.Z );
		}

		public void SetAmmoText( string text ) {
			Text = text;
		}
	};
};