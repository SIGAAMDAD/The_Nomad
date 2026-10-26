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

#include "NMDCalendarSystem.h"
#include "../World/NMDWorldDependencyCompiler.h"
#include "../NMDWorldStateHost.h"
#include <Framework/Events/NMDEventRegistry.h>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/core/property_info.hpp>
#include <godot_cpp/variant/string_name.hpp>

#include <cmath>

using namespace godot;

NMDCalendarSystem::NMDCalendarSystem( void )
	: m_DatabasePath( "/root/NMDWorldStateHost" ),
	  m_pOwner( NULL ), m_bPaused( false ), m_fTimeMultiplier( 1.0f ),
	  m_pCalendarEvent( NULL ), m_pTimedEvent( NULL ),
	  m_pWeatherEvent( NULL ), m_pWaterEvent( NULL )
{
}

NMDCalendarSystem::~NMDCalendarSystem( void )
{
	DisconnectDatabase();
}

void NMDCalendarSystem::DisconnectDatabase( void )
{
	m_Simulation.setSimulationCallbacks( NULL, NULL, NULL, NULL, NULL );
	m_Simulation.setDatabase( NULL );
	m_pOwner = NULL;
}

void NMDCalendarSystem::_ready( void )
{
	refresh_database();
	set_process( true );
}

void NMDCalendarSystem::_exit_tree( void )
{
	DisconnectDatabase();
	set_process( false );
}

bool NMDCalendarSystem::refresh_database( void )
{
	DisconnectDatabase();
	Node *pNode = get_node_or_null( m_DatabasePath );
	m_pOwner = Object::cast_to<NMDWorldStateHost>( pNode );
	if ( m_pOwner == NULL ) {
		return false;
	}
	m_Simulation.setDatabase( &m_pOwner->getDatabase() );
	m_Simulation.setSimulationCallbacks(
		CalendarCallback, TimedCallback, WeatherCallback, HydrologyCallback, this );
	return true;
}

bool NMDCalendarSystem::finalize_world_indices( void )
{
	return m_pOwner != NULL && NMDWorldDependencyCompiler::rebuild( m_pOwner->getDatabase() ) &&
		m_Simulation.buildClimateGraph() && m_Simulation.buildWeatherTimelineIndex();
}

void NMDCalendarSystem::_process( double fDelta )
{
	if ( m_pOwner == NULL || m_bPaused || m_fTimeMultiplier == 0.0f ) {
		return;
	}
	m_Simulation.advanceWorldRealSeconds( fDelta * m_fTimeMultiplier );
}

void NMDCalendarSystem::set_database_path( const NodePath& path )
{
	m_DatabasePath = path;
	if ( is_inside_tree() ) {
		refresh_database();
	}
}

NodePath NMDCalendarSystem::get_database_path( void ) const
{
	return m_DatabasePath;
}

void NMDCalendarSystem::set_simulation_paused( bool paused )
{
	m_bPaused = paused;
}

bool NMDCalendarSystem::is_simulation_paused( void ) const
{
	return m_bPaused;
}

void NMDCalendarSystem::set_time_multiplier( float multiplier )
{
	if ( std::isfinite( multiplier ) && multiplier >= 0.0f && multiplier <= 10000.0f ) {
		m_fTimeMultiplier = multiplier;
	}
}

float NMDCalendarSystem::get_time_multiplier( void ) const
{
	return m_fTimeMultiplier;
}

NMDWorldDatabase *NMDCalendarSystem::getDatabase( void ) const
{
	return m_pOwner == NULL ? NULL : &m_pOwner->getDatabase();
}

NMDWorldSimulation& NMDCalendarSystem::getSimulation( void )
{
	return m_Simulation;
}

bool NMDCalendarSystem::advance_seconds( int32_t seconds )
{
	return seconds >= 0 && m_pOwner != NULL &&
		m_Simulation.advanceWorldSeconds( static_cast<uint32_t>( seconds ) );
}

int64_t NMDCalendarSystem::get_world_seconds( void ) const
{
	return m_pOwner != NULL ? static_cast<int64_t>(
		m_pOwner->getDatabase().getWorldSeconds() ) : -1;
}

Dictionary NMDCalendarSystem::get_calendar_date( void ) const
{
	Dictionary result;
	nmdCalendarDate_t date;
	if ( m_pOwner == NULL || !m_Simulation.getCalendarDate( date ) ) {
		return result;
	}
	result[ "year" ] = static_cast<int64_t>( date.nYear );
	result[ "month" ] = date.nMonth;
	result[ "day" ] = date.nDayOfMonth;
	result[ "day_of_year" ] = date.nDayOfYear;
	result[ "hour" ] = date.nHour;
	result[ "minute" ] = date.nMinute;
	result[ "second" ] = date.nSecond;
	result[ "season" ] = date.nSeason;
	return result;
}

Dictionary NMDCalendarSystem::get_region_weather( int32_t regionId ) const
{
	Dictionary result;
	nmdWeatherHot_t hot;
	if ( m_pOwner == NULL || regionId < 0 ||
		regionId >= static_cast<int32_t>( NMD_CLIMATE_REGION_INVALID_ID ) ||
		!m_pOwner->getDatabase().getRegionWeather(
			static_cast<nmdClimateRegionId_t>( regionId ), hot ) ) {
		return result;
	}
	result[ "kind" ] = hot.eWeather;
	result[ "temperature_c" ] = hot.fTemperatureC;
	result[ "humidity" ] = hot.fRelativeHumidity;
	result[ "cloud_cover" ] = hot.fCloudCover;
	result[ "wind_mps" ] = hot.fWindSpeedMps;
	result[ "wind_direction_rad" ] = hot.fWindAngleRadians;
	result[ "precipitation_mm_per_hour" ] = hot.fPrecipitationMmPerHour;
	result[ "pressure_hpa" ] = hot.fPressureHpa;
	result[ "visibility_m" ] = hot.fVisibilityMeters;
	return result;
}

Dictionary NMDCalendarSystem::get_celestial_sample( int32_t regionId ) const
{
	Dictionary result;
	nmdCelestialSample_t sample;
	if ( m_pOwner == NULL || regionId < 0 ||
		regionId >= static_cast<int32_t>( NMD_CLIMATE_REGION_INVALID_ID ) ||
		!m_Simulation.getCelestialSample(
			static_cast<nmdClimateRegionId_t>( regionId ), sample ) ) {
		return result;
	}
	result[ "sun_elevation_rad" ] = sample.fSunElevationRadians;
	result[ "sun_azimuth_rad" ] = sample.fSunAzimuthRadians;
	result[ "sunlight_factor" ] = sample.fSunlightFactor;
	result[ "moon_elevation_rad" ] = sample.fMoonElevationRadians;
	result[ "moon_phase" ] = sample.fMoonPhase01;
	result[ "moon_illumination" ] = sample.fMoonIllumination;
	result[ "daylight_hours" ] = sample.fDaylightHours;
	return result;
}

Dictionary NMDCalendarSystem::get_region_water( int32_t regionId ) const
{
	Dictionary result;
	nmdWaterCycleHot_t water;
	if ( m_pOwner == NULL || regionId < 0 ||
		regionId >= static_cast<int32_t>( NMD_CLIMATE_REGION_INVALID_ID ) ||
		!m_pOwner->getDatabase().getRegionWaterCycle(
			static_cast<nmdClimateRegionId_t>( regionId ), water ) ) {
		return result;
	}
	result[ "vapor_mm" ] = water.fAtmosphericVaporMm;
	result[ "cloud_mm" ] = water.fCloudWaterMm;
	result[ "soil_mm" ] = water.fSoilWaterMm;
	result[ "groundwater_mm" ] = water.fGroundwaterMm;
	result[ "surface_mm" ] = water.fSurfaceWaterMm;
	result[ "snowpack_mm" ] = water.fSnowpackMm;
	result[ "last_precipitation_mm" ] = water.fLastPrecipitationMm;
	result[ "last_evaporation_mm" ] = water.fLastEvaporationMm;
	result[ "last_runoff_mm" ] = water.fLastRunoffMm;
	result[ "hydrology" ] = water.eHydrology;
	return result;
}

void NMDCalendarSystem::AttachEventRegistry( NMDEventRegistry& registry )
{
	m_pCalendarEvent = registry.getEvent<nmdCalendarChangedEvent_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), "NMDCalendarChangedEvent" );
	m_pTimedEvent = registry.getEvent<nmdWorldTimedEventTriggered_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), "NMDWorldTimedEvent" );
	m_pWeatherEvent = registry.getEvent<nmdWeatherChangedEvent_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), "NMDWeatherChangedEvent" );
	m_pWaterEvent = registry.getEvent<nmdHydrologyChangedEvent_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), "NMDHydrologyChangedEvent" );
}

void NMDCalendarSystem::CalendarCallback( void *pUser,
	const nmdCalendarChangedEvent_t& event )
{
	NMDCalendarSystem *pSystem = static_cast<NMDCalendarSystem *>( pUser );
	if ( pSystem->m_pCalendarEvent != NULL ) {
		pSystem->m_pCalendarEvent->publish( event );
	}
	pSystem->emit_signal( StringName( "calendar_changed" ),
		static_cast<int64_t>( event.eChange ),
		static_cast<int64_t>( event.nPrevious ),
		static_cast<int64_t>( event.nCurrent ),
		static_cast<int64_t>( event.nDaysElapsed ) );
}

void NMDCalendarSystem::TimedCallback( void *pUser,
	const nmdWorldTimedEventTriggered_t& event )
{
	NMDCalendarSystem *pSystem = static_cast<NMDCalendarSystem *>( pUser );
	if ( pSystem->m_pTimedEvent != NULL ) {
		pSystem->m_pTimedEvent->publish( event );
	}
	pSystem->emit_signal( StringName( "world_timed_event" ),
		static_cast<int64_t>( event.nEventId ),
		static_cast<int64_t>( event.nOccurrences ),
		static_cast<int64_t>( event.nFirstDueWorldSecond ) );
}

void NMDCalendarSystem::WeatherCallback( void *pUser,
	const nmdWeatherChangedEvent_t& event )
{
	NMDCalendarSystem *pSystem = static_cast<NMDCalendarSystem *>( pUser );
	if ( pSystem->m_pWeatherEvent != NULL ) {
		pSystem->m_pWeatherEvent->publish( event );
	}
	pSystem->emit_signal( StringName( "region_weather_changed" ),
		static_cast<int64_t>( event.nRegionId ),
		static_cast<int64_t>( event.ePrevious ),
		static_cast<int64_t>( event.eCurrent ) );
}

void NMDCalendarSystem::HydrologyCallback( void *pUser,
	const nmdHydrologyChangedEvent_t& event )
{
	NMDCalendarSystem *pSystem = static_cast<NMDCalendarSystem *>( pUser );
	if ( pSystem->m_pWaterEvent != NULL ) {
		pSystem->m_pWaterEvent->publish( event );
	}
	pSystem->emit_signal( StringName( "region_hydrology_changed" ),
		static_cast<int64_t>( event.nRegionId ),
		static_cast<int64_t>( event.ePrevious ),
		static_cast<int64_t>( event.eCurrent ) );
}

void NMDCalendarSystem::_bind_methods( void )
{
	ClassDB::bind_method( D_METHOD( "set_database_path", "path" ), &NMDCalendarSystem::set_database_path );
	ClassDB::bind_method( D_METHOD( "get_database_path" ), &NMDCalendarSystem::get_database_path );
	ClassDB::bind_method( D_METHOD( "set_simulation_paused", "paused" ), &NMDCalendarSystem::set_simulation_paused );
	ClassDB::bind_method( D_METHOD( "is_simulation_paused" ), &NMDCalendarSystem::is_simulation_paused );
	ClassDB::bind_method( D_METHOD( "set_time_multiplier", "multiplier" ), &NMDCalendarSystem::set_time_multiplier );
	ClassDB::bind_method( D_METHOD( "get_time_multiplier" ), &NMDCalendarSystem::get_time_multiplier );
	ClassDB::bind_method( D_METHOD( "refresh_database" ), &NMDCalendarSystem::refresh_database );
	ClassDB::bind_method( D_METHOD( "finalize_world_indices" ), &NMDCalendarSystem::finalize_world_indices );
	ClassDB::bind_method( D_METHOD( "advance_seconds", "seconds" ), &NMDCalendarSystem::advance_seconds );
	ClassDB::bind_method( D_METHOD( "get_world_seconds" ), &NMDCalendarSystem::get_world_seconds );
	ClassDB::bind_method( D_METHOD( "get_calendar_date" ), &NMDCalendarSystem::get_calendar_date );
	ClassDB::bind_method( D_METHOD( "get_region_weather", "region_id" ), &NMDCalendarSystem::get_region_weather );
	ClassDB::bind_method( D_METHOD( "get_celestial_sample", "region_id" ), &NMDCalendarSystem::get_celestial_sample );
	ClassDB::bind_method( D_METHOD( "get_region_water", "region_id" ), &NMDCalendarSystem::get_region_water );

	ADD_PROPERTY( PropertyInfo( Variant::NODE_PATH, "database_path" ),
		"set_database_path", "get_database_path" );
	ADD_PROPERTY( PropertyInfo( Variant::BOOL, "simulation_paused" ),
		"set_simulation_paused", "is_simulation_paused" );
	ADD_PROPERTY( PropertyInfo( Variant::FLOAT, "time_multiplier" ),
		"set_time_multiplier", "get_time_multiplier" );

	ADD_SIGNAL( MethodInfo( "calendar_changed",
		PropertyInfo( Variant::INT, "change_kind" ),
		PropertyInfo( Variant::INT, "previous_value" ),
		PropertyInfo( Variant::INT, "current_value" ),
		PropertyInfo( Variant::INT, "days_elapsed" ) ) );
	ADD_SIGNAL( MethodInfo( "world_timed_event",
		PropertyInfo( Variant::INT, "event_id" ),
		PropertyInfo( Variant::INT, "occurrences" ),
		PropertyInfo( Variant::INT, "first_due_second" ) ) );
	ADD_SIGNAL( MethodInfo( "region_weather_changed",
		PropertyInfo( Variant::INT, "region_id" ),
		PropertyInfo( Variant::INT, "previous_kind" ),
		PropertyInfo( Variant::INT, "current_kind" ) ) );
	ADD_SIGNAL( MethodInfo( "region_hydrology_changed",
		PropertyInfo( Variant::INT, "region_id" ),
		PropertyInfo( Variant::INT, "previous_state" ),
		PropertyInfo( Variant::INT, "current_state" ) ) );
}
