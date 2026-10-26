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

#ifndef __NMD_WORLD_QUEST_LAYOUT_CANDIDATE_H__
#define __NMD_WORLD_QUEST_LAYOUT_CANDIDATE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"
#include <cstring>

// C04 compile-only layout candidate. The shipping 64-byte quest record and
// its borrowed-pointer API remain unchanged until workloads justify migration.
typedef struct nmdWorldQuestHeaderCandidate_s
{
	uint32_t nFlags;
	uint32_t nFirstObjective;
	uint16_t nCurrentObjective;
	uint8_t nObjectiveCount;
	uint8_t nReserved;
	uint32_t nReserved2;
} nmdWorldQuestHeaderCandidate_t;

static_assert( sizeof( nmdWorldQuestHeaderCandidate_t ) == 16u,
	"Candidate quest header must be 16 bytes" );
static_assert( sizeof( nmdWorldObjectiveHot_t ) == 8u,
	"The candidate must preserve 8-byte objective records" );

// Storage-only comparison; does not predict traversal speed or conversion cost.
inline uint64_t nmdWorldQuestCurrentPayloadBytes( uint32_t nQuestCount,
	uint32_t nOverflowObjectiveCount )
{
	return static_cast<uint64_t>( nQuestCount ) * sizeof( nmdWorldQuestHot_t ) +
		static_cast<uint64_t>( nOverflowObjectiveCount ) * sizeof( nmdWorldObjectiveHot_t );
}

inline uint64_t nmdWorldQuestCandidatePayloadBytes( uint32_t nQuestCount,
	uint32_t nTotalObjectiveCount )
{
	return static_cast<uint64_t>( nQuestCount ) * sizeof( nmdWorldQuestHeaderCandidate_t ) +
		static_cast<uint64_t>( nTotalObjectiveCount ) * sizeof( nmdWorldObjectiveHot_t );
}


// Materialize the alternative layout in caller-owned scratch, preserving the
// original quest and objective ABI in the authoritative database.
inline bool nmdWorldBuildQuestCandidate( const nmdWorldQuestHot_t& quest,
	const nmdWorldObjectiveHot_t *pOverflow, uint32_t nOverflowCount,
	uint32_t nFirstObjective, nmdWorldQuestHeaderCandidate_t& header,
	nmdWorldObjectiveHot_t *pObjectives, uint32_t nObjectiveCapacity )
{
	const uint32_t total = quest.nObjectiveCount;
	const uint32_t inlineCount = total < NMD_WORLD_QUEST_INLINE_COUNT ?
		total : NMD_WORLD_QUEST_INLINE_COUNT;
	const uint32_t overflow = total - inlineCount;
	if ( nObjectiveCapacity < total || ( total != 0u && pObjectives == NULL ) ||
		nOverflowCount < overflow || ( overflow != 0u && pOverflow == NULL ) ) {
		return false;
	}
	if ( inlineCount != 0u ) {
		memcpy( pObjectives, quest.szObjectives, inlineCount * sizeof( nmdWorldObjectiveHot_t ) );
	}
	if ( overflow != 0u ) {
		memcpy( pObjectives + inlineCount, pOverflow, overflow * sizeof( nmdWorldObjectiveHot_t ) );
	}
	header = { quest.nFlags, nFirstObjective, quest.nCurrentObjective,
		quest.nObjectiveCount, 0u, 0u };
	return true;
}

#endif // __NMD_WORLD_QUEST_LAYOUT_CANDIDATE_H__
