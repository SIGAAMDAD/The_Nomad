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

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay {
	public abstract class HUDComponent : IDisposable {
		public abstract bool Visible { get; }
		public abstract Color Color { get; }
		public abstract HUDPreset Visibility { get; }
		public abstract float FadeTime { get; }

		public abstract void Dispose();
	};
};