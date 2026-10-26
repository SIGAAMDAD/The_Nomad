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

#ifndef __NMD_WORLD_DEPENDENCY_COMPILER_H__
#define __NMD_WORLD_DEPENDENCY_COMPILER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"

// Builds fact-major, within-fact registration-order CSR into caller-owned
// arena segments. No allocations or gameplay-condition evaluation.
class NMDWorldDependencyCompiler
{
public:
	static bool rebuild( NMDWorldDatabase& db );
	static bool build( const nmdWorldDependencyBuild_t *pBuild, uint32_t nEntries,
		uint32_t nFactCount, uint32_t *pOffsets, uint32_t *pCursor,
		nmdWorldDependency_t *pOutput );
};

#endif // __NMD_WORLD_DEPENDENCY_COMPILER_H__
