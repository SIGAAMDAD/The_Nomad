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
using System.Collections.Generic;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Application.Gameplay.Combat
{
	internal sealed class FirearmModService
	{
		private readonly InstalledFirearmMod[] _modSlots = new InstalledFirearmMod[(int)FirearmModSlot.Count];
		private readonly FirearmDefinition _definition;

		public FirearmModService( FirearmDefinition definition )
		{
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
		}

		public void Clear()
		{
		}

		public bool IsSlotUsed( FirearmModSlot slot )
		{
			return _modSlots[ (int)slot ].IsValid;
		}

		public bool CanInstallMod( FirearmModDefinition mod )
		{
			if ( !SupportsSlot( mod.Slot ) ) {
				return false;
			}

			if ( mod.RequiredFirearmFlags != FirearmFlags.None && (_definition.Flags & mod.BlockedFirearmFlags) != 0 ) {
				return false;
			}

			if ( mod.Flags.HasFlag( FirearmModFlags.Unique ) && _modSlots[(int)mod.Slot].IsValid ) {
				return false;
			}

			return true;
		}

		public bool TryInstallMod( FirearmModDefinition mod )
		{
			if ( !CanInstallMod( mod ) ) {
				return false;
			}

			_modSlots[ (int)mod.Slot ] = new InstalledFirearmMod(
				mod.Slot,
				mod.Id
			);

			return true;
		}

		private bool SupportsSlot( FirearmModSlot slot )
		{
			return slot switch {
				FirearmModSlot.Optic => _definition.Flags.HasFlag( FirearmFlags.SupportsOptics ),
				FirearmModSlot.Barrel => _definition.Flags.HasFlag( FirearmFlags.SupportsBarrelMods ),
				FirearmModSlot.Magazine => _definition.Flags.HasFlag( FirearmFlags.SupportsMagazineMods ),
				FirearmModSlot.Underbarrel => _definition.Flags.HasFlag( FirearmFlags.SupportsUnderbarrel ),

				FirearmModSlot.Stock => true,
				FirearmModSlot.Grip => true,
				FirearmModSlot.Receiver => true,
				FirearmModSlot.Charm => true,

				_ => false
			};
		}

		public FirearmResolvedStats Resolve( IReadOnlyList<FirearmModDefinition> mods )
		{
			FirearmFlags flags = _definition.Flags;

			float fireRate = _definition.FireRate;
			float magazineSize = _definition.MagazineSize;

			float adsSpeed = _definition.BaseAimDownSightsSpeed;
			float adsAccuracy = 1.0f;
			float hipFireAccuracy = _definition.BaseHipFireAccuracy;
			float movementAccuracy = 1.0f;

			float recoilKick = 1.0f;
			float recoilRecovery = _definition.BaseRecoilRecovery;
			float spreadBloom = 1.0f;
			float spreadRecovery = 1.0f;

			float reloadSpeed = _definition.BaseReloadSpeed;
			float equipSpeed = _definition.BaseEquipSpeed;

			float jamChance = _definition.BaseJamChance;
			float misfireChance = 0.0f;
			float heatPerShot = 1.0f;
			float heatRecovery = 1.0f;
			float dirtAccumulation = 1.0f;

			float loudness = 1.0f;
			float muzzleFlash = 1.0f;
			float shooterReveal = 1.0f;

			float weight = 1.0f;
			float handling = 1.0f;

			foreach ( FirearmModDefinition mod in mods ) {
				flags |= mod.AddedFirearmFlags;
				flags &= ~mod.RemovedFirearmFlags;

				foreach ( FirearmStatModifier modifier in mod.Modifiers ) {
					ref float target = ref GetStatRef(
						modifier.Stat,
						ref fireRate,
						ref magazineSize,
						ref adsSpeed,
						ref adsAccuracy,
						ref hipFireAccuracy,
						ref movementAccuracy,
						ref recoilKick,
						ref recoilRecovery,
						ref spreadBloom,
						ref spreadRecovery,
						ref reloadSpeed,
						ref equipSpeed,
						ref jamChance,
						ref misfireChance,
						ref heatPerShot,
						ref heatRecovery,
						ref dirtAccumulation,
						ref loudness,
						ref muzzleFlash,
						ref shooterReveal,
						ref weight,
						ref handling
					);

					ApplyModifier( ref target, modifier );
				}
			}

			return new FirearmResolvedStats {
				Flags = flags,
				FireRate = fireRate,
				MagazineSize = Math.Max( 1, (int)MathF.Round( magazineSize ) ),

				AimDownSightsSpeed = adsSpeed,
				AimDownSightsAccuracy = adsAccuracy,
				HipFireAccuracy = hipFireAccuracy,
				MovementAccuracy = movementAccuracy,

				RecoilKick = recoilKick,
				RecoilRecovery = recoilRecovery,
				SpreadBloom = spreadBloom,
				SpreadRecovery = spreadRecovery,

				ReloadSpeed = reloadSpeed,
				EquipSpeed = equipSpeed,

				JamChance = jamChance,
				MisfireChance = misfireChance,
				HeatPerShot = heatPerShot,
				HeatRecovery = heatRecovery,
				DirtAccumulation = dirtAccumulation,

				Loudness = loudness,
				MuzzleFlash = muzzleFlash,
				ShooterRevealStrength = shooterReveal,

				Weight = weight,
				Handling = handling
			};
		}

		private static void ApplyModifier( ref float value, FirearmStatModifier modifier )
		{
			switch ( modifier.Operation ) {
				case FirearmModOperation.Add:
					value += modifier.Value;
					break;

				case FirearmModOperation.Multiply:
					value *= modifier.Value;
					break;

				case FirearmModOperation.Override:
					value = modifier.Value;
					break;
			}
		}

		private static ref float GetStatRef(
			FirearmModStat stat,
			ref float fireRate,
			ref float magazineSize,
			ref float adsSpeed,
			ref float adsAccuracy,
			ref float hipFireAccuracy,
			ref float movementAccuracy,
			ref float recoilKick,
			ref float recoilRecovery,
			ref float spreadBloom,
			ref float spreadRecovery,
			ref float reloadSpeed,
			ref float equipSpeed,
			ref float jamChance,
			ref float misfireChance,
			ref float heatPerShot,
			ref float heatRecovery,
			ref float dirtAccumulation,
			ref float loudness,
			ref float muzzleFlash,
			ref float shooterReveal,
			ref float weight,
			ref float handling
		)
		{
			switch ( stat ) {
				case FirearmModStat.FireRate:
					return ref fireRate;
				case FirearmModStat.MagazineSize:
					return ref magazineSize;
				case FirearmModStat.AimDownSightsSpeed:
					return ref adsSpeed;
				case FirearmModStat.AimDownSightsAccuracy:
					return ref adsAccuracy;
				case FirearmModStat.HipFireAccuracy:
					return ref hipFireAccuracy;
				case FirearmModStat.MovementAccuracy:
					return ref movementAccuracy;
				case FirearmModStat.RecoilKick:
					return ref recoilKick;
				case FirearmModStat.RecoilRecovery:
					return ref recoilRecovery;
				case FirearmModStat.SpreadBloom:
					return ref spreadBloom;
				case FirearmModStat.SpreadRecovery:
					return ref spreadRecovery;
				case FirearmModStat.ReloadSpeed:
					return ref reloadSpeed;
				case FirearmModStat.EquipSpeed:
					return ref equipSpeed;
				case FirearmModStat.JamChance:
					return ref jamChance;
				case FirearmModStat.MisfireChance:
					return ref misfireChance;
				case FirearmModStat.HeatPerShot:
					return ref heatPerShot;
				case FirearmModStat.HeatRecovery:
					return ref heatRecovery;
				case FirearmModStat.DirtAccumulation:
					return ref dirtAccumulation;
				case FirearmModStat.Loudness:
					return ref loudness;
				case FirearmModStat.MuzzleFlash:
					return ref muzzleFlash;
				case FirearmModStat.ShooterRevealStrength:
					return ref shooterReveal;
				case FirearmModStat.Weight:
					return ref weight;
				case FirearmModStat.Handling:
					return ref handling;
				default:
					return ref handling;
			}
		}
	};
};
