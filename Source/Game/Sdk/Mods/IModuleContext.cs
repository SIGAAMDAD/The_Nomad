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

using Nomad.Modding;
using Nomad.Modding.CVars;
using Nomad.Modding.Events;
using Nomad.Modding.FileSystem;

namespace Nomad.Game.Sdk.Mods
{
	public interface IModuleContext
	{
		string ModuleId { get; }
		ModuleInfo Module { get; }

		string RuntimeApiVersion { get; }

		IModEventRegistry Events { get; }
		IModLogger Logger { get; }
		IModFileSystem FileSystem { get; }
		IModCVarSystem CVarSystem { get; }
	}
}
