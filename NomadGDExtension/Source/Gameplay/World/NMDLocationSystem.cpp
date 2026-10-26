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

#include "NMDLocationSystem.h"
#include "../Story/NMDWorldConditions.h"
#include "../NMDWorldStateHost.h"

#include <Framework/Events/NMDEventRegistry.h>
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/string_name.hpp>

using namespace godot;

NMDLocationSystem::NMDLocationSystem( void ) : m_pDatabase( NULL ), m_pHost( NULL ),
	m_HostPath( "/root/NMDWorldStateHost" ),
	m_pStatusEvent( NULL ), m_pPresenceEvent( NULL )
{
}

NMDLocationSystem::~NMDLocationSystem( void )
{
	if ( m_pHost != NULL ) {
		m_pHost->setLocationSystem( NULL );
	}
}

void NMDLocationSystem::_ready( void )
{
	NMDWorldStateHost *pHost = Object::cast_to<NMDWorldStateHost>( get_node_or_null( m_HostPath ) );
	if ( pHost != NULL ) {
		m_pHost = pHost;
		bindDatabase( &pHost->getDatabase() );
		pHost->setLocationSystem( this );
	}
}

void NMDLocationSystem::_exit_tree( void )
{
	if ( m_pHost != NULL ) {
		m_pHost->setLocationSystem( NULL );
	}
	m_pHost = NULL;
	m_pDatabase = NULL;
}

void NMDLocationSystem::setHostPath( const NodePath& path ) { m_HostPath = path; }
NodePath NMDLocationSystem::getHostPath( void ) const { return m_HostPath; }
void NMDLocationSystem::bindDatabase( NMDWorldDatabase *pDatabase ) { m_pDatabase = pDatabase; }
bool NMDLocationSystem::isDatabaseBound( void ) const { return m_pDatabase != NULL; }
NMDWorldDatabase& NMDLocationSystem::getDatabase( void ) { assert( m_pDatabase != NULL ); return *m_pDatabase; }
const NMDWorldDatabase& NMDLocationSystem::getDatabase( void ) const { assert( m_pDatabase != NULL ); return *m_pDatabase; }

void NMDLocationSystem::AttachEventRegistry( NMDEventRegistry& registry )
{
	m_pStatusEvent = registry.getEvent<nmdLocationStatusChangedEvent_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), NMD_STRINGIZE( NMDLocationStatusChangedEvent ) );
	m_pPresenceEvent = registry.getEvent<nmdLocationPresenceChangedEvent_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), NMD_STRINGIZE( NMDLocationPresenceChangedEvent ) );
}

NMDLocationStatusChangedEvent NMDLocationSystem::GetLocationStatusChangedEvent( void ) const
{
	return m_pStatusEvent;
}

NMDLocationPresenceChangedEvent NMDLocationSystem::GetLocationPresenceChangedEvent( void ) const
{
	return m_pPresenceEvent;
}

nmdWorldHandle_t NMDLocationSystem::ResolveLocation( const NMDWorldDatabase& db, int32_t nId )
{
	return nId >= 0 && nId < static_cast<int32_t>( NMD_LOCATION_INVALID_ID ) ?
		db.getLocationHandle( static_cast<nmdLocationId_t>( nId ) ) :
		nmdWorldHandle_t{ NMD_WORLD_HANDLE_INVALID };
}

bool NMDLocationSystem::has_location( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	return ResolveLocation( *m_pDatabase, nId ).isValid();
}

int32_t NMDLocationSystem::get_location_status( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return -1;
	}
	nmdLocationStatus_t status;
	return m_pDatabase->getLocationStatus( ResolveLocation( *m_pDatabase, nId ), status ) ?
		static_cast<int32_t>( status ) : -1;
}

int32_t NMDLocationSystem::get_location_traits( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return -1;
	}
	nmdLocationData_t hot;
	return m_pDatabase->getLocationData( ResolveLocation( *m_pDatabase, nId ), hot ) ?
		static_cast<int32_t>( hot.nStateAndTraits & NMD_LOCATION_TRAITS_MASK ) : -1;
}

int32_t NMDLocationSystem::get_location_region( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return -1;
	}
	nmdWorldLocationCold_t cold;
	return m_pDatabase->getLocationDefinition( ResolveLocation( *m_pDatabase, nId ), cold ) ?
		static_cast<int32_t>( cold.nRegionId ) : -1;
}

Vector3 NMDLocationSystem::get_location_position( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return Vector3();
	}
	nmdWorldLocationCold_t cold;
	return m_pDatabase->getLocationDefinition( ResolveLocation( *m_pDatabase, nId ), cold ) ?
		Vector3( cold.position[ 0 ], cold.position[ 1 ], cold.position[ 2 ] ) : Vector3();
}

float NMDLocationSystem::get_compass_radius( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return 0.0f;
	}
	nmdWorldLocationCold_t cold;
	return m_pDatabase->getLocationDefinition( ResolveLocation( *m_pDatabase, nId ), cold ) ?
		cold.fCompassRadius : 0.0f;
}

bool NMDLocationSystem::has_visited( int32_t nId ) const
{
	return get_location_status( nId ) >= NMD_LOCATION_STATUS_VISITED;
}

bool NMDLocationSystem::is_completed( int32_t nId ) const
{
	return get_location_status( nId ) == NMD_LOCATION_STATUS_COMPLETED;
}

bool NMDLocationSystem::is_player_inside( int32_t nId ) const
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	uint16_t nOccupancy = 0u;
	return m_pDatabase->getLocationOccupancy( ResolveLocation( *m_pDatabase, nId ), nOccupancy ) &&
		nOccupancy != 0u;
}

bool NMDLocationSystem::promoteStatus( nmdWorldHandle_t hLocation,
	nmdLocationStatus_t status, nmdLocationChangeReason_t reason )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	nmdLocationStatus_t previous;
	if ( !m_pDatabase->getLocationStatus( hLocation, previous ) ) {
		return false;
	}
	if ( previous >= status ) {
		return true;
	}
	if ( !m_pDatabase->setLocationStatus( hLocation, status ) ) {
		return false;
	}
	nmdLocationData_t hot;
	m_pDatabase->getLocationData( hLocation, hot );
	nmdLocationStatusChangedEvent_t event = {};
	event.nId = static_cast<nmdLocationId_t>( hLocation.getIndex() );
	event.ePrevious = previous;
	event.eCurrent = status;
	event.eReason = reason;
	event.nTraits = static_cast<uint16_t>( hot.nStateAndTraits & NMD_LOCATION_TRAITS_MASK );
	OnStatusChanged( event );
	return true;
}

bool NMDLocationSystem::mark_discovered( int32_t nId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	return promoteStatus( ResolveLocation( *m_pDatabase, nId ),
		NMD_LOCATION_STATUS_DISCOVERED, NMD_LOCATION_CHANGE_PROXIMITY );
}

bool NMDLocationSystem::mark_visited( int32_t nId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	return promoteStatus( ResolveLocation( *m_pDatabase, nId ),
		NMD_LOCATION_STATUS_VISITED, NMD_LOCATION_CHANGE_ENTERED );
}

bool NMDLocationSystem::mark_completed( int32_t nId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldHandle_t hLocation = ResolveLocation( *m_pDatabase, nId );
	nmdLocationStatus_t current;
	return m_pDatabase->getLocationStatus( hLocation, current ) &&
		current >= NMD_LOCATION_STATUS_VISITED &&
		promoteStatus( hLocation, NMD_LOCATION_STATUS_COMPLETED, NMD_LOCATION_CHANGE_GAMEPLAY );
}

bool NMDLocationSystem::reevaluate_completion( int32_t nId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldHandle_t hLocation = ResolveLocation( *m_pDatabase, nId );
	nmdLocationStatus_t current;
	if ( !m_pDatabase->getLocationStatus( hLocation, current ) ) {
		return false;
	}
	if ( current < NMD_LOCATION_STATUS_VISITED || current == NMD_LOCATION_STATUS_COMPLETED ) {
		return true;
	}
	const nmdWorldCondition_t *pConditions = NULL;
	uint16_t nCount = 0u;
	if ( !m_pDatabase->getLocationConditions( hLocation, pConditions, nCount ) ) {
		return false;
	}
	if ( nCount && nmdWorldEvaluateConditions( *m_pDatabase, pConditions, nCount ) ) {
		return promoteStatus( hLocation, NMD_LOCATION_STATUS_COMPLETED, NMD_LOCATION_CHANGE_COMPLETION_RULE );
	}
	return true;
}

bool NMDLocationSystem::BeginPlayerOverlap( nmdLocationId_t nId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldHandle_t hLocation = m_pDatabase->getLocationHandle( nId );
	uint16_t nOccupancy;
	if ( !m_pDatabase->getLocationOccupancy( hLocation, nOccupancy ) || nOccupancy == UINT16_MAX ) {
		return false;
	}
	if ( !m_pDatabase->setLocationOccupancy( hLocation, static_cast<uint16_t>( nOccupancy + 1u ) ) ) {
		return false;
	}
	if ( nOccupancy == 0u ) {
		OnPresenceChanged( { nId, 1u, 0u } );
	}
	mark_visited( nId );
	reevaluate_completion( nId );
	return true;
}

bool NMDLocationSystem::EndPlayerOverlap( nmdLocationId_t nId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldHandle_t hLocation = m_pDatabase->getLocationHandle( nId );
	uint16_t nOccupancy;
	if ( !m_pDatabase->getLocationOccupancy( hLocation, nOccupancy ) || nOccupancy == 0u ) {
		return false;
	}
	if ( !m_pDatabase->setLocationOccupancy( hLocation, static_cast<uint16_t>( nOccupancy - 1u ) ) ) {
		return false;
	}
	if ( nOccupancy == 1u ) {
		OnPresenceChanged( { nId, 0u, 0u } );
	}
	return true;
}

void NMDLocationSystem::OnStatusChanged( const nmdLocationStatusChangedEvent_t& event )
{
	if ( m_pStatusEvent != NULL ) {
		m_pStatusEvent->publish( event );
	}
	emit_signal( StringName( "location_status_changed" ),
		static_cast<int64_t>( event.nId ), static_cast<int64_t>( event.ePrevious ),
		static_cast<int64_t>( event.eCurrent ), static_cast<int64_t>( event.eReason ) );
}

void NMDLocationSystem::OnPresenceChanged( const nmdLocationPresenceChangedEvent_t& event )
{
	if ( m_pPresenceEvent != NULL ) {
		m_pPresenceEvent->publish( event );
	}
	emit_signal( StringName( "location_presence_changed" ),
		static_cast<int64_t>( event.nId ), event.bInside != 0u );
}

void NMDLocationSystem::_bind_methods( void )
{
	ClassDB::bind_method( D_METHOD( "set_host_path", "path" ), &NMDLocationSystem::setHostPath );
	ClassDB::bind_method( D_METHOD( "get_host_path" ), &NMDLocationSystem::getHostPath );
	ADD_PROPERTY( PropertyInfo( Variant::NODE_PATH, "host_path" ), "set_host_path", "get_host_path" );
	ClassDB::bind_method( D_METHOD( "has_location", "location_id" ), &NMDLocationSystem::has_location );
	ClassDB::bind_method( D_METHOD( "get_location_status", "location_id" ), &NMDLocationSystem::get_location_status );
	ClassDB::bind_method( D_METHOD( "get_location_traits", "location_id" ), &NMDLocationSystem::get_location_traits );
	ClassDB::bind_method( D_METHOD( "get_location_region", "location_id" ), &NMDLocationSystem::get_location_region );
	ClassDB::bind_method( D_METHOD( "get_location_position", "location_id" ), &NMDLocationSystem::get_location_position );
	ClassDB::bind_method( D_METHOD( "get_compass_radius", "location_id" ), &NMDLocationSystem::get_compass_radius );
	ClassDB::bind_method( D_METHOD( "has_visited", "location_id" ), &NMDLocationSystem::has_visited );
	ClassDB::bind_method( D_METHOD( "is_completed", "location_id" ), &NMDLocationSystem::is_completed );
	ClassDB::bind_method( D_METHOD( "is_player_inside", "location_id" ), &NMDLocationSystem::is_player_inside );
	ClassDB::bind_method( D_METHOD( "mark_discovered", "location_id" ), &NMDLocationSystem::mark_discovered );
	ClassDB::bind_method( D_METHOD( "mark_visited", "location_id" ), &NMDLocationSystem::mark_visited );
	ClassDB::bind_method( D_METHOD( "mark_completed", "location_id" ), &NMDLocationSystem::mark_completed );
	ClassDB::bind_method( D_METHOD( "reevaluate_completion", "location_id" ), &NMDLocationSystem::reevaluate_completion );
	ADD_SIGNAL( MethodInfo( "location_status_changed",
		PropertyInfo( Variant::INT, "location_id" ),
		PropertyInfo( Variant::INT, "previous_status" ),
		PropertyInfo( Variant::INT, "current_status" ),
		PropertyInfo( Variant::INT, "reason" ) ) );
	ADD_SIGNAL( MethodInfo( "location_presence_changed",
		PropertyInfo( Variant::INT, "location_id" ),
		PropertyInfo( Variant::BOOL, "inside" ) ) );
}
