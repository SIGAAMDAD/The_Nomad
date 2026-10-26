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

#ifndef __NMD_WORLD_ARENA_LAYOUT_H__
#define __NMD_WORLD_ARENA_LAYOUT_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldDatabase.h"
#include <new>
#include <cstring>
#include <cassert>

typedef struct nmdWorldArenaSegmentSpec_s
{
	nmdWorldArenaSegmentId_t eSegment;
	uint32_t nCapacity;
	uint32_t nStride;
	nmdWorldArenaAccess_t eAccess;
} nmdWorldArenaSegmentSpec_t;

// Stateless layout helper: no allocations, global state, or record ownership.
class NMDWorldArenaLayout
{
public:
	static bool build( const nmdWorldArenaSegmentSpec_t *pSpecs, uint32_t nSpecCount,
		uint32_t nAlignment, nmdWorldArenaSegmentInfo_t *pSegments,
		uint32_t nSegmentCount, uint64_t& nEndBytes );
	static uint32_t getCommittedCapacity( nmdWorldArenaSegmentId_t eSegment,
		const nmdWorldDatabaseCapacities_t& capacities, uint32_t nReservedCapacity );
	static uint64_t getCommittedBytes( const nmdWorldArenaSegmentInfo_t& segment,
		uint32_t nCommittedCapacity, uint32_t nPageSize, bool bVirtualCommit );

	template<typename T>
	static void bind( nmdWorldArenaArray_t<T>& array, uint8_t *pBase,
		const nmdWorldArenaSegmentInfo_t& segment, bool bReset = false )
	{
		const uint32_t retained = bReset ? 0u : array.size();
		assert( retained <= segment.nCommittedCapacity );
		T *pNext = segment.nCapacity == 0u ? NULL :
			reinterpret_cast<T *>( pBase + segment.nOffsetBytes );
		for ( uint32_t i = 0u; i < segment.nCommittedCapacity; ++i ) {
			::new ( static_cast<void *>( pNext + i ) ) T();
		}
		if ( retained ) {
			memcpy( pNext, array.data(), static_cast<size_t>( retained ) * sizeof( T ) );
		}
		array.bind( pNext, segment.nCapacity, retained, segment.nCommittedCapacity );
	}

	template<typename T>
	static void grow( nmdWorldArenaArray_t<T>& array, uint32_t nCommitted )
	{
		assert( nCommitted >= array.committedCapacity() && nCommitted <= array.capacity() );
		for ( uint32_t i = array.committedCapacity(); i < nCommitted; ++i ) {
			::new ( static_cast<void *>( array.data() + i ) ) T();
		}
		array.setCommittedCapacity( nCommitted );
	}
};

static_assert( sizeof( nmdWorldArenaSegmentSpec_t ) <= 64u, "Arena spec must fit in a cache line" );

#endif // __NMD_WORLD_ARENA_LAYOUT_H__
