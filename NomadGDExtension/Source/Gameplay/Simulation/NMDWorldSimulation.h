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

#ifndef __NMD_WORLD_SIMULATION_H__
#define __NMD_WORLD_SIMULATION_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"

// Owns world-time progression, regional weather/hydrology algorithms and events.
// NMDWorldDatabase is the sole owner of persistent hot and cold state.
class NMDWorldSimulation
{
public:
	NMDWorldSimulation( void );
	void setDatabase( NMDWorldDatabase *pDatabase );
	NMDWorldDatabase *getDatabase( void ) const;

	bool initializeCalendar( const nmdCalendarDefinition_t& definition,
		uint64_t nInitialWorldSeconds = 0u, uint32_t nSeed = 0x4E4D4431u );
	bool getCalendarDate( nmdCalendarDate_t& date ) const;
	bool getCalendarDateAt( uint64_t nWorldSeconds, nmdCalendarDate_t& date ) const;
	bool getCalendarHot( nmdCalendarHot_t& hot ) const;
	bool getCalendarDefinition( nmdCalendarDefinition_t& definition ) const;
	uint64_t getWorldSeconds( void ) const;
	bool getCelestialSample( nmdClimateRegionId_t nRegionId, nmdCelestialSample_t& sample ) const;
	// Time advances in authored seconds; fixed regional substeps are deterministic.
	bool advanceWorldSeconds( uint32_t nSeconds );
	bool advanceWorldRealSeconds( double fRealDeltaSeconds );
	// Schedule before play: period=0 for a one-shot event.
	bool registerWorldTimedEvent( const nmdWorldTimedEvent_t& event );

	bool reserveClimateRegions( uint32_t nRegions, uint32_t nTimedEvents );
	bool registerClimateRegion( nmdClimateRegionId_t nRegionId,
		const nmdClimateSeasonalCold_t& seasonal,
		const nmdClimateGeographyCold_t& geography,
		const nmdWaterCycleHot_t *pInitialWater = NULL );
	// Validates downstream links; call after region definitions are loaded.
	bool buildClimateGraph( void );
	bool buildWeatherTimelineIndex( void );
	uint32_t getClimateRegionCount( void ) const;
	bool getClimateSeasonal( nmdClimateRegionId_t nRegionId, nmdClimateSeasonalCold_t& seasonal ) const;
	bool getClimateGeography( nmdClimateRegionId_t nRegionId, nmdClimateGeographyCold_t& geography ) const;
	bool getRegionWeather( nmdClimateRegionId_t nRegionId, nmdWeatherHot_t& weather ) const;
	bool getRegionWaterCycle( nmdClimateRegionId_t nRegionId, nmdWaterCycleHot_t& water ) const;
	bool getLocationWeather( nmdWorldHandle_t hLocation, nmdWeatherHot_t& weather ) const;
	bool getBlendedWeather( nmdClimateRegionId_t nFromRegion,
		nmdClimateRegionId_t nToRegion, float fBlend, nmdWeatherSample_t& sample ) const;
	// Authored weather timeline is cold. Time windows are evaluated on fixed steps.
	bool registerWeatherTimelineEvent( const nmdWeatherTimelineEvent_t& event );
	void reserveWeatherTimelineEvents( uint32_t nEvents );

	// Snapshot: same dense climate registration order/IDs must exist on restore.
	// Logical PODs are serialized field-by-field by the host save codec.
	bool writeSimulationSnapshot( nmdSimulationSnapshotHeader_t& header,
		nmdClimateRegionId_t *pRegionIds, nmdWeatherHot_t *pWeather,
		nmdWaterCycleHot_t *pWater, uint32_t nCapacity ) const;
	bool restoreSimulationSnapshot( const nmdSimulationSnapshotHeader_t& header,
		const nmdClimateRegionId_t *pRegionIds, const nmdWeatherHot_t *pWeather,
		const nmdWaterCycleHot_t *pWater );
	void setSimulationCallbacks( nmdCalendarChangedFn_t pCalendar,
		nmdTimedEventFn_t pTimed, nmdWeatherChangedFn_t pWeather,
		nmdHydrologyChangedFn_t pHydrology, void *pUser );

	private:
	bool getClimateIndex( nmdClimateRegionId_t nRegionId, uint32_t& nIndex ) const;
	void advanceCalendarNotifications( uint64_t nPreviousSeconds, uint64_t nNewSeconds );
	void dispatchTimedEvents( uint64_t nPreviousSeconds, uint64_t nNewSeconds );
	void stepRegionalSimulation( uint32_t nStepSeconds );
	void selectWeatherFront( uint32_t nIndex, uint32_t nStepSeconds );
	void updateRegionWater( uint32_t nIndex, float fStepHours, float fSeasonTemperature,
		float fSeasonHumidity, float fStormChance );
	void applyRegionalWaterFlux( void );
	void rebuildTimedEventDeadlines( void );
	float sampleSeason( const float *pValues, uint64_t nWorldSeconds ) const;


	NMDWorldDatabase *m_pDatabase;
	uint32_t m_nCalendarYearDays;
	uint32_t m_nSimulationSeed;
	bool m_bCalendarInitialized;
	bool m_bClimateGraphReady;
	bool m_bSimulationAdvancing;
	// Preallocated, per-step working buffers; authoritative state stays in the database.
	eastl::vector<nmdWaterFlux_t> m_RegionFlux;
	eastl::vector<nmdWeatherHot_t> m_WeatherScratch;
	eastl::vector<nmdWaterCycleHot_t> m_WaterScratch;
	nmdCalendarHot_t m_CalendarHotCache;
	nmdCalendarDefinition_t m_CalendarDefinitionCache;
	eastl::vector<uint64_t> m_TimedNextDue;
	uint64_t m_nNextTimedEventWorldSecond;
	nmdCalendarChangedFn_t m_pCalendarChanged;
	nmdTimedEventFn_t m_pWorldTimed;
	nmdWeatherChangedFn_t m_pRegionWeatherChanged;
	nmdHydrologyChangedFn_t m_pHydrologyChanged;
	void *m_pSimulationCallbackUser;
};

#endif // __NMD_WORLD_SIMULATION_H__
