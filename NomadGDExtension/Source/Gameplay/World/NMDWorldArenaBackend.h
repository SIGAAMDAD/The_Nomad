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

#ifndef __NMD_WORLD_ARENA_BACKEND_H__
#define __NMD_WORLD_ARENA_BACKEND_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

typedef struct nmdWorldArenaBlock_s
{
	void *pBase;
	uint32_t nReservedBytes;
	uint32_t nCommittedBytes;
} nmdWorldArenaBlock_t;

class NMDWorldArenaBackend
{
public:
	static uint32_t getPageSize( void )
	{
#ifdef _WIN32
		SYSTEM_INFO info = {};
		GetSystemInfo( &info );
		return info.dwPageSize;
#else
		return 64u; // A portable, fully accessible aligned heap block.
#endif
	}

	static bool getVirtualCommitSupported( void )
	{
#ifdef _WIN32
		return true;
#else
		return false;
#endif
	}

	static bool reserve( uint32_t nBytes, nmdWorldArenaBlock_t& block )
	{
		block = {};
		if ( nBytes == 0u ) {
			return false;
		}
#ifdef _WIN32
		void *pBase = VirtualAlloc( NULL, nBytes, MEM_RESERVE, PAGE_NOACCESS );
#else
		void *pBase = ::operator new( nBytes, std::align_val_t( 64 ), std::nothrow );
#endif
		if ( pBase == NULL ) {
			return false;
		}
		block.pBase = pBase;
		block.nReservedBytes = nBytes;
#ifndef _WIN32
		block.nCommittedBytes = nBytes;
#endif
		return true;
	}

	// The caller guarantees page-aligned, nonoverlapping ranges and tracks deltas.
	static bool commit( nmdWorldArenaBlock_t& block, uint32_t nOffset, uint32_t nBytes )
	{
		if ( block.pBase == NULL || nOffset > block.nReservedBytes ||
			 nBytes > block.nReservedBytes - nOffset ) {
			return false;
		}
		if ( nBytes == 0u ) {
			return true;
		}
#ifdef _WIN32
		const uint32_t page = getPageSize();
		if ( page == 0u || nOffset % page || nBytes % page ||
			 nBytes > UINT32_MAX - block.nCommittedBytes ) {
			return false;
		}
		void *pStart = static_cast<uint8_t *>( block.pBase ) + nOffset;
		if ( VirtualAlloc( pStart, nBytes, MEM_COMMIT, PAGE_READWRITE ) != pStart ) {
			return false;
		}
		block.nCommittedBytes += nBytes;
#endif
		return true;
	}

	static bool decommit( nmdWorldArenaBlock_t& block, uint32_t nOffset, uint32_t nBytes )
	{
		if ( block.pBase == NULL || nOffset > block.nReservedBytes || nBytes > block.nReservedBytes - nOffset ) {
			return false;
		}
		if ( nBytes == 0u ) {
			return true;
		}
#ifdef _WIN32
		const uint32_t page = getPageSize();
		if ( page == 0u || nOffset % page || nBytes % page || nBytes > block.nCommittedBytes ) {
			return false;
		}
		void *pStart = static_cast<uint8_t *>( block.pBase ) + nOffset;
		if ( !VirtualFree( pStart, nBytes, MEM_DECOMMIT ) ) {
			return false;
		}
		block.nCommittedBytes -= nBytes;
#endif
		return true;
	}

	static void release( nmdWorldArenaBlock_t& block )
	{
		if ( block.pBase == NULL ) {
			return;
		}
#ifdef _WIN32
		VirtualFree( block.pBase, 0, MEM_RELEASE );
#else
		::operator delete( block.pBase, std::align_val_t( 64 ) );
#endif
		block = {};
	}
};

static_assert( sizeof( nmdWorldArenaBlock_t ) <= 64u,
			   "World arena backend descriptor must fit a cacheline" );

#endif // __NMD_WORLD_ARENA_BACKEND_H__
