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

#include "NMDQuestTree.h"

NMDQuestTreeView::NMDQuestTreeView( const NMDWorldDatabase& db, nmdWorldHandle_t hStory )
	: m_pDatabase( &db ), m_hStory( hStory )
{
}

bool NMDQuestTreeView::getRange( nmdWorldStoryTreeRange_t& range ) const
{
	return m_pDatabase->getStoryTreeRange( m_hStory, range );
}

bool NMDQuestTreeView::getLink( uint16_t local, nmdWorldQuestTreeLink_t& link ) const
{
	nmdWorldStoryTreeRange_t range = {};
	return getRange( range ) && local < range.nQuestCount &&
		   m_pDatabase->getQuestTreeLink( range.nFirstQuest + local, link );
}

bool NMDQuestTreeView::getQuestHandle( uint16_t local, nmdWorldHandle_t& handle ) const
{
	nmdWorldStoryTreeRange_t range = {};
	if ( !getRange( range ) || local >= range.nQuestCount ) {
		return false;
	}
	handle = { static_cast<uint32_t>( NMD_WORLD_RECORD_QUEST ) << NMD_WORLD_HANDLE_TYPE_SHIFT |
			   ( range.nFirstQuest + local ) };
	return true;
}

bool NMDQuestTreeView::getParent( uint16_t local, uint16_t& parent ) const
{
	nmdWorldQuestTreeLink_t link = {};
	if ( !getLink( local, link ) ) {
		return false;
	}
	parent = link.nParentQuest;
	return true;
}

bool NMDQuestTreeView::getFirstChild( uint16_t local, uint16_t& child ) const
{
	nmdWorldQuestTreeLink_t link = {};
	if ( !getLink( local, link ) ) {
		return false;
	}
	child = link.nFirstChildQuest;
	return true;
}

bool NMDQuestTreeView::getNextSibling( uint16_t local, uint16_t& sibling ) const
{
	nmdWorldQuestTreeLink_t link = {};
	if ( !getLink( local, link ) ) {
		return false;
	}
	sibling = link.nNextSiblingQuest;
	return true;
}

bool NMDQuestTreeView::forEachChild( uint16_t local, nmdQuestVisitFn_t visitor, void *pUser ) const
{
	if ( visitor == NULL ) {
		return false;
	}
	uint16_t child = UINT16_MAX;
	if ( !getFirstChild( local, child ) ) {
		return false;
	}
	nmdWorldStoryTreeRange_t range = {};
	if ( !getRange( range ) ) {
		return false;
	}
	uint32_t steps = 0u;
	while ( child != UINT16_MAX ) {
		if ( steps++ >= range.nQuestCount || child >= range.nQuestCount || !visitor( child, pUser ) ) {
			return false;
		}
		if ( !getNextSibling( child, child ) ) {
			return false;
		}
	}
	return true;
}

bool NMDQuestTreeView::traverseDepthFirst( uint16_t root, uint16_t *pStack, uint32_t nCapacity, nmdQuestVisitFn_t visitor, void *pUser ) const
{
	nmdWorldStoryTreeRange_t range;
	memset( &range, 0, sizeof( range ) );

	if ( !getRange( range ) || root >= range.nQuestCount || nCapacity < range.nQuestCount || pStack == NULL || visitor == NULL ) {
		return false;
	}

	uint32_t count = 1u;
	uint32_t visited = 0u;
	pStack[ 0 ] = root;

	while ( count != 0u ) {
		const uint16_t current = pStack[ --count ];
		if ( ++visited > range.nQuestCount || !visitor( current, pUser ) ) {
			return false;
		}
		uint16_t child = UINT16_MAX;
		if ( !getFirstChild( current, child ) ) {
			return false;
		}
		uint32_t siblings = 0u;
		const uint32_t first = count;
		while ( child != UINT16_MAX ) {
			if ( child >= range.nQuestCount || ++siblings > range.nQuestCount || count >= nCapacity ) {
				return false;
			}
			pStack[ count++ ] = child;
			if ( !getNextSibling( child, child ) ) {
				return false;
			}
		}

		// The stack is LIFO; reverse siblings so first-child traversal stays deterministic.
		for ( uint32_t left = first, right = count; left < right && left < --right; ++left ) {
			const uint16_t temp = pStack[ left ];
			pStack[ left ] = pStack[ right ];
			pStack[ right ] = temp;
		}
	}

	return true;
}

NMDObjectiveTreeView::NMDObjectiveTreeView( const NMDWorldDatabase& db, nmdWorldHandle_t quest )
	: m_pDatabase( &db ), m_hQuest( quest )
{
}

bool NMDObjectiveTreeView::getParent( uint16_t index, uint16_t& parent ) const
{
	const nmdWorldObjectiveHot_t *pObjective = m_pDatabase->getObjective( m_hQuest, index );
	if ( pObjective == NULL ) {
		return false;
	}
	parent = pObjective->nParentObjective;
	return true;
}

bool NMDObjectiveTreeView::forEachChild( uint16_t parent,
										 nmdQuestVisitFn_t visitor, void *pUser ) const
{
	const nmdWorldQuestHot_t *pQuest = m_pDatabase->getQuest( m_hQuest );
	if ( pQuest == NULL || visitor == NULL ||
		 ( parent != NMD_WORLD_OBJECTIVE_NO_PARENT && parent >= pQuest->nObjectiveCount ) ) {
		return false;
	}
	for ( uint16_t i = 0u; i < pQuest->nObjectiveCount; ++i ) {
		const nmdWorldObjectiveHot_t *pObjective = m_pDatabase->getObjective( m_hQuest, i );
		if ( pObjective == NULL ) {
			return false;
		}
		if ( pObjective->nParentObjective == parent && !visitor( i, pUser ) ) {
			return false;
		}
	}
	return true;
}
