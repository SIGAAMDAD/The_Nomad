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

#include "NMDQuestContentCompiler.h"
#include "../World/NMDWorldContentCompiler.h"

bool NMDQuestContentCompiler::validate( const NMDWorldDatabase& db,
										uint16_t current, const nmdWorldObjectiveDesc_t *objectives, uint32_t count )
{
	if ( count > UINT8_MAX || ( count && objectives == NULL ) ||
		 ( current != UINT16_MAX && current >= count ) ) {
		return false;
	}
	uint32_t totalConditions = 0u;
	for ( uint32_t i = 0u; i < count; ++i ) {
		const nmdWorldObjectiveDesc_t& desc = objectives[ i ];
		if ( desc.eStatus >= NMD_QUEST_OBJECTIVE_STATUS_COUNT ||
			 ( desc.nFlags & ~NMD_WORLD_OBJECTIVE_FLAGS_MASK ) ||
			 ( desc.nConditionCount && desc.pConditions == NULL ) ) {
			return false;
		}
		uint32_t parent = desc.nParentObjective;
		for ( uint32_t depth = 0u; parent != NMD_WORLD_OBJECTIVE_NO_PARENT; ++depth ) {
			if ( parent >= count || parent == i || depth >= count ) {
				return false;
			}
			parent = objectives[ parent ].nParentObjective;
		}
		totalConditions += desc.nConditionCount;
		if ( totalConditions > UINT16_MAX ) {
			return false;
		}
		for ( uint32_t j = 0u; j < desc.nConditionCount; ++j ) {
			if ( !NMDWorldContentCompiler::validateCondition( db, desc.pConditions[ j ] ) ) {
				return false;
			}
		}
	}
	return true;
}

nmdWorldHandle_t NMDQuestContentCompiler::registerQuest( NMDWorldDatabase& db, nmdWorldAtom_t atom,
														 uint32_t flags, uint16_t current, const nmdWorldObjectiveDesc_t *objectives, uint32_t count )
{
	const nmdWorldHandle_t invalid = { NMD_WORLD_HANDLE_INVALID };
	if ( !validate( db, current, objectives, count ) ) {
		return invalid;
	}
	const nmdWorldHandle_t existing = db.getRecordHandle( atom, NMD_WORLD_RECORD_QUEST );
	if ( existing.isValid() ) {
		return existing;
	}
	uint32_t needed = 0u;
	for ( uint32_t i = 0u; i < count; ++i ) {
		for ( uint32_t j = 0u; j < objectives[ i ].nConditionCount; ++j ) {
			bool duplicate = false;
			for ( uint32_t k = 0u; k < j; ++k ) {
				if ( objectives[ i ].pConditions[ k ].nFactId == objectives[ i ].pConditions[ j ].nFactId ) {
					duplicate = true;
					break;
				}
			}
			if ( !duplicate ) {
				++needed;
			}
		}
	}
	nmdWorldArenaSegmentInfo_t segment = {};
	if ( !db.getArenaSegmentInfo( NMD_WORLD_ARENA_DEPENDENCY_BUILD, segment ) ||
		 static_cast<uint64_t>( segment.nCount ) + needed > segment.nCommittedCapacity ) {
		return invalid;
	}
	const nmdWorldHandle_t quest = db.setQuestDefinition( atom, flags, current, objectives, count );
	if ( !quest.isValid() ) {
		return quest;
	}
	for ( uint32_t i = 0u; i < count; ++i ) {
		for ( uint32_t j = 0u; j < objectives[ i ].nConditionCount; ++j ) {
			bool duplicate = false;
			for ( uint32_t k = 0u; k < j; ++k ) {
				if ( objectives[ i ].pConditions[ k ].nFactId == objectives[ i ].pConditions[ j ].nFactId ) {
					duplicate = true;
					break;
				}
			}
			if ( !duplicate ) {
				db.setFactDependency( objectives[ i ].pConditions[ j ].nFactId, quest,
									  static_cast<uint16_t>( i ) );
			}
		}
	}
	return quest;
}

// Validate a single rooted quest tree before publishing any cold topology records.
bool NMDQuestContentCompiler::compileTree( NMDWorldDatabase& db, nmdWorldHandle_t hStory,
										   const nmdQuestData_t *pQuests, uint16_t nCount, uint32_t nFirst, uint16_t nRoot )
{
	if ( nCount == 0u || pQuests == NULL || nRoot >= nCount ||
		 static_cast<uint64_t>( nFirst ) + nCount > db.getQuestCount() ||
		 db.getArenaMode() != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	nmdWorldStoryTreeRange_t existing = {};
	if ( !db.getStoryTreeRange( hStory, existing ) ) {
		return false;
	}
	// A quest range belongs to one storyline. Overlaps make runtime parent
	// propagation ambiguous and would silently choose the first owner.
	const uint64_t nEnd = static_cast<uint64_t>( nFirst ) + nCount;
	for ( uint32_t i = 0u; i < db.getStoryCount(); ++i ) {
		if ( i == hStory.getIndex() ) {
			continue;
		}
		const nmdWorldHandle_t hOther = {
			( static_cast<uint32_t>( NMD_WORLD_RECORD_STORY ) << NMD_WORLD_HANDLE_TYPE_SHIFT ) | i
		};
		nmdWorldStoryTreeRange_t other = {};
		if ( !db.getStoryTreeRange( hOther, other ) ) {
			return false;
		}
		if ( other.nQuestCount != 0u &&
			static_cast<uint64_t>( other.nFirstQuest ) < nEnd &&
			static_cast<uint64_t>( nFirst ) <
				static_cast<uint64_t>( other.nFirstQuest ) + other.nQuestCount ) {
			return false;
		}
	}

	uint32_t edges = 0u;
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		const nmdQuestData_t& q = pQuests[ i ];
		if ( q.nChildPolicy > NMD_QUEST_CHILD_POLICY_AT_LEAST ||
			 ( q.nParentQuest != UINT16_MAX && q.nParentQuest >= nCount ) ||
			 ( q.nFirstChildQuest != UINT16_MAX && q.nFirstChildQuest >= nCount ) ||
			 ( q.nNextSiblingQuest != UINT16_MAX && q.nNextSiblingQuest >= nCount ) ||
			 ( i == nRoot && ( q.nParentQuest != UINT16_MAX || q.nNextSiblingQuest != UINT16_MAX ) ) ||
			 ( i != nRoot && q.nParentQuest == UINT16_MAX ) ) {
			return false;
		}
		uint32_t children = 0u;
		uint16_t child = q.nFirstChildQuest;
		while ( child != UINT16_MAX ) {
			if ( child >= nCount || ++children >= nCount ||
				pQuests[ child ].nParentQuest != i ) {
				return false;
			}
			child = pQuests[ child ].nNextSiblingQuest;
		}
		if ( q.nChildPolicy == NMD_QUEST_CHILD_POLICY_AT_LEAST &&
			q.nRequiredChildren > children ) {
			return false;
		}
		uint32_t depth = 0u;
		uint16_t parent = q.nParentQuest;
		while ( parent != UINT16_MAX ) {
			if ( parent >= nCount || parent == i || ++depth >= nCount ) {
				return false;
			}
			parent = pQuests[ parent ].nParentQuest;
		}
		if ( i != nRoot ) {
			uint32_t occurrences = 0u;
			uint16_t child = pQuests[ q.nParentQuest ].nFirstChildQuest;
			uint32_t scanned = 0u;
			while ( child != UINT16_MAX ) {
				if ( child >= nCount || ++scanned > nCount ) {
					return false;
				}
				if ( child == i ) {
					++occurrences;
				}
				child = pQuests[ child ].nNextSiblingQuest;
			}
			if ( occurrences != 1u ) {
				return false;
			}
			++edges;
		}
	}
	if ( edges != nCount - 1u ) {
		return false;
	}
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		const nmdQuestData_t& q = pQuests[ i ];
		const nmdWorldQuestTreeLink_t link = { q.nParentQuest, q.nFirstChildQuest,
											   q.nNextSiblingQuest, q.nChildPolicy, q.nRequiredChildren };
		if ( !db.setQuestTreeLink( nFirst + i, link ) ) {
			return false;
		}
	}
	return db.setStoryTreeRange( hStory, { nFirst, nCount, nRoot } );
}
