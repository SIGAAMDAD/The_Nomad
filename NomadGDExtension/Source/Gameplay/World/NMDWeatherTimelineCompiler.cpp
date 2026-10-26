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

#include "NMDWeatherTimelineCompiler.h"

namespace
{
	static bool less( const nmdWeatherTimelineIndexEntry_t& a,
		const nmdWeatherTimelineIndexEntry_t& b )
	{
		if ( a.nDenseRegionIndex != b.nDenseRegionIndex ) {
			return a.nDenseRegionIndex < b.nDenseRegionIndex;
		}
		if ( a.nStartWorldSecond != b.nStartWorldSecond ) {
			return a.nStartWorldSecond < b.nStartWorldSecond;
		}
		return a.nEventIndex < b.nEventIndex;
	}
	static void sift( nmdWeatherTimelineIndexEntry_t *entries, uint32_t first, uint32_t count )
	{
		uint32_t root = first;
		while ( root < count / 2u ) {
			const uint32_t child = root * 2u + 1u;
			if ( child >= count ) {
				break;
			}
			uint32_t largest = child;
			if ( child + 1u < count && less( entries[child], entries[child + 1u] ) ) {
				largest = child + 1u;
			}
			if ( !less( entries[root], entries[largest] ) ) {
				break;
			}
			const nmdWeatherTimelineIndexEntry_t tmp = entries[root];
			entries[root] = entries[largest];
			entries[largest] = tmp;
			root = largest;
		}
	}
	static void sort( nmdWeatherTimelineIndexEntry_t *entries, uint32_t count )
	{
		if ( count < 2u ) {
			return;
		}
		for ( uint32_t i = count / 2u; i-- > 0u; ) {
			sift( entries, i, count );
		}
		for ( uint32_t end = count; end > 1u; --end ) {
			const nmdWeatherTimelineIndexEntry_t tmp = entries[0];
			entries[0] = entries[end - 1u];
			entries[end - 1u] = tmp;
			sift( entries, 0u, end - 1u );
		}
	}
}

bool NMDWeatherTimelineCompiler::build( const NMDWorldDatabase& db,
	const nmdWeatherTimelineEvent_t *pEvents, uint32_t nEventCount,
	uint32_t nRegionCount, nmdWeatherTimelineIndexEntry_t *pEntries,
	nmdWeatherTimelineRange_t *pRanges )
{
	if ( nRegionCount && pRanges == NULL ) {
		return false;
	}
	if ( nEventCount && ( pEvents == NULL || pEntries == NULL ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < nEventCount; ++i ) {
		uint32_t region = 0u;
		if ( !db.getClimateIndex( pEvents[i].nRegionId, region ) ||
			region >= nRegionCount ||
			pEvents[i].nStartWorldSecond > UINT64_MAX - pEvents[i].nDurationSeconds ) {
			return false;
		}
	}
	for ( uint32_t i = 0u; i < nEventCount; ++i ) {
		uint32_t region = 0u;
		db.getClimateIndex( pEvents[i].nRegionId, region );
		pEntries[i] = { pEvents[i].nStartWorldSecond, 0u, i, static_cast<uint16_t>( region ), 0u };
	}
	sort( pEntries, nEventCount );
	uint32_t pos = 0u;
	for ( uint32_t region = 0u; region < nRegionCount; ++region ) {
		nmdWeatherTimelineRange_t& range = pRanges[region];
		range.nFirst = pos;
		uint64_t maxEnd = 0u;
		while ( pos < nEventCount && pEntries[pos].nDenseRegionIndex == region ) {
			nmdWeatherTimelineIndexEntry_t& entry = pEntries[pos];
			const nmdWeatherTimelineEvent_t& event = pEvents[entry.nEventIndex];
			const uint64_t end = event.nStartWorldSecond + event.nDurationSeconds;
			if ( end > maxEnd ) {
				maxEnd = end;
			}
			entry.nPrefixMaxEndSecond = maxEnd;
			++pos;
		}
		range.nCount = pos - range.nFirst;
	}
	return true;
}

bool NMDWeatherTimelineCompiler::rebuild( NMDWorldDatabase& db )
{
	const nmdWeatherTimelineEvent_t *events = NULL;
	nmdWeatherTimelineIndexEntry_t *entries = NULL;
	nmdWeatherTimelineRange_t *ranges = NULL;
	uint32_t eventCapacity = 0u, regionCapacity = 0u;
	if ( !db.getWeatherCompileStorage( events, entries, ranges, eventCapacity, regionCapacity ) ) {
		return false;
	}
	const uint32_t eventCount = db.getWeatherTimelineEventCount();
	const uint32_t regionCount = db.getClimateRegionCount();
	if ( eventCount > eventCapacity || regionCount > regionCapacity ||
		!build( db, events, eventCount, regionCount, entries, ranges ) ) {
		return false;
	}
	return db.setCompiledWeatherIndexCounts( eventCount, regionCount );
}
