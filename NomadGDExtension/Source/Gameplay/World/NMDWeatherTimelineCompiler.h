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

#ifndef __NMD_WEATHER_TIMELINE_COMPILER_H__
#define __NMD_WEATHER_TIMELINE_COMPILER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"
class NMDWeatherTimelineCompiler
{
public:
	static bool rebuild( NMDWorldDatabase& db );
	static bool build( const NMDWorldDatabase& db, const nmdWeatherTimelineEvent_t *pEvents,
		uint32_t nEventCount, uint32_t nRegionCount,
		nmdWeatherTimelineIndexEntry_t *pEntries, nmdWeatherTimelineRange_t *pRanges );
};

#endif // __NMD_WEATHER_TIMELINE_COMPILER_H__
