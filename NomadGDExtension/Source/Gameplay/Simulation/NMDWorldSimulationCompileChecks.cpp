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

// Compile-only contract checks. No runtime or Godot scene is required.
#include "../World/NMDWorldDatabase.h"
#include <type_traits>

static_assert( std::is_standard_layout<nmdCalendarHot_t>::value,
	"Calendar hot must remain standard layout" );
static_assert( std::is_standard_layout<nmdWeatherHot_t>::value,
	"Weather hot must remain standard layout" );
static_assert( std::is_standard_layout<nmdWaterCycleHot_t>::value,
	"Hydrology hot must remain standard layout" );
static_assert( std::is_trivially_copyable<nmdCalendarHot_t>::value,
	"Calendar hot must remain trivially copyable" );
static_assert( std::is_trivially_copyable<nmdWeatherHot_t>::value,
	"Weather hot must remain trivially copyable" );
static_assert( std::is_trivially_copyable<nmdWaterCycleHot_t>::value,
	"Hydrology hot must remain trivially copyable" );
static_assert( std::is_trivially_copyable<nmdSimulationSnapshotHeader_t>::value,
	"Snapshot header must remain trivially copyable" );
static_assert( std::is_trivially_copyable<nmdClimateSeasonalCold_t>::value,
	"Seasonal climate config must remain trivially copyable" );
static_assert( std::is_trivially_copyable<nmdClimateGeographyCold_t>::value,
	"Geographic climate config must remain trivially copyable" );
static_assert( sizeof( nmdCalendarHot_t ) + sizeof( nmdWorldTimedEvent_t ) <= 64,
	"Clock and timed event fit within one cacheline together" );
