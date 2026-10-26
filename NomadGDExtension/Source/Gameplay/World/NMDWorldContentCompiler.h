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

#ifndef __NMD_WORLD_CONTENT_COMPILER_H__
#define __NMD_WORLD_CONTENT_COMPILER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"

// Authoring-side semantic validation and registration; no persistent ownership.
class NMDWorldContentCompiler
{
public:
	static bool seal( NMDWorldDatabase& db );
	static bool validateFact( nmdWorldFactType_t eType, nmdWorldValue_t value );
	static bool validateCondition( const NMDWorldDatabase& db, const nmdWorldCondition_t& condition );
	static bool validateLocation( const NMDWorldDatabase& db,
		const nmdWorldLocationDefinition_t& definition,
		const nmdWorldCondition_t *pConditions, uint16_t nCount );
	static bool validateCalendar( const nmdCalendarDefinition_t& definition );
	static bool validateCalendarHot( const nmdCalendarDefinition_t& definition, const nmdCalendarHot_t& hot );
	static bool validateSeasonal( const nmdClimateSeasonalCold_t& seasonal );
	static bool validateGeography( const nmdClimateGeographyCold_t& geography );
	static bool validateWeather( const nmdWeatherHot_t& weather );
	static bool validateWater( const nmdWaterCycleHot_t& water );
	static bool validateWeatherEvent( const NMDWorldDatabase& db, const nmdWeatherTimelineEvent_t& event );
	static bool validateClimateLinks( const NMDWorldDatabase& db, const nmdClimateLinks_t& links );

	static nmdWorldFactId_t registerFact( NMDWorldDatabase& db, nmdWorldAtom_t pAtom,
		nmdWorldFactType_t eType, nmdWorldValue_t initialValue );
	static nmdWorldHandle_t registerLocation( NMDWorldDatabase& db, nmdWorldAtom_t pAtom,
		const nmdWorldLocationDefinition_t& definition, const nmdWorldCondition_t *pConditions,
		uint16_t nConditions, nmdLocationId_t nAuthoredId = NMD_LOCATION_INVALID_ID );
};

#endif // __NMD_WORLD_CONTENT_COMPILER_H__
