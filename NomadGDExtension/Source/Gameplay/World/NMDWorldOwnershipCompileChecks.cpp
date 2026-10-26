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
#include "NMDWorldIdIndex.h"
#include "NMDWorldArenaLayout.h"
#include "NMDWorldContentCompiler.h"
#include "NMDWorldDependencyCompiler.h"
#include "NMDWeatherTimelineCompiler.h"
#include "NMDClimateGraphCompiler.h"
#include "../Story/NMDQuestContentCompiler.h"
#include <type_traits>

static_assert( sizeof( nmdWorldHandle_t ) == 4u, "R00: handle ABI changed" );
static_assert( sizeof( nmdWorldIdIndexEntry_t ) == 8u, "R01: index entry grew" );
static_assert( sizeof( nmdLocationData_t ) == 8u, "R00: location hot ABI changed" );
static_assert( sizeof( nmdWorldObjectiveHot_t ) == 8u, "R00: objective hot ABI changed" );
static_assert( sizeof( nmdWorldQuestHot_t ) == 64u, "R00: quest hot ABI changed" );
static_assert( sizeof( nmdWeatherHot_t ) == 64u, "R00: weather hot ABI changed" );
static_assert( sizeof( nmdWaterCycleHot_t ) == 64u, "R00: water hot ABI changed" );
static_assert( NMD_WORLD_RECORD_FACT == 1 && NMD_WORLD_RECORD_LOCATION == 2 &&
	NMD_WORLD_RECORD_QUEST == 3 && NMD_WORLD_RECORD_STORY == 4, "R00: original type IDs changed" );
static_assert( NMD_WORLD_RECORD_REGION == 5 && NMD_WORLD_RECORD_TIMED_EVENT == 6 &&
	NMD_WORLD_RECORD_WEATHER_EVENT == 7, "R01: numeric ID namespaces changed" );
static_assert( std::is_trivially_copyable<nmdWorldIdKey_t>::value,
	"R01: identity keys must remain trivial" );
static_assert( !std::is_copy_constructible<NMDWorldDatabase>::value,
	"R00: database must exclusively own the arena" );

void nmdWorldOwnershipCompileCheck( NMDWorldDatabase& db, nmdWorldAtom_t atom )
{
	nmdWorldHandle_t h = db.getRecordHandle( atom, NMD_WORLD_RECORD_QUEST );
	nmdWorldIdKey_t key = NMDWorldIdIndex::getAtomKey( atom, NMD_WORLD_RECORD_QUEST );
	(void)NMDWorldIdIndex::hash( key );
	(void)NMDWorldContentCompiler::registerFact( db, atom, NMD_WORLD_FACT_BOOLEAN,
		nmdWorldValue_t::fromBoolean( false ) );
	(void)NMDWorldDependencyCompiler::rebuild( db );
	(void)NMDWeatherTimelineCompiler::rebuild( db );
	(void)NMDClimateGraphCompiler::build( db );
	(void)NMDWorldContentCompiler::seal( db );
	(void)h;
}

// R04-R09: storage, codec, quest traversal, and runtime authority are independent.
#include "NMDWorldSnapshotCodec.h"
#include "NMDWorldFactChangeDispatcher.h"
#include "../Story/NMDQuestTree.h"
#include "../Story/NMDQuestSystem.h"
#include "../NMDWorldStateHost.h"
#include <utility>

static_assert( sizeof( nmdWorldQuestTreeLink_t ) == 8u, "R05: quest links must remain 8 bytes" );
static_assert( sizeof( nmdWorldStoryTreeRange_t ) == 8u, "R05: story ranges must remain 8 bytes" );
static_assert( std::is_trivially_copyable<nmdWorldQuestTreeLink_t>::value,
	"R05: topology must remain arena-resident POD" );
static_assert( std::is_same<decltype( std::declval<NMDWorldDatabase&>().getCalendarHot(
	std::declval<nmdCalendarHot_t&>() ) ), bool>::value,
	"R07: simulation must access the clock through a typed getter" );
static_assert( std::is_same<decltype( std::declval<NMDWorldDatabase&>().setRegionWeatherByIndex(
	0u, std::declval<const nmdWeatherHot_t&>() ) ), bool>::value,
	"R07: hot weather publication must use a setter" );

void nmdWorldFinalOwnershipCompileCheck( NMDWorldDatabase& database,
	NMDQuestSystem& quests, nmdWorldHandle_t story, nmdWorldHandle_t quest )
{
	const NMDQuestTreeView tree( database, story );
	nmdWorldStoryTreeRange_t range = {};
	(void)tree.getRange( range );
	(void)quests.reevaluateQuest( quest );
	nmdSimulationSnapshotHeader_t header = {};
	(void)NMDWorldSnapshotCodec::writeSimulation( database, header,
		NULL, NULL, NULL, 0u );
	(void)NMDWorldFactChangeDispatcher::dispatch( database, 0u, &quests, NULL );
	(void)sizeof( NMDWorldStateHost );
}
