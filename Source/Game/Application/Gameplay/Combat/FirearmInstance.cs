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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Combat;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Events.Combat;
using Nomad.Game.Domain.Interfaces.Combat;
using Nomad.Game.Domain.Interfaces.Player.Inventory;

namespace Nomad.Game.Application.Gameplay.Combat
{
	/*
	===================================================================================

	FirearmInstance

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal class FirearmInstance : WeaponInstance<FirearmDefinition>, IFirearmInstance
	{
		public FirearmMagazine AmmoSnapshot => _magazineService.Snapshot;
		public FirearmResolvedStats Stats => _resolvedStats;

		private readonly FirearmModService _modService;
		private readonly FirearmMagazineService _magazineService;

		private FirearmResolvedStats _resolvedStats;

		public IGameEvent<FirearmJammedEventArgs> FirearmJammed => _jammed;
		private readonly IGameEvent<FirearmJammedEventArgs> _jammed = null;

		public IGameEvent<FirearmReloadedEventArgs> FirearmReloaded => _reloaded;
		private readonly IGameEvent<FirearmReloadedEventArgs> _reloaded = null;

		public FirearmInstance( ItemInstanceId id, IGameEventRegistryService eventFactory, FirearmDefinition definition )
			: base( new EntityId( id.Value ), eventFactory, definition )
		{
			_modService = new FirearmModService( definition );
			_resolvedStats = _modService.Resolve( Array.Empty<FirearmModDefinition>() );

			_jammed = eventFactory.GetEvent<FirearmJammedEventArgs>(
				$"{Id}:{FirearmJammedEventArgs.Name}",
				FirearmJammedEventArgs.NameSpace
			);

			_reloaded = eventFactory.GetEvent<FirearmReloadedEventArgs>(
				$"{Id}:{FirearmReloadedEventArgs.Name}",
				FirearmReloadedEventArgs.NameSpace
			);
		}

		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}

			base.Dispose( disposing );

			_jammed.Dispose();
			_reloaded.Dispose();
		}

		public bool TryReload( IStorageUnit inventory )
		{
			return _magazineService.TryReload( inventory );
		}

		public bool TryStartUse()
		{
			if ( !_magazineService.CanUse ) {
				return false;
			}
			return true;
		}

		public bool TryEndUse()
		{
			throw new NotImplementedException();
		}

		public bool TryAddMod( FirearmModSlot slot, FirearmModDefinition mod )
		{
			throw new NotImplementedException();
		}

		public bool TryRemoveMod( FirearmModSlot slot )
		{
			throw new NotImplementedException();
		}
	};
};
