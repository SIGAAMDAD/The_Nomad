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

#ifndef __NMD_WORLD_FACT_CHANGE_DISPATCHER_H__
#define __NMD_WORLD_FACT_CHANGE_DISPATCHER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"
class NMDQuestSystem;
class NMDLocationSystem;

// Explicit domain-event routing. Database never publishes gameplay events.
class NMDWorldFactChangeDispatcher
{
public:
	static bool dispatch( const NMDWorldDatabase& db, nmdWorldFactId_t nId,
		NMDQuestSystem *pQuests, NMDLocationSystem *pLocations );
};

#endif // __NMD_WORLD_FACT_CHANGE_DISPATCHER_H__
