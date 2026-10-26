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

#ifndef __NMD_WORLD_FACT_TYPES_H__
#define __NMD_WORLD_FACT_TYPES_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldIds.h"
#include <Framework/Events/NMDEvent.h>

//
// World Fact Data - 2 bytes
//
// Fact identifiers are implicit array indices.
// Both story and world systems share the same value pool.
//

typedef struct nmdWorldFactData_s
{
	nmdWorldFact_t nValue;
} nmdWorldFactData_t;

typedef struct nmdWorldFactChangedEvent_s
{
	const nmdWorldFactId_t nFactId;
	const nmdWorldFact_t prevValue;
	const nmdWorldFact_t currentValue;
} nmdWorldFactChangedEvent_t;

typedef NMDEvent<nmdWorldFactChangedEvent_t> *NMDWorldFactChangedEvent;

/*
===========================================================================
Constants
===========================================================================
*/

#define NMD_WORLD_FACT_INVALID_ID 0xFFFFu
#define NMD_WORLD_FACT_MAX_COUNT 0xFFFFu
#define NMD_WORLD_FACT_MIN_BUCKETS 16u

static_assert( sizeof( nmdWorldFactData_t ) == 2,
	"World fact data exceeds 2 bytes" );

#endif // __NMD_WORLD_FACT_TYPES_H__
