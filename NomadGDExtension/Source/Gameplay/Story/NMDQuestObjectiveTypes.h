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

#ifndef __NMD_QUEST_OBJECTIVE_TYPES_H__
#define __NMD_QUEST_OBJECTIVE_TYPES_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldFactTypes.h"
#include <Framework/Core/Containers/NMDStrings.h>

/*
===========================================================================
Quest Objective Definitions
===========================================================================
*/

typedef enum nmdQuestObjectiveStatus_e : uint8_t
{
	/// @brief The current objective.
	NMD_QUEST_OBJECTIVE_STATUS_ACTIVE = 0,

	/// @brief Queued but not current objective.
	NMD_QUEST_OBJECTIVE_STATUS_INACTIVE,

	/// @brief The objective was failed.
	NMD_QUEST_OBJECTIVE_STATUS_FAILED,

	/// @brief A completed objective.
	NMD_QUEST_OBJECTIVE_STATUS_COMPLETED,

	NMD_QUEST_OBJECTIVE_STATUS_COUNT
} nmdQuestObjectiveStatus_t;

typedef enum nmdQuestObjectiveFlags_e : uint16_t
{
	NMD_QUEST_OBJECTIVE_FLAG_NONE = NOMAD_BIT( 0 ),
	NMD_QUEST_OBJECTIVE_FLAG_HAS_QUANITTY = NOMAD_BIT( 1 ),
	NMD_QUEST_OBJECTIVE_FLAG_HAS_QUANTITY = NMD_QUEST_OBJECTIVE_FLAG_HAS_QUANITTY,
	NMD_QUEST_OBJECTIVE_FLAG_IS_REQUIRED = NOMAD_BIT( 2 ),
	NMD_QUEST_OBJECTIVE_FLAG_CAN_FAIL = NOMAD_BIT( 3 ),
	NMD_QUEST_OBJECTIVE_FLAG_HIDDEN = NOMAD_BIT( 4 ),
	NMD_QUEST_OBJECTIVE_FLAG_FAILS_QUEST = NOMAD_BIT( 5 ),
	NMD_QUEST_OBJECTIVE_FLAG_HIDE_PROGRESS = NOMAD_BIT( 6 ),
	NMD_QUEST_OBJECTIVE_FLAG_HAS_WORLD_MARKER = NOMAD_BIT( 7 ),
	NMD_QUEST_OBJECTIVE_FLAG_HAS_TIME_LIMIT = NOMAD_BIT( 8 ),
	NMD_QUEST_OBJECTIVE_FLAT_BACKGROUND_PROGRESS = NOMAD_BIT( 9 ),
	NMD_QUEST_OBJECTIVE_FLAG_BACKGROUND_PROGRESS = NMD_QUEST_OBJECTIVE_FLAT_BACKGROUND_PROGRESS
} nmdQuestObjectiveFlags_t;

typedef enum nmdQuestObjectiveConditionComparison_s : uint8_t
{
	NMD_QUEST_OJBECTIVE_CONDITION_COMP_EQ = 0,
	NMD_QUEST_OJBECTIVE_CONDITION_COMP_NEQ,
	NMD_QUEST_OJBECTIVE_CONDITION_COMP_GT,
	NMD_QUEST_OJBECTIVE_CONDITION_COMP_GTE,
	NMD_QUEST_OJBECTIVE_CONDITION_COMP_LT,
	NMD_QUEST_OJBECTIVE_CONDITION_COMP_LTE
} nmdQuestObjectiveConditionComparison_t;

typedef enum nmdQuestObjectiveConditionStatus_e : uint8_t
{
	NMD_QUEST_OBJECTIVE_CONDITION_STATUS_NONE = 0,
	NMD_QUEST_OBJECTIVE_CONDITION_STATUS_INACTIVE,
	NMD_QUEST_OBJECTIVE_CONDITION_STATUS_ACTIVE,
	NMD_QUEST_OBJECTIVE_CONDITION_STATUS_FAILED,
	NMD_QUEST_OBJECTIVE_CONDITION_STATUS_SUCCEEDED
} nmdQuestObjectiveConditionStatus_t;

typedef struct nmdQuestObjectiveCondition_s
{
	nmdQuestObjectiveConditionComparison_t eComparison;
	nmdQuestObjectiveConditionStatus_t eStatus;
	nmdWorldFact_t requiredValue;
} nmdQuestObjectiveCondition_t;

typedef struct nmdQuestObjectiveData_s
{
	const NMDStringAtom *pId;
	const NMDStringAtom *pDisplayName;

	nmdQuestObjectiveFlags_t eFlags;
	nmdQuestObjectiveStatus_t eStatus;

	uint32_t nParentObjective;

	uint32_t nConditions;
	nmdQuestObjectiveCondition_t szConditions[0];
} nmdQuestObjectiveData_t;

#endif // __NMD_QUEST_OBJECTIVE_TYPES_H__
