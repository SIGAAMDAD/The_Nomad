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

#ifndef __NMD_QUEST_TREE_H__
#define __NMD_QUEST_TREE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"

typedef bool (*nmdQuestVisitFn_t)( uint16_t nLocalIndex, void *pUser );

class NMDQuestTreeView
{
public:
	NMDQuestTreeView( const NMDWorldDatabase& db, nmdWorldHandle_t hStory );
	bool getParent( uint16_t nQuest, uint16_t& nParent ) const;
	bool getFirstChild( uint16_t nQuest, uint16_t& nChild ) const;
	bool getNextSibling( uint16_t nQuest, uint16_t& nSibling ) const;
	bool forEachChild( uint16_t nQuest, nmdQuestVisitFn_t visitor, void *pUser ) const;
	bool traverseDepthFirst( uint16_t nRoot, uint16_t *pStack,
		uint32_t nCapacity, nmdQuestVisitFn_t visitor, void *pUser ) const;
	bool getQuestHandle( uint16_t nLocalIndex, nmdWorldHandle_t& handle ) const;
	bool getRange( nmdWorldStoryTreeRange_t& range ) const;
private:
	bool getLink( uint16_t nQuest, nmdWorldQuestTreeLink_t& link ) const;
	const NMDWorldDatabase *m_pDatabase;
	nmdWorldHandle_t m_hStory;
};

// Objective topology uses authored parent indices. These scans are intended for
// editor/UI traversal and allocate no storage or temporary node objects.
class NMDObjectiveTreeView
{
public:
	NMDObjectiveTreeView( const NMDWorldDatabase& db, nmdWorldHandle_t quest );
	bool getParent( uint16_t index, uint16_t& parent ) const;
	bool forEachChild( uint16_t parent, nmdQuestVisitFn_t visitor, void *pUser ) const;
private:
	const NMDWorldDatabase *m_pDatabase;
	nmdWorldHandle_t m_hQuest;
};

#endif // __NMD_QUEST_TREE_H__
