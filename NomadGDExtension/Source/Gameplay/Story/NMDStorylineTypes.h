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

#ifndef __NMD_STORYLINE_TYPES_H__
#define __NMD_STORYLINE_TYPES_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDQuestTypes.h"

//
// Storyline Data - 16 bytes
//
// Each storyline describes a contiguous quest tree.
// Its quest indices are relative to nFirstQuest.
//

typedef struct nmdStorylineData_s
{
	const uint32_t nId;
	const uint32_t nFirstQuest;

	const uint16_t nQuestCount;
	const uint16_t nRootQuest;

	uint16_t nActiveQuestCount;
	uint16_t nCompletedQuestCount;
} nmdStorylineData_t;

/*
===========================================================================
Storyline Runtime Flags
===========================================================================
*/

typedef enum nmdStoryFlags_e : uint32_t
{
	NMD_STORY_FLAG_NONE = 0u,
	NMD_STORY_FLAG_ACTIVE = NOMAD_BIT( 0 ),
	NMD_STORY_FLAG_COMPLETED = NOMAD_BIT( 1 ),
	NMD_STORY_FLAG_FAILED = NOMAD_BIT( 2 )
} nmdStoryFlags_t;

static_assert( sizeof( nmdStorylineData_t ) == 16,
	"Storyline data exceeds 16 bytes" );

#endif // __NMD_STORYLINE_TYPES_H__
