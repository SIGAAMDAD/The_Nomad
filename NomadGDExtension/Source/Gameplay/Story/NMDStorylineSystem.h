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

#ifndef __NMD_STORYLINE_SYSTEM_H__
#define __NMD_STORYLINE_SYSTEM_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"

class NMDQuestSystem;

// Storyline progression is gameplay logic. The database stores only the
// authored quest range and the compact mutable story state.
class NMDStorylineSystem
{
public:
	NMDStorylineSystem( void );

	void setDatabase( NMDWorldDatabase *pDatabase );
	void setQuestSystem( NMDQuestSystem *pQuestSystem );

	bool startStory( nmdWorldHandle_t hStory );
	bool setBranch( nmdWorldHandle_t hStory, uint32_t nBranchId );
	bool reevaluateStory( nmdWorldHandle_t hStory );
	bool notifyQuestChanged( nmdWorldHandle_t hQuest );

private:
	NMDWorldDatabase *m_pDatabase;
	NMDQuestSystem *m_pQuestSystem;
};

#endif // __NMD_STORYLINE_SYSTEM_H__
