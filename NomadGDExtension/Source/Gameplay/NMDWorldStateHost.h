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

#ifndef __NMD_WORLD_STATE_HOST_H__
#define __NMD_WORLD_STATE_HOST_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "World/NMDWorldDatabase.h"
#include "Story/NMDQuestSystem.h"
#include "Story/NMDStorylineSystem.h"
#include <godot_cpp/classes/node.hpp>

class NMDEventRegistry;
class NMDLocationSystem;

// The only Node that owns an NMDWorldDatabase instance.
class NMDWorldStateHost : public godot::Node
{
	GDCLASS( NMDWorldStateHost, godot::Node )
public:
	NMDWorldStateHost( void );
	~NMDWorldStateHost( void );
	NMDWorldDatabase& getDatabase( void );
	const NMDWorldDatabase& getDatabase( void ) const;
	NMDQuestSystem& getQuestSystem( void );
	NMDStorylineSystem& getStorylineSystem( void );
	NMDWorldFactValueChangedEvent getFactValueChangedEvent( void ) const;
	void setLocationSystem( NMDLocationSystem *pSystem );
	void attachEventRegistry( NMDEventRegistry& registry );
	bool notifyFactChanged( nmdWorldFactId_t nId );
	bool setFactInteger( nmdWorldFactId_t nId, int32_t value );
	int32_t getFactInteger( nmdWorldFactId_t nId ) const;
	bool setFactFloat( nmdWorldFactId_t nId, float value );
	float getFactFloat( nmdWorldFactId_t nId ) const;
	bool setFactBoolean( nmdWorldFactId_t nId, bool value );
	bool getFactBoolean( nmdWorldFactId_t nId ) const;
	int32_t getFactCount( void ) const;
	int32_t getLocationCount( void ) const;
	int32_t getQuestCount( void ) const;
protected:
	static void _bind_methods( void );
private:
	bool publishFactChange( nmdWorldFactId_t nId, nmdWorldFactType_t eType,
		nmdWorldValue_t previous, nmdWorldValue_t current );

	NMDWorldDatabase m_Database;
	NMDQuestSystem m_QuestSystem;
	NMDStorylineSystem m_StorylineSystem;
	NMDLocationSystem *m_pLocationSystem;
	NMDWorldFactValueChangedEvent m_pFactValueChangedEvent;
};

#endif // __NMD_WORLD_STATE_HOST_H__
