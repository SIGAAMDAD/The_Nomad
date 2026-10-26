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

#include "NMDWorldSnapshotCodec.h"
#include "NMDWorldContentCompiler.h"

#include <cmath>
#include <cstdint>

namespace
{
	static void finalizeDomain( NMDWorldDatabase& db, nmdWorldDirtyDomain_t domain )
	{
		db.setDirtyBaselineDomain( domain, NMD_WORLD_DIRTY_SAVE );
		db.setDirtyResyncDomain( domain, NMD_WORLD_DIRTY_CHANGES );
	}
}

bool NMDWorldSnapshotCodec::writeSimulation( const NMDWorldDatabase& db,
	nmdSimulationSnapshotHeader_t& header, nmdClimateRegionId_t *pIds,
	nmdWeatherHot_t *pWeather, nmdWaterCycleHot_t *pWater, uint32_t capacity )
{
	const uint32_t count = db.getClimateRegionCount();
	if ( count > UINT16_MAX || capacity < count ||
		( count && ( pIds == NULL || pWeather == NULL || pWater == NULL ) ) ||
		!db.getCalendarHot( header.calendar ) ) {
		return false;
	}
	header.nMagic = NMD_SIMULATION_SNAPSHOT_MAGIC;
	header.nVersion = NMD_SIMULATION_SNAPSHOT_VERSION;
	header.nRegionCount = static_cast<uint16_t>( count );
	for ( uint32_t i = 0u; i < count; ++i ) {
		if ( !db.getClimateRegionId( i, pIds[i] ) ) {
			return false;
		}
		pWeather[i] = db.getRegionWeatherHotData()[i];
		pWater[i] = db.getRegionWaterHotData()[i];
	}
	return true;
}

bool NMDWorldSnapshotCodec::restoreSimulation( NMDWorldDatabase& db,
	const nmdSimulationSnapshotHeader_t& header, const nmdClimateRegionId_t *pIds,
	const nmdWeatherHot_t *pWeather, const nmdWaterCycleHot_t *pWater )
{
	nmdCalendarDefinition_t def = {};
	const uint32_t count = db.getClimateRegionCount();
	if ( header.nMagic != NMD_SIMULATION_SNAPSHOT_MAGIC ||
		header.nVersion != NMD_SIMULATION_SNAPSHOT_VERSION ||
		header.nRegionCount != count || !db.getCalendarDefinition( def ) ||
		!NMDWorldContentCompiler::validateCalendarHot( def, header.calendar ) ||
		( count && ( pIds == NULL || pWeather == NULL || pWater == NULL ) ) ) {
		return false;
	}
	if ( header.calendar.nWeatherRemainderSeconds !=
		header.calendar.nWorldSeconds % def.nWeatherStepSeconds ) {
		return false;
	}
	uint32_t yearDays = 0u;
	for ( uint32_t i = 0u; i < def.nMonths; ++i ) {
		yearDays += def.nMonthLengths[i];
	}
	if ( yearDays == 0u || header.calendar.nWorldSeconds / 86400u / yearDays >= UINT32_MAX ) {
		return false;
	}
	for ( uint32_t i = 0u; i < count; ++i ) {
		nmdClimateRegionId_t actual = NMD_CLIMATE_REGION_INVALID_ID;
		if ( !db.getClimateRegionId( i, actual ) || actual != pIds[i] ||
			!NMDWorldContentCompiler::validateWeather( pWeather[i] ) ||
			!NMDWorldContentCompiler::validateWater( pWater[i] ) ) {
			return false;
		}
	}
	if ( !db.setSimulationStateBatch( header.calendar, pWeather, pWater, count ) ) {
		return false;
	}
	finalizeDomain( db, NMD_WORLD_DIRTY_CALENDAR );
	finalizeDomain( db, NMD_WORLD_DIRTY_WEATHER );
	finalizeDomain( db, NMD_WORLD_DIRTY_WATER );
	return true;
}

bool NMDWorldSnapshotCodec::writeLocations( const NMDWorldDatabase& db,
	nmdLocationSaveRecord_t *pOutput, uint32_t capacity )
{
	const uint32_t count = db.getLocationCount();
	if ( capacity < count || ( count && pOutput == NULL ) ) {
		return false;
	}
	const nmdLocationData_t *pHot = db.getLocationHotData();
	for ( uint32_t i = 0u; i < count; ++i ) {
		pOutput[i] = { pHot[i].nId, static_cast<uint8_t>( pHot[i].nStateAndTraits & NMD_LOCATION_STATUS_MASK ),
			0u, pHot[i].nPlayerRenownScore, pHot[i].nPlayerFearScore };
	}
	return true;
}

bool NMDWorldSnapshotCodec::restoreLocations( NMDWorldDatabase& db,
	const nmdLocationSaveRecord_t *pRecords, uint32_t nCount )
{
	if ( nCount != db.getLocationCount() || ( nCount && pRecords == NULL ) ) {
		return false;
	}
	const nmdLocationData_t *pHot = db.getLocationHotData();
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		if ( pRecords[i].nId != pHot[i].nId || pRecords[i].nStatus >= NMD_LOCATION_STATUS_COUNT ) {
			return false;
		}
	}
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		nmdLocationData_t hot = pHot[i];
		hot.nStateAndTraits = static_cast<uint16_t>(
			( hot.nStateAndTraits & NMD_LOCATION_TRAITS_MASK ) | pRecords[i].nStatus );
		hot.nPlayerRenownScore = pRecords[i].nPlayerRenownScore;
		hot.nPlayerFearScore = pRecords[i].nPlayerFearScore;
		if ( !db.setLocationData( db.getLocationHandle( pRecords[i].nId ), hot ) ) {
			return false;
		}
	}
	finalizeDomain( db, NMD_WORLD_DIRTY_LOCATION );
	return true;
}
