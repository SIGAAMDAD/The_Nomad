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

// Compile-only ABI and public blackboard interface checks.
#include "NMDWorldDatabase.h"
#include "NMDWeatherTimelineCompiler.h"
#include "NMDWorldDependencyCompiler.h"
#include "NMDWorldQuestLayoutCandidate.h"
#include "../Story/NMDWorldConditions.h"
#include "../Simulation/NMDWorldSimulation.h"
#include <type_traits>

#define NMD_WORLD_POD_CHECK( Type ) \
	static_assert( sizeof( Type ) <= 64u, #Type " exceeds one cache line" ); \
	static_assert( std::is_trivially_copyable<Type>::value, #Type " is not trivially copyable" )

NMD_WORLD_POD_CHECK( nmdWorldCompiledFactHandle_t );
NMD_WORLD_POD_CHECK( nmdWorldDirtyRange_t );
NMD_WORLD_POD_CHECK( nmdWorldDirtyRevisions_t );
NMD_WORLD_POD_CHECK( nmdWorldValue_t );
NMD_WORLD_POD_CHECK( nmdWorldCondition_t );
NMD_WORLD_POD_CHECK( nmdWorldObjectiveHot_t );
NMD_WORLD_POD_CHECK( nmdWorldQuestHot_t );
NMD_WORLD_POD_CHECK( nmdLocationData_t );
NMD_WORLD_POD_CHECK( nmdWorldLocationCold_t );
NMD_WORLD_POD_CHECK( nmdWorldStoryState_t );
NMD_WORLD_POD_CHECK( nmdCalendarHot_t );
NMD_WORLD_POD_CHECK( nmdCalendarDefinition_t );
NMD_WORLD_POD_CHECK( nmdClimateSeasonalCold_t );
NMD_WORLD_POD_CHECK( nmdClimateGeographyCold_t );
NMD_WORLD_POD_CHECK( nmdWeatherHot_t );
NMD_WORLD_POD_CHECK( nmdWaterCycleHot_t );
NMD_WORLD_POD_CHECK( nmdWorldDependency_t );
NMD_WORLD_POD_CHECK( nmdWorldDatabaseCapacities_t );
NMD_WORLD_POD_CHECK( nmdWorldArenaSegmentInfo_t );
NMD_WORLD_POD_CHECK( nmdWorldArenaBlock_t );
NMD_WORLD_POD_CHECK( nmdWorldQuestHeaderCandidate_t );
NMD_WORLD_POD_CHECK( nmdClimateLinks_t );
NMD_WORLD_POD_CHECK( nmdWeatherTimelineIndexEntry_t );
NMD_WORLD_POD_CHECK( nmdWeatherTimelineRange_t );
NMD_WORLD_POD_CHECK( nmdWorldCompiledIndexRevisions_t );
static_assert( sizeof( nmdWorldArenaArray_t<nmdWorldQuestHot_t> ) <= 64u,
	"Typed arena descriptors exceed one cache line" );
NMD_WORLD_POD_CHECK( nmdWorldTimedEvent_t );
NMD_WORLD_POD_CHECK( nmdWeatherTimelineEvent_t );

static_assert( !std::is_copy_constructible<NMDWorldDatabase>::value,
	"World database must remain a single authoritative owner" );
static_assert( alignof( nmdWorldQuestHot_t ) >= 64u, "Quest hot alignment degraded" );
static_assert( alignof( nmdWeatherHot_t ) >= 64u, "Weather hot alignment degraded" );
static_assert( alignof( nmdWaterCycleHot_t ) >= 64u, "Hydrology hot alignment degraded" );

// Do not run this function; it deliberately only typechecks the external API.
void nmdWorldDatabaseCompileOnlySurface( NMDWorldDatabase& db, NMDWorldSimulation& sim,
	nmdWorldAtom_t atom, nmdWorldHandle_t h, nmdLocationId_t locationId, nmdClimateRegionId_t regionId )
{
	nmdWorldArenaSegmentInfo_t segment = {};
	db.getArenaSegmentInfo( NMD_WORLD_ARENA_WEATHER_HOT, segment );
	(void)db.getArenaBase();
	(void)db.getArenaSizeBytes();
	(void)db.getArenaReservedBytes();
	(void)db.getArenaCommittedBytes();
	(void)db.getArenaLiveBytes();
	(void)db.getArenaVirtualCommitSupported();
	nmdWorldDatabaseCapacities_t commits = {};
	db.getCommittedCapacities( commits );
	(void)db.setCommittedCapacities( commits );
	(void)nmdWorldQuestCurrentPayloadBytes( 10u, 0u );
	(void)nmdWorldQuestCandidatePayloadBytes( 10u, 30u );
	nmdWorldQuestHot_t sourceQuest = {};
	nmdWorldQuestHeaderCandidate_t candidate = {};
	nmdWorldObjectiveHot_t candidateObjectives[ NMD_WORLD_QUEST_INLINE_COUNT ] = {};
	(void)nmdWorldBuildQuestCandidate( sourceQuest, NULL, 0u, 0u,
		candidate, candidateObjectives, NMD_WORLD_QUEST_INLINE_COUNT );
	(void)db.getArenaGeneration();
	const nmdWorldArenaMode_t arenaMode = db.getArenaMode();
	(void)db.setArenaMode( arenaMode );
	uint32_t wordCount = 0u, revision = 0u;
	uint64_t dirty[2] = {};
	(void)db.getDirtyWords( NMD_WORLD_DIRTY_FACT, NMD_WORLD_DIRTY_SAVE,
		dirty, 2u, wordCount, revision );
	(void)db.getDirtyRevision( NMD_WORLD_DIRTY_QUEST, NMD_WORLD_DIRTY_CHANGES );
	(void)db.setDirtyAcknowledged( NMD_WORLD_DIRTY_FACT, NMD_WORLD_DIRTY_SAVE,
		revision, dirty, wordCount );
	(void)db.setDirtyBaseline( NMD_WORLD_DIRTY_SAVE );
	(void)db.setDirtyResync( NMD_WORLD_DIRTY_CHANGES );
	nmdWorldFactId_t factId = db.setFactDefinition( atom, NMD_WORLD_FACT_INTEGER,
		nmdWorldValue_t::fromInteger( 0 ) );
	int32_t value = 0;
	db.getFactInteger( factId, value );
	db.setFactInteger( factId, 1 );
	nmdWorldCompiledFactHandle_t compiledFact = {};
	db.getCompiledFactHandle( factId, NMD_WORLD_FACT_INTEGER, compiledFact );
	nmdWorldValue_t compiledValue = {};
	db.getCompiledFactValue( compiledFact, compiledValue );
	db.setCompiledFactValue( compiledFact, compiledValue );
	(void)db.getFactDefinitionGeneration();
	nmdLocationData_t location = {};
	db.getLocationData( h, location );
	db.setLocationData( h, location );
	nmdLocationStatus_t status;
	db.getLocationStatus( db.getLocationHandle( locationId ), status );
	db.setLocationStatus( h, NMD_LOCATION_STATUS_VISITED );
	nmdCalendarHot_t clock = {};
	db.getCalendarHot( clock );
	db.setCalendarHot( clock );
	nmdWeatherHot_t weather = {};
	db.getRegionWeather( regionId, weather );
	db.setRegionWeather( regionId, weather );
	db.setRegionWeatherByIndex( 0u, weather );
	nmdWaterCycleHot_t water = {};
	db.getRegionWaterCycle( regionId, water );
	db.setRegionWaterCycle( regionId, water );
	db.setRegionWaterCycleByIndex( 0u, water );
	nmdClimateLinks_t links = {};
	db.getClimateLinks( 0u, links );
	db.setClimateLinks( 0u, links );
	db.setClimateLinksRevision( db.getClimateTopologyRevision() );
	(void)db.getClimateLinksRevision();
	const nmdWeatherTimelineIndexEntry_t *pWeatherIndices = NULL;
	uint32_t nWeatherIndices = 0u;
	NMDWeatherTimelineCompiler::rebuild( db );
	NMDWorldDependencyCompiler::rebuild( db );
	db.getWeatherTimelineIndexRange( 0u, pWeatherIndices, nWeatherIndices );
	(void)db.getWeatherTimelineIndexRevision();
	(void)db.getWeatherTimelineRevision();
	sim.buildWeatherTimelineIndex();
	sim.advanceWorldSeconds( 1u );
}
