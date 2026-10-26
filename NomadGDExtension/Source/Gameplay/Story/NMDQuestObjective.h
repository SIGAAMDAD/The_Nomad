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

#ifndef __NMD_QUEST_OBJECTIVE_H__
#define __NMD_QUEST_OBJECTIVE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"
#include <godot_cpp/classes/ref_counted.hpp>

class NMDQuestSystem;

class NMDQuestObjective : public godot::RefCounted
{
	GDCLASS( NMDQuestObjective, godot::RefCounted )
public:
	NMDQuestObjective( void );
	void bind( NMDWorldDatabase *pDatabase, NMDQuestSystem *pSystem,
		nmdWorldHandle_t quest, uint16_t nObjective );
	void start( void );
	bool isActive( void ) const;
	bool isFailed( void ) const;
	bool isCompleted( void ) const;
	bool isOptional( void ) const;
	bool setStatus( nmdQuestObjectiveStatus_t status );
	nmdQuestObjectiveStatus_t getStatus( void ) const;
protected:
	static void _bind_methods( void );
private:
	const nmdWorldObjectiveHot_t *getHot( void ) const;
	NMDWorldDatabase *m_pDatabase;
	NMDQuestSystem *m_pSystem;
	nmdWorldHandle_t m_hQuest;
	uint16_t m_nObjective;
};

#endif // __NMD_QUEST_OBJECTIVE_H__
