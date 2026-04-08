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
using Nomad.Core.UI;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay {
	internal sealed class HudComponentView : IHudComponentView {
		public bool Visible {
			get => _component.Visible;
			set => _component.Visible = value;
		}

		private readonly IUIElement _component;

		public HudComponentView( IUIElement component ) {
			_component = component;
		}

		public void SetColor( Vector4 color ) {
			_component.Color = Color.FromArgb( (int)color.W, (int)color.X, (int)color.Y, (int)color.Z );
		}
	};
};