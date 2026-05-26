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

using System.Numerics;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Entities;

namespace Nomad.Game.Sdk.Interactables
{
    /// <summary>
    /// Immutable request payload for an entity interaction attempt.
    ///
    /// This describes who is interacting, what they are interacting with, what kind of
    /// interaction is being attempted, and the relevant runtime facts needed to validate
    /// the interaction.
    /// </summary>
    public readonly struct EntityInteractionContext
    {
        /// <summary>
        /// Entity performing the interaction.
        ///
        /// Usually the player, NPC, AI actor, script driver, or another world entity.
        /// </summary>
        public EntityId ActorId { get; }

        /// <summary>
        /// Entity being interacted with.
        /// </summary>
        public EntityId TargetId { get; }

        /// <summary>
        /// Broad interaction category.
        /// </summary>
        public EntityInteractionKind Kind { get; }

        /// <summary>
        /// Optional specific interaction verb.
        ///
        /// Examples:
        /// "read_note"
        /// "loot_all"
        /// "open_slow"
        /// "drink"
        /// "rest_checkpoint"
        /// "unlock_with_key"
        ///
        /// Use this when EntityInteractionKind is not specific enough.
        /// </summary>
        public InternString Verb { get; }

        /// <summary>
        /// Optional item/tool used for the interaction.
        ///
        /// Examples:
        /// lockpick, key, repair kit, weapon, consumable, quest item.
        /// </summary>
        public EntityId ToolEntityId { get; }

        /// <summary>
        /// Optional world-space actor position at the time of interaction.
        /// </summary>
        public Vector2 ActorPosition { get; }

        /// <summary>
        /// Optional world-space target position at the time of interaction.
        /// </summary>
        public Vector2 TargetPosition { get; }

        /// <summary>
        /// Actor-facing or aim direction.
        /// Useful for directional interactions, cones, line checks, doors, and facing gates.
        /// </summary>
        public Vector2 Direction { get; }

        /// <summary>
        /// Distance between actor and target at request time.
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// Maximum range allowed by the caller/input system.
        /// The target can still enforce stricter rules.
        /// </summary>
        public float MaxDistance { get; }

        /// <summary>
        /// Simulation/network tick for deterministic or replicated interactions.
        /// </summary>
        public uint Tick { get; }

        /// <summary>
        /// Optional caller-local sequence number.
        /// Useful for input/network request ordering.
        /// </summary>
        public ushort Sequence { get; }

        /// <summary>
        /// Additional interaction modifiers.
        /// </summary>
        public EntityInteractionFlags Flags { get; }

        public bool HasActor => ActorId.IsValid;
        public bool HasTarget => TargetId.IsValid;
        public bool HasTool => ToolEntityId.IsValid;
        public bool HasVerb => !string.IsNullOrEmpty(Verb);
        public bool IsPreviewOnly => Flags.HasFlag(EntityInteractionFlags.PreviewOnly);
        public bool IsForced => Flags.HasFlag(EntityInteractionFlags.Forced);
        public bool ShouldCheckRange => !Flags.HasFlag(EntityInteractionFlags.IgnoreRange);
        public bool ShouldCheckLineOfSight => !Flags.HasFlag(EntityInteractionFlags.IgnoreLineOfSight);
        public bool IsInRange => !ShouldCheckRange || Distance <= MaxDistance;

        public EntityInteractionContext(
            EntityId actorId,
            EntityId targetId,
            EntityInteractionKind kind,
            InternString verb,
            EntityId toolEntityId,
            Vector2 actorPosition,
            Vector2 targetPosition,
            Vector2 direction,
            float distance,
            float maxDistance,
            uint tick,
            ushort sequence,
            EntityInteractionFlags flags
        )
        {
            ActorId = actorId;
            TargetId = targetId;
            Kind = kind;
            Verb = verb;
            ToolEntityId = toolEntityId;

            ActorPosition = actorPosition;
            TargetPosition = targetPosition;

            Direction = direction.LengthSquared() > 0.0001f
                ? Vector2.Normalize(direction)
                : Vector2.Zero;

            Distance = distance;
            MaxDistance = maxDistance;

            Tick = tick;
            Sequence = sequence;
            Flags = flags;
        }

        public static EntityInteractionContext Simple(EntityId actorId, EntityId targetId, EntityInteractionKind kind, uint tick = 0)
        {
            return new EntityInteractionContext(
                actorId,
                targetId,
                kind,
                verb: default,
                toolEntityId: EntityId.Invalid,
                actorPosition: Vector2.Zero,
                targetPosition: Vector2.Zero,
                direction: Vector2.Zero,
                distance: 0.0f,
                maxDistance: float.MaxValue,
                tick: tick,
                sequence: 0,
                flags: EntityInteractionFlags.None
            );
        }

        public static EntityInteractionContext WithPositions(
            EntityId actorId,
            EntityId targetId,
            EntityInteractionKind kind,
            Vector2 actorPosition,
            Vector2 targetPosition,
            float maxDistance,
            uint tick = 0,
            EntityInteractionFlags flags = EntityInteractionFlags.None
        )
        {
            Vector2 delta = targetPosition - actorPosition;
            float distance = delta.Length();

            return new EntityInteractionContext(
                actorId,
                targetId,
                kind,
                verb: default,
                toolEntityId: EntityId.Invalid,
                actorPosition: actorPosition,
                targetPosition: targetPosition,
                direction: distance > 0.0001f ? delta / distance : Vector2.Zero,
                distance: distance,
                maxDistance: maxDistance,
                tick: tick,
                sequence: 0,
                flags: flags
            );
        }

        public EntityInteractionContext WithVerb(InternString verb)
        {
            return new EntityInteractionContext(
                ActorId,
                TargetId,
                Kind,
                verb,
                ToolEntityId,
                ActorPosition,
                TargetPosition,
                Direction,
                Distance,
                MaxDistance,
                Tick,
                Sequence,
                Flags
            );
        }

        public EntityInteractionContext WithTool(EntityId toolEntityId)
        {
            return new EntityInteractionContext(
                ActorId,
                TargetId,
                Kind,
                Verb,
                toolEntityId,
                ActorPosition,
                TargetPosition,
                Direction,
                Distance,
                MaxDistance,
                Tick,
                Sequence,
                Flags
            );
        }

        public EntityInteractionContext WithFlags(EntityInteractionFlags flags)
        {
            return new EntityInteractionContext(
                ActorId,
                TargetId,
                Kind,
                Verb,
                ToolEntityId,
                ActorPosition,
                TargetPosition,
                Direction,
                Distance,
                MaxDistance,
                Tick,
                Sequence,
                flags
            );
        }

        public EntityInteractionContext AddFlags(EntityInteractionFlags flags)
        {
            return WithFlags(Flags | flags);
        }
    }
}
