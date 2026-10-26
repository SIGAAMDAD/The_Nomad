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

#include "NMDClimateGraphCompiler.h"
#include "NMDWorldContentCompiler.h"
#include <cmath>

bool NMDClimateGraphCompiler::build( NMDWorldDatabase& db )
{
	const uint32_t count = db.getClimateRegionCount();
	const nmdClimateGeographyCold_t *geo = db.getClimateGeographyColdData();

	if ( count && geo == NULL ) {
		return false;
	}

	for ( uint32_t i = 0; i < count; ++i ) {
		const uint16_t ids[ 2 ] = { geo[ i ].nDrainRegionId, geo[ i ].nDownwindRegionId };
		for ( uint32_t k = 0; k < 2; ++k ) {
			if ( ids[ k ] == NMD_CLIMATE_REGION_INVALID_ID ) {
				continue;
			}
			uint32_t target = 0;
			if ( !db.getClimateIndex( ids[ k ], target ) || target >= count ||
				 !std::isfinite( geo[ i ].fAreaKm2 / geo[ target ].fAreaKm2 ) ) {
				return false;
			}
		}
	}
	for ( uint32_t i = 0; i < count; ++i ) {
		nmdClimateLinks_t links = {
			.fDrainAreaScale = 0.0f,
			.fDownwindAreaScale = 0.0f,
			.nDrainIndex = NMD_CLIMATE_REGION_INVALID_ID,
			.nDownwindIndex = NMD_CLIMATE_REGION_INVALID_ID
		};

		uint32_t target = 0;
		if ( geo[ i ].nDrainRegionId != NMD_CLIMATE_REGION_INVALID_ID ) {
			db.getClimateIndex( geo[ i ].nDrainRegionId, target );
			links.nDrainIndex = static_cast<uint16_t>( target );
			links.fDrainAreaScale = geo[ i ].fAreaKm2 / geo[ target ].fAreaKm2;
		}
		if ( geo[ i ].nDownwindRegionId != NMD_CLIMATE_REGION_INVALID_ID ) {
			db.getClimateIndex( geo[ i ].nDownwindRegionId, target );
			links.nDownwindIndex = static_cast<uint16_t>( target );
			links.fDownwindAreaScale = geo[ i ].fAreaKm2 / geo[ target ].fAreaKm2;
		}
		if ( !NMDWorldContentCompiler::validateClimateLinks( db, links ) || !db.setClimateLinks( i, links ) ) {
			return false;
		}
	}
	return db.setClimateLinksRevision( db.getClimateTopologyRevision() );
}
