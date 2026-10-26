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

#include "NMDQuestObjective.h"
#include "NMDQuestSystem.h"
#include <godot_cpp/core/class_db.hpp>

using namespace godot;

NMDQuestObjective::NMDQuestObjective( void )
	: m_pDatabase( NULL ), m_pSystem( NULL ),
	m_hQuest{ NMD_WORLD_HANDLE_INVALID }, m_nObjective( UINT16_MAX )
{
}

void NMDQuestObjective::bind( NMDWorldDatabase *pDatabase, NMDQuestSystem *pSystem,
	nmdWorldHandle_t quest, uint16_t index )
{
	m_pDatabase = pDatabase;
	m_pSystem = pSystem;
	m_hQuest = quest;
	m_nObjective = index;
}

const nmdWorldObjectiveHot_t *NMDQuestObjective::getHot( void ) const
{
	return m_pDatabase == NULL ? NULL : m_pDatabase->getObjective( m_hQuest, m_nObjective );
}

nmdQuestObjectiveStatus_t NMDQuestObjective::getStatus( void ) const
{
	const nmdWorldObjectiveHot_t *pHot = getHot();
	return pHot == NULL ? NMD_QUEST_OBJECTIVE_STATUS_INACTIVE :
		static_cast<nmdQuestObjectiveStatus_t>( ( pHot->nFlagsAndStatus & NMD_WORLD_OBJECTIVE_STATUS_MASK ) >>
			NMD_WORLD_OBJECTIVE_STATUS_SHIFT );
}

bool NMDQuestObjective::setStatus( nmdQuestObjectiveStatus_t status )
{
	return m_pSystem != NULL && m_pSystem->setObjectiveStatus( m_hQuest, m_nObjective, status );
}

void NMDQuestObjective::start( void )
{
	setStatus( NMD_QUEST_OBJECTIVE_STATUS_ACTIVE );
}

bool NMDQuestObjective::isActive( void ) const
{
	const nmdQuestObjectiveStatus_t status = getStatus();
	return status == NMD_QUEST_OBJECTIVE_STATUS_ACTIVE;
}

bool NMDQuestObjective::isFailed( void ) const
{
	const nmdQuestObjectiveStatus_t status = getStatus();
	return status == NMD_QUEST_OBJECTIVE_STATUS_FAILED;
}

bool NMDQuestObjective::isCompleted( void ) const
{
	const nmdQuestObjectiveStatus_t status = getStatus();
	return status == NMD_QUEST_OBJECTIVE_STATUS_COMPLETED;
}

bool NMDQuestObjective::isOptional( void ) const
{
	const nmdWorldObjectiveHot_t *pHot = getHot();
	return pHot != NULL && ( pHot->nFlagsAndStatus & NMD_QUEST_OBJECTIVE_FLAG_IS_REQUIRED ) == 0u;
}

void NMDQuestObjective::_bind_methods( void )
{
	ClassDB::bind_method( D_METHOD( "start" ), &NMDQuestObjective::start );
	ClassDB::bind_method( D_METHOD( "is_active" ), &NMDQuestObjective::isActive );
	ClassDB::bind_method( D_METHOD( "is_failed" ), &NMDQuestObjective::isFailed );
	ClassDB::bind_method( D_METHOD( "is_completed" ), &NMDQuestObjective::isCompleted );
	ClassDB::bind_method( D_METHOD( "is_optional" ), &NMDQuestObjective::isOptional );
	ClassDB::bind_method( D_METHOD( "get_status" ), &NMDQuestObjective::getStatus );
}
