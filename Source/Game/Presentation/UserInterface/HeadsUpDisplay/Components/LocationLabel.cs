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
using Nomad.Game.Application.Configuration.Enums;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay {
	public sealed class LocationLabel : HUDComponent {
		public override bool Visible => _visible;
		private bool _visible = true;

		public override Color Color => throw new System.NotImplementedException();

		public override HUDPreset Visibility => throw new System.NotImplementedException();

		public override float FadeTime => throw new System.NotImplementedException();

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void Dispose() {
		}
	};
};