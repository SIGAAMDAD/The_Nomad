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

#include "NMDWorldLocation.h"

#include <godot_cpp/core/class_db.hpp>

NMDWorldLocation::NMDWorldLocation( void )
{
}

NMDWorldLocation::~NMDWorldLocation( void )
{
}

const NMDStringAtom *NMDWorldLocation::getId( void ) const
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	if ( pSystem == NULL || get_location_id() < 0 || get_location_id() >= static_cast<int32_t>( NMD_LOCATION_INVALID_ID ) ) {
		return NULL;
	}
	const NMDWorldDatabase& db = pSystem->getDatabase();
	return static_cast<const NMDStringAtom *>(
		db.getRecordAtom( db.getLocationHandle( static_cast<nmdLocationId_t>( get_location_id() ) ) ) );
}

const NMDStringAtom *NMDWorldLocation::getDescription( void ) const
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	if ( pSystem == NULL || get_location_id() < 0 || get_location_id() >= static_cast<int32_t>( NMD_LOCATION_INVALID_ID ) ) {
		return NULL;
	}
	nmdWorldLocationCold_t cold;
	if ( !pSystem->getDatabase().getLocationDefinition(
		pSystem->getDatabase().getLocationHandle( static_cast<nmdLocationId_t>( get_location_id() ) ), cold ) ) {
		return NULL;
	}
	return static_cast<const NMDStringAtom *>( cold.pDescription );
}

void NMDWorldLocation::triggerDiscover( void )
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	if ( pSystem != NULL ) {
		pSystem->mark_discovered( get_location_id() );
	}
}

bool NMDWorldLocation::isCompleted( void ) const
{
	return getStatus() == NMD_LOCATION_STATUS_COMPLETED;
}

nmdLocationStatus_t NMDWorldLocation::getStatus( void ) const
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	if ( pSystem == NULL ) {
		return NMD_LOCATION_STATUS_UNDISCOVERED;
	}
	const int32_t nStatus = pSystem->get_location_status( get_location_id() );
	return nStatus < 0 ? NMD_LOCATION_STATUS_UNDISCOVERED
		: static_cast<nmdLocationStatus_t>( nStatus );
}

void NMDWorldLocation::setStatus( nmdLocationStatus_t eStatus )
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	if ( pSystem == NULL ) {
		return;
	}
	switch ( eStatus ) {
		case NMD_LOCATION_STATUS_DISCOVERED:
			pSystem->mark_discovered( get_location_id() );
			break;
		case NMD_LOCATION_STATUS_VISITED:
			pSystem->mark_visited( get_location_id() );
			break;
		case NMD_LOCATION_STATUS_COMPLETED:
			pSystem->mark_completed( get_location_id() );
			break;
		default:
			break;
	}
}

NMDLocationStatusChangedEvent NMDWorldLocation::getLocationStatusChangedEvent( void ) const
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	return pSystem != NULL ? pSystem->GetLocationStatusChangedEvent() : NULL;
}

int32_t NMDWorldLocation::get_status( void ) const
{
	NMDLocationSystem *pSystem = GetLocationSystem();
	return pSystem != NULL ? pSystem->get_location_status( get_location_id() ) : -1;
}

void NMDWorldLocation::_bind_methods( void )
{
	godot::ClassDB::bind_method( D_METHOD( "get_status" ), &NMDWorldLocation::get_status );
	godot::ClassDB::bind_method( D_METHOD( "trigger_discover" ), &NMDWorldLocation::triggerDiscover );
}
