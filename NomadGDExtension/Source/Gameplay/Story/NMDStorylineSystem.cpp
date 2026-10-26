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

#include "NMDStorylineSystem.h"
#include "NMDQuestSystem.h"
#include "NMDQuestTree.h"

NMDStorylineSystem::NMDStorylineSystem( void ) :
	m_pDatabase( NULL ), m_pQuestSystem( NULL )
{
}

void NMDStorylineSystem::setDatabase( NMDWorldDatabase *pDatabase )
{
	m_pDatabase = pDatabase;
}

void NMDStorylineSystem::setQuestSystem( NMDQuestSystem *pQuestSystem )
{
	m_pQuestSystem = pQuestSystem;
}

bool NMDStorylineSystem::startStory( nmdWorldHandle_t hStory )
{
	if ( m_pDatabase == NULL || m_pQuestSystem == NULL ) {
		return false;
	}

	const NMDQuestTreeView tree( *m_pDatabase, hStory );
	nmdWorldStoryTreeRange_t range = {};
	nmdWorldHandle_t hRoot = { NMD_WORLD_HANDLE_INVALID };
	if ( !tree.getRange( range ) || range.nQuestCount == 0u ||
		!tree.getQuestHandle( range.nRootQuest, hRoot ) ||
		!m_pQuestSystem->startQuest( hRoot ) ) {
		return false;
	}
	return reevaluateStory( hStory );
}

bool NMDStorylineSystem::setBranch( nmdWorldHandle_t hStory, uint32_t nBranchId )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}
	nmdWorldStoryState_t state = {};
	if ( !m_pDatabase->getStory( hStory, state ) ) {
		return false;
	}
	if ( state.nBranchId == nBranchId ) {
		return true;
	}
	state.nBranchId = nBranchId;
	++state.nRevision;
	return m_pDatabase->setStory( hStory, state );
}

bool NMDStorylineSystem::reevaluateStory( nmdWorldHandle_t hStory )
{
	if ( m_pDatabase == NULL ) {
		return false;
	}

	const NMDQuestTreeView tree( *m_pDatabase, hStory );
	nmdWorldStoryTreeRange_t range = {};
	nmdWorldStoryState_t state = {};
	if ( !tree.getRange( range ) || !m_pDatabase->getStory( hStory, state ) ) {
		return false;
	}
	// An empty range is a valid authored story without a current quest.
	if ( range.nQuestCount != 0u && range.nRootQuest >= range.nQuestCount ) {
		return false;
	}

	uint16_t nFirstActive = NMD_WORLD_INVALID_LOCAL_INDEX;
	uint16_t nCompleted = 0u;
	uint32_t nRootFlags = 0u;
	for ( uint32_t i = 0u; i < range.nQuestCount; ++i ) {
		nmdWorldHandle_t hQuest = { NMD_WORLD_HANDLE_INVALID };
		if ( !tree.getQuestHandle( static_cast<uint16_t>( i ), hQuest ) ) {
			return false;
		}
		const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( hQuest );
		if ( pQuest == NULL ) {
			return false;
		}
		if ( i == range.nRootQuest ) {
			nRootFlags = pQuest->nFlags;
		}
		if ( pQuest->nFlags & NMD_QUEST_FLAG_COMPLETED ) {
			++nCompleted;
		}
		if ( nFirstActive == NMD_WORLD_INVALID_LOCAL_INDEX &&
			( pQuest->nFlags & NMD_QUEST_FLAG_ACTIVE ) != 0u ) {
			nFirstActive = static_cast<uint16_t>( i );
		}
	}

	uint32_t nFlags = state.nFlags & ~static_cast<uint32_t>(
		NMD_STORY_FLAG_ACTIVE | NMD_STORY_FLAG_COMPLETED | NMD_STORY_FLAG_FAILED );
	if ( ( nRootFlags & NMD_QUEST_FLAG_COMPLETED ) != 0u ) {
		nFlags |= NMD_STORY_FLAG_COMPLETED;
	} else if ( ( nRootFlags & NMD_QUEST_FLAG_FAILED ) != 0u ) {
		nFlags |= NMD_STORY_FLAG_FAILED;
	} else if ( nFirstActive != NMD_WORLD_INVALID_LOCAL_INDEX ) {
		nFlags |= NMD_STORY_FLAG_ACTIVE;
	}

	if ( state.nFlags == nFlags && state.nActiveNode == nFirstActive &&
		state.nCompletedNodes == nCompleted ) {
		return true;
	}
	state.nFlags = nFlags;
	state.nActiveNode = nFirstActive;
	state.nCompletedNodes = nCompleted;
	++state.nRevision;
	return m_pDatabase->setStory( hStory, state );
}

bool NMDStorylineSystem::notifyQuestChanged( nmdWorldHandle_t hQuest )
{
	if ( m_pDatabase == NULL || hQuest.getType() != NMD_WORLD_RECORD_QUEST ||
		!m_pDatabase->getRecordExists( hQuest ) ) {
		return false;
	}
	bool bSuccess = true;
	const uint32_t nQuest = hQuest.getIndex();
	for ( uint32_t i = 0u; i < m_pDatabase->getStoryCount(); ++i ) {
		const nmdWorldHandle_t hStory = {
			( static_cast<uint32_t>( NMD_WORLD_RECORD_STORY ) << NMD_WORLD_HANDLE_TYPE_SHIFT ) | i
		};
		nmdWorldStoryTreeRange_t range = {};
		if ( !m_pDatabase->getStoryTreeRange( hStory, range ) ) {
			bSuccess = false;
			continue;
		}
		if ( range.nQuestCount != 0u && nQuest >= range.nFirstQuest &&
			nQuest - range.nFirstQuest < range.nQuestCount && !reevaluateStory( hStory ) ) {
			bSuccess = false;
		}
	}
	return bSuccess;
}
