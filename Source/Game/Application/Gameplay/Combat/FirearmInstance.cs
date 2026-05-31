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
using Nomad.Game.Sdk.Combat;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Events.Combat;
using Nomad.Game.Sdk.Inventory;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Application.Gameplay.Combat
{
	/*
	===================================================================================

	FirearmInstance

	===================================================================================
	*/
	/// <summary>
	/// Runtime state and behavior for a concrete firearm item.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The instance combines immutable catalog data from
	/// <see cref="FirearmDefinition"/> with mutable magazine and modification
	/// state. The resolved <see cref="Stats"/> value is firearm handling only:
	/// fire rate, magazine size, reload speed, aiming behavior, reliability,
	/// attachment effects, and similar mechanical values.
	/// </para>
	/// <para>
	/// Projectile output stays ammo-owned. Damage, range, projectile velocity,
	/// recoil impulse, and ammo category are read from <see cref="LoadedAmmo"/>
	/// and are not copied into firearm stats. Compatibility is checked by the
	/// firearm's accepted data-loaded ammo types, allowing one firearm to accept
	/// several granular cartridges while rejecting others.
	/// </para>
	/// </remarks>
	internal sealed class FirearmInstance : WeaponInstance<FirearmDefinition>, IFirearmInstance
	{
		public FirearmDefinition FirearmDefinition => definition;
		public FirearmMagazine AmmoSnapshot => _magazineService.Snapshot;
		public FirearmResolvedStats Stats => _resolvedStats;
		public AmmoDefinition? LoadedAmmo => _magazineService.LoadedAmmo;

		private readonly FirearmModService _modService;
		private readonly FirearmMagazineService _magazineService;

		private FirearmResolvedStats _resolvedStats;
		private bool _isUsing;

		public IGameEvent<FirearmJammedEventArgs> FirearmJammed => _jammed;
		private readonly IGameEvent<FirearmJammedEventArgs> _jammed = null;

		public IGameEvent<FirearmReloadedEventArgs> FirearmReloaded => _reloaded;
		private readonly IGameEvent<FirearmReloadedEventArgs> _reloaded = null;

		public IGameEvent<FirearmUsedEventArgs> FirearmUsed => _used;
		private readonly IGameEvent<FirearmUsedEventArgs> _used = null;

		public FirearmInstance( ItemInstanceId id, IGameEventRegistryService eventFactory, FirearmDefinition definition )
			: base( new EntityId( id.Value ), id, eventFactory, definition )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_modService = new FirearmModService( definition );
			_magazineService = new FirearmMagazineService( this );
			_resolvedStats = _modService.Resolve( Array.Empty<FirearmModDefinition>() );

			_jammed = eventFactory
				.GetEvent<FirearmJammedEventArgs>(
					FirearmJammedEventArgs.Name,
					FirearmJammedEventArgs.NameSpace
				);

			_reloaded = eventFactory
				.GetEvent<FirearmReloadedEventArgs>(
					FirearmReloadedEventArgs.Name,
					FirearmReloadedEventArgs.NameSpace
				);

			_used = eventFactory
				.GetEvent<FirearmUsedEventArgs>(
					FirearmUsedEventArgs.Name,
					FirearmUsedEventArgs.NameSpace
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
			_used.Dispose();
		}

		public bool TryReload( IStorageUnit inventory )
		{
			if ( !_magazineService.TryReload( inventory, out int roundsLoaded ) ) {
				return false;
			}

			_reloaded.Publish( new FirearmReloadedEventArgs( InstanceId, roundsLoaded ) );
			return true;
		}

		public bool TrySetAmmo( AmmoDefinition ammoDefinition )
		{
			return _magazineService.TrySetAmmo( ammoDefinition );
		}

		public bool TryStartUse()
		{
			if ( _isUsing || !_magazineService.TryConsumeRound() ) {
				return false;
			}

			_isUsing = true;
			_used.Publish( new FirearmUsedEventArgs( InstanceId ) );
			return true;
		}

		public bool TryEndUse()
		{
			if ( !_isUsing ) {
				return false;
			}

			_isUsing = false;
			return true;
		}

		public bool TryAddMod( FirearmModSlot slot, FirearmModDefinition mod )
		{
			ArgumentGuard.ThrowIfNull( mod, nameof( mod ) );

			if ( slot != mod.Slot || !_modService.TryInstallMod( mod ) ) {
				return false;
			}

			RefreshResolvedStats();
			return true;
		}

		public bool TryRemoveMod( FirearmModSlot slot )
		{
			if ( !_modService.TryRemoveMod( slot ) ) {
				return false;
			}

			RefreshResolvedStats();
			return true;
		}

		private void RefreshResolvedStats()
		{
			_resolvedStats = _modService.Resolve( _modService.GetInstalledMods() );
		}
	};
};
