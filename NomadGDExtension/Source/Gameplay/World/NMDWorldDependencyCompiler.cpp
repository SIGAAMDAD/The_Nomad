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

#include "NMDWorldDependencyCompiler.h"
#include <cstring>

bool NMDWorldDependencyCompiler::build( const nmdWorldDependencyBuild_t *pBuild,
	uint32_t nEntries, uint32_t nFactCount, uint32_t *pOffsets,
	uint32_t *pCursor, nmdWorldDependency_t *pOutput )
{
	if ( pOffsets == NULL || pCursor == NULL ||
		( nEntries && ( pBuild == NULL || pOutput == NULL ) ) ) {
		return false;
	}
	// Preflight the entire dataset before writing the published arrays.
	for ( uint32_t i = 0u; i < nEntries; ++i ) {
		if ( pBuild[i].nFactId >= nFactCount ) {
			return false;
		}
	}
	memset( pOffsets, 0, static_cast<size_t>( nFactCount + 1u ) * sizeof( uint32_t ) );
	for ( uint32_t i = 0u; i < nEntries; ++i ) {
		++pOffsets[ pBuild[i].nFactId + 1u ];
	}
	for ( uint32_t i = 1u; i <= nFactCount; ++i ) {
		pOffsets[i] += pOffsets[i - 1u];
	}
	memcpy( pCursor, pOffsets, static_cast<size_t>( nFactCount + 1u ) * sizeof( uint32_t ) );
	for ( uint32_t i = 0u; i < nEntries; ++i ) {
		const nmdWorldDependencyBuild_t& item = pBuild[i];
		pOutput[ pCursor[item.nFactId]++ ] = { item.hOwner, item.nItemIndex, 0u };
	}
	return true;
}

bool NMDWorldDependencyCompiler::rebuild( NMDWorldDatabase& db )
{
	const nmdWorldDependencyBuild_t *pBuild = NULL;
	uint32_t entries = 0u;
	uint32_t *pOffsets = NULL;
	uint32_t *pCursor = NULL;
	nmdWorldDependency_t *pOutput = NULL;
	uint32_t offsetCapacity = 0u, dependencyCapacity = 0u;
	if ( !db.getDependencyBuildData( pBuild, entries ) ||
		!db.getDependencyCompileStorage( pOffsets, pCursor, pOutput,
			offsetCapacity, dependencyCapacity ) ||
		db.getFactCount() + 1u > offsetCapacity || entries > dependencyCapacity ) {
		return false;
	}
	if ( !build( pBuild, entries, db.getFactCount(), pOffsets, pCursor, pOutput ) ) {
		return false;
	}
	return db.setCompiledDependencyCount( entries );
}
