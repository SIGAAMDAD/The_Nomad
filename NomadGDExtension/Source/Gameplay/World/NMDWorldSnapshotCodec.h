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

#ifndef __NMD_WORLD_SNAPSHOT_CODEC_H__
#define __NMD_WORLD_SNAPSHOT_CODEC_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"

// Logical snapshot translation and semantic validation. No arena ownership.
class NMDWorldSnapshotCodec
{
public:
	static bool writeSimulation( const NMDWorldDatabase& db,
		nmdSimulationSnapshotHeader_t& header, nmdClimateRegionId_t *pIds,
		nmdWeatherHot_t *pWeather, nmdWaterCycleHot_t *pWater, uint32_t nCapacity );
	static bool restoreSimulation( NMDWorldDatabase& db,
		const nmdSimulationSnapshotHeader_t& header, const nmdClimateRegionId_t *pIds,
		const nmdWeatherHot_t *pWeather, const nmdWaterCycleHot_t *pWater );
	static bool writeLocations( const NMDWorldDatabase& db,
		nmdLocationSaveRecord_t *pOutput, uint32_t nCapacity );
	static bool restoreLocations( NMDWorldDatabase& db,
		const nmdLocationSaveRecord_t *pRecords, uint32_t nCount );
};

#endif // __NMD_WORLD_SNAPSHOT_CODEC_H__
