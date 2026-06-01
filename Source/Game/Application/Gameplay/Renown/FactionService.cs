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

using System.Collections.Generic;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Game.Sdk.Renown;

namespace Nomad.Game.Application.Gameplay.Renown
{
	/*
	===================================================================================

	FactionService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class FactionService : IFactionService
	{
		private readonly Dictionary<FactionInstanceId, IFactionInstance> _instances = new();

		public FactionService( IFileSystem fileSystem, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			var files = fileSystem.GetFiles( $"{fileSystem.GetResourcePath()}/DataCache/Factions", "*.json", true );
		}

		public IReadOnlyCollection<IFactionInstance> GetActiveFactions()
		{
			return _instances.Values;
		}

		public bool TryGetFaction( FactionInstanceId factionId, out IFactionInstance instance )
		{
			throw new System.NotImplementedException();
		}
	};
};
