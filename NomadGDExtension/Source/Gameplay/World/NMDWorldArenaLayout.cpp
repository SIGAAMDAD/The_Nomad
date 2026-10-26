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

#include "NMDWorldArenaLayout.h"
#include <cstring>

bool NMDWorldArenaLayout::build( const nmdWorldArenaSegmentSpec_t *pSpecs, uint32_t nSpecCount,
	uint32_t nAlignment, nmdWorldArenaSegmentInfo_t *pSegments,
	uint32_t nSegmentCount, uint64_t& nEndBytes )
{
	if ( pSpecs == NULL || pSegments == NULL || nAlignment < 64u ||
		( nAlignment & ( nAlignment - 1u ) ) != 0u ) {
		return false;
	}
	memset( pSegments, 0, static_cast<size_t>( nSegmentCount ) * sizeof( *pSegments ) );
	uint64_t total = 0u;
	for ( uint32_t i = 0u; i < nSpecCount; ++i ) {
		const nmdWorldArenaSegmentSpec_t& spec = pSpecs[i];
		if ( spec.eSegment >= nSegmentCount || spec.nStride == 0u ) {
			return false;
		}
		const uint64_t offset = ( total + nAlignment - 1u ) & ~uint64_t( nAlignment - 1u );
		const uint64_t end = offset + static_cast<uint64_t>( spec.nCapacity ) * spec.nStride;
		if ( end > UINT32_MAX ) {
			return false;
		}
		pSegments[spec.eSegment] = { static_cast<uint32_t>( offset ), spec.nStride,
			spec.nCapacity, 0u, 0u, 0u, static_cast<uint8_t>( spec.eAccess ), {} };
		total = end;
	}
	nEndBytes = total;
	return true;
}

uint64_t NMDWorldArenaLayout::getCommittedBytes( const nmdWorldArenaSegmentInfo_t& segment,
	uint32_t nCommittedCapacity, uint32_t nPageSize, bool bVirtualCommit )
{
	const uint64_t entries = bVirtualCommit ? nCommittedCapacity : segment.nCapacity;
	const uint64_t bytes = entries * segment.nElementBytes;
	return ( bytes + nPageSize - 1u ) & ~uint64_t( nPageSize - 1u );
}

uint32_t NMDWorldArenaLayout::getCommittedCapacity( nmdWorldArenaSegmentId_t id,
	const nmdWorldDatabaseCapacities_t& c, uint32_t reserved )
{
	if ( reserved == 0u ) {
		return 0u;
	}
	uint64_t count = 0u;
	switch ( id ) {
		case NMD_WORLD_ARENA_FACT_VALUES:
		case NMD_WORLD_ARENA_FACT_TYPES:
		case NMD_WORLD_ARENA_FACT_ATOMS: count = c.nFacts; break;
		case NMD_WORLD_ARENA_LOCATIONS_HOT:
		case NMD_WORLD_ARENA_LOCATION_OCCUPANCY:
		case NMD_WORLD_ARENA_LOCATION_COLD:
		case NMD_WORLD_ARENA_LOCATION_ATOMS: count = c.nLocations; break;
		case NMD_WORLD_ARENA_QUESTS_HOT:
		case NMD_WORLD_ARENA_QUEST_ATOMS:
		case NMD_WORLD_ARENA_QUEST_META:
		case NMD_WORLD_ARENA_QUEST_TREE_LINKS: count = c.nQuests; break;
		case NMD_WORLD_ARENA_OBJECTIVES_OVERFLOW: count = c.nOverflowObjectives; break;
		case NMD_WORLD_ARENA_STORIES_HOT:
		case NMD_WORLD_ARENA_STORY_ATOMS:
		case NMD_WORLD_ARENA_STORY_TREE_RANGES: count = c.nStories; break;
		case NMD_WORLD_ARENA_WEATHER_HOT:
		case NMD_WORLD_ARENA_WATER_HOT:
		case NMD_WORLD_ARENA_REGION_IDS:
		case NMD_WORLD_ARENA_CLIMATE_LINKS:
		case NMD_WORLD_ARENA_WEATHER_INDEX_RANGES:
		case NMD_WORLD_ARENA_CLIMATE_SEASONAL:
		case NMD_WORLD_ARENA_CLIMATE_GEOGRAPHY: count = c.nRegions; break;
		case NMD_WORLD_ARENA_REGION_LOOKUP: count = 0u; break;
		case NMD_WORLD_ARENA_WEATHER_INDEX_ENTRIES:
		case NMD_WORLD_ARENA_WEATHER_TIMELINE: count = c.nWeatherEvents; break;
		case NMD_WORLD_ARENA_LOOKUP_BUCKETS:
			count = NMD_WORLD_INDEX_INITIAL_BUCKETS;
			while ( count < ( static_cast<uint64_t>( c.nFacts ) + c.nLocations + c.nQuests + c.nStories + c.nRegions + c.nTimedEvents + c.nWeatherEvents ) * 2u ) {
				count <<= 1u;
			}
			break;
		case NMD_WORLD_ARENA_LOOKUP_ENTRIES:
			count = static_cast<uint64_t>( c.nFacts ) + c.nLocations + c.nQuests + c.nStories + c.nRegions + c.nTimedEvents + c.nWeatherEvents;
			break;
		case NMD_WORLD_ARENA_DEPENDENCIES:
		case NMD_WORLD_ARENA_DEPENDENCY_BUILD: count = c.nDependencies; break;
		case NMD_WORLD_ARENA_DEPENDENCY_BUCKETS:
			count = 8u;
			while ( count < static_cast<uint64_t>( c.nDependencies ) * 2u ) {
				count <<= 1u;
			}
			break;
		case NMD_WORLD_ARENA_DEPENDENCY_OFFSETS:
		case NMD_WORLD_ARENA_DEPENDENCY_CURSOR: count = static_cast<uint64_t>( c.nFacts ) + 1u; break;
		case NMD_WORLD_ARENA_LOCATION_CONDITIONS: count = c.nLocationConditions; break;
		case NMD_WORLD_ARENA_QUEST_CONDITIONS: count = c.nQuestConditions; break;
		case NMD_WORLD_ARENA_TIMED_EVENTS: count = c.nTimedEvents; break;
		case NMD_WORLD_ARENA_DIRTY_WORDS:
			// Two dirty channels occupy sparse subranges at reserved offsets.
			// They must remain fully writable until the bitset layout is redesigned.
			count = reserved; break;
		case NMD_WORLD_ARENA_CALENDAR_HOT:
		case NMD_WORLD_ARENA_CALENDAR_COLD:
		case NMD_WORLD_ARENA_COMPILED_REVISIONS:
		case NMD_WORLD_ARENA_DIRTY_REVISIONS: count = 1u; break;
		default: return 0u;
	}
	return count > reserved ? UINT32_MAX : static_cast<uint32_t>( count );
}
