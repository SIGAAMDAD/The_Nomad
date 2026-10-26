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

#ifndef __NMD_QUEST_CONTENT_COMPILER_H__
#define __NMD_QUEST_CONTENT_COMPILER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"

class NMDQuestContentCompiler
{
public:
	static bool compileTree( NMDWorldDatabase& db, nmdWorldHandle_t hStory,
							 const nmdQuestData_t *pQuests, uint16_t nCount,
							 uint32_t nFirstQuest, uint16_t nRoot );
	static bool validate( const NMDWorldDatabase& db, uint16_t nCurrentObjective,
						  const nmdWorldObjectiveDesc_t *pObjectives, uint32_t nCount );
	static nmdWorldHandle_t registerQuest( NMDWorldDatabase& db, nmdWorldAtom_t pAtom,
										   uint32_t nFlags, uint16_t nCurrentObjective,
										   const nmdWorldObjectiveDesc_t *pObjectives, uint32_t nCount );
};

#endif // __NMD_QUEST_CONTENT_COMPILER_H__
