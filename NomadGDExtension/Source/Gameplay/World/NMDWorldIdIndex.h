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

#ifndef __NMD_WORLD_ID_INDEX_H__
#define __NMD_WORLD_ID_INDEX_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <cstdint>
#include <cstddef>

// Transient full identity; the arena keeps only a cached hash and an index.
typedef enum nmdWorldIdKeyKind_e : uint8_t
{
	NMD_WORLD_ID_ATOM = 0,
	NMD_WORLD_ID_U16,
	NMD_WORLD_ID_U32,
	NMD_WORLD_ID_COMPOUND
} nmdWorldIdKeyKind_t;

typedef struct nmdWorldIdKey_s
{
	uint64_t nBits;
	uint32_t nNamespace;
	nmdWorldIdKeyKind_t eKind;
	uint8_t nReserved[ 3 ];
} nmdWorldIdKey_t;

typedef struct nmdWorldIdIndexEntry_s
{
	uint32_t nHash;
	uint32_t nValue;
} nmdWorldIdIndexEntry_t;

class NMDWorldIdIndex
{
public:
	static nmdWorldIdKey_t getAtomKey( const void *pAtom, uint32_t nNamespace )
	{
		return { static_cast<uint64_t>( reinterpret_cast<uintptr_t>( pAtom ) ), nNamespace, NMD_WORLD_ID_ATOM, {} };
	}
	static nmdWorldIdKey_t getNumericKey( uint32_t nId, uint32_t nNamespace, bool bWide = false )
	{
		return { nId, nNamespace, bWide ? NMD_WORLD_ID_U32 : NMD_WORLD_ID_U16, {} };
	}
	static nmdWorldIdKey_t getDependencyKey( uint16_t nFact, uint32_t nOwner, uint16_t nItem )
	{
		return { ( static_cast<uint64_t>( nOwner ) << 32 ) | ( static_cast<uint64_t>( nFact ) << 16 ) |
					 nItem,
				 0x44455053u,
				 NMD_WORLD_ID_COMPOUND,
				 {} };
	}
	static uint32_t hash( const nmdWorldIdKey_t& key )
	{
		uint64_t nHash = key.nBits ^ ( static_cast<uint64_t>( key.nNamespace ) * 0x9e3779b97f4a7c15ULL ) ^
						 ( static_cast<uint64_t>( key.eKind ) * 0xd6e8feb86659fd93ULL );
		nHash ^= nHash >> 33;
		nHash *= 0xff51afd7ed558ccdULL;
		nHash ^= nHash >> 33;
		nHash *= 0xc4ceb9fe1a85ec53ULL;
		nHash ^= nHash >> 33;
		return static_cast<uint32_t>( nHash );
	}
	static bool equals( const nmdWorldIdKey_t& a, const nmdWorldIdKey_t& b )
	{
		return a.nBits == b.nBits && a.nNamespace == b.nNamespace && a.eKind == b.eKind;
	}

	// Shared collision-safe probe for atoms, numeric IDs and compound keys.
	template <typename TMatcher>
	static bool probe( const uint32_t *pBuckets, uint32_t nBucketCount,
					   const nmdWorldIdKey_t& key, TMatcher match,
					   uint32_t& nSlot, bool& bFound )
	{
		bFound = false;
		nSlot = 0u;
		if ( pBuckets == nullptr || nBucketCount == 0u ||
			 ( nBucketCount & ( nBucketCount - 1u ) ) != 0u ) {
			return false;
		}
		const uint32_t mask = nBucketCount - 1u;
		uint32_t bucket = hash( key ) & mask;
		for ( uint32_t attempts = 0u; attempts < nBucketCount; ++attempts ) {
			const uint32_t plusOne = pBuckets[ bucket ];
			if ( plusOne == 0u ) {
				nSlot = bucket;
				return true;
			}
			if ( match( plusOne - 1u, key ) ) {
				nSlot = bucket;
				bFound = true;
				return true;
			}
			bucket = ( bucket + 1u ) & mask;
		}
		return false;
	}

	// One bounded, linear-probing implementation for all identity domains.
	// The resolver reconstructs a full key from an arena-backed entry value.
	template <typename TEntry, typename TResolver>
	static bool findSlot( const uint32_t *pBuckets, uint32_t nBucketCount,
						  const TEntry *pEntries, uint32_t nEntryCount, const nmdWorldIdKey_t& key,
						  TResolver resolver, uint32_t& nSlot, bool& bFound )
	{
		bFound = false;
		nSlot = 0u;
		if ( pBuckets == nullptr || pEntries == nullptr || nBucketCount == 0u ||
			 ( nBucketCount & ( nBucketCount - 1u ) ) != 0u ) {
			return false;
		}
		const uint32_t nHash = hash( key );
		return probe( pBuckets, nBucketCount, key, [ & ]( uint32_t index, const nmdWorldIdKey_t& searchKey )
					  {
						  return index < nEntryCount && pEntries[ index ].nHash == nHash &&
								 equals( resolver( pEntries[ index ] ), searchKey );
					  },
					  nSlot, bFound );
	}

	static bool rebuild( uint32_t *pBuckets, uint32_t nBucketCount,
						 const nmdWorldIdIndexEntry_t *pEntries, uint32_t nEntryCount )
	{
		if ( pBuckets == nullptr || nBucketCount == 0u ||
			 ( nBucketCount & ( nBucketCount - 1u ) ) != 0u ||
			 ( nEntryCount != 0u && pEntries == nullptr ) ) {
			return false;
		}
		for ( uint32_t i = 0u; i < nBucketCount; ++i ) {
			pBuckets[ i ] = 0u;
		}
		const uint32_t nMask = nBucketCount - 1u;
		for ( uint32_t i = 0u; i < nEntryCount; ++i ) {
			uint32_t nSlot = pEntries[ i ].nHash & nMask;
			uint32_t nAttempt = 0u;
			while ( pBuckets[ nSlot ] != 0u && nAttempt++ < nBucketCount ) {
				nSlot = ( nSlot + 1u ) & nMask;
			}
			if ( nAttempt >= nBucketCount ) {
				return false;
			}
			pBuckets[ nSlot ] = i + 1u;
		}
		return true;
	}
};

static_assert( sizeof( nmdWorldIdKey_t ) == 16u, "Transient identity key must be 16 bytes" );
static_assert( sizeof( nmdWorldIdIndexEntry_t ) == 8u, "Arena identity entries must be 8 bytes" );

#endif // __NMD_WORLD_ID_INDEX_H__
