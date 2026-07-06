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
using Nomad.Game.Sdk.Entities;

namespace Nomad.Game.Gameplay.Entity
{
	/*
	===================================================================================

	StatusEffectInstance

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class StatusEffectInstance
	{
		public bool IsExpired => Definition.DurationSeconds > 0.0f && RemainingSeconds <= 0.0f;

		public StatusEffectDefinition Definition { get; }

		public EntityId TargetEntityId { get; }
		public EntityId? SourceEntityId { get; }

		public byte Stacks { get; private set; }

		public float RemainingSeconds { get; private set; }
		public float TimeUntilNextTickSeconds { get; private set; }
		public uint AppliedAtTick { get; }

		public StatusEffectInstance(
			StatusEffectDefinition definition,
			EntityId targetEntityId,
			EntityId? sourceEntityId,
			byte stacks,
			uint appliedAtTick
		)
		{
			Definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
			TargetEntityId = targetEntityId;
			SourceEntityId = sourceEntityId;
			Stacks = stacks;
			AppliedAtTick = appliedAtTick;

			RemainingSeconds = definition.DurationSeconds;
			TimeUntilNextTickSeconds = definition.TickIntervalSeconds;
		}

		public void RefreshDuration()
		{
			RemainingSeconds = Definition.DurationSeconds;
		}

		public void AddStacks( byte amount )
		{
			int next = Stacks + amount;
			Stacks = (byte)Math.Clamp( next, 1, Definition.MaxStacks );
		}

		public void Replace( byte stacks )
		{
			Stacks = stacks;
			RemainingSeconds = Definition.DurationSeconds;
			TimeUntilNextTickSeconds = Definition.TickIntervalSeconds;
		}

		public bool Advance( float deltaSeconds, out bool shouldTick )
		{
			shouldTick = false;

			if ( Definition.DurationSeconds > 0.0f ) {
				RemainingSeconds -= deltaSeconds;
			}

			if ( Definition.TickIntervalSeconds <= 0.0f ) {
				return IsExpired;
			}

			TimeUntilNextTickSeconds -= deltaSeconds;

			if ( TimeUntilNextTickSeconds <= 0.0f ) {
				shouldTick = true;
				TimeUntilNextTickSeconds += Definition.TickIntervalSeconds;
			}

			return IsExpired;
		}
	}
}
