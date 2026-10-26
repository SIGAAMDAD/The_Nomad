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

#ifndef __NMD_WORLD_IDS_H__
#define __NMD_WORLD_IDS_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

typedef uint16_t nmdWorldFact_t;
typedef uint16_t nmdWorldFactId_t;
typedef uint16_t nmdLocationId_t;
typedef uint16_t nmdQuestId_t;

#define NMD_WORLD_INVALID_LOCAL_INDEX 0xFFFFu
#define NMD_WORLD_INVALID_GLOBAL_ID   0xFFFFFFFFu

#endif // __NMD_WORLD_IDS_H__
