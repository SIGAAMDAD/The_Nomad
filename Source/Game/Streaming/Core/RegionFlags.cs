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

namespace Nomad.Game.Streaming
{
	[Flags]
	internal enum RegionFlags : ulong
	{
		None                    = 0UL,

		// Target tier. Exactly one of these should be set after initialization.
		TargetUnloaded          = 1UL << 0,
		TargetCold              = 1UL << 1,
		TargetWarm              = 1UL << 2,
		TargetHot               = 1UL << 3,
		TargetActive            = 1UL << 4,

		// Actual resource state.
		HasManifest             = 1UL << 5,
		HasImposter             = 1UL << 6,
		HasProxy                = 1UL << 7,
		HasFullRes              = 1UL << 8,  // Companion PackedScene is cached/available.
		HasFullInstance         = 1UL << 9,  // Companion scene instance is in the tree.

		// Runtime enabled state.
		VisualImposter          = 1UL << 10,
		VisualProxy             = 1UL << 11,
		VisualFull              = 1UL << 12,

		PhysicsOff              = 1UL << 13,
		PhysicsFull             = 1UL << 14,

		LightingStaticOnly      = 1UL << 15,
		LightingNoShadows       = 1UL << 16,
		LightingFull            = 1UL << 17,

		GameplayActive          = 1UL << 18,

		// Locality/cache information.
		IsResident              = 1UL << 19,
		IsNear                  = 1UL << 20,
		IsPredicted             = 1UL << 21,
		IsBehindPlayer          = 1UL << 22,

		// Queue dedupe.
		QueuedPromotion         = 1UL << 23,
		QueuedDemotion          = 1UL << 24,

		// Pending operation groups.
		NeedsLoad               = 1UL << 25,
		NeedsInstantiate        = 1UL << 26,
		NeedsVisual             = 1UL << 27,
		NeedsLight              = 1UL << 28,
		NeedsPhysics            = 1UL << 29,
		NeedsGameplay           = 1UL << 30,
		NeedsDowngrade          = 1UL << 31,
		NeedsEvict              = 1UL << 32,

		// Extra async/resource state. Kept outside the original bit range.
		SceneLoadRequested      = 1UL << 33,
		SceneLoadFailed         = 1UL << 34,

		TargetMask =
			TargetUnloaded |
			TargetCold |
			TargetWarm |
			TargetHot |
			TargetActive,

		ActualMask =
			HasManifest |
			HasImposter |
			HasProxy |
			HasFullRes |
			HasFullInstance |
			VisualImposter |
			VisualProxy |
			VisualFull |
			PhysicsOff |
			PhysicsFull |
			LightingStaticOnly |
			LightingNoShadows |
			LightingFull |
			GameplayActive |
			IsResident |
			IsNear |
			IsPredicted |
			IsBehindPlayer |
			SceneLoadRequested |
			SceneLoadFailed,

		PendingPromotionMask =
			NeedsLoad |
			NeedsInstantiate |
			NeedsVisual |
			NeedsLight |
			NeedsPhysics |
			NeedsGameplay,

		PendingDemotionMask =
			NeedsDowngrade |
			NeedsEvict,

		PendingMask =
			PendingPromotionMask |
			PendingDemotionMask,

		QueueMask =
			QueuedPromotion |
			QueuedDemotion
	};
};
