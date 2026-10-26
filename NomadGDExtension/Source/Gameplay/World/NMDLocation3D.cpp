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

#include "NMDLocation3D.h"

#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/callable.hpp>
#include <godot_cpp/variant/utility_functions.hpp>

using namespace godot;

NMDLocation3D::NMDLocation3D() :
	m_SystemPath( "/root/NMDLocationSystem" ),
	m_VisitAreaPath( "VisitArea" ),
	m_DiscoveryAreaPath( "DiscoveryArea" ),
	m_PlayerGroup( "player" ),
	m_pSystem( nullptr ),
	m_pVisitArea( nullptr ),
	m_pDiscoveryArea( nullptr ),
	m_nLocationId( -1 ),
	m_bPlayerInside( false )
{
}

void NMDLocation3D::_bind_methods()
{
	ClassDB::bind_method( D_METHOD( "set_location_id", "location_id" ),
		&NMDLocation3D::set_location_id );
	ClassDB::bind_method( D_METHOD( "get_location_id" ),
		&NMDLocation3D::get_location_id );
	ADD_PROPERTY( PropertyInfo( Variant::INT, "location_id", PROPERTY_HINT_RANGE,
		"-1,65534,1" ), "set_location_id", "get_location_id" );

	ClassDB::bind_method( D_METHOD( "set_system_path", "system_path" ),
		&NMDLocation3D::set_system_path );
	ClassDB::bind_method( D_METHOD( "get_system_path" ),
		&NMDLocation3D::get_system_path );
	ADD_PROPERTY( PropertyInfo( Variant::NODE_PATH, "system_path" ),
		"set_system_path", "get_system_path" );

	ClassDB::bind_method( D_METHOD( "set_visit_area_path", "visit_area_path" ),
		&NMDLocation3D::set_visit_area_path );
	ClassDB::bind_method( D_METHOD( "get_visit_area_path" ),
		&NMDLocation3D::get_visit_area_path );
	ADD_PROPERTY( PropertyInfo( Variant::NODE_PATH, "visit_area_path",
		PROPERTY_HINT_NODE_PATH_VALID_TYPES, "Area3D" ),
		"set_visit_area_path", "get_visit_area_path" );

	ClassDB::bind_method( D_METHOD( "set_discovery_area_path", "discovery_area_path" ),
		&NMDLocation3D::set_discovery_area_path );
	ClassDB::bind_method( D_METHOD( "get_discovery_area_path" ),
		&NMDLocation3D::get_discovery_area_path );
	ADD_PROPERTY( PropertyInfo( Variant::NODE_PATH, "discovery_area_path",
		PROPERTY_HINT_NODE_PATH_VALID_TYPES, "Area3D" ),
		"set_discovery_area_path", "get_discovery_area_path" );

	ClassDB::bind_method( D_METHOD( "set_player_group", "player_group" ),
		&NMDLocation3D::set_player_group );
	ClassDB::bind_method( D_METHOD( "get_player_group" ),
		&NMDLocation3D::get_player_group );
	ADD_PROPERTY( PropertyInfo( Variant::STRING_NAME, "player_group" ),
		"set_player_group", "get_player_group" );

	ClassDB::bind_method( D_METHOD( "_on_visit_body_entered", "body" ),
		&NMDLocation3D::_on_visit_body_entered );
	ClassDB::bind_method( D_METHOD( "_on_visit_body_exited", "body" ),
		&NMDLocation3D::_on_visit_body_exited );
	ClassDB::bind_method( D_METHOD( "_on_discovery_body_entered", "body" ),
		&NMDLocation3D::_on_discovery_body_entered );
}

void NMDLocation3D::set_location_id( int32_t nId )
{
	// IDs are authoring-time properties; changing live IDs would corrupt occupancy.
	if ( m_pSystem != nullptr )
	{
		return;
	}
	m_nLocationId = nId;
}

int32_t NMDLocation3D::get_location_id() const
{
	return m_nLocationId;
}

void NMDLocation3D::set_system_path( const NodePath &path )
{
	if ( m_pSystem != nullptr ) {
		return;
	}
	m_SystemPath = path;
}

NodePath NMDLocation3D::get_system_path() const
{
	return m_SystemPath;
}

void NMDLocation3D::set_visit_area_path( const NodePath &path )
{
	if ( m_pSystem != nullptr ) {
		return;
	}
	m_VisitAreaPath = path;
}

NodePath NMDLocation3D::get_visit_area_path() const
{
	return m_VisitAreaPath;
}

void NMDLocation3D::set_discovery_area_path( const NodePath &path )
{
	if ( m_pSystem != nullptr ) {
		return;
	}
	m_DiscoveryAreaPath = path;
}

NodePath NMDLocation3D::get_discovery_area_path() const
{
	return m_DiscoveryAreaPath;
}

void NMDLocation3D::set_player_group( const StringName &group )
{
	if ( m_pSystem != nullptr ) {
		return;
	}
	m_PlayerGroup = group;
}

StringName NMDLocation3D::get_player_group() const
{
	return m_PlayerGroup;
}

void NMDLocation3D::_ready()
{
	m_bPlayerInside = false;
	m_pSystem = Object::cast_to<NMDLocationSystem>( get_node_or_null( m_SystemPath ) );
	if ( m_pSystem == nullptr || !m_pSystem->has_location( m_nLocationId ) )
	{
		UtilityFunctions::push_error( "NMDLocation3D: invalid location_id or missing initialized location system" );
		m_pSystem = nullptr;
		return;
	}

	m_pVisitArea = Object::cast_to<Area3D>( get_node_or_null( m_VisitAreaPath ) );
	if ( m_pVisitArea == nullptr )
	{
		UtilityFunctions::push_error( "NMDLocation3D: VisitArea must reference a child Area3D" );
		m_pSystem = nullptr;
		return;
	}

	m_pVisitArea->set_monitoring( true );
	m_pVisitArea->connect( StringName( "body_entered" ),
		Callable( this, StringName( "_on_visit_body_entered" ) ) );
	m_pVisitArea->connect( StringName( "body_exited" ),
		Callable( this, StringName( "_on_visit_body_exited" ) ) );

	if ( !m_DiscoveryAreaPath.is_empty() )
	{
		m_pDiscoveryArea = Object::cast_to<Area3D>(
			get_node_or_null( m_DiscoveryAreaPath ) );
		if ( m_pDiscoveryArea != nullptr && m_pDiscoveryArea != m_pVisitArea )
		{
			m_pDiscoveryArea->set_monitoring( true );
			m_pDiscoveryArea->connect( StringName( "body_entered" ),
				Callable( this, StringName( "_on_discovery_body_entered" ) ) );
		}
		else
		{
			m_pDiscoveryArea = nullptr;
		}
	}
}

void NMDLocation3D::_exit_tree()
{
	// Streaming out while inside must release the zone's occupancy reference.
	if ( m_bPlayerInside && m_pSystem != nullptr )
	{
		m_pSystem->EndPlayerOverlap( static_cast<nmdLocationId_t>( m_nLocationId ) );
		m_bPlayerInside = false;
	}

	if ( m_pVisitArea != nullptr )
	{
		const Callable onEnter( this, StringName( "_on_visit_body_entered" ) );
		const Callable onExit( this, StringName( "_on_visit_body_exited" ) );
		if ( m_pVisitArea->is_connected( StringName( "body_entered" ), onEnter ) )
		{
			m_pVisitArea->disconnect( StringName( "body_entered" ), onEnter );
		}
		if ( m_pVisitArea->is_connected( StringName( "body_exited" ), onExit ) )
		{
			m_pVisitArea->disconnect( StringName( "body_exited" ), onExit );
		}
	}

	if ( m_pDiscoveryArea != nullptr )
	{
		const Callable onDiscover( this, StringName( "_on_discovery_body_entered" ) );
		if ( m_pDiscoveryArea->is_connected( StringName( "body_entered" ), onDiscover ) )
		{
			m_pDiscoveryArea->disconnect( StringName( "body_entered" ), onDiscover );
		}
	}

	m_pSystem = nullptr;
	m_pVisitArea = nullptr;
	m_pDiscoveryArea = nullptr;
}

bool NMDLocation3D::IsPlayerBody( Node3D *pBody ) const
{
	return pBody != nullptr && pBody->is_in_group( m_PlayerGroup );
}

void NMDLocation3D::_on_visit_body_entered( Node3D *pBody )
{
	if ( m_bPlayerInside || m_pSystem == nullptr || !IsPlayerBody( pBody ) )
	{
		return;
	}

	m_bPlayerInside = true;
	if ( !m_pSystem->BeginPlayerOverlap( static_cast<nmdLocationId_t>( m_nLocationId ) ) )
	{
		m_bPlayerInside = false;
	}
}

void NMDLocation3D::_on_visit_body_exited( Node3D *pBody )
{
	if ( !m_bPlayerInside || m_pSystem == nullptr || !IsPlayerBody( pBody ) )
	{
		return;
	}

	m_bPlayerInside = false;
	m_pSystem->EndPlayerOverlap( static_cast<nmdLocationId_t>( m_nLocationId ) );
}

void NMDLocation3D::_on_discovery_body_entered( Node3D *pBody )
{
	if ( m_pSystem != nullptr && IsPlayerBody( pBody ) )
	{
		m_pSystem->mark_discovered( m_nLocationId );
	}
}

NMDLocationSystem *NMDLocation3D::GetLocationSystem( void ) const
{
	return m_pSystem;
}
