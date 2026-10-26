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

#ifndef __NMD_LOCATION_SYSTEM_H__
#define __NMD_LOCATION_SYSTEM_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"
#include <godot_cpp/classes/node.hpp>
#include <godot_cpp/variant/vector3.hpp>

class NMDEventRegistry;
class NMDWorldStateHost;

// Scene-tree adapter for the one world database. Never owns parallel hot/cold pools.
// Configure the database using getDatabase() during world loading, before streaming.
class NMDLocationSystem : public godot::Node
{
	GDCLASS( NMDLocationSystem, godot::Node )

public:
	NMDLocationSystem( void );
	~NMDLocationSystem( void );

	void _ready( void ) override;
	void _exit_tree( void ) override;
	void setHostPath( const godot::NodePath& path );
	godot::NodePath getHostPath( void ) const;
	void bindDatabase( NMDWorldDatabase *pDatabase );
	bool isDatabaseBound( void ) const;
	NMDWorldDatabase& getDatabase( void );
	const NMDWorldDatabase& getDatabase( void ) const;
	void AttachEventRegistry( NMDEventRegistry& registry );
	NMDLocationStatusChangedEvent GetLocationStatusChangedEvent( void ) const;
	NMDLocationPresenceChangedEvent GetLocationPresenceChangedEvent( void ) const;

	bool has_location( int32_t nId ) const;
	int32_t get_location_status( int32_t nId ) const;
	int32_t get_location_traits( int32_t nId ) const;
	int32_t get_location_region( int32_t nId ) const;
	godot::Vector3 get_location_position( int32_t nId ) const;
	float get_compass_radius( int32_t nId ) const;
	bool has_visited( int32_t nId ) const;
	bool is_completed( int32_t nId ) const;
	bool is_player_inside( int32_t nId ) const;
	bool mark_discovered( int32_t nId );
	bool mark_visited( int32_t nId );
	bool mark_completed( int32_t nId );
	bool reevaluate_completion( int32_t nId );
	bool BeginPlayerOverlap( nmdLocationId_t nId );
	bool EndPlayerOverlap( nmdLocationId_t nId );

protected:
	static void _bind_methods( void );

private:
	static nmdWorldHandle_t ResolveLocation( const NMDWorldDatabase& db, int32_t nId );
			bool promoteStatus( nmdWorldHandle_t hLocation, nmdLocationStatus_t status, nmdLocationChangeReason_t reason );
	void OnStatusChanged( const nmdLocationStatusChangedEvent_t& event );
	void OnPresenceChanged( const nmdLocationPresenceChangedEvent_t& event );

	NMDWorldDatabase *m_pDatabase;
	NMDWorldStateHost *m_pHost;
	godot::NodePath m_HostPath;
	NMDLocationStatusChangedEvent m_pStatusEvent;
	NMDLocationPresenceChangedEvent m_pPresenceEvent;
};

#endif // __NMD_LOCATION_SYSTEM_H__
