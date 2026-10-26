/*
===========================================================================
The Nomad Framework
Copyright (C) 2025-2027 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

#ifndef __NMD_QUEST_TYPES_H__
#define __NMD_QUEST_TYPES_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDQuestObjectiveTypes.h"
#include <Framework/Events/NMDEvent.h>

/*
===========================================================================
Quest Definitions
===========================================================================
*/

typedef enum nmdQuestChildPolicy_e : uint8_t
{
	NMD_QUEST_CHILD_POLICY_ALL,
	NMD_QUEST_CHILD_POLICY_ANY,
	NMD_QUEST_CHILD_POLICY_AT_LEAST
} nmdQuestChildPolicy_t;

typedef enum nmdQuestFlags_e : uint32_t
{
	NMD_QUEST_FLAG_NONE = NOMAD_BIT( 0 ),

	NMD_QUEST_FLAG_DISCOVERED = NOMAD_BIT( 1 ),

	NMD_QUEST_FLAG_LOCKED = NOMAD_BIT( 2 ),

	NMD_QUEST_FLAG_SUSPENDED = NOMAD_BIT( 3 ),

	NMD_QUEST_FLAG_FAILED = NOMAD_BIT( 4 ),

	NMD_QUEST_FLAG_ABANDONED = NOMAD_BIT( 5 ),

	NMD_QUEST_FLAG_ACTIVE = NOMAD_BIT( 6 ),

	NMD_QUEST_FLAG_READY_TO_COMPLETE = NOMAD_BIT( 7 ),

	NMD_QUEST_FLAG_COMPLETED = NOMAD_BIT( 8 ),

	NMD_QUEST_FLAG_IS_HIDDEN = NOMAD_BIT( 9 ),

	NMD_QUEST_FLAG_AUTO_START = NOMAD_BIT( 10 ),

	NMD_QUEST_FLAG_REQUIRES_TURN_IN = NOMAD_BIT( 11 ),

	// Existing compiled content uses bit 11 for both flags; keep this ABI.
	NMD_QUEST_FLAG_HAS_PREREQUISITES = NOMAD_BIT( 11 ),

	NMD_QUEST_FLAG_IS_TIMED = NOMAD_BIT( 12 ),

	NMD_QUEST_FLAG_CAN_ABANDON = NOMAD_BIT( 13 ),

	NMD_QUEST_FLAG_CAN_FAIL = NOMAD_BIT( 14 ),

	NMD_QUEST_FLAG_CAN_RESTART = NOMAD_BIT( 15 ),

	NMD_QUEST_FLAG_RUN_ONCE = NOMAD_BIT( 16 ),

	NMD_QUEST_FLAG_START_ON_GAME_LOAD = NOMAD_BIT( 17 ),

	NMD_QUEST_FLAG_HAS_MULTIPLE_ENDINGS = NOMAD_BIT( 18 ),

	NMD_QUEST_FLAG_FAIL_ON_AREA_EXIT = NOMAD_BIT( 19 ),

	NMD_QUEST_FLAG_FAIL_ON_PLAYER_DEATH = NOMAD_BIT( 20 ),

	NMD_QUEST_FLAG_FAIL_ON_NPC_DEATH = NOMAD_BIT( 21 ),

	NMD_QUEST_FLAG_SUSPENDABLE = NOMAD_BIT( 22 ),

	NMD_QUEST_HAS_COOLDOWN = NOMAD_BIT( 23 ),

	NMD_QUEST_IS_EXCLUSIVE = NOMAD_BIT( 24 )
} nmdQuestFlags_t;

//
// Quest Data - 24 bytes
//
// Objective indices reference the objective pool.
// Parent/child/sibling indices are storyline-local.
// Existing quest flags retain their complete 32-bit width.
//

typedef struct nmdQuestData_s
{
	const uint32_t nId;
	uint32_t eFlags;

	const uint32_t nFirstObjective;

	const uint16_t nObjectiveCount;
	uint16_t nCurrentObjective;

	const uint16_t nParentQuest;
	const uint16_t nFirstChildQuest;
	const uint16_t nNextSiblingQuest;

	const uint8_t nChildPolicy;
	const uint8_t nRequiredChildren;
} nmdQuestData_t;

typedef struct nmdQuestFlagsChangedEvent_s
{
	const uint32_t nQuestId;
	const nmdQuestFlags_t ePrevFlags;
	const nmdQuestFlags_t eCurrentFlags;
} nmdQuestFlagsChangedEvent_t;

typedef NMDEvent<nmdQuestFlagsChangedEvent_t> *NMDQuestFlagsChangedEvent;

static_assert( sizeof( nmdQuestData_t ) == 24,
	"Quest data exceeds 24 bytes" );

#endif // __NMD_QUEST_TYPES_H__
