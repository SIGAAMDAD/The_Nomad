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

#include "NMDWorldSimulation.h"
#include "../World/NMDClimateGraphCompiler.h"
#include "../World/NMDWeatherTimelineCompiler.h"
#include "../World/NMDWorldContentCompiler.h"
#include "../World/NMDWorldSnapshotCodec.h"

#include <cmath>

namespace
{
	static const uint32_t NMD_WORLD_DAY_SECONDS = 86400u;
	static const float NMD_WORLD_PI = 3.14159265358979323846f;

	static float ClampF( float value, float lower, float upper )
	{
		return value < lower ? lower : ( value > upper ? upper : value );
	}

	static float MinF( float left, float right )
	{
		return left < right ? left : right;
	}

	static float MaxF( float left, float right )
	{
		return left > right ? left : right;
	}

	static float TakeWater( float& pool, float requested )
	{
		const float amount = MinF( MaxF( 0.0f, requested ), MaxF( 0.0f, pool ) );
		pool -= amount;
		return amount;
	}

	static uint32_t NextRandom( uint32_t& state )
	{
		if ( state == 0u ) {
			state = 0xA341316Cu;
		}
		state ^= state << 13;
		state ^= state >> 17;
		state ^= state << 5;
		return state;
	}

	static float Random01( uint32_t& state )
	{
		return static_cast<float>( NextRandom( state ) >> 8 ) * ( 1.0f / 16777216.0f );
	}

	static bool IsFiniteNonnegative( float value )
	{
		return std::isfinite( value ) && value >= 0.0f;
	}

	static bool ValidateWaterState( const nmdWaterCycleHot_t& water )
	{
		return IsFiniteNonnegative( water.fAtmosphericVaporMm ) &&
			IsFiniteNonnegative( water.fCloudWaterMm ) &&
			IsFiniteNonnegative( water.fSoilWaterMm ) &&
			IsFiniteNonnegative( water.fGroundwaterMm ) &&
			IsFiniteNonnegative( water.fSurfaceWaterMm ) &&
			IsFiniteNonnegative( water.fSnowpackMm ) &&
			IsFiniteNonnegative( water.fLastPrecipitationMm ) &&
			IsFiniteNonnegative( water.fLastEvaporationMm ) &&
			IsFiniteNonnegative( water.fLastRunoffMm ) &&
			IsFiniteNonnegative( water.fLastInfiltrationMm ) &&
			water.eHydrology < NMD_HYDROLOGY_COUNT;
	}

	static bool IsPrecipitationWeather( uint8_t weather )
	{
		return weather == NMD_WEATHER_RAIN || weather == NMD_WEATHER_THUNDERSTORM ||
			weather == NMD_WEATHER_SNOW || weather == NMD_WEATHER_BLIZZARD;
	}

	static uint32_t CountYearDays( const nmdCalendarDefinition_t& definition )
	{
		uint32_t days = 0u;
		for ( uint32_t i = 0u; i < definition.nMonths; ++i ) {
			days += definition.nMonthLengths[ i ];
		}
		return days;
	}

	static nmdHydrologyState_t ClassifyWater( const nmdWaterCycleHot_t& water,
		const nmdClimateGeographyCold_t& geo )
	{
		if ( geo.fSurfaceCapacityMm > 0.0f &&
			water.fSurfaceWaterMm > geo.fSurfaceCapacityMm ) {
			return NMD_HYDROLOGY_FLOOD;
		}
		if ( geo.fSoilCapacityMm > 0.0f &&
			water.fSoilWaterMm < geo.fSoilCapacityMm * 0.12f ) {
			return NMD_HYDROLOGY_DROUGHT;
		}
		if ( geo.fSoilCapacityMm > 0.0f &&
			water.fSoilWaterMm > geo.fSoilCapacityMm * 0.88f ) {
			return NMD_HYDROLOGY_SATURATED;
		}
		return NMD_HYDROLOGY_NORMAL;
	}
}

NMDWorldSimulation::NMDWorldSimulation( void ) :
	m_pDatabase( NULL ), m_nCalendarYearDays( 0u ), m_nSimulationSeed( 0x4E4D4431u ),
	m_bCalendarInitialized( false ), m_bClimateGraphReady( true ), m_bSimulationAdvancing( false ),
	m_nNextTimedEventWorldSecond( UINT64_MAX ), m_pCalendarChanged( NULL ), m_pWorldTimed( NULL ),
	m_pRegionWeatherChanged( NULL ), m_pHydrologyChanged( NULL ), m_pSimulationCallbackUser( NULL )
{
}

void NMDWorldSimulation::setDatabase( NMDWorldDatabase *pDatabase )
{
	m_pDatabase = pDatabase;
	m_bCalendarInitialized = false;
	m_nCalendarYearDays = 0u;
	m_bClimateGraphReady = pDatabase != NULL &&
		( pDatabase->getClimateRegionCount() == 0u ||
		pDatabase->getClimateLinksRevision() == pDatabase->getClimateTopologyRevision() );
	m_nNextTimedEventWorldSecond = UINT64_MAX;
	m_RegionFlux.clear();
	m_WeatherScratch.clear();
	m_WaterScratch.clear();
	m_CalendarHotCache = {};
	m_CalendarDefinitionCache = {};
	m_TimedNextDue.clear();
	if ( m_pDatabase == NULL ) {
		return;
	}
	nmdCalendarDefinition_t def;
	if ( m_pDatabase->getCalendarDefinition( def ) ) {
		m_CalendarDefinitionCache = def;
		m_pDatabase->getCalendarHot( m_CalendarHotCache );
		m_bCalendarInitialized = true;
		m_nCalendarYearDays = CountYearDays( def );
	}
	m_RegionFlux.resize( m_pDatabase->getClimateRegionCount() );
	m_WeatherScratch.resize( m_pDatabase->getClimateRegionCount() );
	m_WaterScratch.resize( m_pDatabase->getClimateRegionCount() );
	rebuildTimedEventDeadlines();
}

NMDWorldDatabase *NMDWorldSimulation::getDatabase( void ) const
{
	return m_pDatabase;
}

bool NMDWorldSimulation::initializeCalendar(
	const nmdCalendarDefinition_t& definition,
	uint64_t nInitialWorldSeconds, uint32_t nSeed )
{
	if ( m_pDatabase == NULL || m_bSimulationAdvancing || !( m_pDatabase->getClimateRegionCount() == 0u ) || !( m_pDatabase->getWorldTimedEventCount() == 0u ) ||
		definition.nMonths != NMD_CALENDAR_MONTH_COUNT ||
		definition.nWeatherStepSeconds == 0u ||
		definition.nWeatherStepSeconds > NMD_WORLD_DAY_SECONDS ||
		( NMD_WORLD_DAY_SECONDS % definition.nWeatherStepSeconds ) != 0u ||
		!std::isfinite( definition.fWorldSecondsPerRealSecond ) ||
		definition.fWorldSecondsPerRealSecond <= 0.0f ||
		!std::isfinite( definition.fAxialTiltDegrees ) ||
		definition.fAxialTiltDegrees < 0.0f || definition.fAxialTiltDegrees > 90.0f ||
		!std::isfinite( definition.fMoonCycleDays ) ||
		definition.fMoonCycleDays <= 0.0f ) {
		return false;
	}

	for ( uint32_t i = 0u; i < NMD_CALENDAR_MONTH_COUNT; ++i ) {
		if ( definition.nMonthLengths[ i ] == 0u ||
			definition.nMonthLengths[ i ] > 255u ) {
			return false;
		}
	}
	const uint32_t yearDays = CountYearDays( definition );
	if ( yearDays == 0u || yearDays > 0xFFFFu ||
		definition.nSeasonStartDays[ 0 ] != 0u ) {
		return false;
	}
	for ( uint32_t i = 1u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
		if ( definition.nSeasonStartDays[ i ] <= definition.nSeasonStartDays[ i - 1u ] ||
			definition.nSeasonStartDays[ i ] >= yearDays ) {
			return false;
		}
	}
	if ( nInitialWorldSeconds / NMD_WORLD_DAY_SECONDS / yearDays >= UINT32_MAX ) {
		return false;
	}

	if ( !m_pDatabase->setCalendarDefinition( definition ) ) {
		return false;
	}
	m_nCalendarYearDays = yearDays;
	m_CalendarDefinitionCache = definition;
	nmdCalendarHot_t hot = {};
	hot.nWorldSeconds = nInitialWorldSeconds;
	hot.nWeatherRemainderSeconds = static_cast<uint32_t>(
		nInitialWorldSeconds % definition.nWeatherStepSeconds );
	if ( !m_pDatabase->setCalendarHot( hot ) ) {
		return false;
	}
	m_CalendarHotCache = hot;
	m_nSimulationSeed = nSeed != 0u ? nSeed : 0x4E4D4431u;
	m_bCalendarInitialized = true;
	return true;
}

bool NMDWorldSimulation::getCalendarDateAt(
	uint64_t nWorldSeconds, nmdCalendarDate_t& date ) const
{
	if ( !m_bCalendarInitialized || m_nCalendarYearDays == 0u ) {
		return false;
	}
	const uint64_t nAbsoluteDay = nWorldSeconds / NMD_WORLD_DAY_SECONDS;
	const uint64_t nYear = nAbsoluteDay / m_nCalendarYearDays + 1u;
	if ( nYear > UINT32_MAX || nAbsoluteDay > UINT32_MAX ) {
		return false;
	}
	const uint32_t nDayOfYear = static_cast<uint32_t>(
		nAbsoluteDay % m_nCalendarYearDays );
	uint32_t nMonth = 0u;
	uint32_t nDayWithinMonth = nDayOfYear;
	while ( nMonth + 1u < m_CalendarDefinitionCache.nMonths &&
		nDayWithinMonth >= m_CalendarDefinitionCache.nMonthLengths[ nMonth ] ) {
		nDayWithinMonth -= m_CalendarDefinitionCache.nMonthLengths[ nMonth ];
		++nMonth;
	}
	uint32_t nSeason = 0u;
	for ( uint32_t i = 1u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
		if ( nDayOfYear >= m_CalendarDefinitionCache.nSeasonStartDays[ i ] ) {
			nSeason = i;
		}
	}
	const uint32_t seconds = static_cast<uint32_t>(
		nWorldSeconds % NMD_WORLD_DAY_SECONDS );
	date = {};
	date.nYear = static_cast<uint32_t>( nYear );
	date.nDayOfYear = static_cast<uint16_t>( nDayOfYear );
	date.nMonth = static_cast<uint8_t>( nMonth + 1u );
	date.nDayOfMonth = static_cast<uint8_t>( nDayWithinMonth + 1u );
	date.nHour = static_cast<uint8_t>( seconds / 3600u );
	date.nMinute = static_cast<uint8_t>( ( seconds / 60u ) % 60u );
	date.nSecond = static_cast<uint8_t>( seconds % 60u );
	date.nSeason = static_cast<uint8_t>( nSeason );
	date.nAbsoluteDay = static_cast<uint32_t>( nAbsoluteDay );
	return true;
}

bool NMDWorldSimulation::getCalendarDate( nmdCalendarDate_t& date ) const
{
	nmdCalendarHot_t clock = {};
	return m_pDatabase != NULL && m_pDatabase->getCalendarHot( clock ) &&
		getCalendarDateAt( clock.nWorldSeconds, date );
}

bool NMDWorldSimulation::getCalendarHot( nmdCalendarHot_t& hot ) const
{
	if ( !m_bCalendarInitialized ) {
		return false;
	}
	return m_pDatabase->getCalendarHot( hot );
}

bool NMDWorldSimulation::getCalendarDefinition( nmdCalendarDefinition_t& definition ) const
{
	if ( !m_bCalendarInitialized ) {
		return false;
	}
	return m_pDatabase->getCalendarDefinition( definition );
}

uint64_t NMDWorldSimulation::getWorldSeconds( void ) const
{
	return m_pDatabase == NULL ? 0u : m_pDatabase->getWorldSeconds();
}

bool NMDWorldSimulation::getCelestialSample( nmdClimateRegionId_t nRegionId,
	nmdCelestialSample_t& sample ) const
{
	uint32_t index = 0u;
	nmdCalendarDate_t date;
	if ( !getClimateIndex( nRegionId, index ) || !getCalendarDate( date ) ) {
		return false;
	}
	nmdCalendarHot_t clock = {};
	if ( !m_pDatabase->getCalendarHot( clock ) ) {
		return false;
	}
	const float degToRad = NMD_WORLD_PI / 180.0f;
	const float latitude = m_pDatabase->getClimateGeographyColdData()[ index ].fLatitudeDegrees * degToRad;
	const double absoluteDay = static_cast<double>( clock.nWorldSeconds ) /
		NMD_WORLD_DAY_SECONDS;
	const float annualPhase = static_cast<float>(
		( static_cast<double>( date.nDayOfYear ) +
		static_cast<double>( clock.nWorldSeconds % NMD_WORLD_DAY_SECONDS ) /
		NMD_WORLD_DAY_SECONDS ) / m_nCalendarYearDays );
	const float tilt = m_CalendarDefinitionCache.fAxialTiltDegrees * degToRad;
	const float declination = tilt * std::sin( 2.0f * NMD_WORLD_PI *
		( annualPhase - 0.25f ) );
	const float hourAngle = 2.0f * NMD_WORLD_PI * (
		static_cast<float>( clock.nWorldSeconds % NMD_WORLD_DAY_SECONDS ) /
		NMD_WORLD_DAY_SECONDS - 0.5f );
	const float sinAltitude = std::sin( latitude ) * std::sin( declination ) +
		std::cos( latitude ) * std::cos( declination ) * std::cos( hourAngle );
	sample = {};
	sample.fSunElevationRadians = std::asin( ClampF( sinAltitude, -1.0f, 1.0f ) );
	sample.fSunAzimuthRadians = std::atan2( -std::sin( hourAngle ),
		std::tan( declination ) * std::cos( latitude ) -
		std::sin( latitude ) * std::cos( hourAngle ) );
	sample.fSunlightFactor = ClampF( sinAltitude * 4.0f + 0.02f, 0.0f, 1.0f );
	const float daylightCosine = -std::tan( latitude ) * std::tan( declination );
	sample.fDaylightHours = std::acos( ClampF( daylightCosine, -1.0f, 1.0f ) ) *
		24.0f / NMD_WORLD_PI;
	sample.fMoonPhase01 = static_cast<float>( std::fmod(
		absoluteDay / m_CalendarDefinitionCache.fMoonCycleDays, 1.0 ) );
	sample.fMoonIllumination = ( 1.0f - std::cos(
		2.0f * NMD_WORLD_PI * sample.fMoonPhase01 ) ) * 0.5f;
	// Stylized moon: approximately opposite the solar hour angle, with a
	// lunar declination adjustment. Lighting nodes can override this ephemeris.
	const float lunarDeclination = -declination + 0.09f * std::sin(
		2.0f * NMD_WORLD_PI * sample.fMoonPhase01 );
	const float lunarHour = hourAngle + NMD_WORLD_PI;
	const float moonSinAltitude = std::sin( latitude ) * std::sin( lunarDeclination ) +
		std::cos( latitude ) * std::cos( lunarDeclination ) * std::cos( lunarHour );
	sample.fMoonElevationRadians = std::asin( ClampF( moonSinAltitude, -1.0f, 1.0f ) );
	sample.fSolarDeclinationRadians = declination;
	return true;
}

bool NMDWorldSimulation::getClimateIndex( nmdClimateRegionId_t nRegionId,
	uint32_t& nIndex ) const
{
	return m_pDatabase != NULL && m_pDatabase->getClimateIndex( nRegionId, nIndex );
}

bool NMDWorldSimulation::reserveClimateRegions( uint32_t nRegions,
	uint32_t nTimedEvents )
{
	if ( nRegions > NMD_CLIMATE_MAX_REGION_COUNT || m_bSimulationAdvancing ) {
		return false;
	}
	nmdWorldDatabaseCapacities_t capacities = {};
	m_pDatabase->getCapacities( capacities );
	if ( nRegions > capacities.nRegions ) {
		capacities.nRegions = nRegions;
	}
	if ( nTimedEvents > capacities.nTimedEvents ) {
		capacities.nTimedEvents = nTimedEvents;
	}
	if ( !m_pDatabase->setCapacities( capacities ) ) {
		return false;
	}
	m_RegionFlux.reserve( nRegions );
	m_WeatherScratch.reserve( nRegions );
	m_WaterScratch.reserve( nRegions );
	m_TimedNextDue.reserve( nTimedEvents );
	return true;
}

float NMDWorldSimulation::sampleSeason( const float *pValues,
	uint64_t nWorldSeconds ) const
{
	const double day = static_cast<double>( nWorldSeconds %
		( static_cast<uint64_t>( m_nCalendarYearDays ) * NMD_WORLD_DAY_SECONDS ) ) /
		NMD_WORLD_DAY_SECONDS;
	uint32_t season = 0u;
	for ( uint32_t i = 1u; i < NMD_CALENDAR_SEASON_COUNT; ++i ) {
		if ( day >= m_CalendarDefinitionCache.nSeasonStartDays[ i ] ) {
			season = i;
		}
	}
	const uint32_t nextSeason = ( season + 1u ) % NMD_CALENDAR_SEASON_COUNT;
	const double begin = m_CalendarDefinitionCache.nSeasonStartDays[ season ];
	const double end = nextSeason == 0u ? m_nCalendarYearDays :
		m_CalendarDefinitionCache.nSeasonStartDays[ nextSeason ];
	const float blend = static_cast<float>( ( day - begin ) / ( end - begin ) );
	return pValues[ season ] + ( pValues[ nextSeason ] - pValues[ season ] ) * blend;
}

bool NMDWorldSimulation::registerClimateRegion(
	nmdClimateRegionId_t nRegionId,
	const nmdClimateSeasonalCold_t& seasonal,
	const nmdClimateGeographyCold_t& geography,
	const nmdWaterCycleHot_t *pInitialWater )
{
	if ( !m_bCalendarInitialized || m_bSimulationAdvancing ||
		nRegionId == NMD_CLIMATE_REGION_INVALID_ID ||
		m_pDatabase->getClimateRegionCount() >= NMD_CLIMATE_MAX_REGION_COUNT ||
		geography.eBiome >= NMD_CLIMATE_BIOME_COUNT ||
		!std::isfinite( geography.fLatitudeDegrees ) ||
		geography.fLatitudeDegrees < -90.0f || geography.fLatitudeDegrees > 90.0f ||
		!std::isfinite( geography.fElevationMeters ) ||
		!IsFiniteNonnegative( geography.fAreaKm2 ) || geography.fAreaKm2 <= 0.0f ||
		!IsFiniteNonnegative( geography.fSoilCapacityMm ) ||
		!IsFiniteNonnegative( geography.fGroundwaterCapacityMm ) ||
		!IsFiniteNonnegative( geography.fSurfaceCapacityMm ) ||
		!IsFiniteNonnegative( geography.fEvaporationMultiplier ) ||
		!IsFiniteNonnegative( geography.fAtmosphericRechargeMmPerHour ) ||
		!IsFiniteNonnegative( geography.fPrevailingWindMps ) ||
		!IsFiniteNonnegative( geography.fMeanPressureHpa ) ||
		geography.fMeanPressureHpa < 100.0f ||
		( pInitialWater != NULL && !ValidateWaterState( *pInitialWater ) ) ) {
		return false;
	}

	uint32_t unusedIndex = 0u;
	if ( getClimateIndex( nRegionId, unusedIndex ) ) {
		return false;
	}
	for ( uint32_t season = 0u; season < NMD_CALENDAR_SEASON_COUNT; ++season ) {
		if ( !std::isfinite( seasonal.fTemperatureC[ season ] ) ||
			!std::isfinite( seasonal.fRelativeHumidity[ season ] ) ||
			seasonal.fRelativeHumidity[ season ] < 0.0f ||
			seasonal.fRelativeHumidity[ season ] > 1.0f ||
			!std::isfinite( seasonal.fStormChancePerDay[ season ] ) ||
			seasonal.fStormChancePerDay[ season ] < 0.0f ||
			seasonal.fStormChancePerDay[ season ] > 1.0f ) {
			return false;
		}
	}

	const float meanTemperature = sampleSeason( seasonal.fTemperatureC, m_CalendarHotCache.nWorldSeconds ) -
		0.0065f * geography.fElevationMeters;
	const float relativeHumidity = sampleSeason(
		seasonal.fRelativeHumidity, m_CalendarHotCache.nWorldSeconds );
	nmdWaterCycleHot_t water = {};
	if ( pInitialWater != NULL ) {
		water = *pInitialWater;
	} else {
		water.fAtmosphericVaporMm = 8.0f * relativeHumidity *
			std::exp( ClampF( 0.065f * meanTemperature, -4.0f, 4.0f ) );
		water.fSoilWaterMm = geography.fSoilCapacityMm * relativeHumidity * 0.5f;
		water.fGroundwaterMm = geography.fGroundwaterCapacityMm * relativeHumidity * 0.3f;
		water.fSurfaceWaterMm = geography.fSurfaceCapacityMm * relativeHumidity * 0.1f;
	}
	water.eHydrology = ClassifyWater( water, geography );

	nmdWeatherHot_t weather = {};
	weather.fTemperatureC = meanTemperature;
	weather.fRelativeHumidity = relativeHumidity;
	weather.fPressureHpa = geography.fMeanPressureHpa;
	weather.fWindSpeedMps = geography.fPrevailingWindMps;
	weather.fVisibilityMeters = 20000.0f;
	weather.fTargetTemperatureC = meanTemperature;
	weather.fTargetHumidity = relativeHumidity;
	weather.fTargetWindSpeedMps = geography.fPrevailingWindMps;
	weather.fFrontIntensity = 0.0f;
	weather.nRandomState = m_nSimulationSeed ^ (
		static_cast<uint32_t>( nRegionId + 1u ) * 0x9E3779B9u );
	weather.nFrontRemainingSeconds = 0u;
	weather.eWeather = NMD_WEATHER_CLEAR;
	weather.eTargetWeather = NMD_WEATHER_CLEAR;

	if ( !NMDWorldContentCompiler::validateSeasonal( seasonal ) ||
		!NMDWorldContentCompiler::validateGeography( geography ) ||
		!NMDWorldContentCompiler::validateWeather( weather ) ||
		!NMDWorldContentCompiler::validateWater( water ) ||
		!m_pDatabase->setClimateRegion( nRegionId, seasonal, geography, weather, water ) ) {
		return false;
	}
	m_RegionFlux.push_back( nmdWaterFlux_t{} );
	m_WeatherScratch.push_back( weather );
	m_WaterScratch.push_back( water );
	m_bClimateGraphReady = false;
	return true;
}

bool NMDWorldSimulation::buildClimateGraph( void )
{
	if ( m_pDatabase == NULL || m_bSimulationAdvancing ) {
		return false;
	}
	const uint32_t count = m_pDatabase->getClimateRegionCount();
	m_RegionFlux.resize( count );
	m_bClimateGraphReady = NMDClimateGraphCompiler::build( *m_pDatabase );
	return m_bClimateGraphReady;
}

bool NMDWorldSimulation::buildWeatherTimelineIndex( void )
{
	return m_pDatabase != NULL && !m_bSimulationAdvancing &&
		NMDWeatherTimelineCompiler::rebuild( *m_pDatabase );
}

uint32_t NMDWorldSimulation::getClimateRegionCount( void ) const
{
	return static_cast<uint32_t>( m_pDatabase->getClimateRegionCount() );
}

bool NMDWorldSimulation::getClimateSeasonal( nmdClimateRegionId_t nRegionId,
	nmdClimateSeasonalCold_t& seasonal ) const
{
	uint32_t index = 0u;
	if ( !getClimateIndex( nRegionId, index ) ) {
		return false;
	}
	seasonal = m_pDatabase->getClimateSeasonalColdData()[ index ];
	return true;
}

bool NMDWorldSimulation::getClimateGeography( nmdClimateRegionId_t nRegionId,
	nmdClimateGeographyCold_t& geography ) const
{
	uint32_t index = 0u;
	if ( !getClimateIndex( nRegionId, index ) ) {
		return false;
	}
	geography = m_pDatabase->getClimateGeographyColdData()[ index ];
	return true;
}

bool NMDWorldSimulation::getRegionWeather( nmdClimateRegionId_t nRegionId,
	nmdWeatherHot_t& weather ) const
{
	uint32_t index = 0u;
	if ( !getClimateIndex( nRegionId, index ) ) {
		return false;
	}
	return m_pDatabase->getRegionWeather( nRegionId, weather );
}

bool NMDWorldSimulation::getRegionWaterCycle( nmdClimateRegionId_t nRegionId,
	nmdWaterCycleHot_t& water ) const
{
	uint32_t index = 0u;
	if ( !getClimateIndex( nRegionId, index ) ) {
		return false;
	}
	return m_pDatabase->getRegionWaterCycle( nRegionId, water );
}

bool NMDWorldSimulation::getLocationWeather( nmdWorldHandle_t hLocation,
	nmdWeatherHot_t& weather ) const
{
	nmdWorldLocationCold_t cold;
	return m_pDatabase->getLocationDefinition( hLocation, cold ) &&
		cold.nRegionId < NMD_CLIMATE_REGION_INVALID_ID &&
		getRegionWeather( static_cast<nmdClimateRegionId_t>( cold.nRegionId ), weather );
}

bool NMDWorldSimulation::getBlendedWeather(
	nmdClimateRegionId_t nFromRegion, nmdClimateRegionId_t nToRegion,
	float fBlend, nmdWeatherSample_t& sample ) const
{
	uint32_t from = 0u, to = 0u;
	if ( !std::isfinite( fBlend ) || !getClimateIndex( nFromRegion, from ) ||
		!getClimateIndex( nToRegion, to ) ) {
		return false;
	}
	const nmdWeatherHot_t& a = m_pDatabase->getRegionWeatherHotData()[ from ];
	const nmdWeatherHot_t& b = m_pDatabase->getRegionWeatherHotData()[ to ];
	const float t = ClampF( fBlend, 0.0f, 1.0f );
	sample = {};
	sample.fTemperatureC = a.fTemperatureC + ( b.fTemperatureC - a.fTemperatureC ) * t;
	sample.fRelativeHumidity = a.fRelativeHumidity + ( b.fRelativeHumidity - a.fRelativeHumidity ) * t;
	sample.fCloudCover = a.fCloudCover + ( b.fCloudCover - a.fCloudCover ) * t;
	sample.fWindSpeedMps = a.fWindSpeedMps + ( b.fWindSpeedMps - a.fWindSpeedMps ) * t;
	sample.fPrecipitationMmPerHour = a.fPrecipitationMmPerHour +
		( b.fPrecipitationMmPerHour - a.fPrecipitationMmPerHour ) * t;
	sample.fVisibilityMeters = a.fVisibilityMeters + ( b.fVisibilityMeters - a.fVisibilityMeters ) * t;
	sample.fPressureHpa = a.fPressureHpa + ( b.fPressureHpa - a.fPressureHpa ) * t;
	sample.eWeather = t < 0.5f ? a.eWeather : b.eWeather;
	return true;
}

void NMDWorldSimulation::reserveWeatherTimelineEvents( uint32_t nEvents )
{
	if ( !m_bSimulationAdvancing && m_pDatabase != NULL ) {
		nmdWorldDatabaseCapacities_t capacities = {};
		m_pDatabase->getCapacities( capacities );
		if ( nEvents > capacities.nWeatherEvents ) {
			capacities.nWeatherEvents = nEvents;
			m_pDatabase->setCapacities( capacities );
		}
	}
}

bool NMDWorldSimulation::registerWeatherTimelineEvent(
	const nmdWeatherTimelineEvent_t& event )
{
	uint32_t index = 0u;
	if ( m_bSimulationAdvancing || !m_bCalendarInitialized ||
		!getClimateIndex( event.nRegionId, index ) ||
		event.nDurationSeconds == 0u ||
		event.nStartWorldSecond > UINT64_MAX - event.nDurationSeconds ||
		event.eWeather >= NMD_WEATHER_COUNT ||
		!std::isfinite( event.fTemperatureOffsetC ) ||
		!std::isfinite( event.fRelativeHumidity ) ||
		event.fRelativeHumidity > 1.0f ||
		!std::isfinite( event.fWindSpeedMps ) ||
		!IsFiniteNonnegative( event.fIntensityMmPerHour ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < m_pDatabase->getWeatherTimelineEventCount(); ++i ) {
		if ( m_pDatabase->getWeatherTimelineEventData()[ i ].nEventId == event.nEventId ) {
			return false;
		}
	}
	return m_pDatabase->setWeatherTimelineEvent( event );
}

bool NMDWorldSimulation::registerWorldTimedEvent( const nmdWorldTimedEvent_t& event )
{
	if ( !m_bCalendarInitialized || m_bSimulationAdvancing ||
		( event.nPeriodSeconds > 0u &&
			event.nFirstWorldSecond > UINT64_MAX - event.nPeriodSeconds ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < m_pDatabase->getWorldTimedEventCount(); ++i ) {
		if ( m_pDatabase->getWorldTimedEventData()[ i ].nEventId == event.nEventId ) {
			return false;
		}
	}
	if ( !m_pDatabase->setWorldTimedEvent( event ) ) {
		return false;
	}
	rebuildTimedEventDeadlines();
	return true;
}

void NMDWorldSimulation::rebuildTimedEventDeadlines( void )
{
	m_nNextTimedEventWorldSecond = UINT64_MAX;
	m_TimedNextDue.resize( m_pDatabase->getWorldTimedEventCount(), UINT64_MAX );
	for ( uint32_t i = 0u; i < m_pDatabase->getWorldTimedEventCount(); ++i ) {
		const nmdWorldTimedEvent_t& event = m_pDatabase->getWorldTimedEventData()[ i ];
		uint64_t next = event.nFirstWorldSecond;
		if ( next <= m_CalendarHotCache.nWorldSeconds ) {
			if ( event.nPeriodSeconds == 0u ) {
				next = UINT64_MAX;
			} else {
				const uint64_t periods =
					( m_CalendarHotCache.nWorldSeconds - next ) / event.nPeriodSeconds + 1u;
				next = periods > ( UINT64_MAX - next ) / event.nPeriodSeconds
					? UINT64_MAX : next + periods * event.nPeriodSeconds;
			}
		}
		m_TimedNextDue[ i ] = next;
		if ( next < m_nNextTimedEventWorldSecond ) {
			m_nNextTimedEventWorldSecond = next;
		}
	}
}

void NMDWorldSimulation::dispatchTimedEvents(
	uint64_t nPreviousSeconds, uint64_t nNewSeconds )
{
	if ( m_nNextTimedEventWorldSecond > nNewSeconds ) {
		return;
	}
	m_nNextTimedEventWorldSecond = UINT64_MAX;
	m_TimedNextDue.resize( m_pDatabase->getWorldTimedEventCount(), UINT64_MAX );
	for ( uint32_t i = 0u; i < m_pDatabase->getWorldTimedEventCount(); ++i ) {
		const nmdWorldTimedEvent_t& event = m_pDatabase->getWorldTimedEventData()[ i ];
		const uint64_t due = m_TimedNextDue[ i ];
		if ( due == UINT64_MAX || due <= nPreviousSeconds || due > nNewSeconds ) {
			if ( due < m_nNextTimedEventWorldSecond ) {
				m_nNextTimedEventWorldSecond = due;
			}
			continue;
		}
		const uint64_t count = event.nPeriodSeconds == 0u ? 1u :
			( nNewSeconds - due ) / event.nPeriodSeconds + 1u;
		if ( m_pWorldTimed != NULL ) {
			nmdWorldTimedEventTriggered_t triggered = {};
			triggered.nFirstDueWorldSecond = due;
			triggered.nEventId = event.nEventId;
			triggered.nOccurrences = static_cast<uint32_t>( count );
			m_pWorldTimed( m_pSimulationCallbackUser, triggered );
		}
		if ( event.nPeriodSeconds == 0u ||
			count > ( UINT64_MAX - due ) / event.nPeriodSeconds ) {
			m_TimedNextDue[ i ] = UINT64_MAX;
		} else {
			m_TimedNextDue[ i ] = due + count * event.nPeriodSeconds;
		}
		if ( m_TimedNextDue[ i ] < m_nNextTimedEventWorldSecond ) {
			m_nNextTimedEventWorldSecond = m_TimedNextDue[ i ];
		}
	}
}

void NMDWorldSimulation::advanceCalendarNotifications(
	uint64_t nPreviousSeconds, uint64_t nNewSeconds )
{
	if ( m_pCalendarChanged == NULL ||
		( nPreviousSeconds / NMD_WORLD_DAY_SECONDS ) ==
		( nNewSeconds / NMD_WORLD_DAY_SECONDS ) ) {
		return;
	}
	nmdCalendarDate_t previous, current;
	if ( !getCalendarDateAt( nPreviousSeconds, previous ) ||
		!getCalendarDateAt( nNewSeconds, current ) ) {
		return;
	}
	const uint32_t days = current.nAbsoluteDay - previous.nAbsoluteDay;
	if ( days == 0u ) {
		return;
	}
	nmdCalendarChangedEvent_t event = {};
	event.nWorldSecond = nNewSeconds;
	event.nDaysElapsed = days;
	event.eChange = NMD_CALENDAR_CHANGE_DAY;
	event.nPrevious = previous.nDayOfYear;
	event.nCurrent = current.nDayOfYear;
	m_pCalendarChanged( m_pSimulationCallbackUser, event );
	if ( previous.nMonth != current.nMonth || previous.nYear != current.nYear ) {
		event.eChange = NMD_CALENDAR_CHANGE_MONTH;
		event.nPrevious = previous.nMonth;
		event.nCurrent = current.nMonth;
		m_pCalendarChanged( m_pSimulationCallbackUser, event );
	}
	if ( previous.nSeason != current.nSeason || previous.nYear != current.nYear ) {
		event.eChange = NMD_CALENDAR_CHANGE_SEASON;
		event.nPrevious = previous.nSeason;
		event.nCurrent = current.nSeason;
		m_pCalendarChanged( m_pSimulationCallbackUser, event );
	}
	if ( previous.nYear != current.nYear ) {
		event.eChange = NMD_CALENDAR_CHANGE_YEAR;
		event.nPrevious = previous.nYear;
		event.nCurrent = current.nYear;
		m_pCalendarChanged( m_pSimulationCallbackUser, event );
	}
}

bool NMDWorldSimulation::advanceWorldSeconds( uint32_t nSeconds )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	if ( !m_bCalendarInitialized ) {
		setDatabase( m_pDatabase );
	}
	if ( m_RegionFlux.size() != m_pDatabase->getClimateRegionCount() ||
		m_WeatherScratch.size() != m_pDatabase->getClimateRegionCount() ||
		m_WaterScratch.size() != m_pDatabase->getClimateRegionCount() ) {
		return false;
	}
	if ( !m_pDatabase->getCalendarHot( m_CalendarHotCache ) ||
		!m_pDatabase->getCalendarDefinition( m_CalendarDefinitionCache ) ) {
		return false;
	}
	for ( uint32_t i = 0u; i < m_pDatabase->getClimateRegionCount(); ++i ) {
		m_WeatherScratch[i] = m_pDatabase->getRegionWeatherHotData()[i];
		m_WaterScratch[i] = m_pDatabase->getRegionWaterHotData()[i];
	}
	if ( !m_bCalendarInitialized || !m_bClimateGraphReady ||
		m_bSimulationAdvancing ||
		( !( m_pDatabase->getClimateRegionCount() == 0u ) &&
		( m_pDatabase->getClimateLinksRevision() != m_pDatabase->getClimateTopologyRevision() ||
		m_pDatabase->getWeatherTimelineIndexRevision() != m_pDatabase->getWeatherTimelineRevision() ) ) ||
		UINT64_MAX - m_CalendarHotCache.nWorldSeconds < nSeconds ) {
		return false;
	}
	if ( nSeconds == 0u ) {
		return true;
	}
	// Date lookup uses a uint32 day index; fail before mutating any state.
	nmdCalendarDate_t date;
	if ( !getCalendarDateAt( m_CalendarHotCache.nWorldSeconds + nSeconds, date ) ) {
		return false;
	}
	m_bSimulationAdvancing = true;
	const nmdCalendarHot_t previousClock = m_CalendarHotCache;
	const uint64_t previous = previousClock.nWorldSeconds;
	const uint32_t stepSeconds = m_CalendarDefinitionCache.nWeatherStepSeconds;
	uint32_t remaining = nSeconds;
	if ( ( m_pDatabase->getClimateRegionCount() == 0u ) ) {
		m_CalendarHotCache.nWorldSeconds += remaining;
		m_CalendarHotCache.nWeatherRemainderSeconds = static_cast<uint32_t>(
			m_CalendarHotCache.nWorldSeconds % stepSeconds );
	} else {
		while ( remaining != 0u ) {
			const uint32_t toStep = stepSeconds - m_CalendarHotCache.nWeatherRemainderSeconds;
			const uint32_t delta = remaining < toStep ? remaining : toStep;
			m_CalendarHotCache.nWorldSeconds += delta;
			m_CalendarHotCache.nWeatherRemainderSeconds += delta;
			remaining -= delta;
			if ( m_CalendarHotCache.nWeatherRemainderSeconds == stepSeconds ) {
				m_CalendarHotCache.nWeatherRemainderSeconds = 0u;
				if ( !m_pDatabase->setCalendarHot( m_CalendarHotCache ) ) {
					m_bSimulationAdvancing = false;
					return false;
				}
				stepRegionalSimulation( stepSeconds );
			}
		}
	}
	++m_CalendarHotCache.nRevision;
	if ( !m_pDatabase->setCalendarHot( m_CalendarHotCache ) ) {
		m_bSimulationAdvancing = false;
		return false;
	}
	advanceCalendarNotifications( previous, m_CalendarHotCache.nWorldSeconds );
	dispatchTimedEvents( previous, m_CalendarHotCache.nWorldSeconds );
	m_bSimulationAdvancing = false;
	return true;
}

bool NMDWorldSimulation::advanceWorldRealSeconds( double fRealDeltaSeconds )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	if ( !m_bCalendarInitialized ) {
		setDatabase( m_pDatabase );
	}
	if ( !m_bCalendarInitialized || m_bSimulationAdvancing ||
		!std::isfinite( fRealDeltaSeconds ) || fRealDeltaSeconds < 0.0 ) {
		return false;
	}
	if ( !m_pDatabase->getCalendarHot( m_CalendarHotCache ) ||
		!m_pDatabase->getCalendarDefinition( m_CalendarDefinitionCache ) ) {
		return false;
	}
	const double scaled = fRealDeltaSeconds *
		static_cast<double>( m_CalendarDefinitionCache.fWorldSecondsPerRealSecond ) +
		m_CalendarHotCache.fFractionalSeconds;
	if ( !std::isfinite( scaled ) || scaled >= 4294967296.0 ) {
		return false;
	}
	const uint32_t seconds = static_cast<uint32_t>( scaled );
	if ( !advanceWorldSeconds( seconds ) ) {
		return false;
	}
	const double priorFraction = m_CalendarHotCache.fFractionalSeconds;
	m_CalendarHotCache.fFractionalSeconds = scaled - seconds;
	if ( priorFraction != m_CalendarHotCache.fFractionalSeconds &&
		!m_pDatabase->setCalendarHot( m_CalendarHotCache ) ) {
		return false;
	}
	return true;
}

void NMDWorldSimulation::selectWeatherFront( uint32_t nIndex,
	uint32_t nStepSeconds )
{
	nmdWeatherHot_t& weather = m_WeatherScratch[ nIndex ];
	const nmdWeatherTimelineEvent_t *pOverride = NULL;
	const nmdWeatherTimelineIndexEntry_t *pEntries = NULL;
	uint32_t count = 0u;
	const bool ready = m_pDatabase->getWeatherTimelineIndexRange( nIndex, pEntries, count );
	assert( ready );
	if ( !ready ) {
		return;
	}
	const uint64_t now = m_CalendarHotCache.nWorldSeconds;
	// Last start <= now (exclusive upper bound). Handles arbitrary time jumps.
	uint32_t lo = 0u, hi = count;
	while ( lo < hi ) {
		const uint32_t mid = lo + ( hi - lo ) / 2u;
		if ( pEntries[mid].nStartWorldSecond <= now ) {
			lo = mid + 1u;
		} else {
			hi = mid;
		}
	}
	uint32_t winnerIndex = UINT32_MAX;
	while ( lo > 0u ) {
		const nmdWeatherTimelineIndexEntry_t& entry = pEntries[ --lo ];
		if ( entry.nPrefixMaxEndSecond <= now ) {
			break;
		}
		const nmdWeatherTimelineEvent_t& candidate = m_pDatabase->getWeatherTimelineEventData()[entry.nEventIndex];
		if ( now >= candidate.nStartWorldSecond + candidate.nDurationSeconds ) {
			continue;
		}
		if ( pOverride == NULL || candidate.nPriority > pOverride->nPriority ||
			( candidate.nPriority == pOverride->nPriority &&
			( candidate.nStartWorldSecond > pOverride->nStartWorldSecond ||
			( candidate.nStartWorldSecond == pOverride->nStartWorldSecond &&
			entry.nEventIndex < winnerIndex ) ) ) ) {
			pOverride = &candidate;
			winnerIndex = entry.nEventIndex;
		}
	}
	if ( pOverride != NULL ) {
		const nmdClimateSeasonalCold_t& climate = m_pDatabase->getClimateSeasonalColdData()[ nIndex ];
		const nmdClimateGeographyCold_t& geo = m_pDatabase->getClimateGeographyColdData()[ nIndex ];
		weather.eWeather = pOverride->eWeather;
		weather.eTargetWeather = pOverride->eWeather;
		weather.fTargetTemperatureC = sampleSeason(
			climate.fTemperatureC, m_CalendarHotCache.nWorldSeconds ) -
			0.0065f * geo.fElevationMeters + pOverride->fTemperatureOffsetC;
		weather.fTargetHumidity = pOverride->fRelativeHumidity < 0.0f ?
			sampleSeason( climate.fRelativeHumidity, m_CalendarHotCache.nWorldSeconds ) :
			pOverride->fRelativeHumidity;
		weather.fTargetWindSpeedMps = pOverride->fWindSpeedMps < 0.0f ?
			geo.fPrevailingWindMps : pOverride->fWindSpeedMps;
		weather.fFrontIntensity = pOverride->fIntensityMmPerHour;
		weather.nFrontRemainingSeconds = nStepSeconds;
		return;
	}
	if ( weather.nFrontRemainingSeconds > nStepSeconds ) {
		weather.nFrontRemainingSeconds -= nStepSeconds;
		return;
	}
	const nmdClimateSeasonalCold_t& climate = m_pDatabase->getClimateSeasonalColdData()[ nIndex ];
	const nmdClimateGeographyCold_t& geo = m_pDatabase->getClimateGeographyColdData()[ nIndex ];
	const float seasonalTemp = sampleSeason( climate.fTemperatureC,
		m_CalendarHotCache.nWorldSeconds ) - 0.0065f * geo.fElevationMeters;
	const float seasonalHumidity = sampleSeason( climate.fRelativeHumidity,
		m_CalendarHotCache.nWorldSeconds );
	const float stormChance = sampleSeason( climate.fStormChancePerDay,
		m_CalendarHotCache.nWorldSeconds );
	const uint32_t frontHours = 2u + static_cast<uint32_t>(
		Random01( weather.nRandomState ) * 15.0f );
	weather.nFrontRemainingSeconds = frontHours * 3600u;
	weather.fTargetTemperatureC = seasonalTemp +
		( Random01( weather.nRandomState ) - 0.5f ) * 8.0f;
	weather.fTargetHumidity = ClampF( seasonalHumidity +
		( Random01( weather.nRandomState ) - 0.5f ) * 0.25f, 0.02f, 1.0f );
	weather.fTargetWindSpeedMps = MaxF( 0.0f, geo.fPrevailingWindMps *
		( 0.5f + Random01( weather.nRandomState ) * 1.7f ) );
	weather.fFrontIntensity = 1.0f + Random01( weather.nRandomState ) * 12.0f;
	weather.fWindAngleRadians = Random01( weather.nRandomState ) * 2.0f * NMD_WORLD_PI;
	const float wetChance = ClampF( stormChance * frontHours / 24.0f, 0.0f, 1.0f );
	uint8_t target = NMD_WEATHER_CLEAR;
	if ( Random01( weather.nRandomState ) < wetChance ) {
		if ( ( geo.eBiome == NMD_CLIMATE_BIOME_ARID ||
			geo.eBiome == NMD_CLIMATE_BIOME_STEPPE ) &&
			seasonalHumidity < 0.40f ) {
			target = NMD_WEATHER_SANDSTORM;
			weather.fTargetWindSpeedMps = MaxF( weather.fTargetWindSpeedMps, 13.0f );
		} else if ( seasonalTemp <= -1.0f ) {
			target = weather.fTargetWindSpeedMps > 10.0f ?
				NMD_WEATHER_BLIZZARD : NMD_WEATHER_SNOW;
		} else {
			target = weather.fFrontIntensity >= 8.0f ?
				NMD_WEATHER_THUNDERSTORM : NMD_WEATHER_RAIN;
		}
	} else if ( seasonalHumidity >= 0.86f &&
		Random01( weather.nRandomState ) < 0.32f ) {
		target = NMD_WEATHER_FOG;
	} else if ( Random01( weather.nRandomState ) < seasonalHumidity * 0.78f ) {
		target = NMD_WEATHER_CLOUDY;
	}
	weather.eTargetWeather = target;
	weather.eWeather = target;
}

void NMDWorldSimulation::updateRegionWater( uint32_t nIndex,
	float fStepHours, float fSeasonTemperature, float fSeasonHumidity,
	float fStormChance )
{
	(void)fSeasonTemperature;
	(void)fStormChance;
	nmdWeatherHot_t& weather = m_WeatherScratch[ nIndex ];
	nmdWaterCycleHot_t& water = m_WaterScratch[ nIndex ];
	const nmdClimateGeographyCold_t& geo = m_pDatabase->getClimateGeographyColdData()[ nIndex ];
	const float relativeHumidity = ClampF( weather.fRelativeHumidity, 0.0f, 1.0f );

	// Open atmospheric boundary: offshore/upwind moisture entering this climate cell.
	water.fAtmosphericVaporMm += geo.fAtmosphericRechargeMmPerHour * fStepHours *
		( 0.5f + 0.5f * fSeasonHumidity );

	// Evaporation and transpiration move water into the same atmospheric bucket.
	const float temperatureFactor = MaxF( 0.0f, weather.fTemperatureC + 8.0f );
	const float evaporativeDemand = temperatureFactor * 0.025f *
		( 1.0f - relativeHumidity ) * ( 1.0f + weather.fWindSpeedMps * 0.055f ) *
		geo.fEvaporationMultiplier * fStepHours;
	float evaporated = TakeWater( water.fSurfaceWaterMm, evaporativeDemand * 0.75f );
	evaporated += TakeWater( water.fSoilWaterMm, evaporativeDemand - evaporated );
	const float transpirationDemand = temperatureFactor * 0.007f *
		geo.fEvaporationMultiplier * fStepHours *
		ClampF( water.fSoilWaterMm / MaxF( 1.0f, geo.fSoilCapacityMm ), 0.0f, 1.0f );
	evaporated += TakeWater( water.fSoilWaterMm, transpirationDemand );
	water.fAtmosphericVaporMm += evaporated;
	water.fLastEvaporationMm = evaporated;

	// Atmospheric saturation and cloud-building are transfers, not water creation.
	const float saturation = 8.0f * std::exp(
		ClampF( 0.065f * weather.fTemperatureC, -4.0f, 4.0f ) );
	const float cloudAffinity = IsPrecipitationWeather( weather.eWeather ) ? 0.64f :
		( weather.eWeather == NMD_WEATHER_CLOUDY || weather.eWeather == NMD_WEATHER_FOG
			? 0.42f : 0.02f );
	const float cloudThreshold = saturation * ClampF(
		weather.fTargetHumidity - cloudAffinity * 0.25f, 0.24f, 1.0f );
	const float supersaturation = MaxF( 0.0f,
		water.fAtmosphericVaporMm - saturation );
	const float cloudCapture = MaxF( 0.0f,
		water.fAtmosphericVaporMm - cloudThreshold ) * cloudAffinity * fStepHours;
	const float condensed = TakeWater( water.fAtmosphericVaporMm,
		supersaturation + cloudCapture );
	water.fCloudWaterMm += condensed;

	// Rain and snow draw down cloud water. fPrecipitationMmPerHour is *actual* output.
	float precipitated = 0.0f;
	if ( IsPrecipitationWeather( weather.eWeather ) ) {
		const float requested = weather.fFrontIntensity *
			( weather.eWeather == NMD_WEATHER_THUNDERSTORM ? 1.6f : 1.0f ) * fStepHours;
		precipitated = TakeWater( water.fCloudWaterMm, requested );
		if ( weather.fTemperatureC <= 0.0f ||
			weather.eWeather == NMD_WEATHER_SNOW ||
			weather.eWeather == NMD_WEATHER_BLIZZARD ) {
			water.fSnowpackMm += precipitated;
		} else {
			water.fSurfaceWaterMm += precipitated;
		}
	}
	water.fLastPrecipitationMm = precipitated;
	weather.fPrecipitationMmPerHour = fStepHours > 0.0f ? precipitated / fStepHours : 0.0f;

	// Melt, infiltration, percolation, and spring discharge.
	const float melt = TakeWater( water.fSnowpackMm,
		MaxF( 0.0f, weather.fTemperatureC ) * 0.25f * fStepHours );
	water.fSurfaceWaterMm += melt;
	const float soilSpace = MaxF( 0.0f, geo.fSoilCapacityMm - water.fSoilWaterMm );
	const float infiltration = TakeWater( water.fSurfaceWaterMm,
		MinF( soilSpace, 10.0f * fStepHours ) );
	water.fSoilWaterMm += infiltration;
	water.fLastInfiltrationMm = infiltration;
	const float seep = TakeWater( water.fSoilWaterMm,
		MinF( MaxF( 0.0f, water.fSoilWaterMm - geo.fSoilCapacityMm * 0.65f ),
			MaxF( 0.0f, geo.fGroundwaterCapacityMm - water.fGroundwaterMm ) *
			MinF( 1.0f, fStepHours * 0.09f ) ) );
	water.fGroundwaterMm += seep;
	const float springFlow = TakeWater( water.fGroundwaterMm,
		water.fGroundwaterMm * 0.002f * fStepHours );
	water.fSurfaceWaterMm += springFlow;

	// Regional drainage and atmospheric advection are staged until *all* regions step.
	const float excessSurface = MaxF( 0.0f,
		water.fSurfaceWaterMm - geo.fSurfaceCapacityMm );
	const float runoff = TakeWater( water.fSurfaceWaterMm,
		excessSurface * MinF( 1.0f, fStepHours * 0.30f ) );
	water.fLastRunoffMm = runoff;
	const nmdClimateLinks_t& links = m_pDatabase->getClimateLinksData()[nIndex];
	if ( runoff > 0.0f && links.nDrainIndex != NMD_CLIMATE_REGION_INVALID_ID ) {
		m_RegionFlux[links.nDrainIndex].fSurfaceInMm += runoff * links.fDrainAreaScale;
	}
	// Otherwise the runoff leaves the modeled world through an open watershed edge.
	const float advected = TakeWater( water.fAtmosphericVaporMm,
		water.fAtmosphericVaporMm * MinF( 0.25f,
			weather.fWindSpeedMps * fStepHours * 0.012f ) );
	if ( advected > 0.0f && links.nDownwindIndex != NMD_CLIMATE_REGION_INVALID_ID ) {
		m_RegionFlux[links.nDownwindIndex].fAtmosphericInMm += advected * links.fDownwindAreaScale;
	}
	// Otherwise vapor is advected beyond the simulation domain.

	// Presentation samples are coupled to actual water availability.
	weather.fRelativeHumidity = ClampF(
		0.6f * weather.fRelativeHumidity +
		0.4f * ( water.fAtmosphericVaporMm / MaxF( 0.01f, saturation ) ),
		0.0f, 1.0f );
	weather.fCloudCover = ClampF( water.fCloudWaterMm / 7.0f +
		( IsPrecipitationWeather( weather.eWeather ) ? 0.35f : 0.0f ), 0.0f, 1.0f );
	++water.nRevision;
}

void NMDWorldSimulation::applyRegionalWaterFlux( void )
{
	for ( uint32_t i = 0u; i < m_pDatabase->getClimateRegionCount(); ++i ) {
		nmdWaterCycleHot_t& water = m_WaterScratch[ i ];
		water.fAtmosphericVaporMm += m_RegionFlux[ i ].fAtmosphericInMm;
		water.fSurfaceWaterMm += m_RegionFlux[ i ].fSurfaceInMm;
		const uint8_t previous = water.eHydrology;
		water.eHydrology = ClassifyWater( water, m_pDatabase->getClimateGeographyColdData()[ i ] );
		m_pDatabase->setRegionWaterCycleByIndex( i, water );
		if ( previous != water.eHydrology && m_pHydrologyChanged != NULL ) {
			nmdHydrologyChangedEvent_t event = {};
			event.nRegionId = m_pDatabase->getClimateRegionIds()[ i ];
			event.ePrevious = previous;
			event.eCurrent = water.eHydrology;
			event.fSoilWaterMm = water.fSoilWaterMm;
			event.fSurfaceWaterMm = water.fSurfaceWaterMm;
			event.fSnowpackMm = water.fSnowpackMm;
			m_pHydrologyChanged( m_pSimulationCallbackUser, event );
		}
	}
}

void NMDWorldSimulation::stepRegionalSimulation( uint32_t nStepSeconds )
{
	const float hours = static_cast<float>( nStepSeconds ) / 3600.0f;
	const float secondOfDay = static_cast<float>(
		m_CalendarHotCache.nWorldSeconds % NMD_WORLD_DAY_SECONDS );
	const float dailyTemperature = -4.0f * std::cos(
		secondOfDay * ( 2.0f * NMD_WORLD_PI / NMD_WORLD_DAY_SECONDS ) );
	for ( uint32_t i = 0u; i < m_pDatabase->getClimateRegionCount(); ++i ) {
		m_RegionFlux[ i ] = {};
	}
	for ( uint32_t i = 0u; i < m_pDatabase->getClimateRegionCount(); ++i ) {
		const nmdWeatherHot_t beforeWeather = m_WeatherScratch[i];
		const uint8_t previousWeather = beforeWeather.eWeather;
		selectWeatherFront( i, nStepSeconds );
		nmdWeatherHot_t& weather = m_WeatherScratch[ i ];
		const nmdClimateSeasonalCold_t& climate = m_pDatabase->getClimateSeasonalColdData()[ i ];
		const nmdClimateGeographyCold_t& geo = m_pDatabase->getClimateGeographyColdData()[ i ];
		const float seasonTemp = sampleSeason( climate.fTemperatureC,
			m_CalendarHotCache.nWorldSeconds ) - geo.fElevationMeters * 0.0065f;
		const float seasonHumidity = sampleSeason( climate.fRelativeHumidity,
			m_CalendarHotCache.nWorldSeconds );
		const float stormChance = sampleSeason( climate.fStormChancePerDay,
			m_CalendarHotCache.nWorldSeconds );
		const float blend = ClampF( hours / 0.5f, 0.0f, 1.0f );
		const float targetTemp = seasonTemp +
			( weather.fTargetTemperatureC - seasonTemp ) + dailyTemperature;
		weather.fTemperatureC += ( targetTemp - weather.fTemperatureC ) * blend;
		weather.fRelativeHumidity = ClampF( weather.fRelativeHumidity +
			( weather.fTargetHumidity - weather.fRelativeHumidity ) * blend, 0.0f, 1.0f );
		weather.fWindSpeedMps = MaxF( 0.0f, weather.fWindSpeedMps +
			( weather.fTargetWindSpeedMps - weather.fWindSpeedMps ) * blend );
		weather.fPressureHpa = geo.fMeanPressureHpa -
			( weather.eWeather == NMD_WEATHER_THUNDERSTORM ? 12.0f : 0.0f );
		weather.fVisibilityMeters = weather.eWeather == NMD_WEATHER_FOG ? 160.0f :
			weather.eWeather == NMD_WEATHER_SANDSTORM ? 260.0f :
			weather.eWeather == NMD_WEATHER_BLIZZARD ? 320.0f : 20000.0f;
		updateRegionWater( i, hours, seasonTemp, seasonHumidity, stormChance );
		m_pDatabase->setRegionWeatherByIndex( i, weather );
		m_pDatabase->setRegionWaterCycleByIndex( i, m_WaterScratch[i] );
		if ( previousWeather != weather.eWeather && m_pRegionWeatherChanged != NULL ) {
			nmdWeatherChangedEvent_t event = {};
			event.nRegionId = m_pDatabase->getClimateRegionIds()[ i ];
			event.ePrevious = previousWeather;
			event.eCurrent = weather.eWeather;
			event.fTemperatureC = weather.fTemperatureC;
			event.fPrecipitationMmPerHour = weather.fPrecipitationMmPerHour;
			m_pRegionWeatherChanged( m_pSimulationCallbackUser, event );
		}
	}
	applyRegionalWaterFlux();
}

bool NMDWorldSimulation::writeSimulationSnapshot(
	nmdSimulationSnapshotHeader_t& header, nmdClimateRegionId_t *pRegionIds,
	nmdWeatherHot_t *pWeather, nmdWaterCycleHot_t *pWater,
	uint32_t nCapacity ) const
{
	return m_pDatabase != NULL && m_bCalendarInitialized && m_bClimateGraphReady &&
		NMDWorldSnapshotCodec::writeSimulation( *m_pDatabase, header,
			pRegionIds, pWeather, pWater, nCapacity );
}

bool NMDWorldSimulation::restoreSimulationSnapshot(
	const nmdSimulationSnapshotHeader_t& header,
	const nmdClimateRegionId_t *pRegionIds, const nmdWeatherHot_t *pWeather,
	const nmdWaterCycleHot_t *pWater )
{
	if ( m_pDatabase == NULL || !m_bCalendarInitialized || !m_bClimateGraphReady ||
		m_bSimulationAdvancing || m_RegionFlux.size() != m_pDatabase->getClimateRegionCount() ||
		( m_pDatabase->getClimateRegionCount() != 0u &&
		( m_pDatabase->getClimateLinksRevision() != m_pDatabase->getClimateTopologyRevision() ||
		m_pDatabase->getWeatherTimelineIndexRevision() != m_pDatabase->getWeatherTimelineRevision() ) ) ||
		!NMDWorldSnapshotCodec::restoreSimulation( *m_pDatabase,
			header, pRegionIds, pWeather, pWater ) ) {
		return false;
	}
	m_CalendarHotCache = header.calendar;
	for ( uint32_t i = 0u; i < m_RegionFlux.size(); ++i ) {
		m_RegionFlux[i] = {};
		m_WeatherScratch[i] = pWeather[i];
		m_WaterScratch[i] = pWater[i];
	}
	rebuildTimedEventDeadlines();
	return true;
}

void NMDWorldSimulation::setSimulationCallbacks(
	nmdCalendarChangedFn_t pCalendar, nmdTimedEventFn_t pTimed,
	nmdWeatherChangedFn_t pWeather, nmdHydrologyChangedFn_t pHydrology,
	void *pUser )
{
	m_pCalendarChanged = pCalendar;
	m_pWorldTimed = pTimed;
	m_pRegionWeatherChanged = pWeather;
	m_pHydrologyChanged = pHydrology;
	m_pSimulationCallbackUser = pUser;
}
