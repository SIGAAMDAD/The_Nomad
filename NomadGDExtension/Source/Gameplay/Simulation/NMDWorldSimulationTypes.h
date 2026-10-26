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

#ifndef __NMD_WORLD_SIMULATION_TYPES_H__
#define __NMD_WORLD_SIMULATION_TYPES_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

typedef uint16_t nmdClimateRegionId_t;
#define NMD_CLIMATE_REGION_INVALID_ID 0xFFFFu
#define NMD_CLIMATE_MAX_REGION_COUNT 0xFFFFu
#define NMD_CALENDAR_MONTH_COUNT 12u
#define NMD_CALENDAR_SEASON_COUNT 4u
#define NMD_SIMULATION_SNAPSHOT_MAGIC 0x53444D4Eu /* 'NMDS' little-endian */
#define NMD_SIMULATION_SNAPSHOT_VERSION 1u

typedef enum nmdClimateBiome_e : uint8_t
{
	NMD_CLIMATE_BIOME_TEMPERATE = 0,
	NMD_CLIMATE_BIOME_ARID,
	NMD_CLIMATE_BIOME_STEPPE,
	NMD_CLIMATE_BIOME_TROPICAL,
	NMD_CLIMATE_BIOME_SWAMP,
	NMD_CLIMATE_BIOME_ALPINE,
	NMD_CLIMATE_BIOME_POLAR,
	NMD_CLIMATE_BIOME_COASTAL,
	NMD_CLIMATE_BIOME_VOLCANIC,
	NMD_CLIMATE_BIOME_COUNT
} nmdClimateBiome_t;

typedef enum nmdWeatherKind_e : uint8_t
{
	NMD_WEATHER_CLEAR = 0,
	NMD_WEATHER_CLOUDY,
	NMD_WEATHER_RAIN,
	NMD_WEATHER_THUNDERSTORM,
	NMD_WEATHER_SNOW,
	NMD_WEATHER_BLIZZARD,
	NMD_WEATHER_FOG,
	NMD_WEATHER_SANDSTORM,
	NMD_WEATHER_COUNT
} nmdWeatherKind_t;

typedef enum nmdHydrologyState_e : uint8_t
{
	NMD_HYDROLOGY_NORMAL = 0,
	NMD_HYDROLOGY_DROUGHT,
	NMD_HYDROLOGY_SATURATED,
	NMD_HYDROLOGY_FLOOD,
	NMD_HYDROLOGY_COUNT
} nmdHydrologyState_t;

typedef enum nmdCalendarChange_e : uint8_t
{
	NMD_CALENDAR_CHANGE_DAY = 0,
	NMD_CALENDAR_CHANGE_MONTH,
	NMD_CALENDAR_CHANGE_SEASON,
	NMD_CALENDAR_CHANGE_YEAR
} nmdCalendarChange_t;

// HOT: single authoritative clock. Weather substeps are fixed world seconds.
typedef struct nmdCalendarHot_s
{
	uint64_t nWorldSeconds;
	double fFractionalSeconds;
	uint32_t nWeatherRemainderSeconds;
	uint32_t nRevision;
} nmdCalendarHot_t;

// COLD: authoring rules, not evaluated or copied per frame.
typedef struct nmdCalendarDefinition_s
{
	uint16_t nMonthLengths[ NMD_CALENDAR_MONTH_COUNT ];
	uint16_t nSeasonStartDays[ NMD_CALENDAR_SEASON_COUNT ];
	uint32_t nWeatherStepSeconds;
	float fWorldSecondsPerRealSecond;
	float fAxialTiltDegrees;
	float fMoonCycleDays;
	uint8_t nMonths;
	uint8_t nReserved[ 3 ];
} nmdCalendarDefinition_t;

// Transient lookup. Years and months are 1-based; day of year is 0-based.
typedef struct nmdCalendarDate_s
{
	uint32_t nYear;
	uint16_t nDayOfYear;
	uint8_t nMonth;
	uint8_t nDayOfMonth;
	uint8_t nHour;
	uint8_t nMinute;
	uint8_t nSecond;
	uint8_t nSeason;
	uint32_t nAbsoluteDay;
} nmdCalendarDate_t;

// HOT renderer-facing celestial values, evaluated from the central calendar.
typedef struct nmdCelestialSample_s
{
	float fSunElevationRadians;
	float fSunAzimuthRadians;
	float fSunlightFactor;
	float fMoonElevationRadians;
	float fMoonPhase01;
	float fMoonIllumination;
	float fDaylightHours;
	float fSolarDeclinationRadians;
} nmdCelestialSample_t;

// COLD: piecewise-linearly interpolated annual anchors at season starts.
typedef struct nmdClimateSeasonalCold_s
{
	float fTemperatureC[ NMD_CALENDAR_SEASON_COUNT ];
	float fRelativeHumidity[ NMD_CALENDAR_SEASON_COUNT ];
	float fStormChancePerDay[ NMD_CALENDAR_SEASON_COUNT ];
} nmdClimateSeasonalCold_t;

// COLD: landscape, atmosphere boundary, and water routing.
// Destination IDs may be INVALID for out-of-world discharge/advection.
typedef struct nmdClimateGeographyCold_s
{
	float fAreaKm2;
	float fLatitudeDegrees;
	float fElevationMeters;
	float fSoilCapacityMm;
	float fGroundwaterCapacityMm;
	float fSurfaceCapacityMm;
	float fEvaporationMultiplier;
	float fAtmosphericRechargeMmPerHour;
	float fPrevailingWindMps;
	float fMeanPressureHpa;
	uint16_t nDownwindRegionId;
	uint16_t nDrainRegionId;
	uint8_t eBiome;
	uint8_t nReserved[ 3 ];
} nmdClimateGeographyCold_t;

// HOT: all interpolation state and deterministic region RNG fit within 64 B.
typedef struct alignas( 64 ) nmdWeatherHot_s
{
	float fTemperatureC;
	float fRelativeHumidity;
	float fCloudCover;
	float fWindSpeedMps;
	float fWindAngleRadians;
	float fPrecipitationMmPerHour;
	float fPressureHpa;
	float fVisibilityMeters;
	float fTargetTemperatureC;
	float fTargetHumidity;
	float fTargetWindSpeedMps;
	float fFrontIntensity;
	uint32_t nRandomState;
	uint32_t nFrontRemainingSeconds;
	uint8_t eWeather;
	uint8_t eTargetWeather;
	uint16_t nReserved;
} nmdWeatherHot_t;

// HOT: water equivalent, expressed as millimeters over the region's area.
// Stored pools are conserved by local transfers; open boundary flows are explicit.
typedef struct alignas( 64 ) nmdWaterCycleHot_s
{
	float fAtmosphericVaporMm;
	float fCloudWaterMm;
	float fSoilWaterMm;
	float fGroundwaterMm;
	float fSurfaceWaterMm;
	float fSnowpackMm;
	float fLastPrecipitationMm;
	float fLastEvaporationMm;
	float fLastRunoffMm;
	float fLastInfiltrationMm;
	uint32_t nRevision;
	uint8_t eHydrology;
	uint8_t nReserved[ 3 ];
} nmdWaterCycleHot_t;

// SCRATCH: one preallocated entry per registered region (not persisted).
typedef struct nmdWaterFlux_s
{
	float fAtmosphericInMm;
	float fSurfaceInMm;
} nmdWaterFlux_t;

// COLD: an authored, region-specific weather episode with a deterministic window.
// A negative humidity/wind value means use the interpolated climate baseline.
typedef struct nmdWeatherTimelineEvent_s
{
	uint64_t nStartWorldSecond;
	uint32_t nDurationSeconds;
	uint32_t nEventId;
	float fTemperatureOffsetC;
	float fRelativeHumidity;
	float fWindSpeedMps;
	float fIntensityMmPerHour;
	nmdClimateRegionId_t nRegionId;
	uint8_t eWeather;
	uint8_t nPriority;
} nmdWeatherTimelineEvent_t;

// HOT transient renderer sample, including blends across climate boundaries.
typedef struct nmdWeatherSample_s
{
	float fTemperatureC;
	float fRelativeHumidity;
	float fCloudCover;
	float fWindSpeedMps;
	float fPrecipitationMmPerHour;
	float fVisibilityMeters;
	float fPressureHpa;
	uint8_t eWeather;
	uint8_t nReserved[ 3 ];
} nmdWeatherSample_t;

// COLD: authored one-shot or periodic event; due time is derived at restore.
typedef struct nmdWorldTimedEvent_s
{
	uint64_t nFirstWorldSecond;
	uint32_t nEventId;
	uint32_t nPeriodSeconds; // 0 = one shot.
} nmdWorldTimedEvent_t;

typedef struct nmdCalendarChangedEvent_s
{
	uint64_t nWorldSecond;
	uint32_t nDaysElapsed;
	uint32_t nPrevious;
	uint32_t nCurrent;
	uint8_t eChange;
	uint8_t nReserved[ 3 ];
} nmdCalendarChangedEvent_t;

typedef struct nmdWorldTimedEventTriggered_s
{
	uint64_t nFirstDueWorldSecond;
	uint32_t nEventId;
	uint32_t nOccurrences; // Multiple occurrences coalesced on time skip.
} nmdWorldTimedEventTriggered_t;

typedef struct nmdWeatherChangedEvent_s
{
	nmdClimateRegionId_t nRegionId;
	uint8_t ePrevious;
	uint8_t eCurrent;
	float fTemperatureC;
	float fPrecipitationMmPerHour;
} nmdWeatherChangedEvent_t;

typedef struct nmdHydrologyChangedEvent_s
{
	nmdClimateRegionId_t nRegionId;
	uint8_t ePrevious;
	uint8_t eCurrent;
	float fSoilWaterMm;
	float fSurfaceWaterMm;
	float fSnowpackMm;
} nmdHydrologyChangedEvent_t;

// Logical snapshot header. Serialize its members explicitly in the save codec.
typedef struct nmdSimulationSnapshotHeader_s
{
	uint32_t nMagic;
	uint16_t nVersion;
	uint16_t nRegionCount;
	nmdCalendarHot_t calendar;
} nmdSimulationSnapshotHeader_t;

typedef void ( *nmdCalendarChangedFn_t )( void *, const nmdCalendarChangedEvent_t& );
typedef void ( *nmdTimedEventFn_t )( void *, const nmdWorldTimedEventTriggered_t& );
typedef void ( *nmdWeatherChangedFn_t )( void *, const nmdWeatherChangedEvent_t& );
typedef void ( *nmdHydrologyChangedFn_t )( void *, const nmdHydrologyChangedEvent_t& );

static_assert( sizeof( nmdCalendarHot_t ) == 24, "Calendar hot must be 24 B" );
static_assert( sizeof( nmdCalendarDefinition_t ) <= 64, "Calendar cold must fit a line" );
static_assert( sizeof( nmdCalendarDate_t ) <= 64, "Calendar date must fit a line" );
static_assert( sizeof( nmdCelestialSample_t ) == 32, "Celestial sample must be 32 B" );
static_assert( sizeof( nmdClimateSeasonalCold_t ) == 48, "Climate seasonal POD must be 48 B" );
static_assert( sizeof( nmdClimateGeographyCold_t ) == 48, "Climate geography POD must be 48 B" );
static_assert( sizeof( nmdWeatherHot_t ) == 64, "Weather hot must be one cacheline" );
static_assert( alignof( nmdWeatherHot_t ) == 64, "Weather hot records must be cacheline aligned" );
static_assert( sizeof( nmdWaterCycleHot_t ) == 64, "Hydrology hot must be one cacheline" );
static_assert( alignof( nmdWaterCycleHot_t ) == 64, "Water cycle records must be cacheline aligned" );
static_assert( sizeof( nmdWaterFlux_t ) == 8, "Water flux must be 8 B" );
static_assert( sizeof( nmdWeatherTimelineEvent_t ) <= 64, "Weather timeline record must fit a line" );
static_assert( sizeof( nmdWeatherSample_t ) == 32, "Renderer weather sample must be 32 B" );
static_assert( sizeof( nmdWorldTimedEvent_t ) <= 64, "World time event must fit a line" );
static_assert( sizeof( nmdCalendarChangedEvent_t ) <= 64, "Calendar event must fit a line" );
static_assert( sizeof( nmdWorldTimedEventTriggered_t ) <= 64, "Scheduled event must fit a line" );
static_assert( sizeof( nmdWeatherChangedEvent_t ) <= 64, "Weather event must fit a line" );
static_assert( sizeof( nmdHydrologyChangedEvent_t ) <= 64, "Water event must fit a line" );
static_assert( sizeof( nmdSimulationSnapshotHeader_t ) <= 64, "Snapshot header must fit a line" );

#endif // __NMD_WORLD_SIMULATION_TYPES_H__
