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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk;
using Nomad.Game.Gameplay.Entity;

namespace Nomad.Game.Gameplay.Entity
{
	/*
	===================================================================================

	StatusEffectController

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class StatusEffectController
	{
		public EntityId EntityId { get; }

		public IReadOnlyCollection<StatusEffectInstance> ActiveEffects => _effects.Values;

		private readonly Dictionary<InternString, StatusEffectInstance> _effects = new();

		private readonly Action<EntityDamageRequest>? _damageSink;
		private readonly Action<EntityHealRequest>? _healSink;

		public StatusEffectController( EntityId entityId, Action<EntityDamageRequest>? damageSink = null, Action<EntityHealRequest>? healSink = null )
		{
			EntityId = entityId;
			_damageSink = damageSink;
			_healSink = healSink;
		}

		public bool HasEffect( InternString id )
		{
			return _effects.ContainsKey( id );
		}

		public bool TryGetEffect( InternString id, out StatusEffectInstance effect )
		{
			return _effects.TryGetValue( id, out effect );
		}

		public bool Apply(
			StatusEffectDefinition definition,
			EntityId? sourceEntityId,
			byte stacks,
			uint serverTick )
		{
			ArgumentGuard.ThrowIfNull( definition );

			stacks = Math.Max( (byte)1, stacks );

			if ( !_effects.TryGetValue( definition.Id, out StatusEffectInstance? existing ) ) {
				_effects[definition.Id] = new StatusEffectInstance(
					definition,
					EntityId,
					sourceEntityId,
					stacks,
					serverTick
				);

				return true;
			}

			switch ( definition.StackPolicy ) {
				case StatusEffectStackingPolicy.Ignore:
					return false;

				case StatusEffectStackingPolicy.RefreshDuration:
					existing.RefreshDuration();
					return true;

				case StatusEffectStackingPolicy.AddStack:
					existing.AddStacks( stacks );
					return true;

				case StatusEffectStackingPolicy.AddStackAndRefresh:
					existing.AddStacks( stacks );
					existing.RefreshDuration();
					return true;

				case StatusEffectStackingPolicy.Replace:
					existing.Replace( stacks );
					return true;

				case StatusEffectStackingPolicy.StrongestOnly:
					if ( stacks > existing.Stacks ) {
						existing.Replace( stacks );
						return true;
					}

					existing.RefreshDuration();
					return true;

				default:
					return false;
			}
		}

		public bool Remove( InternString id )
		{
			return _effects.Remove( id );
		}

		public void Clear( StatusEffectClearReason reason )
		{
			if ( reason == StatusEffectClearReason.None ) {
				_effects.Clear();
				return;
			}

			List<InternString> toRemove = new();

			foreach ( StatusEffectInstance effect in _effects.Values ) {
				StatusEffectFlags flags = effect.Definition.Flags;

				bool remove =
					reason == StatusEffectClearReason.Death &&
					flags.HasFlag( StatusEffectFlags.RemovedOnDeath );

				remove |=
					reason == StatusEffectClearReason.Respawn &&
					flags.HasFlag( StatusEffectFlags.RemovedOnRespawn );

				remove |=
					reason == StatusEffectClearReason.RoundEnd &&
					flags.HasFlag( StatusEffectFlags.RemovedOnRoundEnd );

				remove |= reason == StatusEffectClearReason.MatchEnd;
				remove |= reason == StatusEffectClearReason.EntityDisposed;
				remove |= reason == StatusEffectClearReason.Cleanse &&
						  flags.HasFlag( StatusEffectFlags.Harmful );

				if ( remove ) {
					toRemove.Add( effect.Definition.Id );
				}
			}

			for ( int i = 0; i < toRemove.Count; i++ ) {
				_effects.Remove( toRemove[i] );
			}
		}

		public void RunFrame( float deltaSeconds, uint serverTick )
		{
			if ( _effects.Count == 0 ) {
				return;
			}

			List<InternString>? expired = null;

			foreach ( StatusEffectInstance effect in _effects.Values ) {
				bool didExpire = effect.Advance( deltaSeconds, out bool shouldTick );

				if ( shouldTick ) {
					TickEffect( effect, serverTick );
				}

				if ( didExpire ) {
					expired ??= new List<InternString>();
					expired.Add( effect.Definition.Id );
				}
			}

			if ( expired == null ) {
				return;
			}

			for ( int i = 0; i < expired.Count; i++ ) {
				_effects.Remove( expired[i] );
			}
		}

		public float EvaluateModifier( StatusEffectModifierTarget target, float baseValue )
		{
			float additive = 0.0f;
			float multiplier = 1.0f;
			float? overrideValue = null;

			foreach ( StatusEffectInstance effect in _effects.Values ) {
				foreach ( StatusEffectModifier modifier in effect.Definition.Modifiers ) {
					if ( modifier.Target != target ) {
						continue;
					}

					float value = modifier.ScalesWithStacks
						? modifier.Value * effect.Stacks
						: modifier.Value;

					switch ( modifier.Operation ) {
						case StatusEffectModifierOperation.Add:
							additive += value;
							break;

						case StatusEffectModifierOperation.Multiply:
							multiplier *= value;
							break;

						case StatusEffectModifierOperation.Override:
							overrideValue = value;
							break;
					}
				}
			}

			if ( overrideValue.HasValue ) {
				return overrideValue.Value;
			}

			return (baseValue + additive) * multiplier;
		}

		private void TickEffect( StatusEffectInstance effect, uint serverTick )
		{
			float stacks = effect.Stacks;

			if ( effect.Definition.DamagePerTick > 0.0f ) {
				_damageSink?.Invoke(
					new EntityDamageRequest(
						attackerId: effect.SourceEntityId,
						source: DamageSource.StatusEffect,
						amount: effect.Definition.DamagePerTick * stacks,
						canKill: true
					)
				);
			}

			if ( effect.Definition.HealPerTick > 0.0f ) {
				_healSink?.Invoke(
					new EntityHealRequest(
						sourceId: effect.SourceEntityId,
						amount: effect.Definition.HealPerTick * stacks,
						allowRevive: false
					)
				);
			}
		}
	};
};
