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

#ifndef __NMD_WORLD_CONDITIONS_H__
#define __NMD_WORLD_CONDITIONS_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"

// Read-only gameplay rules; deliberately outside the world-state blackboard.
bool nmdWorldEvaluateCondition( const NMDWorldDatabase& db, const nmdWorldCondition_t& condition );
bool nmdWorldEvaluateConditions( const NMDWorldDatabase& db,
	const nmdWorldCondition_t *pConditions, uint32_t nCount );
bool nmdWorldEvaluateObjective( const NMDWorldDatabase& db,
	nmdWorldHandle_t hQuest, uint32_t nObjective, bool& bSatisfied );

#endif // __NMD_WORLD_CONDITIONS_H__
