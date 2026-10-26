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

// Compile-only POD layout checks. This translation unit requires no runtime test runner.
#include "NMDWorldDatabase.h"
#include <type_traits>

static_assert( std::is_standard_layout<nmdLocationData_t>::value,
	"Hot location records must be standard-layout PODs" );
static_assert( std::is_trivially_copyable<nmdLocationData_t>::value,
	"Hot location records must be trivially copyable" );
static_assert( std::is_trivially_copyable<nmdWorldLocationCold_t>::value,
	"Cold location records must be trivially copyable" );
static_assert( std::is_trivially_copyable<nmdLocationSaveRecord_t>::value,
	"Location save entries must be trivially copyable" );
static_assert( sizeof( nmdWorldLocationCold_t ) <= 64,
	"Location cold records must fit a cacheline" );
static_assert( sizeof( nmdWorldLocationDefinition_t ) <= 64,
	"Authoring records must fit a cacheline" );
static_assert( sizeof( nmdWorldCondition_t ) == 8,
	"Compiled world predicate must be 8 bytes" );
#if INTPTR_MAX == INT64_MAX
static_assert( sizeof( nmdWorldLocationCold_t ) == 40,
	"x64 cold location record grew beyond 40 bytes" );
static_assert( sizeof( nmdWorldLocationDefinition_t ) == 32,
	"x64 authoring descriptor grew beyond 32 bytes" );
#endif
