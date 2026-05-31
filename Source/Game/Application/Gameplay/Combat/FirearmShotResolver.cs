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
using System.Numerics;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Application.Gameplay.Combat
{
	/// <summary>
	/// Resolves the ballistic outcome of a single firearm shot.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class is intentionally deterministic. It does not roll random spread,
	/// create projectiles, mutate ammunition, or apply damage. Callers are
	/// expected to consume a round through <see cref="IFirearmInstance"/> before
	/// calling the resolver, then publish events or apply damage from the
	/// returned <see cref="FirearmShotResolution"/>.
	/// </para>
	/// <para>
	/// Ammunition is the source of ballistic output. Range, damage, projectile
	/// velocity, and the raw recoil impulse are read from the firearm's
	/// <see cref="IFirearmInstance.LoadedAmmo"/>. Firearm stats are used only
	/// for handling effects such as spread and recoil handling multipliers.
	/// </para>
	/// </remarks>
	internal sealed class FirearmShotResolver
	{
		private const float BASE_SPREAD_RADIANS = 0.035f;
		private const float MINIMUM_ACCURACY = 0.01f;
		private const float MINIMUM_HIT_TOLERANCE = 0.08f;

		/// <summary>
		/// Resolves a shot without target candidates.
		/// </summary>
		/// <param name="firearm">The firearm that fired.</param>
		/// <param name="origin">World-space muzzle or shot origin.</param>
		/// <param name="angleRadians">Aim angle before spread is applied.</param>
		/// <returns>A miss resolution containing the computed shot path.</returns>
		public FirearmShotResolution Resolve(
			IFirearmInstance firearm,
			Vector2 origin,
			float angleRadians
		)
		{
			return Resolve(
				firearm,
				origin,
				angleRadians,
				Array.Empty<ISpatialEntity>(),
				FirearmShotContext.Default
			);
		}

		/// <summary>
		/// Resolves a shot against damageable spatial targets.
		/// </summary>
		/// <param name="firearm">The firearm that fired.</param>
		/// <param name="origin">World-space muzzle or shot origin.</param>
		/// <param name="angleRadians">Aim angle before spread is applied.</param>
		/// <param name="targets">
		/// Spatial entities that can be considered for hit testing. Only active
		/// entities that also implement <see cref="IDamageableEntity"/> are valid
		/// shot targets.
		/// </param>
		/// <returns>
		/// A complete shot resolution containing either the closest valid hit or
		/// the final miss point.
		/// </returns>
		public FirearmShotResolution Resolve(
			IFirearmInstance firearm,
			Vector2 origin,
			float angleRadians,
			IEnumerable<ISpatialEntity> targets
		)
		{
			return Resolve(
				firearm,
				origin,
				angleRadians,
				targets,
				FirearmShotContext.Default
			);
		}

		/// <summary>
		/// Resolves a shot against damageable spatial targets using explicit
		/// handling context.
		/// </summary>
		/// <param name="firearm">The firearm that fired.</param>
		/// <param name="origin">World-space muzzle or shot origin.</param>
		/// <param name="angleRadians">Aim angle before spread is applied.</param>
		/// <param name="targets">
		/// Spatial entities that can be considered for hit testing. Only active
		/// entities that also implement <see cref="IDamageableEntity"/> are valid
		/// shot targets.
		/// </param>
		/// <param name="context">
		/// Shooter and handling context for this shot. The normalized spread
		/// offset allows caller-controlled randomness while keeping this resolver
		/// deterministic.
		/// </param>
		/// <returns>
		/// A complete shot resolution containing either the closest valid hit, a
		/// miss, or a failure reason explaining why the firearm could not produce
		/// a shot path.
		/// </returns>
		public FirearmShotResolution Resolve(
			IFirearmInstance firearm,
			Vector2 origin,
			float angleRadians,
			IEnumerable<ISpatialEntity> targets,
			FirearmShotContext context
		)
		{
			if ( firearm == null ) {
				return FirearmShotResolution.Failed(
					FirearmShotFailureReason.InvalidFirearm,
					origin,
					Vector2.UnitX,
					angleRadians
				);
			}

			AmmoDefinition? ammo = firearm.LoadedAmmo;
			if ( ammo == null ) {
				return FirearmShotResolution.Failed(
					FirearmShotFailureReason.NoLoadedAmmo,
					origin,
					Vector2.UnitX,
					angleRadians
				);
			}

			if ( !firearm.FirearmDefinition.AcceptsAmmo( ammo ) ) {
				return FirearmShotResolution.Failed(
					FirearmShotFailureReason.IncompatibleAmmo,
					origin,
					Vector2.UnitX,
					angleRadians
				);
			}

			if ( !IsFinite( origin ) || !float.IsFinite( angleRadians ) ) {
				return FirearmShotResolution.Failed(
					FirearmShotFailureReason.InvalidDirection,
					origin,
					Vector2.UnitX,
					angleRadians
				);
			}

			if ( ammo.Range <= 0.0f || !float.IsFinite( ammo.Range ) ) {
				return FirearmShotResolution.Failed(
					FirearmShotFailureReason.InvalidRange,
					origin,
					Vector2.UnitX,
					angleRadians
				);
			}

			FirearmResolvedStats stats = firearm.Stats;
			float spreadRadians = CalculateSpreadRadians( stats, context );
			float resolvedAngle = angleRadians + (Math.Clamp( context.NormalizedSpreadOffset, -1.0f, 1.0f ) * spreadRadians);
			Vector2 direction = DirectionFromAngle( resolvedAngle );
			Vector2 endPosition = origin + (direction * ammo.Range);

			ISpatialEntity? hitEntity = null;
			Vector2 hitPosition = endPosition;
			float hitDistance = ammo.Range;
			float hitTolerance = CalculateHitTolerance( context );

			if ( targets != null ) {
				foreach ( ISpatialEntity target in targets ) {
					if ( !CanHitTarget( target, context.ShooterId ) ) {
						continue;
					}

					if ( TryIntersectTarget(
						target,
						origin,
						direction,
						ammo.Range,
						hitTolerance,
						out float targetDistance,
						out Vector2 targetHitPosition
					) && targetDistance < hitDistance ) {
						hitEntity = target;
						hitDistance = targetDistance;
						hitPosition = targetHitPosition;
					}
				}
			}

			return FirearmShotResolution.Resolved(
				origin,
				direction,
				resolvedAngle,
				endPosition,
				hitEntity,
				hitPosition,
				hitDistance,
				ammo.Range,
				ammo.Damage,
				ammo.Velocity,
				CalculateRecoilImpulse( ammo, stats ),
				spreadRadians
			);
		}

		private static bool CanHitTarget( ISpatialEntity? target, EntityId shooterId )
		{
			if ( target == null || !target.IsValid || !target.IsActive || target.IsHidden || target.IsDestroyed ) {
				return false;
			}

			if ( shooterId.IsValid && target.Id == shooterId ) {
				return false;
			}

			return target is IDamageableEntity;
		}

		private static bool TryIntersectTarget(
			ISpatialEntity target,
			Vector2 origin,
			Vector2 direction,
			float range,
			float hitTolerance,
			out float distance,
			out Vector2 hitPosition
		)
		{
			distance = 0.0f;
			hitPosition = origin;

			Vector2 toTarget = target.Position - origin;
			float projectedDistance = Vector2.Dot( toTarget, direction );
			if ( projectedDistance < 0.0f || projectedDistance > range ) {
				return false;
			}

			Vector2 closestPoint = origin + (direction * projectedDistance);
			float radius = MathF.Max( hitTolerance, CalculateTargetRadius( target ) );
			if ( Vector2.DistanceSquared( target.Position, closestPoint ) > radius * radius && !target.ContainsPoint( closestPoint ) ) {
				return false;
			}

			distance = projectedDistance;
			hitPosition = closestPoint;
			return true;
		}

		private static float CalculateSpreadRadians( FirearmResolvedStats stats, FirearmShotContext context )
		{
			float accuracy = context.IsAimingDownSights
				? stats.AimDownSightsAccuracy
				: stats.HipFireAccuracy;

			if ( context.IsMoving ) {
				accuracy *= stats.MovementAccuracy;
			}

			accuracy = MathF.Max( MINIMUM_ACCURACY, accuracy );
			float bloom = MathF.Max( 0.0f, stats.SpreadBloom );

			return BASE_SPREAD_RADIANS * bloom / accuracy;
		}

		private static float CalculateRecoilImpulse( AmmoDefinition ammo, FirearmResolvedStats stats )
		{
			return MathF.Max( 0.0f, ammo.Recoil ) * MathF.Max( 0.0f, stats.RecoilKick );
		}

		private static float CalculateHitTolerance( FirearmShotContext context )
		{
			return MathF.Max( MINIMUM_HIT_TOLERANCE, context.HitTolerance );
		}

		private static float CalculateTargetRadius( ISpatialEntity target )
		{
			Vector2 scale = target.Scale;
			return MathF.Max( MathF.Abs( scale.X ), MathF.Abs( scale.Y ) ) * 0.5f;
		}

		private static Vector2 DirectionFromAngle( float angleRadians )
		{
			return new Vector2( MathF.Cos( angleRadians ), MathF.Sin( angleRadians ) );
		}

		private static bool IsFinite( Vector2 vector )
		{
			return float.IsFinite( vector.X ) && float.IsFinite( vector.Y );
		}
	};

	/// <summary>
	/// Context that affects firearm handling for a single resolved shot.
	/// </summary>
	/// <remarks>
	/// The resolver does not generate randomness. Callers that want spread
	/// variance should provide <see cref="NormalizedSpreadOffset"/> as a value
	/// between -1 and 1, usually from a deterministic simulation RNG.
	/// </remarks>
	internal readonly struct FirearmShotContext
	{
		/// <summary>
		/// Default context for a stationary, unaimed shot with no shooter
		/// exclusion and no spread offset.
		/// </summary>
		public static FirearmShotContext Default { get; } = new FirearmShotContext();

		/// <summary>
		/// Entity that fired the shot. Valid shooter ids are excluded from hit
		/// testing to prevent immediate self-hits.
		/// </summary>
		public EntityId ShooterId { get; }

		/// <summary>
		/// Whether the shooter is aiming down sights for this shot.
		/// </summary>
		public bool IsAimingDownSights { get; }

		/// <summary>
		/// Whether movement handling penalties should affect this shot.
		/// </summary>
		public bool IsMoving { get; }

		/// <summary>
		/// Deterministic spread offset in the range -1 to 1. The resolver clamps
		/// values outside that range before multiplying by computed spread.
		/// </summary>
		public float NormalizedSpreadOffset { get; }

		/// <summary>
		/// Minimum radius, in world units, used when testing ray proximity
		/// against spatial entities.
		/// </summary>
		public float HitTolerance { get; }

		public FirearmShotContext(
			EntityId shooterId = default,
			bool isAimingDownSights = false,
			bool isMoving = false,
			float normalizedSpreadOffset = 0.0f,
			float hitTolerance = 0.08f
		)
		{
			ShooterId = shooterId;
			IsAimingDownSights = isAimingDownSights;
			IsMoving = isMoving;
			NormalizedSpreadOffset = normalizedSpreadOffset;
			HitTolerance = hitTolerance;
		}
	};

	/// <summary>
	/// Reason a shot could not be resolved into a valid ballistic path.
	/// </summary>
	internal enum FirearmShotFailureReason : byte
	{
		None = 0,
		InvalidFirearm,
		NoLoadedAmmo,
		IncompatibleAmmo,
		InvalidDirection,
		InvalidRange
	}

	/// <summary>
	/// Complete deterministic output for one firearm shot.
	/// </summary>
	/// <remarks>
	/// A successful resolution may still be a miss. Check <see cref="DidFire"/>
	/// to know whether the shot produced a path, and <see cref="DidHit"/> to
	/// know whether that path intersected a valid damageable target.
	/// </remarks>
	internal readonly struct FirearmShotResolution
	{
		/// <summary>
		/// Whether the firearm produced a valid shot path.
		/// </summary>
		public bool DidFire => FailureReason == FirearmShotFailureReason.None;

		/// <summary>
		/// Whether the shot hit a valid damageable target.
		/// </summary>
		public bool DidHit => HitEntityId.IsValid;

		/// <summary>
		/// Failure reason when <see cref="DidFire"/> is false.
		/// </summary>
		public FirearmShotFailureReason FailureReason { get; }

		/// <summary>
		/// World-space shot origin.
		/// </summary>
		public Vector2 Origin { get; }

		/// <summary>
		/// Normalized shot direction after spread is applied.
		/// </summary>
		public Vector2 Direction { get; }

		/// <summary>
		/// Final angle after spread is applied.
		/// </summary>
		public float AngleRadians { get; }

		/// <summary>
		/// End point of the shot if it misses every target.
		/// </summary>
		public Vector2 EndPosition { get; }

		/// <summary>
		/// Entity hit by this shot, or null when the shot missed.
		/// </summary>
		public ISpatialEntity? HitEntity { get; }

		/// <summary>
		/// Id of the hit entity, or <see cref="EntityId.Invalid"/> when the shot
		/// missed or failed.
		/// </summary>
		public EntityId HitEntityId { get; }

		/// <summary>
		/// Hit point when the shot struck a target, otherwise the miss end point.
		/// </summary>
		public Vector2 ImpactPosition { get; }

		/// <summary>
		/// Distance from origin to impact or miss end point.
		/// </summary>
		public float TravelDistance { get; }

		/// <summary>
		/// Maximum distance supplied by the loaded ammo.
		/// </summary>
		public float Range { get; }

		/// <summary>
		/// Damage amount supplied by the loaded ammo.
		/// </summary>
		public float Damage { get; }

		/// <summary>
		/// Projectile velocity supplied by the loaded ammo.
		/// </summary>
		public float Velocity { get; }

		/// <summary>
		/// Recoil impulse for this shot. The ammo supplies the raw impulse and
		/// firearm handling stats may scale how that impulse is felt.
		/// </summary>
		public float RecoilImpulse { get; }

		/// <summary>
		/// Maximum angular deviation applied by this shot's handling context.
		/// </summary>
		public float SpreadRadians { get; }

		private FirearmShotResolution(
			FirearmShotFailureReason failureReason,
			Vector2 origin,
			Vector2 direction,
			float angleRadians,
			Vector2 endPosition,
			ISpatialEntity? hitEntity,
			Vector2 impactPosition,
			float travelDistance,
			float range,
			float damage,
			float velocity,
			float recoilImpulse,
			float spreadRadians
		)
		{
			FailureReason = failureReason;
			Origin = origin;
			Direction = direction;
			AngleRadians = angleRadians;
			EndPosition = endPosition;
			HitEntity = hitEntity;
			HitEntityId = hitEntity?.Id ?? EntityId.Invalid;
			ImpactPosition = impactPosition;
			TravelDistance = travelDistance;
			Range = range;
			Damage = damage;
			Velocity = velocity;
			RecoilImpulse = recoilImpulse;
			SpreadRadians = spreadRadians;
		}

		/// <summary>
		/// Converts this shot resolution to the current gameplay damage result
		/// shape.
		/// </summary>
		/// <returns>
		/// A damage result with the hit id and damage amount, or a null hit when
		/// the shot missed or failed.
		/// </returns>
		public DamageResult ToDamageResult()
		{
			return new DamageResult(
				DidHit ? HitEntityId.Value : null,
				DidHit ? Damage : 0.0f
			);
		}

		internal static FirearmShotResolution Failed(
			FirearmShotFailureReason reason,
			Vector2 origin,
			Vector2 direction,
			float angleRadians
		)
		{
			return new FirearmShotResolution(
				reason,
				origin,
				direction,
				angleRadians,
				origin,
				null,
				origin,
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				0.0f
			);
		}

		internal static FirearmShotResolution Resolved(
			Vector2 origin,
			Vector2 direction,
			float angleRadians,
			Vector2 endPosition,
			ISpatialEntity? hitEntity,
			Vector2 impactPosition,
			float travelDistance,
			float range,
			float damage,
			float velocity,
			float recoilImpulse,
			float spreadRadians
		)
		{
			return new FirearmShotResolution(
				FirearmShotFailureReason.None,
				origin,
				direction,
				angleRadians,
				endPosition,
				hitEntity,
				impactPosition,
				travelDistance,
				range,
				damage,
				velocity,
				recoilImpulse,
				spreadRadians
			);
		}
	};
};
