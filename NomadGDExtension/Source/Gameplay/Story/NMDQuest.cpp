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

#include "NMDQuest.h"
#include "NMDQuestSystem.h"
#include <godot_cpp/core/class_db.hpp>

using namespace godot;

NMDQuest::NMDQuest( void ) : m_pDatabase( NULL ), m_pSystem( NULL ),
							 m_hQuest { NMD_WORLD_HANDLE_INVALID }
{
}

void NMDQuest::bind( NMDWorldDatabase *pDatabase, NMDQuestSystem *pSystem,
					 nmdWorldHandle_t handle )
{
	m_pDatabase = pDatabase;
	m_pSystem = pSystem;
	m_hQuest = handle;
}

void NMDQuest::start( void )
{
	if ( m_pSystem != NULL ) {
		m_pSystem->startQuest( m_hQuest );
	}
}

bool NMDQuest::isActive( void ) const
{
	const nmdWorldQuestHot_t *pQuest = m_pDatabase == NULL ? NULL : m_pDatabase->getQuest( m_hQuest );
	return pQuest != NULL && ( pQuest->nFlags & NMD_QUEST_FLAG_ACTIVE ) != 0u;
}

bool NMDQuest::isCompleted( void ) const
{
	const nmdWorldQuestHot_t *pQuest = m_pDatabase == NULL ? NULL : m_pDatabase->getQuest( m_hQuest );
	return pQuest != NULL && ( pQuest->nFlags & NMD_QUEST_FLAG_COMPLETED ) != 0u;
}

bool NMDQuest::isAbandoned( void ) const
{
	const nmdWorldQuestHot_t *pQuest = m_pDatabase == NULL ? NULL : m_pDatabase->getQuest( m_hQuest );
	return pQuest != NULL && ( pQuest->nFlags & NMD_QUEST_FLAG_ABANDONED ) != 0u;
}

int32_t NMDQuest::getActiveObjective( void ) const
{
	const nmdWorldQuestHot_t *pQuest = m_pDatabase == NULL ? NULL : m_pDatabase->getQuest( m_hQuest );
	return pQuest == NULL || pQuest->nCurrentObjective == UINT16_MAX ? -1 : static_cast<int32_t>( pQuest->nCurrentObjective );
}

void NMDQuest::_bind_methods( void )
{
	ClassDB::bind_method( D_METHOD( "start" ), &NMDQuest::start );
	ClassDB::bind_method( D_METHOD( "is_active" ), &NMDQuest::isActive );
	ClassDB::bind_method( D_METHOD( "is_completed" ), &NMDQuest::isCompleted );
	ClassDB::bind_method( D_METHOD( "is_abandoned" ), &NMDQuest::isAbandoned );
	ClassDB::bind_method( D_METHOD( "get_active_objective" ), &NMDQuest::getActiveObjective );
}
