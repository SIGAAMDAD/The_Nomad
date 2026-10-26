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

#ifndef __NMD_GAME_STATE_MANAGER_H__
#define __NMD_GAME_STATE_MANAGER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <godot_cpp/classes/wrapped.hpp>
#include <godot_cpp/classes/ref_counted.hpp>
#include <Framework/Events/NMDEvent.h>
#include <Framework/Events/NMDEventRegistry.h>

typedef enum nmdGameState_e : uint8_t
{
	/// @brief invalid state, if this is the state, we have a problem.
	NMD_GAMESTATE_NONE,

	/// @brief in the menu system.
	NMD_GAMESTATE_MENUS,

	/// @brief in the campaign.
	NMD_GAMESTATE_SOLOPLAY,

	/// @brief in a multiplayer lobby waiting room.
	NMD_GAMESTATE_LOBBY,

	/// @brief in a multiplayer game.
	NMD_GAMESTATE_MULTIPLAYER,

	/// @brief in the pause menu.
	NMD_GAMESTATE_PAUSED,

	NMD_GAMESTATE_COUNT
} nmdGameState_t;

typedef struct nmdGameStateChangedEvent_s
{
	const nmdGameState_t ePrev;
	const nmdGameState_t eCurrent;
} nmdGameStateChangedEvent_t;

typedef NMDEvent<nmdGameStateChangedEvent_t> *NMDGameStateChangedEvent;

class NMDGameStateManager : public godot::RefCounted
{
	GDCLASS( NMDGameStateManager, godot::RefCounted );

public:
	NMDGameStateManager( NMDEventRegistry& rEventRegistry );
	~NMDGameStateManager();

	bool setState( nmdGameState_t eState );

	bool isValidState( nmdGameState_t eState ) const;
	bool canTransitionTo( nmdGameState_t eState ) const;
	nmdGameState_t getState( void ) const;

	NMDGameStateChangedEvent getStateChangedEvent( void );

protected:
	//
	// Godot API
	//

	static void _bind_methods( void );

	bool set_state( int32_t state );
	bool is_valid_state( int32_t state ) const;
	bool can_transition_to( int32_t state ) const;
	int32_t get_state( void ) const;

private:
	NMDGameStateChangedEvent m_pStateChanged;

	nmdGameState_t m_eActiveState;
};

NOMAD_FORCE_INLINE bool NMDGameStateManager::isValidState( nmdGameState_t eState ) const
{
	return eState > NMD_GAMESTATE_NONE && eState < NMD_GAMESTATE_COUNT;
}

NOMAD_FORCE_INLINE nmdGameState_t NMDGameStateManager::getState( void ) const
{
	return m_eActiveState;
}

#endif // __NMD_GAME_STATE_MANAGER_H__
