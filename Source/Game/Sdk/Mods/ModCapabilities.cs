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

namespace Nomad.Game.Sdk.Mods
{
	[Flags]
	public enum ModCapabilities
	{
		None = 0,

		Items = 1 << 0,
		Save = 1 << 1,
		Events = 1 << 2,
		Input = 1 << 3,
		Commands = 1 << 4,
		CVars = 1 << 5,
		FilesRead = 1 << 6,
		FilesWrite = 1 << 7,
		Content = 1 << 8
	}
}
