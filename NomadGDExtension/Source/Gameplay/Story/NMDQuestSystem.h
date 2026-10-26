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

#ifndef __NMD_QUEST_SYSTEM_H__
#define __NMD_QUEST_SYSTEM_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"
#include "NMDQuestTree.h"

// Owns quest transitions and fact-driven condition reevaluation, not storage.
class NMDStorylineSystem;

class NMDQuestSystem
{
public:
	NMDQuestSystem( void );

	void setDatabase( NMDWorldDatabase *pDatabase );
	void setStatusChangedEvent( NMDQuestFlagsChangedEvent pEvent );
	void setStorylineSystem( NMDStorylineSystem *pSystem );
	bool startQuest( nmdWorldHandle_t quest );
	bool completeQuest( nmdWorldHandle_t quest );
	bool failQuest( nmdWorldHandle_t quest );
	bool abandonQuest( nmdWorldHandle_t quest );
	bool setObjectiveStatus( nmdWorldHandle_t quest, uint32_t index,
		nmdQuestObjectiveStatus_t status );
	bool reevaluateObjective( nmdWorldHandle_t quest, uint32_t index );
	bool reevaluateQuest( nmdWorldHandle_t quest );
	bool getChildPolicySatisfied( nmdWorldHandle_t hStory, uint16_t localQuest,
		bool& satisfied ) const;
private:
	bool reevaluateParentAfterCompletion( nmdWorldHandle_t quest );
	bool setQuestFlags( nmdWorldHandle_t quest, uint32_t flags );
	NMDWorldDatabase *m_pDatabase;
	NMDStorylineSystem *m_pStorylineSystem;
	NMDQuestFlagsChangedEvent m_pStatusChanged;
};

#endif // __NMD_QUEST_SYSTEM_H__
