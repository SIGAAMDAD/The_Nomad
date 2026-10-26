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

#include "NMDWorldStateHost.h"
#include "World/NMDLocationSystem.h"
#include "World/NMDWorldFactChangeDispatcher.h"
#include <Framework/Events/NMDEventRegistry.h>
#include <godot_cpp/core/class_db.hpp>

using namespace godot;

NMDWorldStateHost::NMDWorldStateHost( void ) :
	m_pLocationSystem( NULL ), m_pFactValueChangedEvent( NULL )
{
	m_QuestSystem.setDatabase( &m_Database );
	m_QuestSystem.setStorylineSystem( &m_StorylineSystem );
	m_StorylineSystem.setDatabase( &m_Database );
	m_StorylineSystem.setQuestSystem( &m_QuestSystem );
}

NMDWorldStateHost::~NMDWorldStateHost( void )
{
	if ( m_pLocationSystem != NULL ) {
		m_pLocationSystem->bindDatabase( NULL );
	}
}

NMDWorldDatabase& NMDWorldStateHost::getDatabase( void ) { return m_Database; }
const NMDWorldDatabase& NMDWorldStateHost::getDatabase( void ) const { return m_Database; }
NMDQuestSystem& NMDWorldStateHost::getQuestSystem( void ) { return m_QuestSystem; }
NMDStorylineSystem& NMDWorldStateHost::getStorylineSystem( void ) { return m_StorylineSystem; }
NMDWorldFactValueChangedEvent NMDWorldStateHost::getFactValueChangedEvent( void ) const
{
	return m_pFactValueChangedEvent;
}
void NMDWorldStateHost::setLocationSystem( NMDLocationSystem *pSystem ) { m_pLocationSystem = pSystem; }

void NMDWorldStateHost::attachEventRegistry( NMDEventRegistry& registry )
{
	m_QuestSystem.setStatusChangedEvent( registry.getEvent<nmdQuestFlagsChangedEvent_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), "NMDQuestFlagsChangedEvent" ) );
	m_pFactValueChangedEvent = registry.getEvent<nmdWorldFactChange_t>(
		NMD_STRINGIZE( NMDWorldDatabase ), "NMDWorldFactValueChangedEvent" );
}

bool NMDWorldStateHost::notifyFactChanged( nmdWorldFactId_t nId )
{
	return NMDWorldFactChangeDispatcher::dispatch( m_Database, nId,
		&m_QuestSystem, m_pLocationSystem );
}

bool NMDWorldStateHost::publishFactChange( nmdWorldFactId_t nId, nmdWorldFactType_t eType,
	nmdWorldValue_t previous, nmdWorldValue_t current )
{
	const bool bDispatched = notifyFactChanged( nId );
	if ( m_pFactValueChangedEvent != NULL ) {
		const nmdWorldFactChange_t event = { nId, eType, 0u, previous, current };
		m_pFactValueChangedEvent->publish( event );
	}
	return bDispatched;
}

bool NMDWorldStateHost::setFactInteger( nmdWorldFactId_t nId, int32_t value )
{
	int32_t previous = 0;
	if ( !m_Database.getFactInteger( nId, previous ) ) {
		return false;
	}
	if ( previous == value ) {
		return true;
	}
	if ( !m_Database.setFactInteger( nId, value ) ) {
		return false;
	}
	return publishFactChange( nId, NMD_WORLD_FACT_INTEGER,
		nmdWorldValue_t::fromInteger( previous ), nmdWorldValue_t::fromInteger( value ) );
}

int32_t NMDWorldStateHost::getFactInteger( nmdWorldFactId_t nId ) const
{
	int32_t result = 0;
	return m_Database.getFactInteger( nId, result ) ? result : 0;
}

bool NMDWorldStateHost::setFactFloat( nmdWorldFactId_t nId, float value )
{
	float previous = 0.0f;
	if ( !m_Database.getFactFloat( nId, previous ) ) {
		return false;
	}
	const nmdWorldValue_t oldValue = nmdWorldValue_t::fromFloat( previous );
	const nmdWorldValue_t newValue = nmdWorldValue_t::fromFloat( value );
	if ( oldValue.nBits == newValue.nBits ) {
		return true;
	}
	if ( !m_Database.setFactFloat( nId, value ) ) {
		return false;
	}
	return publishFactChange( nId, NMD_WORLD_FACT_FLOAT, oldValue, newValue );
}

float NMDWorldStateHost::getFactFloat( nmdWorldFactId_t nId ) const
{
	float result = 0.0f;
	return m_Database.getFactFloat( nId, result ) ? result : 0.0f;
}

bool NMDWorldStateHost::setFactBoolean( nmdWorldFactId_t nId, bool value )
{
	bool previous = false;
	if ( !m_Database.getFactBoolean( nId, previous ) ) {
		return false;
	}
	if ( previous == value ) {
		return true;
	}
	if ( !m_Database.setFactBoolean( nId, value ) ) {
		return false;
	}
	return publishFactChange( nId, NMD_WORLD_FACT_BOOLEAN,
		nmdWorldValue_t::fromBoolean( previous ), nmdWorldValue_t::fromBoolean( value ) );
}

bool NMDWorldStateHost::getFactBoolean( nmdWorldFactId_t nId ) const
{
	bool result = false;
	return m_Database.getFactBoolean( nId, result ) ? result : false;
}

int32_t NMDWorldStateHost::getFactCount( void ) const { return static_cast<int32_t>( m_Database.getFactCount() ); }
int32_t NMDWorldStateHost::getLocationCount( void ) const { return static_cast<int32_t>( m_Database.getLocationCount() ); }
int32_t NMDWorldStateHost::getQuestCount( void ) const { return static_cast<int32_t>( m_Database.getQuestCount() ); }

void NMDWorldStateHost::_bind_methods( void )
{
	ClassDB::bind_method( D_METHOD( "get_fact_count" ), &NMDWorldStateHost::getFactCount );
	ClassDB::bind_method( D_METHOD( "get_location_count" ), &NMDWorldStateHost::getLocationCount );
	ClassDB::bind_method( D_METHOD( "get_quest_count" ), &NMDWorldStateHost::getQuestCount );
	ClassDB::bind_method( D_METHOD( "get_fact_integer", "fact_id" ), &NMDWorldStateHost::getFactInteger );
	ClassDB::bind_method( D_METHOD( "set_fact_integer", "fact_id", "value" ), &NMDWorldStateHost::setFactInteger );
	ClassDB::bind_method( D_METHOD( "get_fact_float", "fact_id" ), &NMDWorldStateHost::getFactFloat );
	ClassDB::bind_method( D_METHOD( "set_fact_float", "fact_id", "value" ), &NMDWorldStateHost::setFactFloat );
	ClassDB::bind_method( D_METHOD( "get_fact_boolean", "fact_id" ), &NMDWorldStateHost::getFactBoolean );
	ClassDB::bind_method( D_METHOD( "set_fact_boolean", "fact_id", "value" ), &NMDWorldStateHost::setFactBoolean );
}
