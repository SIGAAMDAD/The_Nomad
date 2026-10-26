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

#ifndef __NMD_CALENDAR_SYSTEM_H__
#define __NMD_CALENDAR_SYSTEM_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldSimulation.h"
#include <godot_cpp/classes/node.hpp>
#include <godot_cpp/variant/node_path.hpp>
#include <godot_cpp/variant/dictionary.hpp>

class NMDWorldStateHost;
class NMDEventRegistry;

// Scene-tree time controller; does not own or duplicate world records.
// NMDWorldStateHost exclusively owns NMDWorldDatabase.
class NMDCalendarSystem : public godot::Node
{
	GDCLASS( NMDCalendarSystem, godot::Node )

public:
	NMDCalendarSystem( void );
	~NMDCalendarSystem( void );

	void _ready( void ) override;
	void _exit_tree( void ) override;
	void _process( double fDelta ) override;

	void set_database_path( const godot::NodePath& path );
	godot::NodePath get_database_path( void ) const;
	void set_simulation_paused( bool paused );
	bool is_simulation_paused( void ) const;
	void set_time_multiplier( float multiplier );
	float get_time_multiplier( void ) const;
	bool refresh_database( void );
	// Call after world/mod climate and weather episodes have been registered.
	bool finalize_world_indices( void );
	bool advance_seconds( int32_t seconds );
	int64_t get_world_seconds( void ) const;
	godot::Dictionary get_calendar_date( void ) const;
	godot::Dictionary get_region_weather( int32_t regionId ) const;
	godot::Dictionary get_celestial_sample( int32_t regionId ) const;
	godot::Dictionary get_region_water( int32_t regionId ) const;

	void AttachEventRegistry( NMDEventRegistry& registry );
	NMDWorldDatabase *getDatabase( void ) const;
	NMDWorldSimulation& getSimulation( void );

protected:
	static void _bind_methods( void );

private:
	static void CalendarCallback( void *pUser, const nmdCalendarChangedEvent_t& event );
	static void TimedCallback( void *pUser, const nmdWorldTimedEventTriggered_t& event );
	static void WeatherCallback( void *pUser, const nmdWeatherChangedEvent_t& event );
	static void HydrologyCallback( void *pUser, const nmdHydrologyChangedEvent_t& event );
	void DisconnectDatabase( void );

	godot::NodePath m_DatabasePath;
	NMDWorldStateHost *m_pOwner;
	NMDWorldSimulation m_Simulation;
	bool m_bPaused;
	float m_fTimeMultiplier;
	NMDEvent<nmdCalendarChangedEvent_t> *m_pCalendarEvent;
	NMDEvent<nmdWorldTimedEventTriggered_t> *m_pTimedEvent;
	NMDEvent<nmdWeatherChangedEvent_t> *m_pWeatherEvent;
	NMDEvent<nmdHydrologyChangedEvent_t> *m_pWaterEvent;
};

#endif // __NMD_CALENDAR_SYSTEM_H__
