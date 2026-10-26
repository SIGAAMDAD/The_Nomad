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

#include "NMDWorldFactChangeDispatcher.h"
#include "../Story/NMDQuestSystem.h"
#include "NMDLocationSystem.h"

bool NMDWorldFactChangeDispatcher::dispatch( const NMDWorldDatabase& db,
	nmdWorldFactId_t nId, NMDQuestSystem *pQuests, NMDLocationSystem *pLocations )
{
	const nmdWorldDependency_t *pDependencies = NULL;
	uint32_t nCount = 0u;
	if ( !db.getFactDependencies( nId, pDependencies, nCount ) ) {
		return false;
	}
	bool result = true;
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		const nmdWorldDependency_t& dependency = pDependencies[i];
		switch ( dependency.hOwner.getType() ) {
			case NMD_WORLD_RECORD_LOCATION:
				if ( pLocations != NULL && !pLocations->reevaluate_completion(
					static_cast<int32_t>( dependency.hOwner.getIndex() ) ) ) {
					result = false;
				}
				break;
			case NMD_WORLD_RECORD_QUEST:
				if ( pQuests != NULL && !pQuests->reevaluateObjective(
					dependency.hOwner, dependency.nItemIndex ) ) {
					result = false;
				}
				break;
			default: break;
		}
	}
	return result;
}
