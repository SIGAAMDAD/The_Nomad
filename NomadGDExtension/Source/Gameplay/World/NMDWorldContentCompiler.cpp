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

#include "NMDWorldContentCompiler.h"
#include <cmath>
#include <limits>

bool NMDWorldContentCompiler::validateFact( nmdWorldFactType_t eType, nmdWorldValue_t value )
{
	return eType >= NMD_WORLD_FACT_FLOAT && eType <= NMD_WORLD_FACT_BOOLEAN &&
		( eType != NMD_WORLD_FACT_FLOAT || std::isfinite( value.asFloat() ) ) &&
		( eType != NMD_WORLD_FACT_BOOLEAN || value.nBits <= 1u );
}

bool NMDWorldContentCompiler::validateCondition( const NMDWorldDatabase& db,
	const nmdWorldCondition_t& condition )
{
	nmdWorldFactType_t type;
	nmdWorldValue_t value;
	if ( !db.getFact( condition.nFactId, type, value ) ) {
		return false;
	}
	return condition.eValueType == type && condition.eComparison <= NMD_WORLD_COMP_LTE &&
		( type != NMD_WORLD_FACT_BOOLEAN || condition.eComparison <= NMD_WORLD_COMP_NEQ );
}

bool NMDWorldContentCompiler::validateLocation( const NMDWorldDatabase& db,
	const nmdWorldLocationDefinition_t& definition,
	const nmdWorldCondition_t *pConditions, uint16_t nCount )
{
	if ( ( definition.nTraits & NMD_LOCATION_STATUS_MASK ) != 0u ||
		!std::isfinite( definition.fCompassRadius ) || definition.fCompassRadius < 0.0f ||
		( nCount && pConditions == NULL ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < 3u; ++i ) {
		if ( !std::isfinite( definition.position[i] ) ) {
			return false;
		}
	}
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		if ( !validateCondition( db, pConditions[i] ) ) {
			return false;
		}
	}
	return true;
}

bool NMDWorldContentCompiler::validateCalendar( const nmdCalendarDefinition_t& def )
{
	if ( def.nMonths == 0u || def.nMonths > NMD_CALENDAR_MONTH_COUNT ||
		def.nWeatherStepSeconds == 0u || def.nWeatherStepSeconds > 86400u ||
		86400u % def.nWeatherStepSeconds != 0u ||
		!std::isfinite( def.fWorldSecondsPerRealSecond ) ||
		def.fWorldSecondsPerRealSecond <= 0.0f ||
		!std::isfinite( def.fAxialTiltDegrees ) || def.fAxialTiltDegrees < 0.0f ||
		def.fAxialTiltDegrees > 90.0f || !std::isfinite( def.fMoonCycleDays ) ||
		def.fMoonCycleDays <= 0.0f ) {
		return false;
	}
	uint32_t days = 0u;
	for ( uint32_t i = 0u; i < def.nMonths; ++i ) {
		if ( def.nMonthLengths[i] == 0u ) {
			return false;
		}
		days += def.nMonthLengths[i];
	}
	if ( days > UINT16_MAX || def.nSeasonStartDays[0] != 0u ) {
		return false;
	}
	for ( uint32_t i = 0u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
		if ( def.nSeasonStartDays[i] >= days ||
			( i && def.nSeasonStartDays[i] <= def.nSeasonStartDays[i - 1u] ) ) {
			return false;
		}
	}
	return true;
}

bool NMDWorldContentCompiler::validateCalendarHot( const nmdCalendarDefinition_t& def,
	const nmdCalendarHot_t& hot )
{
	return std::isfinite( hot.fFractionalSeconds ) && hot.fFractionalSeconds >= 0.0 &&
		hot.fFractionalSeconds < 1.0 && def.nWeatherStepSeconds > 0u &&
		hot.nWeatherRemainderSeconds < def.nWeatherStepSeconds;
}

bool NMDWorldContentCompiler::validateSeasonal( const nmdClimateSeasonalCold_t& s )
{
	for ( uint32_t i = 0u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
		if ( !std::isfinite( s.fTemperatureC[i] ) ||
			!std::isfinite( s.fRelativeHumidity[i] ) ||
			!std::isfinite( s.fStormChancePerDay[i] ) ||
			s.fRelativeHumidity[i] < 0.0f || s.fRelativeHumidity[i] > 1.0f ||
			s.fStormChancePerDay[i] < 0.0f || s.fStormChancePerDay[i] > 1.0f ) {
			return false;
		}
	}
	return true;
}

bool NMDWorldContentCompiler::validateGeography( const nmdClimateGeographyCold_t& g )
{
	const float values[] = { g.fAreaKm2, g.fLatitudeDegrees, g.fElevationMeters,
		g.fSoilCapacityMm, g.fGroundwaterCapacityMm, g.fSurfaceCapacityMm,
		g.fEvaporationMultiplier, g.fAtmosphericRechargeMmPerHour,
		g.fPrevailingWindMps, g.fMeanPressureHpa };
	for ( float value : values ) {
		if ( !std::isfinite( value ) ) {
		return false;
	}
	}
	return g.eBiome < NMD_CLIMATE_BIOME_COUNT && g.fAreaKm2 > 0.0f &&
		g.fLatitudeDegrees >= -90.0f && g.fLatitudeDegrees <= 90.0f &&
		g.fSoilCapacityMm >= 0.0f && g.fGroundwaterCapacityMm >= 0.0f &&
		g.fSurfaceCapacityMm >= 0.0f && g.fEvaporationMultiplier >= 0.0f &&
		g.fAtmosphericRechargeMmPerHour >= 0.0f && g.fPrevailingWindMps >= 0.0f &&
		g.fMeanPressureHpa >= 0.0f;
}

bool NMDWorldContentCompiler::validateWeather( const nmdWeatherHot_t& w )
{
	return w.eWeather < NMD_WEATHER_COUNT && w.eTargetWeather < NMD_WEATHER_COUNT &&
		std::isfinite( w.fTemperatureC ) && std::isfinite( w.fRelativeHumidity ) &&
		std::isfinite( w.fCloudCover ) && std::isfinite( w.fWindSpeedMps ) &&
		std::isfinite( w.fWindAngleRadians ) && std::isfinite( w.fPrecipitationMmPerHour ) &&
		std::isfinite( w.fPressureHpa ) && std::isfinite( w.fVisibilityMeters ) &&
		std::isfinite( w.fTargetTemperatureC ) && std::isfinite( w.fTargetHumidity ) &&
		std::isfinite( w.fTargetWindSpeedMps ) && std::isfinite( w.fFrontIntensity ) &&
		w.fRelativeHumidity >= 0.0f && w.fRelativeHumidity <= 1.0f &&
		w.fTargetHumidity >= 0.0f && w.fTargetHumidity <= 1.0f &&
		w.fCloudCover >= 0.0f && w.fCloudCover <= 1.0f &&
		w.fWindSpeedMps >= 0.0f && w.fTargetWindSpeedMps >= 0.0f &&
		w.fPrecipitationMmPerHour >= 0.0f && w.fPressureHpa >= 0.0f &&
		w.fVisibilityMeters >= 0.0f && w.fFrontIntensity >= 0.0f;
}

bool NMDWorldContentCompiler::validateWater( const nmdWaterCycleHot_t& w )
{
	const float values[] = { w.fAtmosphericVaporMm, w.fCloudWaterMm, w.fSoilWaterMm,
		w.fGroundwaterMm, w.fSurfaceWaterMm, w.fSnowpackMm, w.fLastPrecipitationMm,
		w.fLastEvaporationMm, w.fLastRunoffMm, w.fLastInfiltrationMm };
	if ( w.eHydrology >= NMD_HYDROLOGY_COUNT ) {
		return false;
	}
	for ( float value : values ) {
		if ( !std::isfinite( value ) || value < 0.0f ) {
		return false;
	}
	}
	return true;
}

bool NMDWorldContentCompiler::validateWeatherEvent( const NMDWorldDatabase& db,
	const nmdWeatherTimelineEvent_t& event )
{
	uint32_t region = 0u;
	return db.getClimateIndex( event.nRegionId, region ) && event.nDurationSeconds != 0u &&
		event.nStartWorldSecond <= UINT64_MAX - event.nDurationSeconds &&
		event.eWeather < NMD_WEATHER_COUNT && std::isfinite( event.fTemperatureOffsetC ) &&
		std::isfinite( event.fRelativeHumidity ) && std::isfinite( event.fWindSpeedMps ) &&
		std::isfinite( event.fIntensityMmPerHour ) && event.fIntensityMmPerHour >= 0.0f &&
		event.fRelativeHumidity <= 1.0f;
}

bool NMDWorldContentCompiler::validateClimateLinks( const NMDWorldDatabase& db,
	const nmdClimateLinks_t& links )
{
	const uint32_t count = db.getClimateRegionCount();
	return std::isfinite( links.fDrainAreaScale ) && std::isfinite( links.fDownwindAreaScale ) &&
		links.fDrainAreaScale >= 0.0f && links.fDownwindAreaScale >= 0.0f &&
		( links.nDrainIndex == NMD_CLIMATE_REGION_INVALID_ID || links.nDrainIndex < count ) &&
		( links.nDownwindIndex == NMD_CLIMATE_REGION_INVALID_ID || links.nDownwindIndex < count );
}

nmdWorldFactId_t NMDWorldContentCompiler::registerFact( NMDWorldDatabase& db,
	nmdWorldAtom_t atom, nmdWorldFactType_t type, nmdWorldValue_t value )
{
	return validateFact( type, value ) ? db.setFactDefinition( atom, type, value ) :
		NMD_WORLD_FACT_INVALID_ID;
}

nmdWorldHandle_t NMDWorldContentCompiler::registerLocation( NMDWorldDatabase& db,
	nmdWorldAtom_t atom, const nmdWorldLocationDefinition_t& definition,
	const nmdWorldCondition_t *conditions, uint16_t count, nmdLocationId_t authoredId )
{
	const nmdWorldHandle_t invalid = { NMD_WORLD_HANDLE_INVALID };
	if ( !validateLocation( db, definition, conditions, count ) ) {
		return invalid;
	}
	const nmdWorldHandle_t existing = db.getRecordHandle( atom, NMD_WORLD_RECORD_LOCATION );
	if ( existing.isValid() ) {
		return existing;
	}
	uint32_t needed = 0u;
	for ( uint32_t i = 0u; i < count; ++i ) {
		bool duplicate = false;
		for ( uint32_t j = 0u; j < i; ++j ) {
			if ( conditions[j].nFactId == conditions[i].nFactId ) { duplicate = true; break; }
		}
		if ( !duplicate ) {
			++needed;
		}
	}
	nmdWorldArenaSegmentInfo_t segment = {};
	if ( !db.getArenaSegmentInfo( NMD_WORLD_ARENA_DEPENDENCY_BUILD, segment ) ||
		static_cast<uint64_t>( segment.nCount ) + needed > segment.nCommittedCapacity ) {
		return invalid;
	}
	const nmdWorldHandle_t result = db.setLocationDefinition( atom, definition, conditions, count, authoredId );
	if ( !result.isValid() ) {
		return result;
	}
	for ( uint32_t i = 0u; i < count; ++i ) {
		bool duplicate = false;
		for ( uint32_t j = 0u; j < i; ++j ) {
			if ( conditions[j].nFactId == conditions[i].nFactId ) { duplicate = true; break; }
		}
		if ( !duplicate ) {
			db.setFactDependency( conditions[i].nFactId, result, 0u );
		}
	}
	return result;
}

bool NMDWorldContentCompiler::seal( NMDWorldDatabase& db )
{
	if ( db.getArenaMode() != NMD_WORLD_ARENA_AUTHORING ||
		db.getClimateLinksRevision() != db.getClimateTopologyRevision() ||
		db.getWeatherTimelineIndexRevision() != db.getWeatherTimelineRevision() ) {
		return false;
	}
	return db.setArenaMode( NMD_WORLD_ARENA_SEALED_RUNTIME );
}
