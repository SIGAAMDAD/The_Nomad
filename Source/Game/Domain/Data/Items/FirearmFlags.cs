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

namespace Nomad.Game.Domain.Data.Items
{
	/*
	===================================================================================

	FirearmFlags

	===================================================================================
	*/
	/// <summary>
	/// Describes the mechanical, handling, reliability, ammunition, stealth, attachment,
	/// and special-purpose traits of a firearm.
	///
	/// These flags should define what the weapon is allowed to do and which gameplay
	/// systems need to process it. They should not contain mutable runtime state such as
	/// current ammo, heat, dirt, chamber state, jam state, or durability.
	/// </summary>

	[Flags]
	public enum FirearmFlags : ulong
	{
		/// <summary>
		/// No firearm traits are enabled.
		/// </summary>
		None = 0,

		// ---------------------------------------------------------------------
		// Basic operation
		// ---------------------------------------------------------------------

		/// <summary>
		/// Allows the player to enter an aimed/precision firing stance.
		/// Usually reduces spread, increases accuracy, narrows movement options,
		/// and may change camera/reticle behavior.
		/// </summary>
		CanAimDownSights = 1UL << 0,

		/// <summary>
		/// Allows the weapon to be fired without aiming down sights.
		/// Used by sidearms, shotguns, compact weapons, and fast close-range weapons.
		/// </summary>
		CanHipFire = 1UL << 1,

		/// <summary>
		/// Allows the weapon to fire while the player is moving normally.
		/// If absent, the player must be idle, braced, crouched, or otherwise stabilized.
		/// </summary>
		CanFireWhileMoving = 1UL << 2,

		/// <summary>
		/// Allows the weapon to fire during dash movement.
		/// Typically reserved for light, one-hand-capable, or highly mobile weapons.
		/// </summary>
		CanFireWhileDashing = 1UL << 3,

		/// <summary>
		/// Allows the weapon to fire while sliding.
		/// Useful for aggressive mobility weapons and cinematic close-range combat.
		/// </summary>
		CanFireWhileSliding = 1UL << 4,

		/// <summary>
		/// Allows the weapon to fire while airborne or jumping.
		/// If absent, firing should be blocked or heavily penalized while airborne.
		/// </summary>
		CanFireWhileJumping = 1UL << 5,

		/// <summary>
		/// Allows the weapon to fire during a parry window or parry state.
		/// Intended for weapons that can be used during defensive/counter actions.
		/// </summary>
		CanFireDuringParry = 1UL << 6,

		// ---------------------------------------------------------------------
		// Handling / cycling
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon has a manual action such as pump, lever, bolt, or break operation.
		/// The firing loop should require an explicit or animated cycling step.
		/// </summary>
		HasManualAction = 1UL << 7,

		/// <summary>
		/// After firing, the weapon cannot fire again until its action has been cycled.
		/// Usually paired with HasManualAction.
		/// </summary>
		RequiresCycleAfterShot = 1UL << 8,

		/// <summary>
		/// The weapon must be cocked/charged before it can fire.
		/// Used for weapons that have a readying step before the first shot or after reload.
		/// </summary>
		RequiresCocking = 1UL << 9,

		/// <summary>
		/// The weapon has a burst limiter.
		/// The fire-control system should stop firing after the configured burst count.
		/// </summary>
		HasBurstLimiter = 1UL << 10,

		/// <summary>
		/// The weapon fires from an open-bolt state.
		/// Gameplay systems may use this for first-shot delay, heat behavior, reliability,
		/// or animation differences.
		/// </summary>
		OpenBolt = 1UL << 11,

		/// <summary>
		/// The weapon fires from a closed-bolt state.
		/// Gameplay systems may use this for accuracy, chamber handling, reload behavior,
		/// or animation differences.
		/// </summary>
		ClosedBolt = 1UL << 12,

		// ---------------------------------------------------------------------
		// Feeding / reload behavior
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon feeds from a detachable magazine.
		/// Reloads generally swap the magazine as a single unit.
		/// </summary>
		HasDetachableMagazine = 1UL << 13,

		/// <summary>
		/// The weapon has an internal magazine.
		/// Reload behavior usually inserts rounds directly into the weapon.
		/// </summary>
		HasInternalMagazine = 1UL << 14,

		/// <summary>
		/// The weapon uses a tube magazine.
		/// Reload behavior is usually one shell/round at a time and may support interruptible reloads.
		/// </summary>
		HasTubeMagazine = 1UL << 15,

		/// <summary>
		/// The weapon uses a cylinder.
		/// Reload and ammo display should use chamber/cylinder-style logic instead of magazine logic.
		/// </summary>
		HasCylinder = 1UL << 16,

		/// <summary>
		/// The weapon only holds one shot before requiring reload or cycling.
		/// Used for single-shot pistols, rifles, launchers, or improvised firearms.
		/// </summary>
		SingleShot = 1UL << 17,

		/// <summary>
		/// The weapon opens for loading/unloading.
		/// Reload animation and chamber state should use break-action behavior.
		/// </summary>
		BreakAction = 1UL << 18,

		/// <summary>
		/// The weapon reloads one round or shell at a time.
		/// Allows partial reloads and reload interruption.
		/// </summary>
		PerShellReload = 1UL << 19,

		/// <summary>
		/// The weapon supports tactical reloads.
		/// Reloading before empty may preserve chambered rounds or use a faster reload path.
		/// </summary>
		SupportsTacticalReload = 1UL << 20,

		// ---------------------------------------------------------------------
		// Reliability / maintenance
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon can jam.
		/// Reliability systems may trigger stoppages based on condition, dirt, heat, ammo, or weather.
		/// </summary>
		CanJam = 1UL << 21,

		/// <summary>
		/// The weapon can build heat through firing.
		/// Heat systems may affect accuracy, fire rate, reliability, damage, or forced cooldown.
		/// </summary>
		CanOverheat = 1UL << 22,

		/// <summary>
		/// The weapon accumulates dirt/fouling through use or environment exposure.
		/// Maintenance systems may reduce reliability or increase jam/misfire risk.
		/// </summary>
		GetsDirty = 1UL << 23,

		/// <summary>
		/// The weapon is affected by weather conditions.
		/// Sand, rain, mud, ash, or storms may alter reliability, handling, or visibility.
		/// </summary>
		WeatherSensitive = 1UL << 24,

		/// <summary>
		/// The weapon has an elevated chance to misfire.
		/// Usually used for crude, damaged, old, cursed, or unstable weapons.
		/// </summary>
		MisfireProne = 1UL << 25,

		/// <summary>
		/// Explosive ammunition has additional failure risk in this weapon.
		/// Used by reliability/damage systems to model dangerous ammo-weapon combinations.
		/// </summary>
		ExplosiveAmmoRisk = 1UL << 26,

		// ---------------------------------------------------------------------
		// Projectile / penetration traits
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon fires multiple pellets per shot.
		/// Ballistics should use spread-pattern logic instead of a single projectile ray/path.
		/// </summary>
		PelletSpread = 1UL << 27,

		/// <summary>
		/// The weapon can fire slug-style single projectiles.
		/// Usually modifies shotgun-like weapons into precision/high-impact single-shot behavior.
		/// </summary>
		FiresSlug = 1UL << 28,

		/// <summary>
		/// The weapon can use armor-piercing ammunition.
		/// Ammo systems may allow AP ammo selection and armor systems should account for it.
		/// </summary>
		ArmorPiercingCapable = 1UL << 29,

		/// <summary>
		/// The weapon can use hollow-point or soft-target ammunition.
		/// Damage systems may increase unarmored flesh damage and reduce armor performance.
		/// </summary>
		HollowPointCapable = 1UL << 30,

		/// <summary>
		/// The weapon can use explosive ammunition.
		/// Damage systems may spawn area damage, impact effects, or instability checks.
		/// </summary>
		ExplosiveAmmoCapable = 1UL << 31,

		/// <summary>
		/// The weapon can use incendiary ammunition.
		/// Damage systems may apply burn, ignite materials, or create fire effects.
		/// </summary>
		IncendiaryCapable = 1UL << 32,

		/// <summary>
		/// The weapon uses energy-based firing behavior instead of conventional ballistic ammo.
		/// May use charge, heat, battery, beam, pulse, or nonstandard damage rules.
		/// </summary>
		EnergyWeapon = 1UL << 33,

		/// <summary>
		/// The weapon can use subsonic ammunition.
		/// </summary>
		SubsonicCapable = 1UL << 34,

		// ---------------------------------------------------------------------
		// Recoil / ergonomics
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon has high recoil.
		/// Firing should significantly affect aim recovery, spread, camera impulse, or stance control.
		/// </summary>
		HighRecoil = 1UL << 35,

		/// <summary>
		/// The weapon is intended to be used with two hands.
		/// May restrict offhand actions, shield use, parry use, or one-handed firing.
		/// </summary>
		TwoHanded = 1UL << 36,

		/// <summary>
		/// The weapon can be used one-handed.
		/// Allows firing with an occupied offhand, during certain movement states, or while using items.
		/// </summary>
		OneHandCapable = 1UL << 37,

		/// <summary>
		/// The weapon is heavy enough to affect handling.
		/// May slow aim, reduce movement speed, delay equip time, or restrict mobility actions.
		/// </summary>
		HeavyWeapon = 1UL << 38,

		/// <summary>
		/// The weapon can be concealed.
		/// Stealth, disguise, inspection, and social systems may treat it as hideable.
		/// </summary>
		Concealable = 1UL << 39,

		// ---------------------------------------------------------------------
		// Stealth / signature
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon produces a very loud firing report.
		/// AI hearing, alert propagation, and stealth systems should treat it as highly detectable.
		/// </summary>
		LoudReport = 1UL << 40,

		/// <summary>
		/// The weapon can accept a suppressor or similar signature-reducing barrel device.
		/// Attachment systems may allow suppressor installation.
		/// </summary>
		SuppressorCompatible = 1UL << 41,

		/// <summary>
		/// Firing this weapon strongly reveals the shooter's position.
		/// AI, minimap, threat indicators, or multiplayer reveal systems may expose the shooter.
		/// </summary>
		RevealsShooterPosition = 1UL << 42,

		/// <summary>
		/// The weapon produces a large muzzle flash.
		/// Visibility, night combat, stealth, and AI perception systems may use this as a visual cue.
		/// </summary>
		MuzzleFlashHeavy = 1UL << 43,

		// ---------------------------------------------------------------------
		// Attachment compatibility
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon supports optic attachments.
		/// Allows sights, scopes, magnifiers, or targeting devices.
		/// </summary>
		SupportsOptics = 1UL << 44,

		/// <summary>
		/// The weapon supports barrel attachments.
		/// Allows suppressors, brakes, compensators, flash hiders, or other muzzle/barrel devices.
		/// </summary>
		SupportsBarrelMods = 1UL << 45,

		/// <summary>
		/// The weapon supports magazine or feeding-system attachments.
		/// Allows extended magazines, quick magazines, alternate cylinders, or feed upgrades.
		/// </summary>
		SupportsMagazineMods = 1UL << 46,

		/// <summary>
		/// The weapon supports ammunition modifiers.
		/// Allows special ammo selection or ammo conversion where compatible.
		/// </summary>
		SupportsAmmoMods = 1UL << 47,

		/// <summary>
		/// The weapon supports underbarrel attachments.
		/// Allows grips, launchers, bayonets, lights, or other underbarrel devices.
		/// </summary>
		SupportsUnderbarrel = 1UL << 48,

		// ---------------------------------------------------------------------
		// Special behavior
		// ---------------------------------------------------------------------

		/// <summary>
		/// The weapon is intended for anti-material use.
		/// It should be effective against cover, machinery, vehicles, structures, or equipment.
		/// </summary>
		AntiMaterial = 1UL << 49,

		/// <summary>
		/// The weapon is intended for anti-armor use.
		/// It should perform well against armored enemies, heavy units, or armor plates.
		/// </summary>
		AntiArmor = 1UL << 50,

		/// <summary>
		/// The weapon is closer to a siege/heavy emplacement weapon than a normal firearm.
		/// May restrict mobility, require bracing, use special ammo, or damage structures heavily.
		/// </summary>
		SiegeWeapon = 1UL << 51,

		/// <summary>
		/// Projectiles from this weapon can ricochet.
		/// Ballistics should allow rebound behavior from valid surfaces.
		/// </summary>
		CanRicochet = 1UL << 52,

		/// <summary>
		/// Projectiles from this weapon can pass through targets.
		/// Ballistics should continue tracing after a hit when penetration conditions are met.
		/// </summary>
		CanPenetrateTargets = 1UL << 53,

		/// <summary>
		/// The weapon can stagger heavy enemies.
		/// Stagger systems should allow this weapon to interrupt or destabilize large targets.
		/// </summary>
		CanStaggerHeavies = 1UL << 54
	};
};
