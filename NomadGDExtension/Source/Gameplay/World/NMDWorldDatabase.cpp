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

#include "NMDWorldDatabase.h"
#include "NMDWorldArenaLayout.h"
#include <cmath>
#include <cstring>
#include <limits>

nmdWorldValue_t nmdWorldValue_t::fromFloat( float value )
{
	nmdWorldValue_t result;
	memcpy( &result.nBits, &value, sizeof( value ) );
	return result;
}

nmdWorldValue_t nmdWorldValue_t::fromInteger( int32_t value )
{
	nmdWorldValue_t result;
	memcpy( &result.nBits, &value, sizeof( value ) );
	return result;
}

nmdWorldValue_t nmdWorldValue_t::fromBoolean( bool value )
{
	return { value ? 1u : 0u };
}

float nmdWorldValue_t::asFloat( void ) const
{
	float result;
	memcpy( &result, &nBits, sizeof( result ) );
	return result;
}

int32_t nmdWorldValue_t::asInteger( void ) const
{
	int32_t result;
	memcpy( &result, &nBits, sizeof( result ) );
	return result;
}

bool nmdWorldValue_t::asBoolean( void ) const
{
	return nBits != 0u;
}

namespace
{


	static bool sameLocation( const nmdLocationData_t& a, const nmdLocationData_t& b )
	{
		return a.nId == b.nId && a.nStateAndTraits == b.nStateAndTraits &&
			a.nPlayerRenownScore == b.nPlayerRenownScore && a.nPlayerFearScore == b.nPlayerFearScore;
	}

	static bool sameObjective( const nmdWorldObjectiveHot_t& a, const nmdWorldObjectiveHot_t& b )
	{
		return a.nFirstCondition == b.nFirstCondition && a.nFlagsAndStatus == b.nFlagsAndStatus &&
			a.nProgress == b.nProgress && a.nConditionCount == b.nConditionCount &&
			a.nParentObjective == b.nParentObjective;
	}

	static bool sameQuest( const nmdWorldQuestHot_t& a, const nmdWorldQuestHot_t& b )
	{
		if ( a.nFlags != b.nFlags || a.nCurrentObjective != b.nCurrentObjective ||
			a.nObjectiveCount != b.nObjectiveCount ) {
			return false;
		}
		for ( uint32_t i = 0u; i < a.nObjectiveCount && i < NMD_WORLD_QUEST_INLINE_COUNT; ++i ) {
			if ( !sameObjective( a.szObjectives[i], b.szObjectives[i] ) ) {
				return false;
			}
		}
		return true;
	}

	static bool sameStory( const nmdWorldStoryState_t& a, const nmdWorldStoryState_t& b )
	{
		return a.nFlags == b.nFlags && a.nBranchId == b.nBranchId &&
			a.nActiveNode == b.nActiveNode && a.nCompletedNodes == b.nCompletedNodes &&
			a.nRevision == b.nRevision;
	}

	static bool sameCalendar( const nmdCalendarHot_t& a, const nmdCalendarHot_t& b )
	{
		return a.nWorldSeconds == b.nWorldSeconds && a.fFractionalSeconds == b.fFractionalSeconds &&
			a.nWeatherRemainderSeconds == b.nWeatherRemainderSeconds && a.nRevision == b.nRevision;
	}

	static bool sameWeather( const nmdWeatherHot_t& a, const nmdWeatherHot_t& b )
	{
#define NMD_SAME_WEATHER( Field ) ( a.Field == b.Field )
		return NMD_SAME_WEATHER( fTemperatureC ) && NMD_SAME_WEATHER( fRelativeHumidity ) &&
			NMD_SAME_WEATHER( fCloudCover ) && NMD_SAME_WEATHER( fWindSpeedMps ) &&
			NMD_SAME_WEATHER( fWindAngleRadians ) && NMD_SAME_WEATHER( fPrecipitationMmPerHour ) &&
			NMD_SAME_WEATHER( fPressureHpa ) && NMD_SAME_WEATHER( fVisibilityMeters ) &&
			NMD_SAME_WEATHER( fTargetTemperatureC ) && NMD_SAME_WEATHER( fTargetHumidity ) &&
			NMD_SAME_WEATHER( fTargetWindSpeedMps ) && NMD_SAME_WEATHER( fFrontIntensity ) &&
			NMD_SAME_WEATHER( nRandomState ) && NMD_SAME_WEATHER( nFrontRemainingSeconds ) &&
			NMD_SAME_WEATHER( eWeather ) && NMD_SAME_WEATHER( eTargetWeather );
#undef NMD_SAME_WEATHER
	}

	static bool sameWater( const nmdWaterCycleHot_t& a, const nmdWaterCycleHot_t& b )
	{
#define NMD_SAME_WATER( Field ) ( a.Field == b.Field )
		return NMD_SAME_WATER( fAtmosphericVaporMm ) && NMD_SAME_WATER( fCloudWaterMm ) &&
			NMD_SAME_WATER( fSoilWaterMm ) && NMD_SAME_WATER( fGroundwaterMm ) &&
			NMD_SAME_WATER( fSurfaceWaterMm ) && NMD_SAME_WATER( fSnowpackMm ) &&
			NMD_SAME_WATER( fLastPrecipitationMm ) && NMD_SAME_WATER( fLastEvaporationMm ) &&
			NMD_SAME_WATER( fLastRunoffMm ) && NMD_SAME_WATER( fLastInfiltrationMm ) &&
			NMD_SAME_WATER( nRevision ) && NMD_SAME_WATER( eHydrology );
#undef NMD_SAME_WATER
	}

	static bool sameCalendarDefinition( const nmdCalendarDefinition_t& a, const nmdCalendarDefinition_t& b )
	{
		if ( a.nMonths != b.nMonths || a.nWeatherStepSeconds != b.nWeatherStepSeconds ||
			a.fWorldSecondsPerRealSecond != b.fWorldSecondsPerRealSecond ||
			a.fAxialTiltDegrees != b.fAxialTiltDegrees || a.fMoonCycleDays != b.fMoonCycleDays ) {
			return false;
		}
		for ( uint32_t i = 0u; i < NMD_CALENDAR_MONTH_COUNT; ++i ) {
			if ( a.nMonthLengths[i] != b.nMonthLengths[i] ) {
				return false;
			}
		}
		for ( uint32_t i = 0u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
			if ( a.nSeasonStartDays[i] != b.nSeasonStartDays[i] ) {
				return false;
			}
		}
		return true;
	}

	static bool sameSeasonal( const nmdClimateSeasonalCold_t& a, const nmdClimateSeasonalCold_t& b )
	{
		for ( uint32_t i = 0u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
			if ( a.fTemperatureC[i] != b.fTemperatureC[i] ||
				a.fRelativeHumidity[i] != b.fRelativeHumidity[i] ||
				a.fStormChancePerDay[i] != b.fStormChancePerDay[i] ) {
				return false;
			}
		}
		return true;
	}

	static bool sameGeography( const nmdClimateGeographyCold_t& a, const nmdClimateGeographyCold_t& b )
	{
#define NMD_SAME_GEO( Field ) ( a.Field == b.Field )
		return NMD_SAME_GEO( fAreaKm2 ) && NMD_SAME_GEO( fLatitudeDegrees ) &&
			NMD_SAME_GEO( fElevationMeters ) && NMD_SAME_GEO( fSoilCapacityMm ) &&
			NMD_SAME_GEO( fGroundwaterCapacityMm ) && NMD_SAME_GEO( fSurfaceCapacityMm ) &&
			NMD_SAME_GEO( fEvaporationMultiplier ) && NMD_SAME_GEO( fAtmosphericRechargeMmPerHour ) &&
			NMD_SAME_GEO( fPrevailingWindMps ) && NMD_SAME_GEO( fMeanPressureHpa ) &&
			NMD_SAME_GEO( nDownwindRegionId ) && NMD_SAME_GEO( nDrainRegionId ) &&
			NMD_SAME_GEO( eBiome );
#undef NMD_SAME_GEO
	}


}

void NMDWorldDatabase::rebuildDependencyBuckets( void )
{
	m_DependencyBuckets.clear();
	m_DependencyBuckets.resize( m_DependencyBuckets.committedCapacity(), 0u );
	// Dependency values are indices into the authoring-only build array.
	if ( m_DependencyBuckets.empty() ) {
		return;
	}
	for ( uint32_t i = 0u; i < m_DependencyBuild.size(); ++i ) {
		const nmdWorldDependencyBuild_t& entry = m_DependencyBuild[i];
		const nmdWorldIdKey_t key = NMDWorldIdIndex::getDependencyKey(
			entry.nFactId, entry.hOwner.nValue, entry.nItemIndex );
		uint32_t bucket = 0u;
		bool found = false;
		const bool valid = NMDWorldIdIndex::probe( m_DependencyBuckets.data(),
			m_DependencyBuckets.size(), key,
			[]( uint32_t, const nmdWorldIdKey_t& ) { return false; }, bucket, found );
		assert( valid && !found );
		if ( !valid || found ) {
			return;
		}
		m_DependencyBuckets[bucket] = i + 1u;
	}
}

NMDWorldDatabase::NMDWorldDatabase( void ) :
	m_ArenaBlock( {} ), m_DesiredCommitCapacities( {} ), m_bExplicitCommitBudget( false ),
	m_nDependencyContentCapacity( 0u ), m_nArenaGeneration( 0u ),
	m_nFactDefinitionGeneration( 1u ), m_eArenaMode( NMD_WORLD_ARENA_AUTHORING ),
	m_bDependencyIndexDirty( false ), m_bHasCalendar( false )
{
	memset( m_ArenaSegments, 0, sizeof( m_ArenaSegments ) );
	memset( m_DirtyRanges, 0, sizeof( m_DirtyRanges ) );
}

NMDWorldDatabase::~NMDWorldDatabase( void )
{
	NMDWorldArenaBackend::release( m_ArenaBlock );
}

nmdWorldHandle_t NMDWorldDatabase::makeHandle( nmdWorldRecordType_t eType, uint32_t nIndex )
{
	return { ( static_cast<uint32_t>( eType ) << NMD_WORLD_HANDLE_TYPE_SHIFT ) |
		( nIndex & NMD_WORLD_HANDLE_INDEX_MASK ) };
}


void NMDWorldDatabase::rehashLookup( void )
{
	m_LookupBuckets.clear();
	m_LookupBuckets.resize( m_LookupBuckets.committedCapacity(), 0u );
	if ( !m_LookupBuckets.empty() ) {
		NMDWorldIdIndex::rebuild( m_LookupBuckets.data(), m_LookupBuckets.size(),
			m_LookupEntries.data(), m_LookupEntries.size() );
	}
}

bool NMDWorldDatabase::ensureLookupCapacity( uint32_t nEntries )
{
	return nEntries <= m_LookupEntries.committedCapacity() && !m_LookupBuckets.empty() &&
		static_cast<uint64_t>( nEntries ) * 4u <
		static_cast<uint64_t>( m_LookupBuckets.size() ) * 3u;
}

bool NMDWorldDatabase::getIdentityKey( nmdWorldHandle_t h, nmdWorldIdKey_t& key ) const
{
	const uint32_t index = h.getIndex();
	switch ( h.getType() ) {
		case NMD_WORLD_RECORD_FACT:
			if ( index >= m_FactAtoms.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getAtomKey( m_FactAtoms[index], h.getType() ); return true;
		case NMD_WORLD_RECORD_LOCATION:
			if ( index >= m_LocationAtoms.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getAtomKey( m_LocationAtoms[index], h.getType() ); return true;
		case NMD_WORLD_RECORD_QUEST:
			if ( index >= m_QuestAtoms.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getAtomKey( m_QuestAtoms[index], h.getType() ); return true;
		case NMD_WORLD_RECORD_STORY:
			if ( index >= m_StoryAtoms.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getAtomKey( m_StoryAtoms[index], h.getType() ); return true;
		case NMD_WORLD_RECORD_REGION:
			if ( index >= m_RegionIds.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getNumericKey( m_RegionIds[index], h.getType() ); return true;
		case NMD_WORLD_RECORD_TIMED_EVENT:
			if ( index >= m_TimedEvents.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getNumericKey( m_TimedEvents[index].nEventId, h.getType(), true ); return true;
		case NMD_WORLD_RECORD_WEATHER_EVENT:
			if ( index >= m_WeatherTimelineEvents.size() ) {
				return false;
			}
			key = NMDWorldIdIndex::getNumericKey( m_WeatherTimelineEvents[index].nEventId, h.getType(), true ); return true;
		default: return false;
	}
}

bool NMDWorldDatabase::findIdentity( const nmdWorldIdKey_t& key, nmdWorldHandle_t& handle ) const
{
	handle = { NMD_WORLD_HANDLE_INVALID };
	uint32_t bucket = 0u;
	bool found = false;
	const auto resolve = [this]( const nmdWorldIndexEntry_t& entry ) -> nmdWorldIdKey_t {
		nmdWorldIdKey_t foundKey = {};
		getIdentityKey( { entry.nValue }, foundKey );
		return foundKey;
	};
	if ( !NMDWorldIdIndex::findSlot( m_LookupBuckets.data(), m_LookupBuckets.size(),
		m_LookupEntries.data(), m_LookupEntries.size(), key, resolve, bucket, found ) || !found ) {
		return false;
	}
	handle = { m_LookupEntries[ m_LookupBuckets[bucket] - 1u ].nValue };
	return true;
}

bool NMDWorldDatabase::insertRecord( const nmdWorldIdKey_t& key, nmdWorldHandle_t hRecord )
{
	if ( !ensureLookupCapacity( m_LookupEntries.size() + 1u ) ) {
		return false;
	}
	uint32_t bucket = 0u;
	bool found = false;
	const auto resolve = [this]( const nmdWorldIndexEntry_t& entry ) -> nmdWorldIdKey_t {
		nmdWorldIdKey_t foundKey = {};
		getIdentityKey( { entry.nValue }, foundKey );
		return foundKey;
	};
	if ( !NMDWorldIdIndex::findSlot( m_LookupBuckets.data(), m_LookupBuckets.size(),
		m_LookupEntries.data(), m_LookupEntries.size(), key, resolve, bucket, found ) || found ) {
		return false;
	}
	if ( !m_LookupEntries.push_back( { NMDWorldIdIndex::hash( key ), hRecord.nValue } ) ) {
		return false;
	}
	m_LookupBuckets[bucket] = m_LookupEntries.size();
	return true;
}

bool NMDWorldDatabase::getArenaCapacitiesValid( const nmdWorldDatabaseCapacities_t& c ) const
{
	const uint64_t nRecords = static_cast<uint64_t>( c.nFacts ) + c.nLocations + c.nQuests + c.nStories + c.nRegions + c.nTimedEvents + c.nWeatherEvents;
	return c.nFacts <= NMD_WORLD_FACT_MAX_COUNT &&
		c.nLocations <= NMD_LOCATION_INVALID_ID && c.nRegions <= NMD_CLIMATE_MAX_REGION_COUNT &&
		c.nQuests <= NMD_WORLD_HANDLE_INDEX_MASK && c.nStories <= NMD_WORLD_HANDLE_INDEX_MASK &&
		nRecords <= UINT32_MAX / 2u &&
		c.nFacts >= m_FactValues.size() && c.nLocations >= m_Locations.size() &&
		c.nQuests >= m_Quests.size() && c.nStories >= m_Stories.size() &&
		c.nRegions >= m_RegionIds.size() && c.nQuestConditions >= m_Conditions.size() &&
		c.nLocationConditions >= m_LocationConditions.size() &&
		c.nOverflowObjectives >= m_OverflowObjectives.size() &&
		c.nDependencies >= m_DependencyBuild.size() && c.nDependencies >= m_Dependencies.size() &&
		c.nTimedEvents >= m_TimedEvents.size() && c.nWeatherEvents >= m_WeatherTimelineEvents.size() &&
		nRecords >= m_LookupEntries.size();
}


// C03: a reserved element count is independent from its writable high-water.
// Derived lookup capacities follow the committed record predictions.

// C03: precommit already-reserved pages at an explicit quiescent boundary.
// All new page commits complete before any array descriptor is made writable.
bool NMDWorldDatabase::setArenaCommitments( const nmdWorldDatabaseCapacities_t& budget )
{
	if ( m_ArenaBlock.pBase == NULL || !getArenaCapacitiesValid( budget ) ) {
		return false;
	}
	const uint32_t pageSize = NMDWorldArenaBackend::getPageSize();
	uint32_t target[ NMD_WORLD_ARENA_SEGMENT_COUNT ] = {};
	uint32_t targetBytes[ NMD_WORLD_ARENA_SEGMENT_COUNT ] = {};
	for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
		const nmdWorldArenaSegmentInfo_t& segment = m_ArenaSegments[i];
		const uint32_t count = NMDWorldArenaLayout::getCommittedCapacity( static_cast<nmdWorldArenaSegmentId_t>( i ),
			budget, segment.nCapacity );
		if ( count < segment.nCommittedCapacity || count > segment.nCapacity ) {
			return false;
		}
		const uint64_t bytes = static_cast<uint64_t>( count ) * segment.nElementBytes;
		const uint64_t rounded = ( bytes + pageSize - 1u ) & ~uint64_t( pageSize - 1u );
		// A non-Windows aligned heap is physically committed in full.
		const uint64_t physical = NMDWorldArenaBackend::getVirtualCommitSupported() ? rounded :
			( ( static_cast<uint64_t>( segment.nCapacity ) * segment.nElementBytes + pageSize - 1u ) &
				~uint64_t( pageSize - 1u ) );
		if ( physical > UINT32_MAX ||
			physical > static_cast<uint64_t>( m_ArenaBlock.nReservedBytes ) - segment.nOffsetBytes ) {
			return false;
		}
		target[i] = count;
		targetBytes[i] = static_cast<uint32_t>( physical );
	}
	// Commit in segment order. A failure decommits only pages acquired here,
	// leaving live records, dirty bits, logical capacities and generation intact.
	uint32_t committedThrough = 0u;
	for ( ; committedThrough < NMD_WORLD_ARENA_SEGMENT_COUNT; ++committedThrough ) {
		const nmdWorldArenaSegmentInfo_t& segment = m_ArenaSegments[committedThrough];
		const uint32_t extra = targetBytes[committedThrough] - segment.nCommittedBytes;
		if ( !NMDWorldArenaBackend::commit( m_ArenaBlock,
			segment.nOffsetBytes + segment.nCommittedBytes, extra ) ) {
			break;
		}
	}
	if ( committedThrough != NMD_WORLD_ARENA_SEGMENT_COUNT ) {
		for ( uint32_t i = 0u; i < committedThrough; ++i ) {
			const nmdWorldArenaSegmentInfo_t& segment = m_ArenaSegments[i];
			const uint32_t extra = targetBytes[i] - segment.nCommittedBytes;
			const bool reverted = NMDWorldArenaBackend::decommit( m_ArenaBlock,
				segment.nOffsetBytes + segment.nCommittedBytes, extra );
			assert( reverted );
			( void )reverted;
		}
		return false;
	}
#define NMD_COMMIT_ARRAY( Array, Id ) NMDWorldArenaLayout::grow( Array, target[ Id ] )
	NMD_COMMIT_ARRAY( m_FactValues, NMD_WORLD_ARENA_FACT_VALUES );
	NMD_COMMIT_ARRAY( m_Locations, NMD_WORLD_ARENA_LOCATIONS_HOT );
	NMD_COMMIT_ARRAY( m_Quests, NMD_WORLD_ARENA_QUESTS_HOT );
	NMD_COMMIT_ARRAY( m_OverflowObjectives, NMD_WORLD_ARENA_OBJECTIVES_OVERFLOW );
	NMD_COMMIT_ARRAY( m_Stories, NMD_WORLD_ARENA_STORIES_HOT );
	NMD_COMMIT_ARRAY( m_Calendar, NMD_WORLD_ARENA_CALENDAR_HOT );
	NMD_COMMIT_ARRAY( m_RegionWeather, NMD_WORLD_ARENA_WEATHER_HOT );
	NMD_COMMIT_ARRAY( m_WaterCycles, NMD_WORLD_ARENA_WATER_HOT );
	NMD_COMMIT_ARRAY( m_LocationOccupancy, NMD_WORLD_ARENA_LOCATION_OCCUPANCY );
	NMD_COMMIT_ARRAY( m_RegionIds, NMD_WORLD_ARENA_REGION_IDS );
	NMD_COMMIT_ARRAY( m_ClimateLinks, NMD_WORLD_ARENA_CLIMATE_LINKS );
	NMD_COMMIT_ARRAY( m_WeatherIndexEntries, NMD_WORLD_ARENA_WEATHER_INDEX_ENTRIES );
	NMD_COMMIT_ARRAY( m_WeatherIndexRanges, NMD_WORLD_ARENA_WEATHER_INDEX_RANGES );
	NMD_COMMIT_ARRAY( m_CompiledRevisions, NMD_WORLD_ARENA_COMPILED_REVISIONS );
	NMD_COMMIT_ARRAY( m_DirtyWords, NMD_WORLD_ARENA_DIRTY_WORDS );
	NMD_COMMIT_ARRAY( m_DirtyRevisions, NMD_WORLD_ARENA_DIRTY_REVISIONS );
	NMD_COMMIT_ARRAY( m_LookupBuckets, NMD_WORLD_ARENA_LOOKUP_BUCKETS );
	NMD_COMMIT_ARRAY( m_LookupEntries, NMD_WORLD_ARENA_LOOKUP_ENTRIES );
	NMD_COMMIT_ARRAY( m_DependencyOffsets, NMD_WORLD_ARENA_DEPENDENCY_OFFSETS );
	NMD_COMMIT_ARRAY( m_Dependencies, NMD_WORLD_ARENA_DEPENDENCIES );
	NMD_COMMIT_ARRAY( m_FactTypes, NMD_WORLD_ARENA_FACT_TYPES );
	NMD_COMMIT_ARRAY( m_FactAtoms, NMD_WORLD_ARENA_FACT_ATOMS );
	NMD_COMMIT_ARRAY( m_LocationCold, NMD_WORLD_ARENA_LOCATION_COLD );
	NMD_COMMIT_ARRAY( m_LocationAtoms, NMD_WORLD_ARENA_LOCATION_ATOMS );
	NMD_COMMIT_ARRAY( m_LocationConditions, NMD_WORLD_ARENA_LOCATION_CONDITIONS );
	NMD_COMMIT_ARRAY( m_QuestAtoms, NMD_WORLD_ARENA_QUEST_ATOMS );
	NMD_COMMIT_ARRAY( m_QuestMeta, NMD_WORLD_ARENA_QUEST_META );
	NMD_COMMIT_ARRAY( m_QuestTreeLinks, NMD_WORLD_ARENA_QUEST_TREE_LINKS );
	NMD_COMMIT_ARRAY( m_Conditions, NMD_WORLD_ARENA_QUEST_CONDITIONS );
	NMD_COMMIT_ARRAY( m_StoryAtoms, NMD_WORLD_ARENA_STORY_ATOMS );
	NMD_COMMIT_ARRAY( m_StoryTreeRanges, NMD_WORLD_ARENA_STORY_TREE_RANGES );
	NMD_COMMIT_ARRAY( m_CalendarCold, NMD_WORLD_ARENA_CALENDAR_COLD );
	NMD_COMMIT_ARRAY( m_ClimateSeasonal, NMD_WORLD_ARENA_CLIMATE_SEASONAL );
	NMD_COMMIT_ARRAY( m_ClimateGeography, NMD_WORLD_ARENA_CLIMATE_GEOGRAPHY );
	NMD_COMMIT_ARRAY( m_TimedEvents, NMD_WORLD_ARENA_TIMED_EVENTS );
	NMD_COMMIT_ARRAY( m_WeatherTimelineEvents, NMD_WORLD_ARENA_WEATHER_TIMELINE );
	NMD_COMMIT_ARRAY( m_DependencyBuild, NMD_WORLD_ARENA_DEPENDENCY_BUILD );
	NMD_COMMIT_ARRAY( m_DependencyBuckets, NMD_WORLD_ARENA_DEPENDENCY_BUCKETS );
	NMD_COMMIT_ARRAY( m_DependencyCursor, NMD_WORLD_ARENA_DEPENDENCY_CURSOR );
#undef NMD_COMMIT_ARRAY
	for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
		m_ArenaSegments[i].nCommittedCapacity = target[i];
		m_ArenaSegments[i].nCommittedBytes = targetBytes[i];
	}
	rehashLookup();
	if ( m_eArenaMode == NMD_WORLD_ARENA_AUTHORING ) {
		rebuildDependencyBuckets();
	}
	return true;
}

bool NMDWorldDatabase::setCommittedCapacities( const nmdWorldDatabaseCapacities_t& budget )
{
	if ( !getArenaCapacitiesValid( budget ) ) {
		return false;
	}
	if ( m_ArenaBlock.pBase == NULL ) {
		m_DesiredCommitCapacities = budget;
		m_bExplicitCommitBudget = true;
		return true;
	}
	nmdWorldDatabaseCapacities_t reserved = {};
	getCapacities( reserved );
	const uint32_t requested[] = { budget.nFacts, budget.nLocations, budget.nQuests,
		budget.nStories, budget.nRegions, budget.nQuestConditions, budget.nLocationConditions,
		budget.nDependencies, budget.nTimedEvents, budget.nWeatherEvents,
		budget.nOverflowObjectives };
	const uint32_t limits[] = { reserved.nFacts, reserved.nLocations, reserved.nQuests,
		reserved.nStories, reserved.nRegions, reserved.nQuestConditions, reserved.nLocationConditions,
		reserved.nDependencies, reserved.nTimedEvents, reserved.nWeatherEvents,
		reserved.nOverflowObjectives };
	for ( uint32_t i = 0u; i < sizeof( requested ) / sizeof( requested[0] ); ++i ) {
		if ( requested[i] > limits[i] ) {
			return false;
		}
	}
	bool shrinks = false;
	for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
		const nmdWorldArenaSegmentInfo_t& segment = m_ArenaSegments[i];
		const uint32_t target = NMDWorldArenaLayout::getCommittedCapacity( static_cast<nmdWorldArenaSegmentId_t>( i ),
			budget, segment.nCapacity );
		if ( target > segment.nCapacity ) {
			return false;
		}
		if ( target < segment.nCommittedCapacity ) {
			shrinks = true;
		}
	}
	// Shrinking page commitment is a transactional repack. Increasing precommit
	// can preserve the reservation and borrowed pointers at a quiescent boundary.
	if ( !shrinks ) {
		if ( !setArenaCommitments( budget ) ) {
			return false;
		}
		m_DesiredCommitCapacities = budget;
		m_bExplicitCommitBudget = true;
		return true;
	}
	const nmdWorldDatabaseCapacities_t previous = m_DesiredCommitCapacities;
	const bool wasExplicit = m_bExplicitCommitBudget;
	m_DesiredCommitCapacities = budget;
	m_bExplicitCommitBudget = true;
	if ( setArenaLayout( reserved, m_eArenaMode ) ) {
		return true;
	}
	m_DesiredCommitCapacities = previous;
	m_bExplicitCommitBudget = wasExplicit;
	return false;
}

bool NMDWorldDatabase::setCapacities( const nmdWorldDatabaseCapacities_t& c )
{
	return setArenaLayout( c, m_eArenaMode );
}

bool NMDWorldDatabase::setArenaLayout( const nmdWorldDatabaseCapacities_t& c,
	nmdWorldArenaMode_t eMode )
{
	if ( ( eMode != NMD_WORLD_ARENA_AUTHORING && eMode != NMD_WORLD_ARENA_SEALED_RUNTIME ) ||
		!getArenaCapacitiesValid( c ) ) {
		return false;
	}
	nmdWorldDatabaseCapacities_t old = {};
	getCapacities( old );
	if ( m_ArenaBlock.pBase != NULL && m_eArenaMode == eMode &&
		memcmp( &old, &c, sizeof( c ) ) == 0 ) {
		const nmdWorldDatabaseCapacities_t& budget = m_bExplicitCommitBudget ? m_DesiredCommitCapacities : c;
		bool same = true;
		for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
			const nmdWorldArenaSegmentInfo_t& segment = m_ArenaSegments[i];
			if ( NMDWorldArenaLayout::getCommittedCapacity( static_cast<nmdWorldArenaSegmentId_t>( i ),
				budget, segment.nCapacity ) != segment.nCommittedCapacity ) { same = false; break; }
		}
		if ( same ) {
			return true;
		}
	}
	const bool bRestoringAuthoring = m_ArenaBlock.pBase != NULL &&
		m_eArenaMode == NMD_WORLD_ARENA_SEALED_RUNTIME && eMode == NMD_WORLD_ARENA_AUTHORING;
	if ( bRestoringAuthoring && ( m_bDependencyIndexDirty ||
		m_DependencyOffsets.size() != m_FactValues.size() + 1u ||
		m_Dependencies.size() > c.nDependencies ) ) {
		return false;
	}

	const uint64_t nRecords = static_cast<uint64_t>( c.nFacts ) + c.nLocations + c.nQuests + c.nStories + c.nRegions + c.nTimedEvents + c.nWeatherEvents;
	uint32_t nRecordBuckets = NMD_WORLD_INDEX_INITIAL_BUCKETS;
	while ( nRecordBuckets < nRecords * 2u ) {
		if ( nRecordBuckets > UINT32_MAX / 2u ) {
			return false;
		}
		nRecordBuckets *= 2u;
	}
		if ( c.nDependencies > UINT32_MAX / 2u ) {
			return false;
		}
	uint32_t nDependencyBuckets = 8u;
	while ( nDependencyBuckets < static_cast<uint64_t>( c.nDependencies ) * 2u ) {
		if ( nDependencyBuckets > UINT32_MAX / 2u ) {
			return false;
		}
		nDependencyBuckets *= 2u;
	}

	nmdWorldDirtyRange_t dirtyRanges[ NMD_WORLD_DIRTY_CHANNEL_COUNT ][ NMD_WORLD_DIRTY_DOMAIN_COUNT ] = {};
	uint32_t nDirtyWords = 0u;
	const uint32_t counts[ NMD_WORLD_DIRTY_DOMAIN_COUNT ] = {
		c.nFacts, c.nLocations, c.nQuests, c.nStories, 1u, c.nRegions, c.nRegions
	};
	for ( uint32_t channel = 0u; channel < NMD_WORLD_DIRTY_CHANNEL_COUNT; ++channel ) {
		for ( uint32_t domain = 0u; domain < NMD_WORLD_DIRTY_DOMAIN_COUNT; ++domain ) {
			const uint32_t words = counts[domain] / 64u + ( ( counts[domain] & 63u ) != 0u );
			if ( words > UINT32_MAX - nDirtyWords ) {
				return false;
			}
			dirtyRanges[channel][domain] = { nDirtyWords, words };
			nDirtyWords += words;
		}
	}
	nmdWorldArenaSegmentInfo_t segments[ NMD_WORLD_ARENA_SEGMENT_COUNT ] = {};
	uint64_t nTotalBytes = 0u;
	const uint32_t nLayoutAlignment = NMDWorldArenaBackend::getPageSize();
	if ( nLayoutAlignment < 64u || ( nLayoutAlignment & ( nLayoutAlignment - 1u ) ) != 0u ) {
		return false;
	}
	const nmdWorldArenaSegmentSpec_t specs[] = {
		{ NMD_WORLD_ARENA_FACT_VALUES, c.nFacts, sizeof( nmdWorldValue_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_LOCATIONS_HOT, c.nLocations, sizeof( nmdLocationData_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_QUESTS_HOT, c.nQuests, sizeof( nmdWorldQuestHot_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_OBJECTIVES_OVERFLOW, c.nOverflowObjectives, sizeof( nmdWorldObjectiveHot_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_STORIES_HOT, c.nStories, sizeof( nmdWorldStoryState_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_CALENDAR_HOT, 1u, sizeof( nmdCalendarHot_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_WEATHER_HOT, c.nRegions, sizeof( nmdWeatherHot_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_WATER_HOT, c.nRegions, sizeof( nmdWaterCycleHot_t ), NMD_WORLD_ARENA_ACCESS_HOT },
		{ NMD_WORLD_ARENA_LOCATION_OCCUPANCY, c.nLocations, sizeof( uint16_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_REGION_IDS, c.nRegions, sizeof( nmdClimateRegionId_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_REGION_LOOKUP, 0u, sizeof( uint32_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_CLIMATE_LINKS, c.nRegions, sizeof( nmdClimateLinks_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_WEATHER_INDEX_ENTRIES, c.nWeatherEvents, sizeof( nmdWeatherTimelineIndexEntry_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_WEATHER_INDEX_RANGES, c.nRegions, sizeof( nmdWeatherTimelineRange_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_COMPILED_REVISIONS, 1u, sizeof( nmdWorldCompiledIndexRevisions_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_DIRTY_WORDS, nDirtyWords, sizeof( uint64_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_DIRTY_REVISIONS, 1u, sizeof( nmdWorldDirtyRevisions_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_LOOKUP_BUCKETS, nRecordBuckets, sizeof( uint32_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_LOOKUP_ENTRIES, static_cast<uint32_t>( nRecords ), sizeof( nmdWorldIndexEntry_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_DEPENDENCY_OFFSETS, c.nFacts + 1u, sizeof( uint32_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_DEPENDENCIES, c.nDependencies, sizeof( nmdWorldDependency_t ), NMD_WORLD_ARENA_ACCESS_WARM },
		{ NMD_WORLD_ARENA_FACT_TYPES, c.nFacts, sizeof( uint8_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_FACT_ATOMS, c.nFacts, sizeof( nmdWorldAtom_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_LOCATION_COLD, c.nLocations, sizeof( nmdWorldLocationCold_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_LOCATION_ATOMS, c.nLocations, sizeof( nmdWorldAtom_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_LOCATION_CONDITIONS, c.nLocationConditions, sizeof( nmdWorldCondition_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_QUEST_ATOMS, c.nQuests, sizeof( nmdWorldAtom_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_QUEST_META, c.nQuests, sizeof( nmdWorldQuestMeta_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_QUEST_TREE_LINKS, c.nQuests, sizeof( nmdWorldQuestTreeLink_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_QUEST_CONDITIONS, c.nQuestConditions, sizeof( nmdWorldCondition_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_STORY_ATOMS, c.nStories, sizeof( nmdWorldAtom_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_STORY_TREE_RANGES, c.nStories, sizeof( nmdWorldStoryTreeRange_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_CALENDAR_COLD, 1u, sizeof( nmdCalendarDefinition_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_CLIMATE_SEASONAL, c.nRegions, sizeof( nmdClimateSeasonalCold_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_CLIMATE_GEOGRAPHY, c.nRegions, sizeof( nmdClimateGeographyCold_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_TIMED_EVENTS, c.nTimedEvents, sizeof( nmdWorldTimedEvent_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_WEATHER_TIMELINE, c.nWeatherEvents, sizeof( nmdWeatherTimelineEvent_t ), NMD_WORLD_ARENA_ACCESS_COLD },
		{ NMD_WORLD_ARENA_DEPENDENCY_BUILD, eMode == NMD_WORLD_ARENA_AUTHORING ? c.nDependencies : 0u, sizeof( nmdWorldDependencyBuild_t ), NMD_WORLD_ARENA_ACCESS_AUTHORING },
		{ NMD_WORLD_ARENA_DEPENDENCY_BUCKETS, eMode == NMD_WORLD_ARENA_AUTHORING ? nDependencyBuckets : 0u, sizeof( uint32_t ), NMD_WORLD_ARENA_ACCESS_AUTHORING },
		{ NMD_WORLD_ARENA_DEPENDENCY_CURSOR, eMode == NMD_WORLD_ARENA_AUTHORING ? c.nFacts + 1u : 0u, sizeof( uint32_t ), NMD_WORLD_ARENA_ACCESS_AUTHORING }
	};
	if ( !NMDWorldArenaLayout::build( specs,
		static_cast<uint32_t>( sizeof( specs ) / sizeof( specs[0] ) ),
		nLayoutAlignment, segments, NMD_WORLD_ARENA_SEGMENT_COUNT, nTotalBytes ) ) {
		return false;
	}
	const uint32_t pageSize = NMDWorldArenaBackend::getPageSize();
	if ( pageSize < 64u || ( pageSize & ( pageSize - 1u ) ) != 0u ) {
		return false;
	}
	const uint64_t nAlignedBytes = ( nTotalBytes + pageSize - 1u ) & ~uint64_t( pageSize - 1u );
	if ( nAlignedBytes == 0u || nAlignedBytes > UINT32_MAX ) {
		return false;
	}
	const nmdWorldDatabaseCapacities_t& budget = m_bExplicitCommitBudget ? m_DesiredCommitCapacities : c;
	if ( !getArenaCapacitiesValid( budget ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
		nmdWorldArenaSegmentInfo_t& segment = segments[i];
		const uint32_t committed = NMDWorldArenaLayout::getCommittedCapacity( static_cast<nmdWorldArenaSegmentId_t>( i ),
			budget, segment.nCapacity );
		if ( committed > segment.nCapacity ) {
			return false;
		}
		const uint64_t physical = NMDWorldArenaLayout::getCommittedBytes( segment,
			committed, pageSize, NMDWorldArenaBackend::getVirtualCommitSupported() );
		if ( physical > UINT32_MAX || physical > nAlignedBytes - segment.nOffsetBytes ) {
			return false;
		}
		segment.nCommittedCapacity = committed;
		segment.nCommittedBytes = static_cast<uint32_t>( physical );
	}
	nmdWorldArenaBlock_t nextBlock = {};
	if ( !NMDWorldArenaBackend::reserve( static_cast<uint32_t>( nAlignedBytes ), nextBlock ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
		const nmdWorldArenaSegmentInfo_t& segment = segments[i];
		if ( !NMDWorldArenaBackend::commit( nextBlock, segment.nOffsetBytes, segment.nCommittedBytes ) ) {
			NMDWorldArenaBackend::release( nextBlock );
			return false;
		}
	}
	uint8_t *pNext = static_cast<uint8_t *>( nextBlock.pBase );
	nmdWorldArenaBlock_t oldBlock = m_ArenaBlock;
	const uint64_t *pOldDirty = m_DirtyWords.data();
	nmdWorldDirtyRange_t oldDirtyRanges[ NMD_WORLD_DIRTY_CHANNEL_COUNT ][ NMD_WORLD_DIRTY_DOMAIN_COUNT ];
	memcpy( oldDirtyRanges, m_DirtyRanges, sizeof( oldDirtyRanges ) );
	// Each typed view is reconstructed inside the new allocation before the
	// previous allocation is freed. All live records retain their dense indices.
	NMDWorldArenaLayout::bind( m_FactValues, pNext, segments[ NMD_WORLD_ARENA_FACT_VALUES ] );
	NMDWorldArenaLayout::bind( m_Locations, pNext, segments[ NMD_WORLD_ARENA_LOCATIONS_HOT ] );
	NMDWorldArenaLayout::bind( m_Quests, pNext, segments[ NMD_WORLD_ARENA_QUESTS_HOT ] );
	NMDWorldArenaLayout::bind( m_OverflowObjectives, pNext, segments[ NMD_WORLD_ARENA_OBJECTIVES_OVERFLOW ] );
	NMDWorldArenaLayout::bind( m_Stories, pNext, segments[ NMD_WORLD_ARENA_STORIES_HOT ] );
	NMDWorldArenaLayout::bind( m_Calendar, pNext, segments[ NMD_WORLD_ARENA_CALENDAR_HOT ] );
	NMDWorldArenaLayout::bind( m_RegionWeather, pNext, segments[ NMD_WORLD_ARENA_WEATHER_HOT ] );
	NMDWorldArenaLayout::bind( m_WaterCycles, pNext, segments[ NMD_WORLD_ARENA_WATER_HOT ] );
	NMDWorldArenaLayout::bind( m_LocationOccupancy, pNext, segments[ NMD_WORLD_ARENA_LOCATION_OCCUPANCY ] );
	NMDWorldArenaLayout::bind( m_RegionIds, pNext, segments[ NMD_WORLD_ARENA_REGION_IDS ] );
	NMDWorldArenaLayout::bind( m_ClimateLinks, pNext, segments[ NMD_WORLD_ARENA_CLIMATE_LINKS ] );
	NMDWorldArenaLayout::bind( m_WeatherIndexEntries, pNext, segments[ NMD_WORLD_ARENA_WEATHER_INDEX_ENTRIES ] );
	NMDWorldArenaLayout::bind( m_WeatherIndexRanges, pNext, segments[ NMD_WORLD_ARENA_WEATHER_INDEX_RANGES ] );
	NMDWorldArenaLayout::bind( m_CompiledRevisions, pNext, segments[ NMD_WORLD_ARENA_COMPILED_REVISIONS ] );
	NMDWorldArenaLayout::bind( m_DirtyRevisions, pNext, segments[ NMD_WORLD_ARENA_DIRTY_REVISIONS ] );
	NMDWorldArenaLayout::bind( m_DirtyWords, pNext, segments[ NMD_WORLD_ARENA_DIRTY_WORDS ], true );
	m_DirtyWords.resize( nDirtyWords, 0u );
	if ( pOldDirty != NULL ) {
		for ( uint32_t channel = 0u; channel < NMD_WORLD_DIRTY_CHANNEL_COUNT; ++channel ) {
			for ( uint32_t domain = 0u; domain < NMD_WORLD_DIRTY_DOMAIN_COUNT; ++domain ) {
				const nmdWorldDirtyRange_t oldRange = oldDirtyRanges[channel][domain];
				const nmdWorldDirtyRange_t nextRange = dirtyRanges[channel][domain];
				const uint32_t count = oldRange.nWordCount < nextRange.nWordCount ?
					oldRange.nWordCount : nextRange.nWordCount;
				if ( count != 0u ) {
					memcpy( m_DirtyWords.data() + nextRange.nFirstWord,
					pOldDirty + oldRange.nFirstWord, static_cast<size_t>( count ) * sizeof( uint64_t ) );
				}
			}
		}
	}
	memcpy( m_DirtyRanges, dirtyRanges, sizeof( m_DirtyRanges ) );
	if ( m_DirtyRevisions.empty() ) {
		nmdWorldDirtyRevisions_t revisions = {};
		for ( uint32_t channel = 0u; channel < NMD_WORLD_DIRTY_CHANNEL_COUNT; ++channel ) {
			for ( uint32_t domain = 0u; domain < NMD_WORLD_DIRTY_DOMAIN_COUNT; ++domain ) {
				revisions.szRevision[channel][domain] = 1u;
			}
		}
		m_DirtyRevisions.push_back( revisions );
	}
	NMDWorldArenaLayout::bind( m_LookupBuckets, pNext, segments[ NMD_WORLD_ARENA_LOOKUP_BUCKETS ], true );
	NMDWorldArenaLayout::bind( m_LookupEntries, pNext, segments[ NMD_WORLD_ARENA_LOOKUP_ENTRIES ] );
	NMDWorldArenaLayout::bind( m_DependencyOffsets, pNext, segments[ NMD_WORLD_ARENA_DEPENDENCY_OFFSETS ] );
	NMDWorldArenaLayout::bind( m_Dependencies, pNext, segments[ NMD_WORLD_ARENA_DEPENDENCIES ] );
	NMDWorldArenaLayout::bind( m_FactTypes, pNext, segments[ NMD_WORLD_ARENA_FACT_TYPES ] );
	NMDWorldArenaLayout::bind( m_FactAtoms, pNext, segments[ NMD_WORLD_ARENA_FACT_ATOMS ] );
	NMDWorldArenaLayout::bind( m_LocationCold, pNext, segments[ NMD_WORLD_ARENA_LOCATION_COLD ] );
	NMDWorldArenaLayout::bind( m_LocationAtoms, pNext, segments[ NMD_WORLD_ARENA_LOCATION_ATOMS ] );
	NMDWorldArenaLayout::bind( m_LocationConditions, pNext, segments[ NMD_WORLD_ARENA_LOCATION_CONDITIONS ] );
	NMDWorldArenaLayout::bind( m_QuestAtoms, pNext, segments[ NMD_WORLD_ARENA_QUEST_ATOMS ] );
	NMDWorldArenaLayout::bind( m_QuestMeta, pNext, segments[ NMD_WORLD_ARENA_QUEST_META ] );
	NMDWorldArenaLayout::bind( m_QuestTreeLinks, pNext, segments[ NMD_WORLD_ARENA_QUEST_TREE_LINKS ] );
	NMDWorldArenaLayout::bind( m_Conditions, pNext, segments[ NMD_WORLD_ARENA_QUEST_CONDITIONS ] );
	NMDWorldArenaLayout::bind( m_StoryAtoms, pNext, segments[ NMD_WORLD_ARENA_STORY_ATOMS ] );
	NMDWorldArenaLayout::bind( m_StoryTreeRanges, pNext, segments[ NMD_WORLD_ARENA_STORY_TREE_RANGES ] );
	NMDWorldArenaLayout::bind( m_CalendarCold, pNext, segments[ NMD_WORLD_ARENA_CALENDAR_COLD ] );
	NMDWorldArenaLayout::bind( m_ClimateSeasonal, pNext, segments[ NMD_WORLD_ARENA_CLIMATE_SEASONAL ] );
	NMDWorldArenaLayout::bind( m_ClimateGeography, pNext, segments[ NMD_WORLD_ARENA_CLIMATE_GEOGRAPHY ] );
	NMDWorldArenaLayout::bind( m_TimedEvents, pNext, segments[ NMD_WORLD_ARENA_TIMED_EVENTS ] );
	NMDWorldArenaLayout::bind( m_WeatherTimelineEvents, pNext, segments[ NMD_WORLD_ARENA_WEATHER_TIMELINE ] );
	NMDWorldArenaLayout::bind( m_DependencyBuild, pNext, segments[ NMD_WORLD_ARENA_DEPENDENCY_BUILD ],
		eMode == NMD_WORLD_ARENA_SEALED_RUNTIME || bRestoringAuthoring );
	NMDWorldArenaLayout::bind( m_DependencyBuckets, pNext, segments[ NMD_WORLD_ARENA_DEPENDENCY_BUCKETS ], true );
	NMDWorldArenaLayout::bind( m_DependencyCursor, pNext, segments[ NMD_WORLD_ARENA_DEPENDENCY_CURSOR ], true );
	m_Calendar.resize( 1u );
	m_CalendarCold.resize( 1u );
	if ( m_CompiledRevisions.empty() ) {
		m_CompiledRevisions.push_back( { 1u, 0u, 1u, 0u } );
	}
	m_DependencyCursor.clear();
	if ( bRestoringAuthoring ) {
		// The canonical CSR index survives sealing. Reconstruct authoring records
		// in deterministic fact-major/within-fact order before reopening registration.
		for ( uint32_t fact = 0u; fact < m_FactValues.size(); ++fact ) {
			for ( uint32_t i = m_DependencyOffsets[fact]; i < m_DependencyOffsets[fact + 1u]; ++i ) {
				const nmdWorldDependency_t& dep = m_Dependencies[i];
				m_DependencyBuild.push_back( { dep.hOwner, static_cast<nmdWorldFactId_t>( fact ), dep.nItemIndex } );
			}
		}
	}
	m_ArenaBlock = nextBlock;
	m_nDependencyContentCapacity = c.nDependencies;
	m_eArenaMode = eMode;
	++m_nArenaGeneration;
	if ( m_nArenaGeneration == 0u ) {
		m_nArenaGeneration = 1u;
	}
	memcpy( m_ArenaSegments, segments, sizeof( segments ) );
	rehashLookup();
		if ( eMode == NMD_WORLD_ARENA_AUTHORING ) {
			rebuildDependencyBuckets();
		}
	NMDWorldArenaBackend::release( oldBlock );
	return true;
}

void NMDWorldDatabase::getCapacities( nmdWorldDatabaseCapacities_t& c ) const
{
	c = { m_FactValues.capacity(), m_Locations.capacity(), m_Quests.capacity(),
		m_Stories.capacity(), m_RegionIds.capacity(), m_Conditions.capacity(),
		m_LocationConditions.capacity(), m_nDependencyContentCapacity,
		m_TimedEvents.capacity(), m_WeatherTimelineEvents.capacity(),
		m_OverflowObjectives.capacity() };
}

const void *NMDWorldDatabase::getArenaBase( void ) const { return m_ArenaBlock.pBase; }
uint32_t NMDWorldDatabase::getArenaSizeBytes( void ) const { return m_ArenaBlock.nReservedBytes; }
uint32_t NMDWorldDatabase::getArenaReservedBytes( void ) const { return m_ArenaBlock.nReservedBytes; }
uint32_t NMDWorldDatabase::getArenaCommittedBytes( void ) const { return m_ArenaBlock.nCommittedBytes; }
bool NMDWorldDatabase::getArenaVirtualCommitSupported( void ) const
{
	return NMDWorldArenaBackend::getVirtualCommitSupported();
}
uint32_t NMDWorldDatabase::getArenaLiveBytes( void ) const
{
	uint64_t total = 0u;
	for ( uint32_t i = 0u; i < NMD_WORLD_ARENA_SEGMENT_COUNT; ++i ) {
		nmdWorldArenaSegmentInfo_t info = {};
		if ( getArenaSegmentInfo( static_cast<nmdWorldArenaSegmentId_t>( i ), info ) ) {
			total += static_cast<uint64_t>( info.nCount ) * info.nElementBytes;
		}
	}
	return static_cast<uint32_t>( total );
}
void NMDWorldDatabase::getCommittedCapacities( nmdWorldDatabaseCapacities_t& c ) const
{
	if ( m_bExplicitCommitBudget ) {
		c = m_DesiredCommitCapacities;
	} else {
		getCapacities( c );
	}
}

uint32_t NMDWorldDatabase::getArenaGeneration( void ) const { return m_nArenaGeneration; }
nmdWorldArenaMode_t NMDWorldDatabase::getArenaMode( void ) const { return m_eArenaMode; }

bool NMDWorldDatabase::setArenaMode( nmdWorldArenaMode_t eMode )
{
	if ( eMode > NMD_WORLD_ARENA_SEALED_RUNTIME || m_ArenaBlock.pBase == NULL ) {
		return false;
	}
	if ( m_eArenaMode == eMode ) {
		return true;
	}
	if ( eMode == NMD_WORLD_ARENA_SEALED_RUNTIME ) {
		if ( m_bDependencyIndexDirty || m_DependencyOffsets.size() != m_FactValues.size() + 1u ||
			m_DependencyBuild.size() != m_Dependencies.size() ) {
			return false;
		}
	}
	nmdWorldDatabaseCapacities_t c = {};
	getCapacities( c );
	return setArenaLayout( c, eMode );
}

uint32_t NMDWorldDatabase::getDirtyRevision( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel ) const
{
	if ( eDomain >= NMD_WORLD_DIRTY_DOMAIN_COUNT || eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT ||
		m_DirtyRevisions.empty() ) {
		return 0u;
	}
	return m_DirtyRevisions[0].szRevision[eChannel][eDomain];
}

bool NMDWorldDatabase::getDirtyWords( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel, uint64_t *pOutput, uint32_t nCapacity,
	uint32_t& nWordCount, uint32_t& nRevision ) const
{
	nWordCount = 0u; nRevision = 0u;
	if ( eDomain >= NMD_WORLD_DIRTY_DOMAIN_COUNT || eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT ||
		m_DirtyRevisions.empty() ) {
		return false;
	}
	const nmdWorldDirtyRange_t range = m_DirtyRanges[eChannel][eDomain];
	nWordCount = range.nWordCount;
	if ( nCapacity < nWordCount || ( nWordCount != 0u && pOutput == NULL ) ) {
		return false;
	}
	if ( nWordCount != 0u ) {
		memcpy( pOutput, m_DirtyWords.data() + range.nFirstWord,
		static_cast<size_t>( nWordCount ) * sizeof( uint64_t ) );
	}
	nRevision = getDirtyRevision( eDomain, eChannel );
	return true;
}

bool NMDWorldDatabase::setDirtyAcknowledged( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel, uint32_t nExpectedRevision,
	const uint64_t *pMask, uint32_t nWordCount )
{
	if ( eDomain >= NMD_WORLD_DIRTY_DOMAIN_COUNT || eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT ||
		m_DirtyRevisions.empty() || nExpectedRevision == 0u ||
		getDirtyRevision( eDomain, eChannel ) != nExpectedRevision ) {
		return false;
	}
	const nmdWorldDirtyRange_t range = m_DirtyRanges[eChannel][eDomain];
	if ( nWordCount != range.nWordCount || ( nWordCount != 0u && pMask == NULL ) ) {
		return false;
	}
	for ( uint32_t word = 0u; word < nWordCount; ++word ) {
		m_DirtyWords[ range.nFirstWord + word ] &= ~pMask[word];
	}
	return true;
}

void NMDWorldDatabase::setDirty( nmdWorldDirtyDomain_t eDomain, uint32_t nIndex )
{
	if ( m_DirtyRevisions.empty() || eDomain >= NMD_WORLD_DIRTY_DOMAIN_COUNT ) {
		return;
	}
	for ( uint32_t channel = 0u; channel < NMD_WORLD_DIRTY_CHANNEL_COUNT; ++channel ) {
		const nmdWorldDirtyRange_t range = m_DirtyRanges[channel][eDomain];
		if ( ( nIndex >> 6u ) >= range.nWordCount ) {
			continue;
		}
		m_DirtyWords[ range.nFirstWord + ( nIndex >> 6u ) ] |= ( uint64_t(1) << ( nIndex & 63u ) );
		uint32_t& revision = m_DirtyRevisions[0].szRevision[channel][eDomain];
		++revision;
		if ( revision == 0u ) {
			revision = 1u;
		}
	}
}

void NMDWorldDatabase::setDirtyIfWeatherChanged( uint32_t nIndex, const nmdWeatherHot_t& before )
{
	if ( nIndex < m_RegionWeather.size() && !sameWeather( before, m_RegionWeather[nIndex] ) ) {
		setDirty( NMD_WORLD_DIRTY_WEATHER, nIndex );
	}
}

void NMDWorldDatabase::setDirtyIfWaterChanged( uint32_t nIndex, const nmdWaterCycleHot_t& before )
{
	if ( nIndex < m_WaterCycles.size() && !sameWater( before, m_WaterCycles[nIndex] ) ) {
		setDirty( NMD_WORLD_DIRTY_WATER, nIndex );
	}
}

void NMDWorldDatabase::setDirtyIfCalendarChanged( const nmdCalendarHot_t& before )
{
	if ( !m_Calendar.empty() && !sameCalendar( before, m_Calendar[0] ) ) {
		setDirty( NMD_WORLD_DIRTY_CALENDAR, 0u );
	}
}

void NMDWorldDatabase::setDirtyDomainBaseline( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel )
{
	const nmdWorldDirtyRange_t range = m_DirtyRanges[eChannel][eDomain];
	if ( range.nWordCount ) {
		memset( m_DirtyWords.data() + range.nFirstWord, 0,
		static_cast<size_t>( range.nWordCount ) * sizeof( uint64_t ) );
	}
	uint32_t& revision = m_DirtyRevisions[0].szRevision[eChannel][eDomain];
	if ( ++revision == 0u ) {
		revision = 1u;
	}
}

void NMDWorldDatabase::setDirtyDomainResync( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel )
{
	const uint32_t counts[ NMD_WORLD_DIRTY_DOMAIN_COUNT ] = {
		m_FactValues.size(), m_Locations.size(), m_Quests.size(), m_Stories.size(),
		m_bHasCalendar ? 1u : 0u, m_RegionIds.size(), m_RegionIds.size()
	};
	const nmdWorldDirtyRange_t range = m_DirtyRanges[eChannel][eDomain];
	for ( uint32_t i = 0u; i < range.nWordCount; ++i ) {
		const uint32_t remaining = counts[eDomain] > i * 64u ? counts[eDomain] - i * 64u : 0u;
		m_DirtyWords[ range.nFirstWord + i ] = remaining >= 64u ? UINT64_MAX :
			( remaining == 0u ? 0u : ( uint64_t(1) << remaining ) - 1u );
	}
	uint32_t& revision = m_DirtyRevisions[0].szRevision[eChannel][eDomain];
	if ( ++revision == 0u ) {
		revision = 1u;
	}
}

bool NMDWorldDatabase::setDirtyBaselineDomain( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel )
{
	if ( eDomain >= NMD_WORLD_DIRTY_DOMAIN_COUNT || eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT ) {
		return false;
	}
	setDirtyDomainBaseline( eDomain, eChannel );
	return true;
}

bool NMDWorldDatabase::setDirtyResyncDomain( nmdWorldDirtyDomain_t eDomain,
	nmdWorldDirtyChannel_t eChannel )
{
	if ( eDomain >= NMD_WORLD_DIRTY_DOMAIN_COUNT || eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT ) {
		return false;
	}
	setDirtyDomainResync( eDomain, eChannel );
	return true;
}

bool NMDWorldDatabase::setDirtyBaseline( nmdWorldDirtyChannel_t eChannel )
{
	if ( eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT || m_DirtyRevisions.empty() ) {
		return false;
	}
	for ( uint32_t domain = 0u; domain < NMD_WORLD_DIRTY_DOMAIN_COUNT; ++domain ) {
		setDirtyDomainBaseline( static_cast<nmdWorldDirtyDomain_t>( domain ), eChannel );
	}
	return true;
}

bool NMDWorldDatabase::setDirtyResync( nmdWorldDirtyChannel_t eChannel )
{
	if ( eChannel >= NMD_WORLD_DIRTY_CHANNEL_COUNT || m_DirtyRevisions.empty() ) {
		return false;
	}
	for ( uint32_t domain = 0u; domain < NMD_WORLD_DIRTY_DOMAIN_COUNT; ++domain ) {
		setDirtyDomainResync( static_cast<nmdWorldDirtyDomain_t>( domain ), eChannel );
	}
	return true;
}

bool NMDWorldDatabase::getArenaSegmentInfo( nmdWorldArenaSegmentId_t eSegment,
	nmdWorldArenaSegmentInfo_t& info ) const
{
	if ( eSegment >= NMD_WORLD_ARENA_SEGMENT_COUNT || m_ArenaBlock.pBase == NULL ) {
		return false;
	}
	info = m_ArenaSegments[ eSegment ];
	switch ( eSegment ) {
		case NMD_WORLD_ARENA_FACT_VALUES: info.nCount = m_FactValues.size(); break;
		case NMD_WORLD_ARENA_LOCATIONS_HOT: info.nCount = m_Locations.size(); break;
		case NMD_WORLD_ARENA_QUESTS_HOT: info.nCount = m_Quests.size(); break;
		case NMD_WORLD_ARENA_OBJECTIVES_OVERFLOW: info.nCount = m_OverflowObjectives.size(); break;
		case NMD_WORLD_ARENA_STORIES_HOT: info.nCount = m_Stories.size(); break;
		case NMD_WORLD_ARENA_CALENDAR_HOT: info.nCount = m_Calendar.size(); break;
		case NMD_WORLD_ARENA_WEATHER_HOT: info.nCount = m_RegionWeather.size(); break;
		case NMD_WORLD_ARENA_WATER_HOT: info.nCount = m_WaterCycles.size(); break;
		case NMD_WORLD_ARENA_LOCATION_OCCUPANCY: info.nCount = m_LocationOccupancy.size(); break;
		case NMD_WORLD_ARENA_REGION_IDS: info.nCount = m_RegionIds.size(); break;
		case NMD_WORLD_ARENA_REGION_LOOKUP: info.nCount = 0u; break;
		case NMD_WORLD_ARENA_CLIMATE_LINKS: info.nCount = m_ClimateLinks.size(); break;
		case NMD_WORLD_ARENA_WEATHER_INDEX_ENTRIES: info.nCount = m_WeatherIndexEntries.size(); break;
		case NMD_WORLD_ARENA_WEATHER_INDEX_RANGES: info.nCount = m_WeatherIndexRanges.size(); break;
		case NMD_WORLD_ARENA_COMPILED_REVISIONS: info.nCount = m_CompiledRevisions.size(); break;
		case NMD_WORLD_ARENA_DIRTY_WORDS: info.nCount = m_DirtyWords.size(); break;
		case NMD_WORLD_ARENA_DIRTY_REVISIONS: info.nCount = m_DirtyRevisions.size(); break;
		case NMD_WORLD_ARENA_LOOKUP_BUCKETS: info.nCount = m_LookupBuckets.size(); break;
		case NMD_WORLD_ARENA_LOOKUP_ENTRIES: info.nCount = m_LookupEntries.size(); break;
		case NMD_WORLD_ARENA_DEPENDENCY_OFFSETS: info.nCount = m_DependencyOffsets.size(); break;
		case NMD_WORLD_ARENA_DEPENDENCIES: info.nCount = m_Dependencies.size(); break;
		case NMD_WORLD_ARENA_FACT_TYPES: info.nCount = m_FactTypes.size(); break;
		case NMD_WORLD_ARENA_FACT_ATOMS: info.nCount = m_FactAtoms.size(); break;
		case NMD_WORLD_ARENA_LOCATION_COLD: info.nCount = m_LocationCold.size(); break;
		case NMD_WORLD_ARENA_LOCATION_ATOMS: info.nCount = m_LocationAtoms.size(); break;
		case NMD_WORLD_ARENA_LOCATION_CONDITIONS: info.nCount = m_LocationConditions.size(); break;
		case NMD_WORLD_ARENA_QUEST_ATOMS: info.nCount = m_QuestAtoms.size(); break;
		case NMD_WORLD_ARENA_QUEST_META: info.nCount = m_QuestMeta.size(); break;
		case NMD_WORLD_ARENA_QUEST_TREE_LINKS: info.nCount = m_QuestTreeLinks.size(); break;
		case NMD_WORLD_ARENA_QUEST_CONDITIONS: info.nCount = m_Conditions.size(); break;
		case NMD_WORLD_ARENA_STORY_ATOMS: info.nCount = m_StoryAtoms.size(); break;
		case NMD_WORLD_ARENA_STORY_TREE_RANGES: info.nCount = m_StoryTreeRanges.size(); break;
		case NMD_WORLD_ARENA_CALENDAR_COLD: info.nCount = m_CalendarCold.size(); break;
		case NMD_WORLD_ARENA_CLIMATE_SEASONAL: info.nCount = m_ClimateSeasonal.size(); break;
		case NMD_WORLD_ARENA_CLIMATE_GEOGRAPHY: info.nCount = m_ClimateGeography.size(); break;
		case NMD_WORLD_ARENA_TIMED_EVENTS: info.nCount = m_TimedEvents.size(); break;
		case NMD_WORLD_ARENA_WEATHER_TIMELINE: info.nCount = m_WeatherTimelineEvents.size(); break;
		case NMD_WORLD_ARENA_DEPENDENCY_BUILD: info.nCount = m_DependencyBuild.size(); break;
		case NMD_WORLD_ARENA_DEPENDENCY_BUCKETS: info.nCount = m_DependencyBuckets.size(); break;
		case NMD_WORLD_ARENA_DEPENDENCY_CURSOR: info.nCount = m_DependencyCursor.size(); break;
		default: return false;
	}
	return true;
}


nmdWorldHandle_t NMDWorldDatabase::getRecordHandle( nmdWorldAtom_t pAtom, nmdWorldRecordType_t eType ) const
{
	nmdWorldHandle_t h = { NMD_WORLD_HANDLE_INVALID };
	if ( pAtom != NULL && eType >= NMD_WORLD_RECORD_FACT && eType <= NMD_WORLD_RECORD_STORY ) {
		findIdentity( NMDWorldIdIndex::getAtomKey( pAtom, eType ), h );
	}
	return h;
}

bool NMDWorldDatabase::getRecordExists( nmdWorldHandle_t hRecord ) const
{
	if ( !hRecord.isValid() ) {
		return false;
	}
	const uint32_t nIndex = hRecord.getIndex();
	switch ( hRecord.getType() ) {
		case NMD_WORLD_RECORD_FACT: return nIndex < m_FactValues.size();
		case NMD_WORLD_RECORD_LOCATION: return nIndex < m_Locations.size();
		case NMD_WORLD_RECORD_QUEST: return nIndex < m_Quests.size();
		case NMD_WORLD_RECORD_STORY: return nIndex < m_Stories.size();
		case NMD_WORLD_RECORD_REGION: return nIndex < m_RegionIds.size();
		case NMD_WORLD_RECORD_TIMED_EVENT: return nIndex < m_TimedEvents.size();
		case NMD_WORLD_RECORD_WEATHER_EVENT: return nIndex < m_WeatherTimelineEvents.size();
		default: return false;
	}
}

nmdWorldAtom_t NMDWorldDatabase::getRecordAtom( nmdWorldHandle_t hRecord ) const
{
	if ( !getRecordExists( hRecord ) ) {
		return NULL;
	}
	switch ( hRecord.getType() ) {
		case NMD_WORLD_RECORD_FACT: return m_FactAtoms[ hRecord.getIndex() ];
		case NMD_WORLD_RECORD_LOCATION: return m_LocationAtoms[ hRecord.getIndex() ];
		case NMD_WORLD_RECORD_QUEST: return m_QuestAtoms[ hRecord.getIndex() ];
		case NMD_WORLD_RECORD_STORY: return m_StoryAtoms[ hRecord.getIndex() ];
		default: return NULL;
	}
}

bool NMDWorldDatabase::isValidFact( nmdWorldFactId_t nId, nmdWorldFactType_t eType ) const
{
	return nId < m_FactTypes.size() && m_FactTypes[ nId ] == eType;
}


nmdWorldFactId_t NMDWorldDatabase::setFactDefinition( nmdWorldAtom_t pAtom,
	nmdWorldFactType_t eType, nmdWorldValue_t value )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING || m_nFactDefinitionGeneration == UINT32_MAX ) {
		return NMD_WORLD_FACT_INVALID_ID;
	}
	if ( pAtom == NULL ) {
		return NMD_WORLD_FACT_INVALID_ID;
	}
	const nmdWorldFactId_t nExisting = getFactId( pAtom );
	if ( nExisting != NMD_WORLD_FACT_INVALID_ID ) {
		return isValidFact( nExisting, eType ) ? nExisting : NMD_WORLD_FACT_INVALID_ID;
	}
	if ( m_FactValues.size() >= NMD_WORLD_FACT_MAX_COUNT ||
		m_FactValues.size() >= m_FactValues.committedCapacity() ||
		!ensureLookupCapacity( static_cast<uint32_t>( m_LookupEntries.size() + 1u ) ) ) {
		return NMD_WORLD_FACT_INVALID_ID;
	}
	const nmdWorldFactId_t nId = static_cast<nmdWorldFactId_t>( m_FactValues.size() );
	m_FactValues.push_back( value ); m_FactTypes.push_back( static_cast<uint8_t>( eType ) );
	m_FactAtoms.push_back( pAtom );
	m_bDependencyIndexDirty = true;
	++m_nFactDefinitionGeneration;
	insertRecord( NMDWorldIdIndex::getAtomKey( pAtom, NMD_WORLD_RECORD_FACT ), makeHandle( NMD_WORLD_RECORD_FACT, nId ) );
	setDirty( NMD_WORLD_DIRTY_FACT, nId );
	return nId;
}

nmdWorldFactId_t NMDWorldDatabase::getFactId( nmdWorldAtom_t pAtom ) const
{
	const nmdWorldHandle_t hFact = getRecordHandle( pAtom, NMD_WORLD_RECORD_FACT );
	return hFact.isValid() ? static_cast<nmdWorldFactId_t>( hFact.getIndex() ) : NMD_WORLD_FACT_INVALID_ID;
}

bool NMDWorldDatabase::getFact( nmdWorldFactId_t nId, nmdWorldFactType_t& eType, nmdWorldValue_t& value ) const
{
	if ( nId >= m_FactValues.size() ) {
		return false;
	}
	eType = static_cast<nmdWorldFactType_t>( m_FactTypes[ nId ] );
	value = m_FactValues[ nId ];
	return true;
}

bool NMDWorldDatabase::setFact( nmdWorldFactId_t nId, nmdWorldFactType_t eType, nmdWorldValue_t value )
{
	if ( !isValidFact( nId, eType ) ) {
		return false;
	}
	if ( eType == NMD_WORLD_FACT_BOOLEAN && value.nBits > 1u ) {
		return false;
	}
	if ( eType == NMD_WORLD_FACT_FLOAT && !std::isfinite( value.asFloat() ) ) {
		return false;
	}
	if ( m_FactValues[ nId ].nBits != value.nBits ) {
		m_FactValues[ nId ] = value;
		setDirty( NMD_WORLD_DIRTY_FACT, nId );
	}
	return true;
}

bool NMDWorldDatabase::getFactFloat( nmdWorldFactId_t nId, float& value ) const
{
	if ( !isValidFact( nId, NMD_WORLD_FACT_FLOAT ) ) {
		return false;
	}
	value = m_FactValues[ nId ].asFloat(); return true;
}

bool NMDWorldDatabase::getFactInteger( nmdWorldFactId_t nId, int32_t& value ) const
{
	if ( !isValidFact( nId, NMD_WORLD_FACT_INTEGER ) ) {
		return false;
	}
	value = m_FactValues[ nId ].asInteger(); return true;
}

bool NMDWorldDatabase::getFactBoolean( nmdWorldFactId_t nId, bool& value ) const
{
	if ( !isValidFact( nId, NMD_WORLD_FACT_BOOLEAN ) ) {
		return false;
	}
	value = m_FactValues[ nId ].asBoolean(); return true;
}

bool NMDWorldDatabase::setFactFloat( nmdWorldFactId_t nId, float value )
{
	return setFact( nId, NMD_WORLD_FACT_FLOAT, nmdWorldValue_t::fromFloat( value ) );
}

bool NMDWorldDatabase::setFactInteger( nmdWorldFactId_t nId, int32_t value )
{
	return setFact( nId, NMD_WORLD_FACT_INTEGER, nmdWorldValue_t::fromInteger( value ) );
}

bool NMDWorldDatabase::setFactBoolean( nmdWorldFactId_t nId, bool value )
{
	return setFact( nId, NMD_WORLD_FACT_BOOLEAN, nmdWorldValue_t::fromBoolean( value ) );
}

uint32_t NMDWorldDatabase::getFactCount( void ) const { return static_cast<uint32_t>( m_FactValues.size() ); }
const nmdWorldValue_t *NMDWorldDatabase::getFactHotData( void ) const { return m_FactValues.data(); }
uint32_t NMDWorldDatabase::getFactDefinitionGeneration( void ) const { return m_nFactDefinitionGeneration; }

bool NMDWorldDatabase::getCompiledFactHandle( nmdWorldFactId_t nId,
	nmdWorldFactType_t eType, nmdWorldCompiledFactHandle_t& handle ) const
{
	if ( !isValidFact( nId, eType ) ) {
		return false;
	}
	handle = { nId, static_cast<uint8_t>( eType ), 0u, m_nFactDefinitionGeneration };
	return true;
}

bool NMDWorldDatabase::getCompiledFactValue( const nmdWorldCompiledFactHandle_t& handle,
	nmdWorldValue_t& value ) const
{
	if ( handle.nDefinitionGeneration != m_nFactDefinitionGeneration ||
		handle.nFactId >= m_FactValues.size() || handle.eType < NMD_WORLD_FACT_FLOAT ||
		handle.eType > NMD_WORLD_FACT_BOOLEAN ) {
		return false;
	}
	value = m_FactValues[ handle.nFactId ];
	return true;
}

bool NMDWorldDatabase::setCompiledFactValue( const nmdWorldCompiledFactHandle_t& handle,
	nmdWorldValue_t value )
{
	if ( handle.nDefinitionGeneration != m_nFactDefinitionGeneration ) {
		return false;
	}
	return setFact( handle.nFactId, static_cast<nmdWorldFactType_t>( handle.eType ), value );
}

bool NMDWorldDatabase::isLocationHandle( nmdWorldHandle_t h ) const
{
	return h.getType() == NMD_WORLD_RECORD_LOCATION && getRecordExists( h );
}

nmdWorldHandle_t NMDWorldDatabase::setLocationDefinition( nmdWorldAtom_t pAtom,
	const nmdWorldLocationDefinition_t& d, const nmdWorldCondition_t *pConditions,
	uint16_t nConditionCount, nmdLocationId_t nAuthoredId )
{
	const nmdWorldHandle_t invalid = { NMD_WORLD_HANDLE_INVALID };
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING || pAtom == NULL ||
		m_Locations.size() >= NMD_LOCATION_INVALID_ID ||
		( nAuthoredId != NMD_LOCATION_INVALID_ID && nAuthoredId != m_Locations.size() ) ||
		( nConditionCount && pConditions == NULL ) ||
		static_cast<uint64_t>( m_LocationConditions.size() ) + nConditionCount > UINT32_MAX ) {
		return invalid;
	}
	const nmdWorldHandle_t old = getRecordHandle( pAtom, NMD_WORLD_RECORD_LOCATION );
	if ( old.isValid() ) {
		return old;
	}
	if ( m_Locations.size() >= m_Locations.committedCapacity() ||
		static_cast<uint64_t>( m_LocationConditions.size() ) + nConditionCount > m_LocationConditions.committedCapacity() ||
		!ensureLookupCapacity( static_cast<uint32_t>( m_LookupEntries.size() + 1u ) ) ) {
		return invalid;
	}
	const nmdLocationId_t nId = static_cast<nmdLocationId_t>( m_Locations.size() );
	m_Locations.push_back( { nId, static_cast<uint16_t>( d.nTraits & NMD_LOCATION_TRAITS_MASK ), 0u, 0u } );
	nmdWorldLocationCold_t cold = {};
	cold.pDescription = d.pDescription; cold.nRegionId = d.nRegionId;
	memcpy( cold.position, d.position, sizeof( cold.position ) );
	cold.fCompassRadius = d.fCompassRadius;
	cold.nFirstCondition = static_cast<uint32_t>( m_LocationConditions.size() );
	cold.nConditionCount = nConditionCount;
	m_LocationCold.push_back( cold );
	m_LocationAtoms.push_back( pAtom ); m_LocationOccupancy.push_back( 0u );
	for ( uint32_t i = 0u; i < nConditionCount; ++i ) {
		m_LocationConditions.push_back( pConditions[ i ] );
	}
	const nmdWorldHandle_t hLocation = makeHandle( NMD_WORLD_RECORD_LOCATION, nId );
	insertRecord( NMDWorldIdIndex::getAtomKey( pAtom, NMD_WORLD_RECORD_LOCATION ), hLocation );
	setDirty( NMD_WORLD_DIRTY_LOCATION, nId );
	return hLocation;
}

nmdWorldHandle_t NMDWorldDatabase::getLocationHandle( nmdLocationId_t nId ) const
{
	return nId < m_Locations.size() ? makeHandle( NMD_WORLD_RECORD_LOCATION, nId ) :
		nmdWorldHandle_t{ NMD_WORLD_HANDLE_INVALID };
}

bool NMDWorldDatabase::getLocationData( nmdWorldHandle_t h, nmdLocationData_t& hot ) const
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	hot = m_Locations[ h.getIndex() ]; return true;
}

bool NMDWorldDatabase::setLocationData( nmdWorldHandle_t h, const nmdLocationData_t& hot )
{
	if ( !isLocationHandle( h ) || hot.nId != h.getIndex() ) {
		return false;
	}
	nmdLocationData_t& current = m_Locations[ h.getIndex() ];
	if ( !sameLocation( current, hot ) ) {
		current = hot;
		setDirty( NMD_WORLD_DIRTY_LOCATION, h.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::getLocationDefinition( nmdWorldHandle_t h, nmdWorldLocationCold_t& cold ) const
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	cold = m_LocationCold[ h.getIndex() ]; return true;
}

bool NMDWorldDatabase::setLocationDefinition( nmdWorldHandle_t h, const nmdWorldLocationDefinition_t& d )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ||
		!isLocationHandle( h ) ) {
		return false;
	}
	nmdWorldLocationCold_t& cold = m_LocationCold[ h.getIndex() ];
	const bool bChanged = cold.pDescription != d.pDescription || cold.nRegionId != d.nRegionId ||
		cold.fCompassRadius != d.fCompassRadius ||
		cold.position[0] != d.position[0] || cold.position[1] != d.position[1] ||
		cold.position[2] != d.position[2] ||
		( m_Locations[ h.getIndex() ].nStateAndTraits & NMD_LOCATION_TRAITS_MASK ) !=
		( d.nTraits & NMD_LOCATION_TRAITS_MASK );
	cold.pDescription = d.pDescription; cold.nRegionId = d.nRegionId;
	memcpy( cold.position, d.position, sizeof( cold.position ) );
	cold.fCompassRadius = d.fCompassRadius;
	m_Locations[ h.getIndex() ].nStateAndTraits = static_cast<uint16_t>(
		( m_Locations[ h.getIndex() ].nStateAndTraits & NMD_LOCATION_STATUS_MASK ) |
		( d.nTraits & NMD_LOCATION_TRAITS_MASK ) );
	if ( bChanged ) {
		setDirty( NMD_WORLD_DIRTY_LOCATION, h.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::getLocationStatus( nmdWorldHandle_t h, nmdLocationStatus_t& status ) const
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	status = static_cast<nmdLocationStatus_t>( m_Locations[ h.getIndex() ].nStateAndTraits & NMD_LOCATION_STATUS_MASK );
	return true;
}

bool NMDWorldDatabase::setLocationStatus( nmdWorldHandle_t h, nmdLocationStatus_t status )
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	nmdLocationData_t& hot = m_Locations[ h.getIndex() ];
	const uint16_t next = static_cast<uint16_t>( ( hot.nStateAndTraits & NMD_LOCATION_TRAITS_MASK ) |
		static_cast<uint16_t>( status ) );
	if ( hot.nStateAndTraits != next ) {
		hot.nStateAndTraits = next;
		setDirty( NMD_WORLD_DIRTY_LOCATION, h.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::getLocationOccupancy( nmdWorldHandle_t h, uint16_t& nCount ) const
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	nCount = m_LocationOccupancy[ h.getIndex() ]; return true;
}

bool NMDWorldDatabase::setLocationOccupancy( nmdWorldHandle_t h, uint16_t nCount )
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	m_LocationOccupancy[ h.getIndex() ] = nCount; return true;
}

bool NMDWorldDatabase::getLocationConditions( nmdWorldHandle_t h,
	const nmdWorldCondition_t *&pConditions, uint16_t& nCount ) const
{
	pConditions = NULL; nCount = 0u;
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	const nmdWorldLocationCold_t& cold = m_LocationCold[ h.getIndex() ];
	nCount = cold.nConditionCount;
	if ( nCount ) {
		pConditions = m_LocationConditions.data() + cold.nFirstCondition;
	}
	return true;
}

bool NMDWorldDatabase::setLocationScores( nmdWorldHandle_t h, uint16_t nRenown, uint16_t nFear )
{
	if ( !isLocationHandle( h ) ) {
		return false;
	}
	nmdLocationData_t& data = m_Locations[ h.getIndex() ];
	if ( data.nPlayerRenownScore != nRenown || data.nPlayerFearScore != nFear ) {
		data.nPlayerRenownScore = nRenown;
		data.nPlayerFearScore = nFear;
		setDirty( NMD_WORLD_DIRTY_LOCATION, h.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::setLocationDataBatch( const nmdLocationData_t *pRecords, uint32_t nCount )
{
	if ( nCount != m_Locations.size() || ( nCount && pRecords == NULL ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		if ( pRecords[i].nId != m_Locations[i].nId ) {
			return false;
		}
	}
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		if ( memcmp( &m_Locations[i], &pRecords[i], sizeof( nmdLocationData_t ) ) != 0 ) {
			m_Locations[i] = pRecords[i];
			setDirty( NMD_WORLD_DIRTY_LOCATION, i );
		}
	}
	return true;
}

uint32_t NMDWorldDatabase::getLocationCount( void ) const { return static_cast<uint32_t>( m_Locations.size() ); }
const nmdLocationData_t *NMDWorldDatabase::getLocationHotData( void ) const { return m_Locations.data(); }
const nmdWorldLocationCold_t *NMDWorldDatabase::getLocationColdData( void ) const { return m_LocationCold.data(); }

nmdWorldHandle_t NMDWorldDatabase::setQuestDefinition( nmdWorldAtom_t pAtom,
	uint32_t nFlags, uint16_t nCurrentObjective,
	const nmdWorldObjectiveDesc_t *pObjectives, uint32_t nCount )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return { NMD_WORLD_HANDLE_INVALID };
	}
	const nmdWorldHandle_t invalid = { NMD_WORLD_HANDLE_INVALID };
	if ( pAtom == NULL || nCount > UINT8_MAX ||
		( nCount && pObjectives == NULL ) ||
		( nCurrentObjective != UINT16_MAX && nCurrentObjective >= nCount ) ||
		m_Quests.size() > NMD_WORLD_HANDLE_INDEX_MASK ) {
		return invalid;
	}
	const nmdWorldHandle_t old = getRecordHandle( pAtom, NMD_WORLD_RECORD_QUEST );
	if ( old.isValid() ) {
		return old;
	}
	// Record packing only. NMDQuestContentCompiler prevalidates the authored graph.
	uint32_t nTotalConditions = 0u;
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		if ( pObjectives[i].nConditionCount && pObjectives[i].pConditions == NULL ) {
			return invalid;
		}
		nTotalConditions += pObjectives[i].nConditionCount;
		if ( nTotalConditions > UINT16_MAX ) {
			return invalid;
		}
	}
	const uint32_t nOverflow = nCount > NMD_WORLD_QUEST_INLINE_COUNT ?
		nCount - NMD_WORLD_QUEST_INLINE_COUNT : 0u;
	if ( m_Quests.size() >= m_Quests.committedCapacity() ||
		static_cast<uint64_t>( m_Conditions.size() ) + nTotalConditions > m_Conditions.committedCapacity() ||
		static_cast<uint64_t>( m_OverflowObjectives.size() ) + nOverflow > m_OverflowObjectives.committedCapacity() ||
		!ensureLookupCapacity( static_cast<uint32_t>( m_LookupEntries.size() + 1u ) ) ) {
		return invalid;
	}
	const nmdWorldHandle_t hQuest = makeHandle( NMD_WORLD_RECORD_QUEST, static_cast<uint32_t>( m_Quests.size() ) );
	nmdWorldQuestHot_t quest = {};
	quest.nFlags = nFlags; quest.nCurrentObjective = nCurrentObjective;
	quest.nObjectiveCount = static_cast<uint8_t>( nCount );
	nmdWorldQuestMeta_t meta = {};
	meta.nFirstCondition = static_cast<uint32_t>( m_Conditions.size() );
	meta.nFirstOverflowObjective = static_cast<uint32_t>( m_OverflowObjectives.size() );
	meta.nTotalConditions = static_cast<uint16_t>( nTotalConditions );
	meta.nOverflowCount = static_cast<uint8_t>( nCount > NMD_WORLD_QUEST_INLINE_COUNT ?
		nCount - NMD_WORLD_QUEST_INLINE_COUNT : 0u );
	uint32_t nRelativeCondition = 0u;
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		const nmdWorldObjectiveDesc_t& desc = pObjectives[ i ];
		nmdWorldObjectiveHot_t objective = {};
		objective.nFirstCondition = static_cast<uint16_t>( nRelativeCondition );
		objective.nFlagsAndStatus = static_cast<uint16_t>( desc.nFlags |
			( static_cast<uint16_t>( desc.eStatus ) << NMD_WORLD_OBJECTIVE_STATUS_SHIFT ) );
		objective.nProgress = desc.nProgress;
		objective.nConditionCount = desc.nConditionCount;
		objective.nParentObjective = desc.nParentObjective;
		if ( i < NMD_WORLD_QUEST_INLINE_COUNT ) {
			quest.szObjectives[ i ] = objective;
		} else {
			m_OverflowObjectives.push_back( objective );
		}
		for ( uint32_t j = 0u; j < desc.nConditionCount; ++j ) {
			m_Conditions.push_back( desc.pConditions[ j ] );
		}
		nRelativeCondition += desc.nConditionCount;
	}
	m_Quests.push_back( quest ); m_QuestAtoms.push_back( pAtom ); m_QuestMeta.push_back( meta );
	m_QuestTreeLinks.push_back( { UINT16_MAX, UINT16_MAX, UINT16_MAX, NMD_QUEST_CHILD_POLICY_ALL, 0u } );
	insertRecord( NMDWorldIdIndex::getAtomKey( pAtom, NMD_WORLD_RECORD_QUEST ), hQuest );
	setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() );
	return hQuest;
}

const nmdWorldQuestHot_t *NMDWorldDatabase::getQuest( nmdWorldHandle_t hQuest ) const
{
	return hQuest.getType() == NMD_WORLD_RECORD_QUEST && getRecordExists( hQuest ) ?
		&m_Quests[ hQuest.getIndex() ] : NULL;
}

bool NMDWorldDatabase::setQuestHot( nmdWorldHandle_t hQuest, const nmdWorldQuestHot_t& hot )
{
	const nmdWorldQuestHot_t *pCurrent = getQuest( hQuest );
	if ( pCurrent == NULL || hot.nObjectiveCount != pCurrent->nObjectiveCount ||
		( hot.nCurrentObjective != UINT16_MAX && hot.nCurrentObjective >= hot.nObjectiveCount ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < hot.nObjectiveCount && i < NMD_WORLD_QUEST_INLINE_COUNT; ++i ) {
		const nmdWorldObjectiveHot_t& old = pCurrent->szObjectives[ i ];
		const nmdWorldObjectiveHot_t& next = hot.szObjectives[ i ];
		if ( next.nFirstCondition != old.nFirstCondition || next.nConditionCount != old.nConditionCount ||
			next.nParentObjective != old.nParentObjective ||
			( ( next.nFlagsAndStatus & NMD_WORLD_OBJECTIVE_STATUS_MASK ) >> NMD_WORLD_OBJECTIVE_STATUS_SHIFT ) >=
				NMD_QUEST_OBJECTIVE_STATUS_COUNT ) {
			return false;
		}
	}
	nmdWorldQuestHot_t& current = m_Quests[ hQuest.getIndex() ];
	if ( !sameQuest( current, hot ) ) {
		current = hot;
		setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() );
	}
	return true;
}

const nmdWorldObjectiveHot_t *NMDWorldDatabase::getObjective( nmdWorldHandle_t hQuest,
	uint32_t nObjective ) const
{
	const nmdWorldQuestHot_t *pQuest = getQuest( hQuest );
	if ( pQuest == NULL || nObjective >= pQuest->nObjectiveCount ) {
		return NULL;
	}
	if ( nObjective < NMD_WORLD_QUEST_INLINE_COUNT ) {
		return &pQuest->szObjectives[ nObjective ];
	}
	const nmdWorldQuestMeta_t& meta = m_QuestMeta[ hQuest.getIndex() ];
	return &m_OverflowObjectives[ meta.nFirstOverflowObjective + nObjective - NMD_WORLD_QUEST_INLINE_COUNT ];
}

nmdWorldObjectiveHot_t *NMDWorldDatabase::getMutableObjective( nmdWorldHandle_t hQuest,
	uint32_t nObjective )
{
	return const_cast<nmdWorldObjectiveHot_t *>( getObjective( hQuest, nObjective ) );
}

bool NMDWorldDatabase::setObjective( nmdWorldHandle_t hQuest, uint32_t nObjective,
	const nmdWorldObjectiveHot_t& next )
{
	nmdWorldObjectiveHot_t *pObjective = getMutableObjective( hQuest, nObjective );
	if ( pObjective == NULL || next.nFirstCondition != pObjective->nFirstCondition ||
		next.nConditionCount != pObjective->nConditionCount ||
		next.nParentObjective != pObjective->nParentObjective ||
		( ( next.nFlagsAndStatus & NMD_WORLD_OBJECTIVE_STATUS_MASK ) >> NMD_WORLD_OBJECTIVE_STATUS_SHIFT ) >=
			NMD_QUEST_OBJECTIVE_STATUS_COUNT ) {
		return false;
	}
	if ( !sameObjective( *pObjective, next ) ) {
		*pObjective = next;
		setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::setQuestFlags( nmdWorldHandle_t hQuest, uint32_t nFlags )
{
	if ( getQuest( hQuest ) == NULL ) {
		return false;
	}
	uint32_t& current = m_Quests[ hQuest.getIndex() ].nFlags;
	if ( current != nFlags ) { current = nFlags; setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() ); }
	return true;
}

bool NMDWorldDatabase::setCurrentObjective( nmdWorldHandle_t hQuest, uint16_t nObjective )
{
	const nmdWorldQuestHot_t *pQuest = getQuest( hQuest );
	if ( pQuest == NULL || ( nObjective != UINT16_MAX && nObjective >= pQuest->nObjectiveCount ) ) {
		return false;
	}
	uint16_t& current = m_Quests[ hQuest.getIndex() ].nCurrentObjective;
	if ( current != nObjective ) { current = nObjective; setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() ); }
	return true;
}

bool NMDWorldDatabase::setObjectiveStatus( nmdWorldHandle_t hQuest, uint32_t nObjective, uint8_t eStatus )
{
	nmdWorldObjectiveHot_t *pObjective = getMutableObjective( hQuest, nObjective );
	if ( pObjective == NULL || eStatus >= NMD_QUEST_OBJECTIVE_STATUS_COUNT ) {
		return false;
	}
	const uint16_t next = static_cast<uint16_t>(
		( pObjective->nFlagsAndStatus & ~NMD_WORLD_OBJECTIVE_STATUS_MASK ) |
		( static_cast<uint32_t>( eStatus ) << NMD_WORLD_OBJECTIVE_STATUS_SHIFT ) );
	if ( pObjective->nFlagsAndStatus != next ) {
		pObjective->nFlagsAndStatus = next;
		setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::setObjectiveProgress( nmdWorldHandle_t hQuest, uint32_t nObjective, uint16_t nProgress )
{
	nmdWorldObjectiveHot_t *pObjective = getMutableObjective( hQuest, nObjective );
	if ( pObjective == NULL ) {
		return false;
	}
	if ( pObjective->nProgress != nProgress ) {
		pObjective->nProgress = nProgress;
		setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::setObjectiveSatisfied( nmdWorldHandle_t hQuest, uint32_t nObjective, bool bSatisfied )
{
	nmdWorldObjectiveHot_t *pObjective = getMutableObjective( hQuest, nObjective );
	if ( pObjective == NULL ) {
		return false;
	}
	const uint16_t next = static_cast<uint16_t>(
		( pObjective->nFlagsAndStatus & ~NMD_WORLD_OBJECTIVE_SATISFIED ) |
		( bSatisfied ? NMD_WORLD_OBJECTIVE_SATISFIED : 0u ) );
	if ( pObjective->nFlagsAndStatus != next ) {
		pObjective->nFlagsAndStatus = next;
		setDirty( NMD_WORLD_DIRTY_QUEST, hQuest.getIndex() );
	}
	return true;
}

bool NMDWorldDatabase::getObjectiveConditions( nmdWorldHandle_t hQuest,
	uint32_t nObjective, const nmdWorldCondition_t *&pConditions, uint8_t& nCount ) const
{
	pConditions = NULL; nCount = 0u;
	const nmdWorldObjectiveHot_t *pObjective = getObjective( hQuest, nObjective );
	if ( pObjective == NULL ) {
		return false;
	}
	const nmdWorldQuestMeta_t& meta = m_QuestMeta[ hQuest.getIndex() ];
	nCount = pObjective->nConditionCount;
	if ( nCount ) {
		pConditions = m_Conditions.data() + meta.nFirstCondition + pObjective->nFirstCondition;
	}
	return true;
}

uint32_t NMDWorldDatabase::getQuestCount( void ) const { return static_cast<uint32_t>( m_Quests.size() ); }
uint32_t NMDWorldDatabase::getQuestConditionCount( void ) const { return static_cast<uint32_t>( m_Conditions.size() ); }
const nmdWorldQuestHot_t *NMDWorldDatabase::getQuestHotData( void ) const { return m_Quests.data(); }

bool NMDWorldDatabase::getQuestTreeLink( uint32_t nIndex, nmdWorldQuestTreeLink_t& link ) const
{
	if ( nIndex >= m_QuestTreeLinks.size() ) {
		return false;
	}
	link = m_QuestTreeLinks[nIndex];
	return true;
}

bool NMDWorldDatabase::setQuestTreeLink( uint32_t nIndex, const nmdWorldQuestTreeLink_t& link )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING || nIndex >= m_QuestTreeLinks.size() ) {
		return false;
	}
	m_QuestTreeLinks[nIndex] = link;
	return true;
}

nmdWorldHandle_t NMDWorldDatabase::setStoryDefinition( nmdWorldAtom_t pAtom, const nmdWorldStoryState_t& state )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return { NMD_WORLD_HANDLE_INVALID };
	}
	const nmdWorldHandle_t invalid = { NMD_WORLD_HANDLE_INVALID };
	if ( pAtom == NULL || m_Stories.size() > NMD_WORLD_HANDLE_INDEX_MASK ) {
		return invalid;
	}
	const nmdWorldHandle_t old = getRecordHandle( pAtom, NMD_WORLD_RECORD_STORY );
	if ( old.isValid() ) {
		return old;
	}
	if ( m_Stories.size() >= m_Stories.committedCapacity() ||
		!ensureLookupCapacity( static_cast<uint32_t>( m_LookupEntries.size() + 1u ) ) ) {
		return invalid;
	}
	const nmdWorldHandle_t hStory = makeHandle( NMD_WORLD_RECORD_STORY, static_cast<uint32_t>( m_Stories.size() ) );
	m_Stories.push_back( state ); m_StoryAtoms.push_back( pAtom );
	m_StoryTreeRanges.push_back( { 0u, 0u, UINT16_MAX } );
	insertRecord( NMDWorldIdIndex::getAtomKey( pAtom, NMD_WORLD_RECORD_STORY ), hStory );
	setDirty( NMD_WORLD_DIRTY_STORY, hStory.getIndex() );
	return hStory;
}

bool NMDWorldDatabase::getStory( nmdWorldHandle_t hStory, nmdWorldStoryState_t& state ) const
{
	if ( hStory.getType() != NMD_WORLD_RECORD_STORY || !getRecordExists( hStory ) ) {
		return false;
	}
	state = m_Stories[ hStory.getIndex() ]; return true;
}

bool NMDWorldDatabase::setStory( nmdWorldHandle_t hStory, const nmdWorldStoryState_t& state )
{
	if ( hStory.getType() != NMD_WORLD_RECORD_STORY || !getRecordExists( hStory ) ) {
		return false;
	}
	nmdWorldStoryState_t& current = m_Stories[ hStory.getIndex() ];
	if ( !sameStory( current, state ) ) {
		current = state;
		setDirty( NMD_WORLD_DIRTY_STORY, hStory.getIndex() );
	}
	return true;
}

uint32_t NMDWorldDatabase::getStoryCount( void ) const { return static_cast<uint32_t>( m_Stories.size() ); }
const nmdWorldStoryState_t *NMDWorldDatabase::getStoryHotData( void ) const { return m_Stories.data(); }

bool NMDWorldDatabase::getStoryTreeRange( nmdWorldHandle_t hStory,
	nmdWorldStoryTreeRange_t& range ) const
{
	if ( hStory.getType() != NMD_WORLD_RECORD_STORY || !getRecordExists( hStory ) ) {
		return false;
	}
	range = m_StoryTreeRanges[hStory.getIndex()];
	return true;
}

bool NMDWorldDatabase::setStoryTreeRange( nmdWorldHandle_t hStory,
	const nmdWorldStoryTreeRange_t& range )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ||
		hStory.getType() != NMD_WORLD_RECORD_STORY || !getRecordExists( hStory ) ||
		static_cast<uint64_t>( range.nFirstQuest ) + range.nQuestCount > m_Quests.size() ) {
		return false;
	}
	m_StoryTreeRanges[hStory.getIndex()] = range;
	return true;
}

bool NMDWorldDatabase::setFactDependency( nmdWorldFactId_t nId, nmdWorldHandle_t hOwner, uint16_t nItemIndex )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING || m_DependencyBuckets.empty() ) {
		return false;
	}
	if ( nId >= m_FactValues.size() || !getRecordExists( hOwner ) ) {
		return false;
	}
	const nmdWorldDependencyBuild_t key = { hOwner, nId, nItemIndex };
	const nmdWorldIdKey_t idKey = NMDWorldIdIndex::getDependencyKey( nId, hOwner.nValue, nItemIndex );
	uint32_t bucket = 0u;
	bool found = false;
	if ( !NMDWorldIdIndex::probe( m_DependencyBuckets.data(), m_DependencyBuckets.size(), idKey,
		[this]( uint32_t index, const nmdWorldIdKey_t& key ) -> bool {
			if ( index >= m_DependencyBuild.size() ) {
				return false;
			}
			const nmdWorldDependencyBuild_t& entry = m_DependencyBuild[index];
			return NMDWorldIdIndex::equals( key, NMDWorldIdIndex::getDependencyKey(
				entry.nFactId, entry.hOwner.nValue, entry.nItemIndex ) );
		}, bucket, found ) ) {
		return false;
	}
	if ( found ) {
		return true;
	}
	if ( m_DependencyBuild.size() >= m_DependencyBuild.committedCapacity() ) {
		return false;
	}
	m_DependencyBuild.push_back( key );
	m_DependencyBuckets[ bucket ] = m_DependencyBuild.size();
	m_bDependencyIndexDirty = true;
	return true;
}

bool NMDWorldDatabase::getDependencyBuildData( const nmdWorldDependencyBuild_t *&pData,
	uint32_t& nCount ) const
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	pData = m_DependencyBuild.data();
	nCount = m_DependencyBuild.size();
	return true;
}

bool NMDWorldDatabase::getDependencyCompileStorage( uint32_t *&pOffsets, uint32_t *&pCursor,
	nmdWorldDependency_t *&pDependencies, uint32_t& nOffsetCapacity,
	uint32_t& nDependencyCapacity )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	pOffsets = m_DependencyOffsets.data();
	pCursor = m_DependencyCursor.data();
	pDependencies = m_Dependencies.data();
	nOffsetCapacity = m_DependencyOffsets.committedCapacity();
	nDependencyCapacity = m_Dependencies.committedCapacity();
	return true;
}

bool NMDWorldDatabase::setCompiledDependencyCount( uint32_t nDependencies )
{
	const uint32_t offsets = m_FactValues.size() + 1u;
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ||
		nDependencies != m_DependencyBuild.size() ||
		offsets > m_DependencyOffsets.committedCapacity() ||
		nDependencies > m_Dependencies.committedCapacity() ) {
		return false;
	}
	// Compiler filled precommitted backing data; publish counts without zero-filling.
	m_DependencyOffsets.bind( m_DependencyOffsets.data(), m_DependencyOffsets.capacity(),
		offsets, m_DependencyOffsets.committedCapacity() );
	m_Dependencies.bind( m_Dependencies.data(), m_Dependencies.capacity(),
		nDependencies, m_Dependencies.committedCapacity() );
	m_bDependencyIndexDirty = false;
	return true;
}

bool NMDWorldDatabase::getFactDependencies( nmdWorldFactId_t nId,
	const nmdWorldDependency_t *&pData, uint32_t& nCount ) const
{
	pData = NULL; nCount = 0u;
	if ( nId >= m_FactValues.size() || m_bDependencyIndexDirty ||
		nId + 1u >= m_DependencyOffsets.size() ) {
		return false;
	}
	const uint32_t begin = m_DependencyOffsets[ nId ];
	nCount = m_DependencyOffsets[ nId + 1u ] - begin;
	if ( nCount ) {
		pData = m_Dependencies.data() + begin;
	}
	return true;
}

uint32_t NMDWorldDatabase::getFactDependencyCount( nmdWorldFactId_t nId ) const
{
	const nmdWorldDependency_t *pData = NULL; uint32_t nCount = 0u;
	return getFactDependencies( nId, pData, nCount ) ? nCount : 0u;
}

bool NMDWorldDatabase::setCalendarDefinition( const nmdCalendarDefinition_t& def )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	// Support calendar-only bootstrap before a world manifest is loaded.
	// Content registration still requires explicit runtime capacity planning.
	if ( m_ArenaBlock.pBase == NULL ) {
		const nmdWorldDatabaseCapacities_t empty = {};
		if ( !setCapacities( empty ) ) {
			return false;
		}
	}
	const bool changed = !m_bHasCalendar || !sameCalendarDefinition( m_CalendarCold[0], def );
	m_CalendarCold[0] = def; m_bHasCalendar = true;
	if ( changed ) {
		setDirty( NMD_WORLD_DIRTY_CALENDAR, 0u );
	}
	return true;
}

bool NMDWorldDatabase::getCalendarDefinition( nmdCalendarDefinition_t& def ) const
{
	if ( !m_bHasCalendar ) {
		return false;
	}
	def = m_CalendarCold[0]; return true;
}

bool NMDWorldDatabase::setCalendarHot( const nmdCalendarHot_t& hot )
{
	if ( !m_bHasCalendar ) {
		return false;
	}
	if ( !sameCalendar( m_Calendar[0], hot ) ) {
		m_Calendar[0] = hot;
		setDirty( NMD_WORLD_DIRTY_CALENDAR, 0u );
	}
	return true;
}

bool NMDWorldDatabase::getCalendarHot( nmdCalendarHot_t& hot ) const
{
	if ( !m_bHasCalendar ) {
		return false;
	}
	hot = m_Calendar[0]; return true;
}

uint64_t NMDWorldDatabase::getWorldSeconds( void ) const { return m_Calendar.empty() ? 0u : m_Calendar[0].nWorldSeconds; }

bool NMDWorldDatabase::setWorldSeconds( uint64_t nSeconds )
{
	if ( !m_bHasCalendar ) {
		return false;
	}
	if ( m_Calendar[0].nWorldSeconds != nSeconds ) {
		m_Calendar[0].nWorldSeconds = nSeconds;
		++m_Calendar[0].nRevision;
		setDirty( NMD_WORLD_DIRTY_CALENDAR, 0u );
	}
	return true;
}


bool NMDWorldDatabase::setClimateRegion( nmdClimateRegionId_t nId,
	const nmdClimateSeasonalCold_t& seasonal, const nmdClimateGeographyCold_t& geo,
	const nmdWeatherHot_t& weather, const nmdWaterCycleHot_t& water )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	if ( nId == NMD_CLIMATE_REGION_INVALID_ID ) {
		return false;
	}
	uint32_t nIndex;
	if ( m_CompiledRevisions.empty() ||
		m_CompiledRevisions[0].nClimateTopology == UINT32_MAX ) {
		return false;
	}
	if ( getClimateIndex( nId, nIndex ) ) {
		const bool changedCold = !sameSeasonal( m_ClimateSeasonal[nIndex], seasonal ) ||
			!sameGeography( m_ClimateGeography[nIndex], geo );
		const nmdWeatherHot_t beforeWeather = m_RegionWeather[nIndex];
		const nmdWaterCycleHot_t beforeWater = m_WaterCycles[nIndex];
		const bool changedWeather = !sameWeather( beforeWeather, weather );
		const bool changedWater = !sameWater( beforeWater, water );
		if ( !changedCold && !changedWeather && !changedWater ) {
			return true;
		}
		if ( changedCold ) {
			++m_CompiledRevisions[0].nClimateTopology;
		}
		m_ClimateSeasonal[ nIndex ] = seasonal;
		m_ClimateGeography[ nIndex ] = geo;
		m_RegionWeather[ nIndex ] = weather;
		m_WaterCycles[ nIndex ] = water;
		if ( changedCold || changedWeather ) {
			setDirty( NMD_WORLD_DIRTY_WEATHER, nIndex );
		}
		if ( changedCold || changedWater ) {
			setDirty( NMD_WORLD_DIRTY_WATER, nIndex );
		}
		return true;
	}
	if ( m_RegionIds.size() >= m_RegionIds.committedCapacity() ||
		m_RegionIds.size() >= NMD_CLIMATE_MAX_REGION_COUNT ||
		!ensureLookupCapacity( m_LookupEntries.size() + 1u ) ) {
		return false;
	}
	if ( m_CompiledRevisions[0].nWeatherTimeline == UINT32_MAX ) {
		return false;
	}
	++m_CompiledRevisions[0].nClimateTopology;
	++m_CompiledRevisions[0].nWeatherTimeline; // New dense region changes timeline ranges.
	nIndex = m_RegionIds.size();
	m_RegionIds.push_back( nId ); m_ClimateSeasonal.push_back( seasonal );
	m_ClimateGeography.push_back( geo ); m_RegionWeather.push_back( weather );
	m_WaterCycles.push_back( water );
	m_ClimateLinks.push_back( { 0.0f, 0.0f,
		NMD_CLIMATE_REGION_INVALID_ID, NMD_CLIMATE_REGION_INVALID_ID } );
	insertRecord( NMDWorldIdIndex::getNumericKey( nId, NMD_WORLD_RECORD_REGION ),
		makeHandle( NMD_WORLD_RECORD_REGION, nIndex ) );
	setDirty( NMD_WORLD_DIRTY_WEATHER, nIndex );
	setDirty( NMD_WORLD_DIRTY_WATER, nIndex );
	return true;
}

bool NMDWorldDatabase::getClimateIndex( nmdClimateRegionId_t nId, uint32_t& nIndex ) const
{
	if ( nId == NMD_CLIMATE_REGION_INVALID_ID ) {
		return false;
	}
	nmdWorldHandle_t h = { NMD_WORLD_HANDLE_INVALID };
	if ( !findIdentity( NMDWorldIdIndex::getNumericKey( nId, NMD_WORLD_RECORD_REGION ), h ) ) {
		return false;
	}
	nIndex = h.getIndex();
	return true;
}

bool NMDWorldDatabase::getClimateRegionId( uint32_t nIndex, nmdClimateRegionId_t& nId ) const
{
	if ( nIndex >= m_RegionIds.size() ) {
		return false;
	}
	nId = m_RegionIds[ nIndex ]; return true;
}

bool NMDWorldDatabase::setClimateLinks( uint32_t nDenseRegionIndex, const nmdClimateLinks_t& links )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	if ( nDenseRegionIndex >= m_RegionIds.size() ) {
		return false;
	}
	m_CompiledRevisions[0].nClimateCompiled = 0u;
	m_ClimateLinks[ nDenseRegionIndex ] = links;
	return true;
}

bool NMDWorldDatabase::getClimateLinks( uint32_t nDenseRegionIndex, nmdClimateLinks_t& links ) const
{
	if ( nDenseRegionIndex >= m_ClimateLinks.size() ) {
		return false;
	}
	links = m_ClimateLinks[ nDenseRegionIndex ]; return true;
}

uint32_t NMDWorldDatabase::getClimateTopologyRevision( void ) const
{
	return m_CompiledRevisions.empty() ? 0u : m_CompiledRevisions[0].nClimateTopology;
}

uint32_t NMDWorldDatabase::getClimateLinksRevision( void ) const
{
	return m_CompiledRevisions.empty() ? 0u : m_CompiledRevisions[0].nClimateCompiled;
}

bool NMDWorldDatabase::setClimateLinksRevision( uint32_t nTopologyRevision )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	if ( m_CompiledRevisions.empty() || nTopologyRevision == 0u ||
		nTopologyRevision != getClimateTopologyRevision() || m_ClimateLinks.size() != m_RegionIds.size() ) {
		return false;
	}
	m_CompiledRevisions[0].nClimateCompiled = nTopologyRevision;
	return true;
}

bool NMDWorldDatabase::getClimateSeasonal( nmdClimateRegionId_t nId, nmdClimateSeasonalCold_t& seasonal ) const
{
	uint32_t index;
	if ( !getClimateIndex( nId, index ) ) {
		return false;
	}
	seasonal = m_ClimateSeasonal[ index ]; return true;
}

bool NMDWorldDatabase::getClimateGeography( nmdClimateRegionId_t nId, nmdClimateGeographyCold_t& geography ) const
{
	uint32_t index;
	if ( !getClimateIndex( nId, index ) ) {
		return false;
	}
	geography = m_ClimateGeography[ index ]; return true;
}

bool NMDWorldDatabase::getRegionWeather( nmdClimateRegionId_t nId, nmdWeatherHot_t& weather ) const
{
	uint32_t index;
	if ( !getClimateIndex( nId, index ) ) {
		return false;
	}
	weather = m_RegionWeather[ index ]; return true;
}

bool NMDWorldDatabase::setRegionWeather( nmdClimateRegionId_t nId, const nmdWeatherHot_t& weather )
{
	uint32_t index;
	return getClimateIndex( nId, index ) && setRegionWeatherByIndex( index, weather );
}

bool NMDWorldDatabase::setRegionWeatherByIndex( uint32_t index, const nmdWeatherHot_t& weather )
{
	if ( index >= m_RegionWeather.size() ) {
		return false;
	}
	if ( !sameWeather( m_RegionWeather[index], weather ) ) {
		m_RegionWeather[index] = weather;
		setDirty( NMD_WORLD_DIRTY_WEATHER, index );
	}
	return true;
}

bool NMDWorldDatabase::getRegionWaterCycle( nmdClimateRegionId_t nId, nmdWaterCycleHot_t& water ) const
{
	uint32_t index;
	if ( !getClimateIndex( nId, index ) ) {
		return false;
	}
	water = m_WaterCycles[ index ]; return true;
}

bool NMDWorldDatabase::setRegionWaterCycle( nmdClimateRegionId_t nId, const nmdWaterCycleHot_t& water )
{
	uint32_t index;
	return getClimateIndex( nId, index ) && setRegionWaterCycleByIndex( index, water );
}

bool NMDWorldDatabase::setRegionWaterCycleByIndex( uint32_t index, const nmdWaterCycleHot_t& water )
{
	if ( index >= m_WaterCycles.size() ) {
		return false;
	}
	if ( !sameWater( m_WaterCycles[index], water ) ) {
		m_WaterCycles[index] = water;
		setDirty( NMD_WORLD_DIRTY_WATER, index );
	}
	return true;
}

bool NMDWorldDatabase::getLocationWeather( nmdWorldHandle_t h, nmdWeatherHot_t& weather ) const
{
	if ( !isLocationHandle( h ) || m_LocationCold[ h.getIndex() ].nRegionId > UINT16_MAX ) {
		return false;
	}
	return getRegionWeather( static_cast<nmdClimateRegionId_t>( m_LocationCold[ h.getIndex() ].nRegionId ), weather );
}

uint32_t NMDWorldDatabase::getClimateRegionCount( void ) const { return static_cast<uint32_t>( m_RegionIds.size() ); }
const nmdWeatherHot_t *NMDWorldDatabase::getRegionWeatherHotData( void ) const { return m_RegionWeather.data(); }
const nmdWaterCycleHot_t *NMDWorldDatabase::getRegionWaterHotData( void ) const { return m_WaterCycles.data(); }
const nmdClimateSeasonalCold_t *NMDWorldDatabase::getClimateSeasonalColdData( void ) const { return m_ClimateSeasonal.data(); }
const nmdClimateGeographyCold_t *NMDWorldDatabase::getClimateGeographyColdData( void ) const { return m_ClimateGeography.data(); }

bool NMDWorldDatabase::setWorldTimedEvent( const nmdWorldTimedEvent_t& event )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	const nmdWorldIdKey_t key = NMDWorldIdIndex::getNumericKey( event.nEventId, NMD_WORLD_RECORD_TIMED_EVENT, true );
	nmdWorldHandle_t h = { NMD_WORLD_HANDLE_INVALID };
	if ( findIdentity( key, h ) ) {
		m_TimedEvents[h.getIndex()] = event;
		return true;
	}
	if ( !ensureLookupCapacity( m_LookupEntries.size() + 1u ) ||
		!m_TimedEvents.push_back( event ) ) {
		return false;
	}
	return insertRecord( key, makeHandle( NMD_WORLD_RECORD_TIMED_EVENT, m_TimedEvents.size() - 1u ) );
}

bool NMDWorldDatabase::getWorldTimedEvent( uint32_t nIndex, nmdWorldTimedEvent_t& event ) const
{
	if ( nIndex >= m_TimedEvents.size() ) {
		return false;
	}
	event = m_TimedEvents[ nIndex ]; return true;
}

uint32_t NMDWorldDatabase::getWorldTimedEventCount( void ) const { return static_cast<uint32_t>( m_TimedEvents.size() ); }

bool NMDWorldDatabase::setWeatherTimelineEvent( const nmdWeatherTimelineEvent_t& event )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	if ( m_CompiledRevisions.empty() || m_CompiledRevisions[0].nWeatherTimeline == UINT32_MAX ) {
		return false;
	}
	const nmdWorldIdKey_t key = NMDWorldIdIndex::getNumericKey( event.nEventId,
		NMD_WORLD_RECORD_WEATHER_EVENT, true );
	nmdWorldHandle_t h = { NMD_WORLD_HANDLE_INVALID };
	if ( findIdentity( key, h ) ) {
		m_WeatherTimelineEvents[h.getIndex()] = event;
	} else {
		if ( !ensureLookupCapacity( m_LookupEntries.size() + 1u ) ||
			!m_WeatherTimelineEvents.push_back( event ) ) {
			return false;
		}
		if ( !insertRecord( key, makeHandle( NMD_WORLD_RECORD_WEATHER_EVENT,
			m_WeatherTimelineEvents.size() - 1u ) ) ) {
			return false;
		}
	}
	++m_CompiledRevisions[0].nWeatherTimeline;
	return true;
}

bool NMDWorldDatabase::getWeatherTimelineEvent( uint32_t nIndex, nmdWeatherTimelineEvent_t& event ) const
{
	if ( nIndex >= m_WeatherTimelineEvents.size() ) {
		return false;
	}
	event = m_WeatherTimelineEvents[ nIndex ]; return true;
}

uint32_t NMDWorldDatabase::getWeatherTimelineEventCount( void ) const
{
	return static_cast<uint32_t>( m_WeatherTimelineEvents.size() );
}

bool NMDWorldDatabase::getWeatherCompileStorage( const nmdWeatherTimelineEvent_t *&pEvents,
	nmdWeatherTimelineIndexEntry_t *&pEntries, nmdWeatherTimelineRange_t *&pRanges,
	uint32_t& nEventCapacity, uint32_t& nRegionCapacity )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING ) {
		return false;
	}
	pEvents = m_WeatherTimelineEvents.data();
	pEntries = m_WeatherIndexEntries.data();
	pRanges = m_WeatherIndexRanges.data();
	nEventCapacity = m_WeatherIndexEntries.committedCapacity();
	nRegionCapacity = m_WeatherIndexRanges.committedCapacity();
	return true;
}

bool NMDWorldDatabase::setCompiledWeatherIndexCounts( uint32_t nEventCount, uint32_t nRegionCount )
{
	if ( m_eArenaMode != NMD_WORLD_ARENA_AUTHORING || m_CompiledRevisions.empty() ||
		nEventCount != m_WeatherTimelineEvents.size() || nRegionCount != m_RegionIds.size() ||
		nEventCount > m_WeatherIndexEntries.committedCapacity() ||
		nRegionCount > m_WeatherIndexRanges.committedCapacity() ) {
		return false;
	}
	m_WeatherIndexEntries.bind( m_WeatherIndexEntries.data(), m_WeatherIndexEntries.capacity(),
		nEventCount, m_WeatherIndexEntries.committedCapacity() );
	m_WeatherIndexRanges.bind( m_WeatherIndexRanges.data(), m_WeatherIndexRanges.capacity(),
		nRegionCount, m_WeatherIndexRanges.committedCapacity() );
	m_CompiledRevisions[0].nWeatherCompiled = m_CompiledRevisions[0].nWeatherTimeline;
	return true;
}

bool NMDWorldDatabase::getWeatherTimelineIndexRange( uint32_t nDenseRegionIndex,
	const nmdWeatherTimelineIndexEntry_t *&pEntries, uint32_t& nCount ) const
{
	pEntries = NULL; nCount = 0u;
	if ( nDenseRegionIndex >= m_RegionIds.size() ||
		getWeatherTimelineIndexRevision() == 0u ||
		getWeatherTimelineIndexRevision() != getWeatherTimelineRevision() ||
		m_WeatherIndexRanges.size() != m_RegionIds.size() ||
		m_WeatherIndexEntries.size() != m_WeatherTimelineEvents.size() ) {
		return false;
	}
	const nmdWeatherTimelineRange_t& range = m_WeatherIndexRanges[nDenseRegionIndex];
	if ( static_cast<uint64_t>( range.nFirst ) + range.nCount > m_WeatherIndexEntries.size() ) {
		return false;
	}
	nCount = range.nCount;
	if ( nCount != 0u ) {
		pEntries = m_WeatherIndexEntries.data() + range.nFirst;
	}
	return true;
}

uint32_t NMDWorldDatabase::getWeatherTimelineRevision( void ) const
{
	return m_CompiledRevisions.empty() ? 0u : m_CompiledRevisions[0].nWeatherTimeline;
}

uint32_t NMDWorldDatabase::getWeatherTimelineIndexRevision( void ) const
{
	return m_CompiledRevisions.empty() ? 0u : m_CompiledRevisions[0].nWeatherCompiled;
}

bool NMDWorldDatabase::setSimulationStateBatch( const nmdCalendarHot_t& calendar,
	const nmdWeatherHot_t *pWeather, const nmdWaterCycleHot_t *pWater,
	uint32_t nRegionCount )
{
	if ( !m_bHasCalendar || nRegionCount != m_RegionIds.size() ||
		( nRegionCount && ( pWeather == NULL || pWater == NULL ) ) ) {
		return false;
	}
	setCalendarHot( calendar );
	for ( uint32_t i = 0u; i < nRegionCount; ++i ) {
		setRegionWeatherByIndex( i, pWeather[i] );
		setRegionWaterCycleByIndex( i, pWater[i] );
	}
	return true;
}

const nmdClimateRegionId_t *NMDWorldDatabase::getClimateRegionIds( void ) const
{
	return m_RegionIds.data();
}

const nmdClimateLinks_t *NMDWorldDatabase::getClimateLinksData( void ) const
{
	return m_ClimateLinks.data();
}

const nmdWorldTimedEvent_t *NMDWorldDatabase::getWorldTimedEventData( void ) const
{
	return m_TimedEvents.data();
}

const nmdWeatherTimelineEvent_t *NMDWorldDatabase::getWeatherTimelineEventData( void ) const
{
	return m_WeatherTimelineEvents.data();
}
