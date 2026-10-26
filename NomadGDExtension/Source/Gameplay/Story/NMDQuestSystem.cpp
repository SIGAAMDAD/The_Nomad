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

#include "NMDQuestSystem.h"
#include "NMDWorldConditions.h"
#include "NMDStorylineSystem.h"

NMDQuestSystem::NMDQuestSystem( void ) : m_pDatabase( NULL ), m_pStorylineSystem( NULL ), m_pStatusChanged( NULL )
{
}

void NMDQuestSystem::setDatabase( NMDWorldDatabase *pDatabase )
{
	m_pDatabase = pDatabase;
}

void NMDQuestSystem::setStatusChangedEvent( NMDQuestFlagsChangedEvent pEvent )
{
	m_pStatusChanged = pEvent;
}

void NMDQuestSystem::setStorylineSystem( NMDStorylineSystem *pSystem )
{
	m_pStorylineSystem = pSystem;
}

bool NMDQuestSystem::setQuestFlags( nmdWorldHandle_t handle, uint32_t flags )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( handle );
	if ( pQuest == NULL ) {
		return false;
	}
	const uint32_t previous = pQuest->nFlags;
	if ( previous == flags ) {
		return true;
	}
	if ( !m_pDatabase->setQuestFlags( handle, flags ) ) {
		return false;
	}
	// Keep story counters/state in sync before publishing gameplay events.
	const bool bStoryUpdated = m_pStorylineSystem == NULL ||
		m_pStorylineSystem->notifyQuestChanged( handle );
	if ( m_pStatusChanged != NULL ) {
		const nmdQuestFlagsChangedEvent_t event = {
			handle.getIndex(), static_cast<nmdQuestFlags_t>( previous ),
			static_cast<nmdQuestFlags_t>( flags )
		};
		m_pStatusChanged->publish( event );
	}
	return bStoryUpdated;
}

bool NMDQuestSystem::startQuest( nmdWorldHandle_t handle )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( handle );
	if ( pQuest == NULL || ( pQuest->nFlags & ( NMD_QUEST_FLAG_LOCKED |
		NMD_QUEST_FLAG_COMPLETED | NMD_QUEST_FLAG_FAILED ) ) != 0u ) {
		return false;
	}
	// An authored current objective becomes active when its quest starts.
	if ( pQuest->nCurrentObjective != UINT16_MAX ) {
		const nmdWorldObjectiveHot_t *pObjective = m_pDatabase->getObjective(
			handle, pQuest->nCurrentObjective );
		if ( pObjective == NULL ) {
			return false;
		}
		const uint16_t status = ( pObjective->nFlagsAndStatus & NMD_WORLD_OBJECTIVE_STATUS_MASK ) >>
			NMD_WORLD_OBJECTIVE_STATUS_SHIFT;
		if ( status == NMD_QUEST_OBJECTIVE_STATUS_INACTIVE &&
			!m_pDatabase->setObjectiveStatus( handle, pQuest->nCurrentObjective,
				NMD_QUEST_OBJECTIVE_STATUS_ACTIVE ) ) {
			return false;
		}
	}
	if ( !setQuestFlags( handle, ( pQuest->nFlags | NMD_QUEST_FLAG_DISCOVERED |
		NMD_QUEST_FLAG_ACTIVE ) & ~static_cast<uint32_t>( NMD_QUEST_FLAG_SUSPENDED ) ) ) {
		return false;
	}
	if ( pQuest->nCurrentObjective != UINT16_MAX ) {
		return reevaluateObjective( handle, pQuest->nCurrentObjective );
	}
	return reevaluateQuest( handle );
}

bool NMDQuestSystem::completeQuest( nmdWorldHandle_t handle )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( handle );
	if ( pQuest == NULL || ( pQuest->nFlags &
		( NMD_QUEST_FLAG_READY_TO_COMPLETE | NMD_QUEST_FLAG_ACTIVE ) ) !=
		( NMD_QUEST_FLAG_READY_TO_COMPLETE | NMD_QUEST_FLAG_ACTIVE ) ) {
		return false;
	}
	if ( !setQuestFlags( handle, ( pQuest->nFlags |
		NMD_QUEST_FLAG_COMPLETED ) & ~static_cast<uint32_t>( NMD_QUEST_FLAG_ACTIVE ) ) ) {
		return false;
	}
	return reevaluateParentAfterCompletion( handle );
}

bool NMDQuestSystem::failQuest( nmdWorldHandle_t handle )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( handle );
	return pQuest != NULL && setQuestFlags( handle,
		( pQuest->nFlags | NMD_QUEST_FLAG_FAILED ) & ~static_cast<uint32_t>( NMD_QUEST_FLAG_ACTIVE ) );
}

bool NMDQuestSystem::abandonQuest( nmdWorldHandle_t handle )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( handle );
	if ( pQuest == NULL || ( pQuest->nFlags & NMD_QUEST_FLAG_CAN_ABANDON ) == 0u ) {
		return false;
	}
	return setQuestFlags( handle, ( pQuest->nFlags | NMD_QUEST_FLAG_ABANDONED ) &
		~static_cast<uint32_t>( NMD_QUEST_FLAG_ACTIVE ) );
}

bool NMDQuestSystem::setObjectiveStatus( nmdWorldHandle_t quest,
	uint32_t index, nmdQuestObjectiveStatus_t status )
{
	if ( m_pDatabase == NULL || status >= NMD_QUEST_OBJECTIVE_STATUS_COUNT ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( quest );
	if ( pQuest == NULL || ( pQuest->nFlags & ( NMD_QUEST_FLAG_COMPLETED |
		NMD_QUEST_FLAG_FAILED | NMD_QUEST_FLAG_ABANDONED ) ) != 0u ) {
		return false;
	}
	const nmdWorldObjectiveHot_t *pObjective = m_pDatabase->getObjective( quest, index );
	if ( pObjective == NULL ) {
		return false;
	}
	const bool bFailsQuest = status == NMD_QUEST_OBJECTIVE_STATUS_FAILED &&
		( pObjective->nFlagsAndStatus & NMD_QUEST_OBJECTIVE_FLAG_FAILS_QUEST ) != 0u;
	if ( !m_pDatabase->setObjectiveStatus( quest, index, static_cast<uint8_t>( status ) ) ) {
		return false;
	}
	if ( bFailsQuest ) {
		return failQuest( quest );
	}
	return reevaluateQuest( quest );
}

bool NMDQuestSystem::reevaluateObjective( nmdWorldHandle_t quest, uint32_t index )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( quest );
	if ( pQuest == NULL ) {
		return false;
	}
	const nmdWorldObjectiveHot_t *pObjective = m_pDatabase->getObjective( quest, index );
	if ( pObjective == NULL ) {
		return false;
	}
	if ( ( pQuest->nFlags & ( NMD_QUEST_FLAG_COMPLETED |
		NMD_QUEST_FLAG_FAILED | NMD_QUEST_FLAG_ABANDONED ) ) != 0u ) {
		return true;
	}
	const bool bHasConditions = pObjective->nConditionCount != 0u;
	bool bSatisfied = false;
	if ( !nmdWorldEvaluateObjective( *m_pDatabase, quest, index, bSatisfied ) ||
		!m_pDatabase->setObjectiveSatisfied( quest, index, bSatisfied ) ) {
		return false;
	}
	const uint16_t nStatus = ( pObjective->nFlagsAndStatus & NMD_WORLD_OBJECTIVE_STATUS_MASK ) >>
		NMD_WORLD_OBJECTIVE_STATUS_SHIFT;
	if ( bSatisfied && bHasConditions &&
		( nStatus == NMD_QUEST_OBJECTIVE_STATUS_ACTIVE ||
			( nStatus == NMD_QUEST_OBJECTIVE_STATUS_INACTIVE &&
			( pObjective->nFlagsAndStatus & NMD_QUEST_OBJECTIVE_FLAG_BACKGROUND_PROGRESS ) != 0u ) ) ) {
		return setObjectiveStatus( quest, index, NMD_QUEST_OBJECTIVE_STATUS_COMPLETED );
	}
	return reevaluateQuest( quest );
}

bool NMDQuestSystem::getChildPolicySatisfied( nmdWorldHandle_t hStory,
	uint16_t local, bool& satisfied ) const
{
	satisfied = false;
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const NMDQuestTreeView tree( *m_pDatabase, hStory );
	nmdWorldStoryTreeRange_t range = {};
	if ( !tree.getRange( range ) || local >= range.nQuestCount ) {
		return false;
	}
	nmdWorldQuestTreeLink_t link = {};
	if ( !m_pDatabase->getQuestTreeLink( range.nFirstQuest + local, link ) ) {
		return false;
	}
	uint32_t total = 0u, completed = 0u;
	uint16_t child = link.nFirstChildQuest;
	while ( child != UINT16_MAX ) {
		if ( child >= range.nQuestCount || ++total > range.nQuestCount ) {
			return false;
		}
		nmdWorldHandle_t hChild = {};
		if ( !tree.getQuestHandle( child, hChild ) ) {
			return false;
		}
		const nmdWorldQuestHot_t *pChild = m_pDatabase->getQuest( hChild );
		if ( pChild == NULL ) {
			return false;
		}
		if ( pChild->nFlags & NMD_QUEST_FLAG_COMPLETED ) {
			++completed;
		}
		if ( !tree.getNextSibling( child, child ) ) {
			return false;
		}
	}
	switch ( link.nChildPolicy ) {
		case NMD_QUEST_CHILD_POLICY_ALL: satisfied = total == completed; break;
		case NMD_QUEST_CHILD_POLICY_ANY: satisfied = total == 0u || completed > 0u; break;
		case NMD_QUEST_CHILD_POLICY_AT_LEAST: satisfied = completed >= link.nRequiredChildren; break;
		default: return false;
	}
	return true;
}

bool NMDQuestSystem::reevaluateQuest( nmdWorldHandle_t quest )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( quest );
	if ( pQuest == NULL ) {
		return false;
	}
	// Terminal quests keep their published outcome and cannot become ready again.
	if ( pQuest->nFlags & ( NMD_QUEST_FLAG_COMPLETED | NMD_QUEST_FLAG_FAILED |
		NMD_QUEST_FLAG_ABANDONED ) ) {
		return true;
	}
	bool ready = true;
	for ( uint32_t i = 0u; i < pQuest->nObjectiveCount; ++i ) {
		const nmdWorldObjectiveHot_t *pObjective = m_pDatabase->getObjective( quest, i );
		if ( pObjective == NULL ) {
			return false;
		}
		const uint16_t status = ( pObjective->nFlagsAndStatus & NMD_WORLD_OBJECTIVE_STATUS_MASK ) >>
			NMD_WORLD_OBJECTIVE_STATUS_SHIFT;
		if ( ( pObjective->nFlagsAndStatus & NMD_QUEST_OBJECTIVE_FLAG_IS_REQUIRED ) &&
			status != NMD_QUEST_OBJECTIVE_STATUS_COMPLETED ) {
			ready = false;
		}
	}
	const uint32_t questIndex = quest.getIndex();
	for ( uint32_t storyIndex = 0u; storyIndex < m_pDatabase->getStoryCount(); ++storyIndex ) {
		const nmdWorldHandle_t hStory = { ( static_cast<uint32_t>( NMD_WORLD_RECORD_STORY ) <<
			NMD_WORLD_HANDLE_TYPE_SHIFT ) | storyIndex };
		nmdWorldStoryTreeRange_t range = {};
		if ( !m_pDatabase->getStoryTreeRange( hStory, range ) || range.nQuestCount == 0u ||
			questIndex < range.nFirstQuest || questIndex - range.nFirstQuest >= range.nQuestCount ) {
			continue;
		}
		bool childrenReady = false;
		if ( !getChildPolicySatisfied( hStory, static_cast<uint16_t>( questIndex - range.nFirstQuest ),
			childrenReady ) ) {
			return false;
		}
		ready = ready && childrenReady;
		break;
	}
	const uint32_t flags = ready ? ( pQuest->nFlags | NMD_QUEST_FLAG_READY_TO_COMPLETE ) :
		( pQuest->nFlags & ~static_cast<uint32_t>( NMD_QUEST_FLAG_READY_TO_COMPLETE ) );
	return setQuestFlags( quest, flags );
}

// Propagate a completed child's changed state into its parent's readiness.
bool NMDQuestSystem::reevaluateParentAfterCompletion( nmdWorldHandle_t quest )
{
	const uint32_t questIndex = quest.getIndex();
	for ( uint32_t storyIndex = 0u; storyIndex < m_pDatabase->getStoryCount(); ++storyIndex ) {
		const nmdWorldHandle_t hStory = { ( static_cast<uint32_t>( NMD_WORLD_RECORD_STORY ) <<
			NMD_WORLD_HANDLE_TYPE_SHIFT ) | storyIndex };
		const NMDQuestTreeView tree( *m_pDatabase, hStory );
		nmdWorldStoryTreeRange_t range = {};
		if ( !tree.getRange( range ) || range.nQuestCount == 0u ||
			questIndex < range.nFirstQuest || questIndex - range.nFirstQuest >= range.nQuestCount ) {
			continue;
		}
		uint16_t parent = UINT16_MAX;
		if ( !tree.getParent( static_cast<uint16_t>( questIndex - range.nFirstQuest ), parent ) ) {
			return false;
		}
		if ( parent == UINT16_MAX ) {
			return true;
		}
		nmdWorldHandle_t hParent = {};
		return tree.getQuestHandle( parent, hParent ) && reevaluateQuest( hParent );
	}
	return true; // Standalone quests have no parent topology.
}
