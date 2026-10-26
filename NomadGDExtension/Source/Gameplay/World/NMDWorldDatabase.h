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

#ifndef __NMD_WORLD_DATABASE_H__
#define __NMD_WORLD_DATABASE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../Story/NMDWorldTypes.h"
#include "../Simulation/NMDWorldSimulationTypes.h"
#include "NMDWorldArena.h"
#include "NMDWorldArenaBackend.h"
#include "NMDWorldIdIndex.h"


#define NMD_WORLD_HANDLE_INVALID 0xFFFFFFFFu
#define NMD_WORLD_HANDLE_INDEX_MASK 0x00FFFFFFu
#define NMD_WORLD_HANDLE_TYPE_SHIFT 24u
#define NMD_WORLD_INDEX_INITIAL_BUCKETS 16u
#define NMD_WORLD_QUEST_INLINE_COUNT 7u
#define NMD_WORLD_OBJECTIVE_NO_PARENT 0xFFu
#define NMD_WORLD_OBJECTIVE_FLAGS_MASK 0x03FFu
#define NMD_WORLD_OBJECTIVE_STATUS_MASK 0x1C00u
#define NMD_WORLD_OBJECTIVE_STATUS_SHIFT 10u
#define NMD_WORLD_OBJECTIVE_SATISFIED 0x2000u

// Identity-only atom: its owner must outlive the database. No string allocations here.
typedef const void *nmdWorldAtom_t;

typedef enum nmdWorldRecordType_e : uint8_t
{
	NMD_WORLD_RECORD_INVALID = 0,
	NMD_WORLD_RECORD_FACT = 1,
	NMD_WORLD_RECORD_LOCATION = 2,
	NMD_WORLD_RECORD_QUEST = 3,
	NMD_WORLD_RECORD_STORY = 4,
	NMD_WORLD_RECORD_REGION = 5,
	NMD_WORLD_RECORD_TIMED_EVENT = 6,
	NMD_WORLD_RECORD_WEATHER_EVENT = 7
} nmdWorldRecordType_t;

typedef enum nmdWorldFactType_e : uint8_t
{
	NMD_WORLD_FACT_FLOAT = 1,
	NMD_WORLD_FACT_INTEGER = 2,
	NMD_WORLD_FACT_BOOLEAN = 3
} nmdWorldFactType_t;

typedef enum nmdWorldComparison_e : uint8_t
{
	NMD_WORLD_COMP_EQ = 0,
	NMD_WORLD_COMP_NEQ,
	NMD_WORLD_COMP_GT,
	NMD_WORLD_COMP_GTE,
	NMD_WORLD_COMP_LT,
	NMD_WORLD_COMP_LTE
} nmdWorldComparison_t;

typedef struct nmdWorldHandle_s
{
	uint32_t nValue;

	bool isValid( void ) const { return nValue != NMD_WORLD_HANDLE_INVALID; }
	nmdWorldRecordType_t getType( void ) const
	{
		return static_cast<nmdWorldRecordType_t>( nValue >> NMD_WORLD_HANDLE_TYPE_SHIFT );
	}
	uint32_t getIndex( void ) const { return nValue & NMD_WORLD_HANDLE_INDEX_MASK; }
} nmdWorldHandle_t;

typedef struct nmdWorldValue_s
{
	uint32_t nBits;
	static nmdWorldValue_s fromFloat( float value );
	static nmdWorldValue_s fromInteger( int32_t value );
	static nmdWorldValue_s fromBoolean( bool value );
	float asFloat( void ) const;
	int32_t asInteger( void ) const;
	bool asBoolean( void ) const;
} nmdWorldValue_t;

typedef struct nmdWorldCondition_s
{
	nmdWorldFactId_t nFactId;
	uint8_t eComparison;
	uint8_t eValueType;
	nmdWorldValue_t requiredValue;
} nmdWorldCondition_t;

typedef struct nmdWorldObjectiveHot_s
{
	uint16_t nFirstCondition; // Relative to owning quest's condition range.
	uint16_t nFlagsAndStatus;
	uint16_t nProgress;
	uint8_t nConditionCount;
	uint8_t nParentObjective;
} nmdWorldObjectiveHot_t;

typedef struct alignas( 64 ) nmdWorldQuestHot_s
{
	uint32_t nFlags;
	uint16_t nCurrentObjective;
	uint8_t nObjectiveCount;
	uint8_t nReserved;
	nmdWorldObjectiveHot_t szObjectives[ NMD_WORLD_QUEST_INLINE_COUNT ];
} nmdWorldQuestHot_t;

typedef struct nmdWorldObjectiveDesc_s
{
	const nmdWorldCondition_t *pConditions;
	uint16_t nFlags;
	uint16_t nProgress;
	uint8_t eStatus;
	uint8_t nParentObjective;
	uint8_t nConditionCount;
	uint8_t nReserved;
} nmdWorldObjectiveDesc_t;

typedef struct nmdWorldLocationDefinition_s
{
	nmdWorldAtom_t pDescription;
	uint32_t nRegionId;
	float position[ 3 ];
	float fCompassRadius;
	uint16_t nTraits;
	uint16_t nReserved;
} nmdWorldLocationDefinition_t;

typedef struct nmdWorldLocationCold_s
{
	nmdWorldAtom_t pDescription;
	uint32_t nRegionId;
	float position[ 3 ];
	float fCompassRadius;
	uint32_t nFirstCondition;
	uint16_t nConditionCount;
	uint16_t nReserved;
} nmdWorldLocationCold_t;

// Compatibility view for pre-blackboard save/import code. Not authoritative.
typedef struct nmdWorldLocationState_s
{
	uint32_t nFlags;
	uint32_t nRegionId;
	float position[ 3 ];
	uint32_t nOccupancy;
} nmdWorldLocationState_t;

// Cold, storyline-local topology. Authored quest/story ABI remains unchanged.
typedef struct nmdWorldQuestTreeLink_s
{
	uint16_t nParentQuest;
	uint16_t nFirstChildQuest;
	uint16_t nNextSiblingQuest;
	uint8_t nChildPolicy;
	uint8_t nRequiredChildren;
} nmdWorldQuestTreeLink_t;

typedef struct nmdWorldStoryTreeRange_s
{
	uint32_t nFirstQuest;
	uint16_t nQuestCount;
	uint16_t nRootQuest;
} nmdWorldStoryTreeRange_t;

typedef struct nmdWorldStoryState_s
{
	uint32_t nFlags;
	uint32_t nBranchId;
	uint16_t nActiveNode;
	uint16_t nCompletedNodes;
	uint32_t nRevision;
} nmdWorldStoryState_t;

typedef struct nmdWorldDependencyBuild_s
{
	nmdWorldHandle_t hOwner;
	nmdWorldFactId_t nFactId;
	uint16_t nItemIndex;
} nmdWorldDependencyBuild_t;

typedef struct nmdWorldDependency_s
{
	nmdWorldHandle_t hOwner;
	uint16_t nItemIndex;
	uint16_t nReserved;
} nmdWorldDependency_t;

// Hot-path validated handles retain logical IDs through arena relocation.
// The generation changes when fact definitions are appended, not when records move.
typedef struct nmdWorldCompiledFactHandle_s
{
	nmdWorldFactId_t nFactId;
	uint8_t eType;
	uint8_t nReserved;
	uint32_t nDefinitionGeneration;
} nmdWorldCompiledFactHandle_t;

typedef enum nmdWorldDirtyDomain_e : uint8_t
{
	NMD_WORLD_DIRTY_FACT = 0,
	NMD_WORLD_DIRTY_LOCATION,
	NMD_WORLD_DIRTY_QUEST,
	NMD_WORLD_DIRTY_STORY,
	NMD_WORLD_DIRTY_CALENDAR,
	NMD_WORLD_DIRTY_WEATHER,
	NMD_WORLD_DIRTY_WATER,
	NMD_WORLD_DIRTY_DOMAIN_COUNT
} nmdWorldDirtyDomain_t;

typedef enum nmdWorldDirtyChannel_e : uint8_t
{
	NMD_WORLD_DIRTY_SAVE = 0,
	NMD_WORLD_DIRTY_CHANGES,
	NMD_WORLD_DIRTY_CHANNEL_COUNT
} nmdWorldDirtyChannel_t;

typedef struct nmdWorldDirtyRange_s
{
	uint32_t nFirstWord;
	uint32_t nWordCount;
} nmdWorldDirtyRange_t;

typedef struct nmdWorldDirtyRevisions_s
{
	uint32_t szRevision[ NMD_WORLD_DIRTY_CHANNEL_COUNT ][ NMD_WORLD_DIRTY_DOMAIN_COUNT ];
} nmdWorldDirtyRevisions_t;

typedef enum nmdWorldArenaMode_e : uint8_t
{
	NMD_WORLD_ARENA_AUTHORING = 0,
	NMD_WORLD_ARENA_SEALED_RUNTIME
} nmdWorldArenaMode_t;

typedef struct nmdWorldFactChange_s
{
	nmdWorldFactId_t nFactId;
	nmdWorldFactType_t eType;
	uint8_t nReserved;
	nmdWorldValue_t oldValue;
	nmdWorldValue_t newValue;
} nmdWorldFactChange_t;

// Full-width, typed runtime fact notifications. The older 16-bit event
// definition remains available to existing consumers of compiled content.
typedef NMDEvent<nmdWorldFactChange_t> *NMDWorldFactValueChangedEvent;

// Runtime capacity predictions. All storage is allocated in one 64-byte-aligned block.
// Explicit setCapacities() may repack the block during authoring; hot setters do not allocate.
typedef struct nmdWorldDatabaseCapacities_s
{
	uint32_t nFacts;
	uint32_t nLocations;
	uint32_t nQuests;
	uint32_t nStories;
	uint32_t nRegions;
	uint32_t nQuestConditions;
	uint32_t nLocationConditions;
	uint32_t nDependencies;
	uint32_t nTimedEvents;
	uint32_t nWeatherEvents;
	uint32_t nOverflowObjectives;
} nmdWorldDatabaseCapacities_t;

// Each segment is a typed contiguous span of the same backing allocation.
// The order follows expected access frequency, not the order of registration.
typedef enum nmdWorldArenaAccess_e : uint8_t
{
	NMD_WORLD_ARENA_ACCESS_HOT = 0,
	NMD_WORLD_ARENA_ACCESS_WARM,
	NMD_WORLD_ARENA_ACCESS_COLD,
	NMD_WORLD_ARENA_ACCESS_AUTHORING
} nmdWorldArenaAccess_t;

typedef enum nmdWorldArenaSegmentId_e : uint8_t
{
	NMD_WORLD_ARENA_FACT_VALUES = 0,
	NMD_WORLD_ARENA_LOCATIONS_HOT,
	NMD_WORLD_ARENA_QUESTS_HOT,
	NMD_WORLD_ARENA_OBJECTIVES_OVERFLOW,
	NMD_WORLD_ARENA_STORIES_HOT,
	NMD_WORLD_ARENA_CALENDAR_HOT,
	NMD_WORLD_ARENA_WEATHER_HOT,
	NMD_WORLD_ARENA_WATER_HOT,
	NMD_WORLD_ARENA_LOCATION_OCCUPANCY,
	NMD_WORLD_ARENA_REGION_IDS,
	NMD_WORLD_ARENA_REGION_LOOKUP,
	NMD_WORLD_ARENA_CLIMATE_LINKS,
	NMD_WORLD_ARENA_WEATHER_INDEX_ENTRIES,
	NMD_WORLD_ARENA_WEATHER_INDEX_RANGES,
	NMD_WORLD_ARENA_COMPILED_REVISIONS,
	NMD_WORLD_ARENA_DIRTY_WORDS,
	NMD_WORLD_ARENA_DIRTY_REVISIONS,
	NMD_WORLD_ARENA_LOOKUP_BUCKETS,
	NMD_WORLD_ARENA_LOOKUP_ENTRIES,
	NMD_WORLD_ARENA_DEPENDENCY_OFFSETS,
	NMD_WORLD_ARENA_DEPENDENCIES,
	NMD_WORLD_ARENA_FACT_TYPES,
	NMD_WORLD_ARENA_FACT_ATOMS,
	NMD_WORLD_ARENA_LOCATION_COLD,
	NMD_WORLD_ARENA_LOCATION_ATOMS,
	NMD_WORLD_ARENA_LOCATION_CONDITIONS,
	NMD_WORLD_ARENA_QUEST_ATOMS,
	NMD_WORLD_ARENA_QUEST_META,
	NMD_WORLD_ARENA_QUEST_CONDITIONS,
	NMD_WORLD_ARENA_STORY_ATOMS,
	NMD_WORLD_ARENA_CALENDAR_COLD,
	NMD_WORLD_ARENA_CLIMATE_SEASONAL,
	NMD_WORLD_ARENA_CLIMATE_GEOGRAPHY,
	NMD_WORLD_ARENA_TIMED_EVENTS,
	NMD_WORLD_ARENA_WEATHER_TIMELINE,
	NMD_WORLD_ARENA_DEPENDENCY_BUILD,
	NMD_WORLD_ARENA_DEPENDENCY_BUCKETS,
	NMD_WORLD_ARENA_DEPENDENCY_CURSOR,
	NMD_WORLD_ARENA_QUEST_TREE_LINKS,
	NMD_WORLD_ARENA_STORY_TREE_RANGES,
	NMD_WORLD_ARENA_SEGMENT_COUNT
} nmdWorldArenaSegmentId_t;

typedef struct nmdWorldArenaSegmentInfo_s
{
	uint32_t nOffsetBytes;
	uint32_t nElementBytes;
	uint32_t nCapacity;
	uint32_t nCount;
	uint32_t nCommittedCapacity;
	uint32_t nCommittedBytes;
	uint8_t eAccess;
	uint8_t nReserved[ 3 ];
} nmdWorldArenaSegmentInfo_t;

// Derived, arena-resident climate routing. Invalid indices denote open boundaries.
typedef struct nmdClimateLinks_s
{
	float fDrainAreaScale;
	float fDownwindAreaScale;
	uint16_t nDrainIndex;
	uint16_t nDownwindIndex;
} nmdClimateLinks_t;

// Region-specific authored weather interval index, ordered by region/start/event index.
typedef struct nmdWeatherTimelineIndexEntry_s
{
	uint64_t nStartWorldSecond;
	uint64_t nPrefixMaxEndSecond; // Inclusive prefix of exclusive event end times.
	uint32_t nEventIndex;
	uint16_t nDenseRegionIndex;
	uint16_t nReserved;
} nmdWeatherTimelineIndexEntry_t;

typedef struct nmdWeatherTimelineRange_s
{
	uint32_t nFirst;
	uint32_t nCount;
} nmdWeatherTimelineRange_t;

// Index validity is world metadata kept inside the single arena.
typedef struct nmdWorldCompiledIndexRevisions_s
{
	uint32_t nClimateTopology;
	uint32_t nClimateCompiled;
	uint32_t nWeatherTimeline;
	uint32_t nWeatherCompiled;
} nmdWorldCompiledIndexRevisions_t;

// Stores facts, locations, quest/objective state, stories and climate in one
// authoritative owner. No ticking, condition evaluation or event dispatch.
// Getter pointers are borrowed views; explicit setCapacities() repacking invalidates them.
class NMDWorldDatabase
{

public:
	NMDWorldDatabase( void );
	~NMDWorldDatabase( void );
	NMDWorldDatabase( const NMDWorldDatabase& ) = delete;
	NMDWorldDatabase& operator=( const NMDWorldDatabase& ) = delete;

	bool setCapacities( const nmdWorldDatabaseCapacities_t& capacities );
	void getCapacities( nmdWorldDatabaseCapacities_t& capacities ) const;
	const void *getArenaBase( void ) const;
	uint32_t getArenaSizeBytes( void ) const; // Reserved bytes; legacy alias.
	uint32_t getArenaReservedBytes( void ) const;
	uint32_t getArenaCommittedBytes( void ) const;
	uint32_t getArenaLiveBytes( void ) const;
	bool getArenaVirtualCommitSupported( void ) const;
	void getCommittedCapacities( nmdWorldDatabaseCapacities_t& capacities ) const;
	bool setCommittedCapacities( const nmdWorldDatabaseCapacities_t& capacities );
	uint32_t getArenaGeneration( void ) const;
	nmdWorldArenaMode_t getArenaMode( void ) const;
	bool setArenaMode( nmdWorldArenaMode_t eMode ); // Explicit quiescent repack.
	bool getArenaSegmentInfo( nmdWorldArenaSegmentId_t eSegment, nmdWorldArenaSegmentInfo_t& info ) const;

	// Two independent arena-resident dirty channels. No callbacks or dispatch.
	uint32_t getDirtyRevision( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel ) const;
	bool getDirtyWords( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel,
		uint64_t *pOutput, uint32_t nWordCapacity, uint32_t& nWordCount, uint32_t& nRevision ) const;
	bool setDirtyAcknowledged( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel,
		uint32_t nExpectedRevision, const uint64_t *pMask, uint32_t nWordCount );
	bool setDirtyBaseline( nmdWorldDirtyChannel_t eChannel );
	bool setDirtyBaselineDomain( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel );
	bool setDirtyResyncDomain( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel );
	bool setDirtyResync( nmdWorldDirtyChannel_t eChannel );

	// Stable typed handles and atom indexing; no string ownership.
	nmdWorldHandle_t getRecordHandle( nmdWorldAtom_t pAtom, nmdWorldRecordType_t eType ) const;
	nmdWorldAtom_t getRecordAtom( nmdWorldHandle_t hRecord ) const;
	bool getRecordExists( nmdWorldHandle_t hRecord ) const;

	// FACTS: values are stored separately from cold type/atom metadata.
	nmdWorldFactId_t setFactDefinition( nmdWorldAtom_t pAtom, nmdWorldFactType_t eType, nmdWorldValue_t initialValue );
	bool getFact( nmdWorldFactId_t nId, nmdWorldFactType_t& eType, nmdWorldValue_t& value ) const;
	bool setFact( nmdWorldFactId_t nId, nmdWorldFactType_t eType, nmdWorldValue_t value );
	bool getFactFloat( nmdWorldFactId_t nId, float& value ) const;
	bool getFactInteger( nmdWorldFactId_t nId, int32_t& value ) const;
	bool getFactBoolean( nmdWorldFactId_t nId, bool& value ) const;
	bool setFactFloat( nmdWorldFactId_t nId, float value );
	bool setFactInteger( nmdWorldFactId_t nId, int32_t value );
	bool setFactBoolean( nmdWorldFactId_t nId, bool value );
	nmdWorldFactId_t getFactId( nmdWorldAtom_t pAtom ) const;
	uint32_t getFactCount( void ) const;
	const nmdWorldValue_t *getFactHotData( void ) const;
	uint32_t getFactDefinitionGeneration( void ) const;
	bool getCompiledFactHandle( nmdWorldFactId_t nId, nmdWorldFactType_t eType,
		nmdWorldCompiledFactHandle_t& handle ) const;
	// Validated authored conditions can construct a handle from the current
	// definition generation without repeating the cold type-array access.
	bool getCompiledFactValue( const nmdWorldCompiledFactHandle_t& handle,
		nmdWorldValue_t& value ) const;
	bool setCompiledFactValue( const nmdWorldCompiledFactHandle_t& handle,
		nmdWorldValue_t value );

	// LOCATIONS: hot progress, cold authoring, transient overlap counters.
	nmdWorldHandle_t setLocationDefinition( nmdWorldAtom_t pAtom, const nmdWorldLocationDefinition_t& definition,
		const nmdWorldCondition_t *pConditions = NULL, uint16_t nConditionCount = 0u,
		nmdLocationId_t nAuthoredId = NMD_LOCATION_INVALID_ID );
	nmdWorldHandle_t getLocationHandle( nmdLocationId_t nId ) const;
	bool getLocationData( nmdWorldHandle_t hLocation, nmdLocationData_t& hot ) const;
	bool setLocationData( nmdWorldHandle_t hLocation, const nmdLocationData_t& hot );
	bool getLocationDefinition( nmdWorldHandle_t hLocation, nmdWorldLocationCold_t& cold ) const;
	bool setLocationDefinition( nmdWorldHandle_t hLocation, const nmdWorldLocationDefinition_t& definition );
	bool getLocationStatus( nmdWorldHandle_t hLocation, nmdLocationStatus_t& eStatus ) const;
	bool setLocationStatus( nmdWorldHandle_t hLocation, nmdLocationStatus_t eStatus );
	bool getLocationOccupancy( nmdWorldHandle_t hLocation, uint16_t& nCount ) const;
	bool setLocationOccupancy( nmdWorldHandle_t hLocation, uint16_t nCount );
	bool getLocationConditions( nmdWorldHandle_t hLocation, const nmdWorldCondition_t *&pConditions, uint16_t& nCount ) const;
	bool setLocationScores( nmdWorldHandle_t hLocation, uint16_t nRenown, uint16_t nFear );
	// Prevalidated bulk storage replacement; no format or gameplay interpretation.
	bool setLocationDataBatch( const nmdLocationData_t *pRecords, uint32_t nCount );
	uint32_t getLocationCount( void ) const;
	const nmdLocationData_t *getLocationHotData( void ) const;
	const nmdWorldLocationCold_t *getLocationColdData( void ) const;

	// QUESTS: no automatic condition evaluation or objective promotion.
	nmdWorldHandle_t setQuestDefinition( nmdWorldAtom_t pAtom, uint32_t nFlags,
		uint16_t nCurrentObjective, const nmdWorldObjectiveDesc_t *pObjectives, uint32_t nCount );
	const nmdWorldQuestHot_t *getQuest( nmdWorldHandle_t hQuest ) const;
	bool setQuestHot( nmdWorldHandle_t hQuest, const nmdWorldQuestHot_t& hot );
	const nmdWorldObjectiveHot_t *getObjective( nmdWorldHandle_t hQuest, uint32_t nObjective ) const;
	bool setObjective( nmdWorldHandle_t hQuest, uint32_t nObjective, const nmdWorldObjectiveHot_t& hot );
	bool setQuestFlags( nmdWorldHandle_t hQuest, uint32_t nFlags );
	bool setCurrentObjective( nmdWorldHandle_t hQuest, uint16_t nObjective );
	bool setObjectiveStatus( nmdWorldHandle_t hQuest, uint32_t nObjective, uint8_t eStatus );
	bool setObjectiveProgress( nmdWorldHandle_t hQuest, uint32_t nObjective, uint16_t nProgress );
	bool setObjectiveSatisfied( nmdWorldHandle_t hQuest, uint32_t nObjective, bool bSatisfied );
	bool getObjectiveConditions( nmdWorldHandle_t hQuest, uint32_t nObjective,
		const nmdWorldCondition_t *&pConditions, uint8_t& nCount ) const;
	uint32_t getQuestCount( void ) const;
	uint32_t getQuestConditionCount( void ) const;
	const nmdWorldQuestHot_t *getQuestHotData( void ) const;
	bool getQuestTreeLink( uint32_t nGlobalQuestIndex, nmdWorldQuestTreeLink_t& link ) const;
	bool setQuestTreeLink( uint32_t nGlobalQuestIndex, const nmdWorldQuestTreeLink_t& link );

	// STORIES.
	nmdWorldHandle_t setStoryDefinition( nmdWorldAtom_t pAtom, const nmdWorldStoryState_t& state );
	bool getStory( nmdWorldHandle_t hStory, nmdWorldStoryState_t& state ) const;
	bool setStory( nmdWorldHandle_t hStory, const nmdWorldStoryState_t& state );
	uint32_t getStoryCount( void ) const;
	const nmdWorldStoryState_t *getStoryHotData( void ) const;
	bool getStoryTreeRange( nmdWorldHandle_t hStory, nmdWorldStoryTreeRange_t& range ) const;
	bool setStoryTreeRange( nmdWorldHandle_t hStory, const nmdWorldStoryTreeRange_t& range );

	// FACT REVERSE DEPENDENCIES: the consumer decides what to re-evaluate.
	bool setFactDependency( nmdWorldFactId_t nId, nmdWorldHandle_t hOwner, uint16_t nItemIndex );
	bool getDependencyBuildData( const nmdWorldDependencyBuild_t *&pData, uint32_t& nCount ) const;
	bool getDependencyCompileStorage( uint32_t *&pOffsets, uint32_t *&pCursor,
		nmdWorldDependency_t *&pDependencies, uint32_t& nOffsetCapacity,
		uint32_t& nDependencyCapacity );
	bool setCompiledDependencyCount( uint32_t nDependencies );
	bool getFactDependencies( nmdWorldFactId_t nId, const nmdWorldDependency_t *&pData, uint32_t& nCount ) const;
	uint32_t getFactDependencyCount( nmdWorldFactId_t nId ) const;

	// CALENDAR: clock values only. NMDCalendarSystem owns advancing time.
	bool setCalendarDefinition( const nmdCalendarDefinition_t& definition );
	bool getCalendarDefinition( nmdCalendarDefinition_t& definition ) const;
	bool setCalendarHot( const nmdCalendarHot_t& hot );
	bool getCalendarHot( nmdCalendarHot_t& hot ) const;
	uint64_t getWorldSeconds( void ) const;
	bool setWorldSeconds( uint64_t nSeconds );

	// CLIMATE: region IDs and authored links are cold; weather/water are hot.
	bool setClimateRegion( nmdClimateRegionId_t nId, const nmdClimateSeasonalCold_t& seasonal,
		const nmdClimateGeographyCold_t& geography,
		const nmdWeatherHot_t& weather, const nmdWaterCycleHot_t& water );
	bool getClimateIndex( nmdClimateRegionId_t nId, uint32_t& nIndex ) const;
	bool getClimateRegionId( uint32_t nIndex, nmdClimateRegionId_t& nId ) const;
	bool setClimateLinks( uint32_t nDenseRegionIndex, const nmdClimateLinks_t& links );
	bool getClimateLinks( uint32_t nDenseRegionIndex, nmdClimateLinks_t& links ) const;
	uint32_t getClimateTopologyRevision( void ) const;
	uint32_t getClimateLinksRevision( void ) const;
	bool setClimateLinksRevision( uint32_t nTopologyRevision );
	bool getClimateSeasonal( nmdClimateRegionId_t nId, nmdClimateSeasonalCold_t& seasonal ) const;
	bool getClimateGeography( nmdClimateRegionId_t nId, nmdClimateGeographyCold_t& geography ) const;
	bool getRegionWeather( nmdClimateRegionId_t nId, nmdWeatherHot_t& weather ) const;
	bool setRegionWeather( nmdClimateRegionId_t nId, const nmdWeatherHot_t& weather );
	bool setRegionWeatherByIndex( uint32_t nDenseIndex, const nmdWeatherHot_t& weather );
	bool getRegionWaterCycle( nmdClimateRegionId_t nId, nmdWaterCycleHot_t& water ) const;
	bool setRegionWaterCycle( nmdClimateRegionId_t nId, const nmdWaterCycleHot_t& water );
	bool setRegionWaterCycleByIndex( uint32_t nDenseIndex, const nmdWaterCycleHot_t& water );
	bool getLocationWeather( nmdWorldHandle_t hLocation, nmdWeatherHot_t& weather ) const;
	uint32_t getClimateRegionCount( void ) const;
	const nmdWeatherHot_t *getRegionWeatherHotData( void ) const;
	const nmdWaterCycleHot_t *getRegionWaterHotData( void ) const;
	const nmdClimateSeasonalCold_t *getClimateSeasonalColdData( void ) const;
	const nmdClimateGeographyCold_t *getClimateGeographyColdData( void ) const;

	// Authored timelines are cold; these setters never dispatch or schedule.
	bool setWorldTimedEvent( const nmdWorldTimedEvent_t& event );
	bool getWorldTimedEvent( uint32_t nIndex, nmdWorldTimedEvent_t& event ) const;
	uint32_t getWorldTimedEventCount( void ) const;
	bool setWeatherTimelineEvent( const nmdWeatherTimelineEvent_t& event );
	bool getWeatherTimelineEvent( uint32_t nIndex, nmdWeatherTimelineEvent_t& event ) const;
	uint32_t getWeatherTimelineEventCount( void ) const;
	bool getWeatherCompileStorage( const nmdWeatherTimelineEvent_t *&pEvents,
		nmdWeatherTimelineIndexEntry_t *&pEntries, nmdWeatherTimelineRange_t *&pRanges,
		uint32_t& nEventCapacity, uint32_t& nRegionCapacity );
	bool setCompiledWeatherIndexCounts( uint32_t nEventCount, uint32_t nRegionCount );
	bool getWeatherTimelineIndexRange( uint32_t nDenseRegionIndex,
		const nmdWeatherTimelineIndexEntry_t *&pEntries, uint32_t& nCount ) const;
	uint32_t getWeatherTimelineRevision( void ) const;
	uint32_t getWeatherTimelineIndexRevision( void ) const;

	// Atomic logical state publication after external codec validation.
	bool setSimulationStateBatch( const nmdCalendarHot_t& calendar,
		const nmdWeatherHot_t *pWeather, const nmdWaterCycleHot_t *pWater,
		uint32_t nRegionCount );
	const nmdClimateRegionId_t *getClimateRegionIds( void ) const;
	const nmdClimateLinks_t *getClimateLinksData( void ) const;
	const nmdWorldTimedEvent_t *getWorldTimedEventData( void ) const;
	const nmdWeatherTimelineEvent_t *getWeatherTimelineEventData( void ) const;

private:
	typedef nmdWorldIdIndexEntry_t nmdWorldIndexEntry_t;

	typedef struct nmdWorldQuestMeta_s
	{
		uint32_t nFirstCondition;
		uint32_t nFirstOverflowObjective;
		uint16_t nTotalConditions;
		uint8_t nOverflowCount;
		uint8_t nReserved;
	} nmdWorldQuestMeta_t;


	bool getArenaCapacitiesValid( const nmdWorldDatabaseCapacities_t& capacities ) const;
	bool setArenaLayout( const nmdWorldDatabaseCapacities_t& capacities, nmdWorldArenaMode_t eMode );
	bool setArenaCommitments( const nmdWorldDatabaseCapacities_t& capacities );
	void setDirty( nmdWorldDirtyDomain_t eDomain, uint32_t nIndex );
	void setDirtyDomainBaseline( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel );
	void setDirtyDomainResync( nmdWorldDirtyDomain_t eDomain, nmdWorldDirtyChannel_t eChannel );
	void setDirtyIfWeatherChanged( uint32_t nIndex, const nmdWeatherHot_t& previous );
	void setDirtyIfWaterChanged( uint32_t nIndex, const nmdWaterCycleHot_t& previous );
	void setDirtyIfCalendarChanged( const nmdCalendarHot_t& previous );
	void rebuildDependencyBuckets( void );

	static nmdWorldHandle_t makeHandle( nmdWorldRecordType_t eType, uint32_t nIndex );
	bool ensureLookupCapacity( uint32_t nEntries );
	void rehashLookup( void );
	bool insertRecord( const nmdWorldIdKey_t& key, nmdWorldHandle_t hRecord );
	bool getIdentityKey( nmdWorldHandle_t hRecord, nmdWorldIdKey_t& key ) const;
	bool findIdentity( const nmdWorldIdKey_t& key, nmdWorldHandle_t& hRecord ) const;
	bool isValidFact( nmdWorldFactId_t nId, nmdWorldFactType_t eType ) const;
	bool isLocationHandle( nmdWorldHandle_t hLocation ) const;
	nmdWorldObjectiveHot_t *getMutableObjective( nmdWorldHandle_t hQuest, uint32_t nObjective );

	// One backing block; only descriptors/pointers live in this controller.
	nmdWorldArenaBlock_t m_ArenaBlock;
	nmdWorldDatabaseCapacities_t m_DesiredCommitCapacities;
	bool m_bExplicitCommitBudget;
	nmdWorldArenaSegmentInfo_t m_ArenaSegments[ NMD_WORLD_ARENA_SEGMENT_COUNT ];

	nmdWorldArenaArray_t<uint32_t> m_LookupBuckets;
	nmdWorldArenaArray_t<nmdWorldIndexEntry_t> m_LookupEntries;
	nmdWorldArenaArray_t<nmdWorldValue_t> m_FactValues;
	nmdWorldArenaArray_t<uint8_t> m_FactTypes;
	nmdWorldArenaArray_t<nmdWorldAtom_t> m_FactAtoms;
	nmdWorldArenaArray_t<nmdLocationData_t> m_Locations;
	nmdWorldArenaArray_t<nmdWorldLocationCold_t> m_LocationCold;
	nmdWorldArenaArray_t<uint16_t> m_LocationOccupancy;
	nmdWorldArenaArray_t<nmdWorldCondition_t> m_LocationConditions;
	nmdWorldArenaArray_t<nmdWorldAtom_t> m_LocationAtoms;
	nmdWorldArenaArray_t<nmdWorldQuestHot_t> m_Quests;
	nmdWorldArenaArray_t<nmdWorldAtom_t> m_QuestAtoms;
	nmdWorldArenaArray_t<nmdWorldQuestMeta_t> m_QuestMeta;
	nmdWorldArenaArray_t<nmdWorldQuestTreeLink_t> m_QuestTreeLinks;
	nmdWorldArenaArray_t<nmdWorldObjectiveHot_t> m_OverflowObjectives;
	nmdWorldArenaArray_t<nmdWorldCondition_t> m_Conditions;
	nmdWorldArenaArray_t<nmdWorldStoryState_t> m_Stories;
	nmdWorldArenaArray_t<nmdWorldAtom_t> m_StoryAtoms;
	nmdWorldArenaArray_t<nmdWorldStoryTreeRange_t> m_StoryTreeRanges;
	nmdWorldArenaArray_t<nmdWorldDependencyBuild_t> m_DependencyBuild;
	nmdWorldArenaArray_t<uint32_t> m_DependencyBuckets;
	nmdWorldArenaArray_t<uint32_t> m_DependencyOffsets;
	nmdWorldArenaArray_t<nmdWorldDependency_t> m_Dependencies;
	nmdWorldArenaArray_t<nmdCalendarHot_t> m_Calendar;
	nmdWorldArenaArray_t<nmdCalendarDefinition_t> m_CalendarCold;
	nmdWorldArenaArray_t<nmdClimateRegionId_t> m_RegionIds;
	nmdWorldArenaArray_t<nmdClimateLinks_t> m_ClimateLinks;
	nmdWorldArenaArray_t<nmdWeatherTimelineIndexEntry_t> m_WeatherIndexEntries;
	nmdWorldArenaArray_t<nmdWeatherTimelineRange_t> m_WeatherIndexRanges;
	nmdWorldArenaArray_t<nmdWorldCompiledIndexRevisions_t> m_CompiledRevisions;
	nmdWorldArenaArray_t<uint64_t> m_DirtyWords;
	nmdWorldArenaArray_t<nmdWorldDirtyRevisions_t> m_DirtyRevisions;
	nmdWorldDirtyRange_t m_DirtyRanges[ NMD_WORLD_DIRTY_CHANNEL_COUNT ][ NMD_WORLD_DIRTY_DOMAIN_COUNT ];
	nmdWorldArenaArray_t<nmdClimateSeasonalCold_t> m_ClimateSeasonal;
	nmdWorldArenaArray_t<nmdClimateGeographyCold_t> m_ClimateGeography;
	nmdWorldArenaArray_t<nmdWeatherHot_t> m_RegionWeather;
	nmdWorldArenaArray_t<nmdWaterCycleHot_t> m_WaterCycles;
	nmdWorldArenaArray_t<nmdWorldTimedEvent_t> m_TimedEvents;
	nmdWorldArenaArray_t<nmdWeatherTimelineEvent_t> m_WeatherTimelineEvents;
	nmdWorldArenaArray_t<uint32_t> m_DependencyCursor;

	uint32_t m_nDependencyContentCapacity;
	uint32_t m_nArenaGeneration;
	uint32_t m_nFactDefinitionGeneration;
	nmdWorldArenaMode_t m_eArenaMode;
	bool m_bDependencyIndexDirty;
	bool m_bHasCalendar;
};

static_assert( sizeof( nmdWorldQuestTreeLink_t ) == 8, "Quest topology record must be 8 bytes" );
static_assert( sizeof( nmdWorldStoryTreeRange_t ) == 8, "Story topology record must be 8 bytes" );
static_assert( sizeof( nmdWorldHandle_t ) == 4, "Handle must be 4 bytes" );
static_assert( sizeof( nmdWorldCompiledFactHandle_t ) == 8, "Compiled fact handle must be 8 bytes" );
static_assert( sizeof( nmdWorldDirtyRange_t ) == 8, "Dirty range must be 8 bytes" );
static_assert( sizeof( nmdWorldDirtyRevisions_t ) <= 64, "Dirty revisions must fit a cacheline" );
static_assert( sizeof( nmdWorldValue_t ) == 4, "Fact must be 4 bytes" );
static_assert( sizeof( nmdWorldCondition_t ) == 8, "Condition must be 8 bytes" );
static_assert( sizeof( nmdWorldObjectiveHot_t ) == 8, "Objective must be 8 bytes" );
static_assert( sizeof( nmdWorldQuestHot_t ) == 64, "Quest hot must occupy one cacheline" );
static_assert( alignof( nmdWorldQuestHot_t ) == 64, "Quest hot must align to a cacheline" );
static_assert( sizeof( nmdWorldDependency_t ) == 8, "Dependency must be 8 bytes" );
static_assert( sizeof( nmdWorldLocationDefinition_t ) <= 64, "Location definition must fit a line" );
static_assert( sizeof( nmdWorldLocationCold_t ) <= 64, "Location cold must fit a line" );
static_assert( sizeof( nmdWorldStoryState_t ) <= 64, "Story hot must fit a line" );
static_assert( sizeof( nmdWorldDatabaseCapacities_t ) <= 64, "Capacities must fit a line" );
static_assert( sizeof( nmdWorldArenaSegmentInfo_t ) <= 64, "Arena segment info must fit a line" );
static_assert( sizeof( nmdClimateLinks_t ) == 12, "Climate link must be 12 bytes" );
static_assert( sizeof( nmdWeatherTimelineIndexEntry_t ) == 24, "Weather index entry must be 24 bytes" );
static_assert( sizeof( nmdWeatherTimelineRange_t ) == 8, "Weather range must be 8 bytes" );
static_assert( sizeof( nmdWorldCompiledIndexRevisions_t ) == 16, "Index revisions must be 16 bytes" );

#endif // __NMD_WORLD_DATABASE_H__
