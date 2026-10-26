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

#include "NMDGameStateManager.h"
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/core/object.hpp>

NMDGameStateManager::NMDGameStateManager( NMDEventRegistry& rEventRegistry )
{
	m_pStateChanged = rEventRegistry.getEvent<nmdGameStateChangedEvent_t>(
		NMD_STRINGIZE( GameStateChanged ),
		NMD_STRINGIZE( NMDGameStateManager ) );
}

NMDGameStateManager::~NMDGameStateManager()
{
	m_pStateChanged->clear();
}

bool NMDGameStateManager::setState( nmdGameState_t eState )
{
	if ( !isValidState( eState ) || !canTransitionTo( eState ) ) {
		return false;
	}

	const nmdGameState_t ePrev = m_eActiveState;
	m_eActiveState = eState;

	m_pStateChanged->publish( { .ePrev = ePrev, .eCurrent = m_eActiveState } );

	return true;
}


bool NMDGameStateManager::canTransitionTo( nmdGameState_t eState ) const
{
	if ( !isValidState( eState ) || m_eActiveState == eState ) {
		return false;
	}

	switch ( eState ) {
		case NMD_GAMESTATE_MENUS:
			return true;
		case NMD_GAMESTATE_SOLOPLAY:
			return m_eActiveState == NMD_GAMESTATE_MENUS || m_eActiveState == NMD_GAMESTATE_PAUSED;
		case NMD_GAMESTATE_PAUSED:
			return m_eActiveState == NMD_GAMESTATE_SOLOPLAY || m_eActiveState == NMD_GAMESTATE_MULTIPLAYER;
		case NMD_GAMESTATE_MULTIPLAYER:
			return m_eActiveState == NMD_GAMESTATE_LOBBY || m_eActiveState == NMD_GAMESTATE_PAUSED;
		case NMD_GAMESTATE_LOBBY:
			return m_eActiveState == NMD_GAMESTATE_MENUS || m_eActiveState == NMD_GAMESTATE_MULTIPLAYER;
		default:
			break;
	}

	return false;
}

bool NMDGameStateManager::set_state( int32_t state )
{
	return setState( (nmdGameState_t)state );
}

bool NMDGameStateManager::is_valid_state( int32_t state ) const
{
	return isValidState( (nmdGameState_t)state );
}

bool NMDGameStateManager::can_transition_to( int32_t state ) const
{
	return canTransitionTo( (nmdGameState_t)state );
}

int32_t NMDGameStateManager::get_state( void ) const
{
	return (int32_t)m_eActiveState;
}

void NMDGameStateManager::_bind_methods( void )
{
	godot::ClassDB::bind_integer_constant( "NMDGameStateManager", "NMDGameState", NMD_STRINGIZE( NMD_GAMESTATE_NONE ), NMD_GAMESTATE_NONE );
	godot::ClassDB::bind_integer_constant( "NMDGameStateManager", "NMDGameState", NMD_STRINGIZE( NMD_GAMESTATE_MENUS ), NMD_GAMESTATE_MENUS );
	godot::ClassDB::bind_integer_constant( "NMDGameStateManager", "NMDGameState", NMD_STRINGIZE( NMD_GAMESTATE_SOLOPLAY ), NMD_GAMESTATE_SOLOPLAY );
	godot::ClassDB::bind_integer_constant( "NMDGameStateManager", "NMDGameState", NMD_STRINGIZE( NMD_GAMESTATE_LOBBY ), NMD_GAMESTATE_LOBBY );
	godot::ClassDB::bind_integer_constant( "NMDGameStateManager", "NMDGameState", NMD_STRINGIZE( NMD_GAMESTATE_MULTIPLAYER ), NMD_GAMESTATE_MULTIPLAYER );
	godot::ClassDB::bind_integer_constant( "NMDGameStateManager", "NMDGameState", NMD_STRINGIZE( NMD_GAMESTATE_PAUSED ), NMD_GAMESTATE_PAUSED );

	godot::ClassDB::bind_method(
		godot::D_METHOD( "set_state", "state" ),
		&NMDGameStateManager::set_state );

	godot::ClassDB::bind_method(
		godot::D_METHOD( "is_valid_state", "state" ),
		&NMDGameStateManager::is_valid_state );

	godot::ClassDB::bind_method(
		godot::D_METHOD( "can_transition_to", "state" ),
		&NMDGameStateManager::can_transition_to );

	godot::ClassDB::bind_method(
		godot::D_METHOD( "get_state" ),
		&NMDGameStateManager::get_state );

	ADD_PROPERTY( godot::PropertyInfo( godot::Variant::INT, "state", godot::PropertyHint::PROPERTY_HINT_ENUM, "None,Menus,Soloplay,Lobby,Multiplayer,Pause" ), "set_state", "get_state" );
}
