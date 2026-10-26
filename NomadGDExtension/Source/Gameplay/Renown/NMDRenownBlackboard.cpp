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

#include "NMDRenownBlackboard.h"

#include <string.h>
#include <limits.h>

// All offsets are relative to the aligned renown segment base.
bool reserveSegment( size_t& nOffset, size_t nCount, size_t nElementSize, size_t nAlignment, size_t& nStart )
{
	if ( nAlignment == 0 || ( nAlignment & ( nAlignment - 1 ) ) != 0 ) {
		return false;
	}
	if ( nOffset > SIZE_MAX - ( nAlignment - 1 ) ) {
		return false;
	}
	nStart = ( nOffset + nAlignment - 1 ) & ~( nAlignment - 1 );
	if ( nCount != 0 && nElementSize > ( SIZE_MAX - nStart ) / nCount ) {
		return false;
	}
	nOffset = nStart + nCount * nElementSize;
	return true;
}

uint32_t mix32( uint32_t nValue )
{
	nValue ^= nValue >> 16;
	nValue *= 0x7feb352du;
	nValue ^= nValue >> 15;
	nValue *= 0x846ca68bu;
	return nValue ^ ( nValue >> 16 );
}

NMDRenownBlackboard::NMDRenownBlackboard()
{
	unbind();
}

void NMDRenownBlackboard::unbind()
{
	m_pArena = NULL;
	m_nBytes = 0;
	memset( &m_Capacity, 0, sizeof( m_Capacity ) );
	m_pHeader = NULL;
	m_pProfiles = NULL;
	m_pTraits = NULL;
	m_pPatterns = NULL;
	m_pIncidents = NULL;
	m_pRumors = NULL;
	m_pSeen = NULL;
	m_pLocations = NULL;
	m_pEdges = NULL;
	m_pProfileIndex = NULL;
	m_pTraitIndex = NULL;
	m_pPatternIndex = NULL;
	m_pValidationSlots = NULL;
	m_nProfileIndexCapacity = 0;
	m_nTraitIndexCapacity = 0;
	m_nPatternIndexCapacity = 0;
	m_nValidationSlotCount = 0;
}

uint32_t NMDRenownBlackboard::nextPowerOfTwo( uint32_t nValue )
{
	if ( nValue > 0x80000000u ) {
		return 0;
	}
	if ( nValue <= 2 ) {
		return 2;
	}
	--nValue;
	nValue |= nValue >> 1;
	nValue |= nValue >> 2;
	nValue |= nValue >> 4;
	nValue |= nValue >> 8;
	nValue |= nValue >> 16;
	return nValue + 1;
}

uint32_t NMDRenownBlackboard::hashKey( uint32_t nA, uint32_t nB, uint32_t nC )
{
	return mix32( nA ^ mix32( nB + 0x9e3779b9u ) ^ mix32( nC + 0x85ebca6bu ) );
}

uint32_t NMDRenownBlackboard::capacityFingerprint( const nmdRenownCapacities_t& capacity )
{
	return hashKey( capacity.nProfiles, capacity.nTraits, capacity.nPatterns ) ^
	       hashKey( capacity.nIncidents, capacity.nRumors, capacity.nSeen ) ^
	       hashKey( capacity.nLocations, capacity.nEdges, NMD_RENOWN_STORAGE_VERSION );
}

size_t NMDRenownBlackboard::getRequiredBytes( const nmdRenownCapacities_t& capacity )
{
	if ( capacity.nIncidents == 0 || capacity.nLocations == 0 ||
	     capacity.nProfiles > 0x20000000u || capacity.nTraits > 0x20000000u ||
	     capacity.nPatterns > 0x20000000u || capacity.nSeen > 0x20000000u ) {
		return 0;
	}

	const uint32_t nProfileSlots = nextPowerOfTwo( capacity.nProfiles * 2u + 1u );
	const uint32_t nTraitSlots = nextPowerOfTwo( capacity.nTraits * 2u + 1u );
	const uint32_t nPatternSlots = nextPowerOfTwo( capacity.nPatterns * 2u + 1u );
	uint32_t nMax = capacity.nProfiles;
	if ( capacity.nTraits > nMax )
		nMax = capacity.nTraits;
	if ( capacity.nPatterns > nMax )
		nMax = capacity.nPatterns;
	if ( capacity.nSeen > nMax )
		nMax = capacity.nSeen;
	const uint32_t nValidationSlots = nextPowerOfTwo( nMax * 2u + 1u );
	if ( nProfileSlots == 0 || nTraitSlots == 0 || nPatternSlots == 0 ||
	     nValidationSlots == 0 ) {
		return 0;
	}

	size_t nOffset = 0;
	size_t nStart = 0;
#define NMD_RESERVE( count, type )                                                          \
	if ( !reserveSegment( nOffset, ( count ), sizeof( type ), alignof( type ), nStart ) ) { \
		return 0;                                                                           \
	}
	NMD_RESERVE( 1, nmdRenownArenaHeader_t );
	NMD_RESERVE( capacity.nProfiles, nmdRenownProfile_t );
	NMD_RESERVE( capacity.nTraits, nmdRenownTrait_t );
	NMD_RESERVE( capacity.nPatterns, nmdRenownPattern_t );
	NMD_RESERVE( capacity.nIncidents, nmdRenownIncidentRecord_t );
	NMD_RESERVE( capacity.nRumors, nmdRenownRumor_t );
	NMD_RESERVE( capacity.nSeen, nmdRenownSeen_t );
	NMD_RESERVE( capacity.nLocations, nmdRenownLocation_t );
	NMD_RESERVE( capacity.nEdges, nmdRenownEdge_t );
	NMD_RESERVE( nProfileSlots, uint32_t );
	NMD_RESERVE( nTraitSlots, uint32_t );
	NMD_RESERVE( nPatternSlots, uint32_t );
	NMD_RESERVE( nValidationSlots, uint32_t );
#undef NMD_RESERVE
	return nOffset;
}

bool NMDRenownBlackboard::bind( void *pArena, size_t nBytes,
                                const nmdRenownCapacities_t& capacity )
{
	unbind();
	const size_t nRequired = getRequiredBytes( capacity );
	if ( pArena == NULL || nRequired == 0 || nBytes < nRequired ||
	     ( reinterpret_cast<uintptr_t>( pArena ) & 7u ) != 0 ) {
		return false;
	}

	m_pArena = static_cast<uint8_t *>( pArena );
	m_nBytes = nRequired;
	m_Capacity = capacity;
	m_nProfileIndexCapacity = nextPowerOfTwo( capacity.nProfiles * 2u + 1u );
	m_nTraitIndexCapacity = nextPowerOfTwo( capacity.nTraits * 2u + 1u );
	m_nPatternIndexCapacity = nextPowerOfTwo( capacity.nPatterns * 2u + 1u );
	uint32_t nMax = capacity.nProfiles;
	if ( capacity.nTraits > nMax )
		nMax = capacity.nTraits;
	if ( capacity.nPatterns > nMax )
		nMax = capacity.nPatterns;
	if ( capacity.nSeen > nMax )
		nMax = capacity.nSeen;
	m_nValidationSlotCount = nextPowerOfTwo( nMax * 2u + 1u );
	size_t nOffset = 0;
	size_t nStart = 0;
#define NMD_BIND( field, count, type )                                             \
	reserveSegment( nOffset, ( count ), sizeof( type ), alignof( type ), nStart ); \
	field = reinterpret_cast<type *>( m_pArena + nStart )
	NMD_BIND( m_pHeader, 1, nmdRenownArenaHeader_t );
	NMD_BIND( m_pProfiles, capacity.nProfiles, nmdRenownProfile_t );
	NMD_BIND( m_pTraits, capacity.nTraits, nmdRenownTrait_t );
	NMD_BIND( m_pPatterns, capacity.nPatterns, nmdRenownPattern_t );
	NMD_BIND( m_pIncidents, capacity.nIncidents, nmdRenownIncidentRecord_t );
	NMD_BIND( m_pRumors, capacity.nRumors, nmdRenownRumor_t );
	NMD_BIND( m_pSeen, capacity.nSeen, nmdRenownSeen_t );
	NMD_BIND( m_pLocations, capacity.nLocations, nmdRenownLocation_t );
	NMD_BIND( m_pEdges, capacity.nEdges, nmdRenownEdge_t );
	NMD_BIND( m_pProfileIndex, m_nProfileIndexCapacity, uint32_t );
	NMD_BIND( m_pTraitIndex, m_nTraitIndexCapacity, uint32_t );
	NMD_BIND( m_pPatternIndex, m_nPatternIndexCapacity, uint32_t );
	NMD_BIND( m_pValidationSlots, m_nValidationSlotCount, uint32_t );
#undef NMD_BIND
	return true;
}

bool NMDRenownBlackboard::initialize( void *pArena, size_t nBytes,
                                      const nmdRenownCapacities_t& capacity )
{
	if ( !bind( pArena, nBytes, capacity ) ) {
		return false;
	}
	memset( m_pArena, 0, m_nBytes );
	m_pHeader->nMagic = NMD_RENOWN_STORAGE_MAGIC;
	m_pHeader->nVersion = NMD_RENOWN_STORAGE_VERSION;
	m_pHeader->nCapacityFingerprint = capacityFingerprint( capacity );
	return true;
}

bool NMDRenownBlackboard::bindExisting( void *pArena, size_t nBytes,
                                        const nmdRenownCapacities_t& capacity )
{
	if ( !bind( pArena, nBytes, capacity ) ) {
		return false;
	}
	if ( m_pHeader->nMagic != NMD_RENOWN_STORAGE_MAGIC ||
	     m_pHeader->nVersion != NMD_RENOWN_STORAGE_VERSION ||
	     m_pHeader->nCapacityFingerprint != capacityFingerprint( capacity ) ||
	     m_pHeader->nProfileCount > capacity.nProfiles ||
	     m_pHeader->nTraitCount > capacity.nTraits ||
	     m_pHeader->nPatternCount > capacity.nPatterns ||
	     m_pHeader->nIncidentCount > capacity.nIncidents ||
	     m_pHeader->nIncidentHead >= capacity.nIncidents ||
	     m_pHeader->nRumorCount > capacity.nRumors ||
	     m_pHeader->nSeenCount > capacity.nSeen ||
	     m_pHeader->nLocationCount > capacity.nLocations ||
	     m_pHeader->nEdgeCount > capacity.nEdges ) {
		unbind();
		return false;
	}
	return true;
}

bool NMDRenownBlackboard::isBound() const
{
	return m_pHeader != NULL;
}

const nmdRenownCapacities_t& NMDRenownBlackboard::getCapacities() const
{
	return m_Capacity;
}

nmdRenownArenaHeader_t NMDRenownBlackboard::getHeader() const
{
	nmdRenownArenaHeader_t result = {};
	if ( isBound() ) {
		result = *m_pHeader;
	}
	return result;
}

uint32_t NMDRenownBlackboard::getWorldMinute() const
{
	return isBound() ? m_pHeader->nWorldMinute : 0;
}

uint32_t NMDRenownBlackboard::getRevision() const
{
	return isBound() ? m_pHeader->nRevision : 0;
}

void NMDRenownBlackboard::setWorldMinute( uint32_t nMinute )
{
	if ( isBound() && m_pHeader->nWorldMinute != nMinute ) {
		m_pHeader->nWorldMinute = nMinute;
		++m_pHeader->nRevision;
	}
}

void NMDRenownBlackboard::setRulesFingerprint( uint32_t nFingerprint )
{
	if ( isBound() && m_pHeader->nRulesFingerprint != nFingerprint ) {
		m_pHeader->nRulesFingerprint = nFingerprint;
		++m_pHeader->nRevision;
	}
}

void NMDRenownBlackboard::setRevision( uint32_t nRevision )
{
	if ( isBound() ) {
		m_pHeader->nRevision = nRevision;
	}
}

void NMDRenownBlackboard::setLastIncidentId( uint64_t nIncidentId )
{
	if ( isBound() ) {
		m_pHeader->nLastIncidentId = nIncidentId;
	}
}

void NMDRenownBlackboard::setLastRumorId( uint64_t nRumorId )
{
	if ( isBound() ) {
		m_pHeader->nLastRumorId = nRumorId;
	}
}

uint64_t NMDRenownBlackboard::reserveRumorId()
{
	if ( !isBound() || m_pHeader->nLastRumorId == UINT64_MAX ) {
		return 0;
	}
	return ++m_pHeader->nLastRumorId;
}

uint32_t NMDRenownBlackboard::getLocationCount() const
{
	return isBound() ? m_pHeader->nLocationCount : 0;
}

uint32_t NMDRenownBlackboard::getEdgeCount() const
{
	return isBound() ? m_pHeader->nEdgeCount : 0;
}

bool NMDRenownBlackboard::getLocationByIndex( uint32_t nIndex, nmdRenownLocation_t& out ) const
{
	if ( !isBound() || nIndex >= m_pHeader->nLocationCount ) {
		return false;
	}
	out = m_pLocations[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::getLocation( uint32_t nLocationId, nmdRenownLocation_t& out ) const
{
	uint32_t nFirst = 0;
	uint32_t nLast = getLocationCount();
	while ( nFirst < nLast ) {
		const uint32_t nMiddle = nFirst + ( nLast - nFirst ) / 2;
		const uint32_t nKey = m_pLocations[ nMiddle ].nLocationId;
		if ( nKey < nLocationId ) {
			nFirst = nMiddle + 1;
		} else {
			nLast = nMiddle;
		}
	}
	if ( nFirst >= getLocationCount() || m_pLocations[ nFirst ].nLocationId != nLocationId ) {
		return false;
	}
	out = m_pLocations[ nFirst ];
	return true;
}

bool NMDRenownBlackboard::getEdgeByIndex( uint32_t nIndex, nmdRenownEdge_t& out ) const
{
	if ( !isBound() || nIndex >= m_pHeader->nEdgeCount ) {
		return false;
	}
	out = m_pEdges[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::setLocationGraph( const nmdRenownLocation_t *pLocations,
                                            uint32_t nLocationCount, const nmdRenownEdge_t *pEdges, uint32_t nEdgeCount,
                                            uint32_t nFingerprint )
{
	if ( !isBound() || nLocationCount > m_Capacity.nLocations ||
	     nEdgeCount > m_Capacity.nEdges ||
	     ( nLocationCount != 0 && pLocations == NULL ) ||
	     ( nEdgeCount != 0 && pEdges == NULL ) ) {
		return false;
	}
	if ( nLocationCount != 0 ) {
		memcpy( m_pLocations, pLocations, nLocationCount * sizeof( *pLocations ) );
	}
	if ( nEdgeCount != 0 ) {
		memcpy( m_pEdges, pEdges, nEdgeCount * sizeof( *pEdges ) );
	}
	m_pHeader->nLocationCount = nLocationCount;
	m_pHeader->nEdgeCount = nEdgeCount;
	m_pHeader->nGraphFingerprint = nFingerprint;
	++m_pHeader->nRevision;
	return true;
}

uint32_t NMDRenownBlackboard::findProfileIndex( uint32_t nSubjectId, uint32_t nLocationId ) const
{
	if ( !isBound() ) {
		return UINT32_MAX;
	}
	const uint32_t nMask = m_nProfileIndexCapacity - 1;
	uint32_t nSlot = hashKey( nSubjectId, nLocationId, 0 ) & nMask;
	for ( uint32_t nProbe = 0; nProbe < m_nProfileIndexCapacity; ++nProbe ) {
		const uint32_t nStored = m_pProfileIndex[ nSlot ];
		if ( nStored == 0 ) {
			break;
		}
		const nmdRenownProfile_t& entry = m_pProfiles[ nStored - 1 ];
		if ( entry.nSubjectId == nSubjectId && entry.nLocationId == nLocationId ) {
			return nStored - 1;
		}
		nSlot = ( nSlot + 1 ) & nMask;
	}
	return UINT32_MAX;
}

uint32_t NMDRenownBlackboard::findTraitIndex( uint32_t nSubjectId, uint32_t nLocationId,
                                              uint32_t nTraitId ) const
{
	if ( !isBound() ) {
		return UINT32_MAX;
	}
	const uint32_t nMask = m_nTraitIndexCapacity - 1;
	uint32_t nSlot = hashKey( nSubjectId, nLocationId, nTraitId ) & nMask;
	for ( uint32_t nProbe = 0; nProbe < m_nTraitIndexCapacity; ++nProbe ) {
		const uint32_t nStored = m_pTraitIndex[ nSlot ];
		if ( nStored == 0 ) {
			break;
		}
		const nmdRenownTrait_t& entry = m_pTraits[ nStored - 1 ];
		if ( entry.nSubjectId == nSubjectId && entry.nLocationId == nLocationId &&
		     entry.nTraitId == nTraitId ) {
			return nStored - 1;
		}
		nSlot = ( nSlot + 1 ) & nMask;
	}
	return UINT32_MAX;
}

uint32_t NMDRenownBlackboard::findPatternIndex( uint32_t nSubjectId, uint32_t nLocationId,
                                                uint8_t nType ) const
{
	if ( !isBound() ) {
		return UINT32_MAX;
	}
	const uint32_t nMask = m_nPatternIndexCapacity - 1;
	uint32_t nSlot = hashKey( nSubjectId, nLocationId, nType ) & nMask;
	for ( uint32_t nProbe = 0; nProbe < m_nPatternIndexCapacity; ++nProbe ) {
		const uint32_t nStored = m_pPatternIndex[ nSlot ];
		if ( nStored == 0 ) {
			break;
		}
		const nmdRenownPattern_t& entry = m_pPatterns[ nStored - 1 ];
		if ( entry.nSubjectId == nSubjectId && entry.nLocationId == nLocationId &&
		     entry.nIncidentType == nType ) {
			return nStored - 1;
		}
		nSlot = ( nSlot + 1 ) & nMask;
	}
	return UINT32_MAX;
}

bool NMDRenownBlackboard::insertProfileIndex( uint32_t nIndex )
{
	const nmdRenownProfile_t& entry = m_pProfiles[ nIndex ];
	const uint32_t nMask = m_nProfileIndexCapacity - 1;
	uint32_t nSlot = hashKey( entry.nSubjectId, entry.nLocationId, 0 ) & nMask;
	for ( uint32_t nProbe = 0; nProbe < m_nProfileIndexCapacity; ++nProbe ) {
		if ( m_pProfileIndex[ nSlot ] == 0 ) {
			m_pProfileIndex[ nSlot ] = nIndex + 1;
			return true;
		}
		nSlot = ( nSlot + 1 ) & nMask;
	}
	return false;
}

bool NMDRenownBlackboard::insertTraitIndex( uint32_t nIndex )
{
	const nmdRenownTrait_t& entry = m_pTraits[ nIndex ];
	const uint32_t nMask = m_nTraitIndexCapacity - 1;
	uint32_t nSlot = hashKey( entry.nSubjectId, entry.nLocationId, entry.nTraitId ) & nMask;
	for ( uint32_t nProbe = 0; nProbe < m_nTraitIndexCapacity; ++nProbe ) {
		if ( m_pTraitIndex[ nSlot ] == 0 ) {
			m_pTraitIndex[ nSlot ] = nIndex + 1;
			return true;
		}
		nSlot = ( nSlot + 1 ) & nMask;
	}
	return false;
}

bool NMDRenownBlackboard::insertPatternIndex( uint32_t nIndex )
{
	const nmdRenownPattern_t& entry = m_pPatterns[ nIndex ];
	const uint32_t nMask = m_nPatternIndexCapacity - 1;
	uint32_t nSlot = hashKey( entry.nSubjectId, entry.nLocationId, entry.nIncidentType ) & nMask;
	for ( uint32_t nProbe = 0; nProbe < m_nPatternIndexCapacity; ++nProbe ) {
		if ( m_pPatternIndex[ nSlot ] == 0 ) {
			m_pPatternIndex[ nSlot ] = nIndex + 1;
			return true;
		}
		nSlot = ( nSlot + 1 ) & nMask;
	}
	return false;
}

uint32_t NMDRenownBlackboard::getProfileCount() const
{
	return isBound() ? m_pHeader->nProfileCount : 0;
}

bool NMDRenownBlackboard::getProfile( uint32_t nSubjectId, uint32_t nLocationId,
                                      nmdRenownProfile_t& out ) const
{
	const uint32_t nIndex = findProfileIndex( nSubjectId, nLocationId );
	if ( nIndex == UINT32_MAX ) {
		return false;
	}
	out = m_pProfiles[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::getProfileByIndex( uint32_t nIndex, nmdRenownProfile_t& out ) const
{
	if ( nIndex >= getProfileCount() ) {
		return false;
	}
	out = m_pProfiles[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::canSetProfile( uint32_t nSubjectId, uint32_t nLocationId ) const
{
	nmdRenownLocation_t location = {};
	if ( nSubjectId == 0 || !getLocation( nLocationId, location ) ) {
		return false;
	}
	return findProfileIndex( nSubjectId, nLocationId ) != UINT32_MAX ||
	       getProfileCount() < m_Capacity.nProfiles;
}

bool NMDRenownBlackboard::setProfile( const nmdRenownProfile_t& value )
{
	nmdRenownLocation_t location = {};
	if ( !isBound() || value.nSubjectId == 0 ||
	     !getLocation( value.nLocationId, location ) ) {
		return false;
	}
	const uint32_t nExisting = findProfileIndex( value.nSubjectId, value.nLocationId );
	if ( nExisting != UINT32_MAX ) {
		m_pProfiles[ nExisting ] = value;
	} else {
		if ( m_pHeader->nProfileCount >= m_Capacity.nProfiles ) {
			return false;
		}
		const uint32_t nNew = m_pHeader->nProfileCount;
		m_pProfiles[ nNew ] = value;
		if ( !insertProfileIndex( nNew ) ) {
			return false;
		}
		++m_pHeader->nProfileCount;
	}
	++m_pHeader->nRevision;
	return true;
}

uint32_t NMDRenownBlackboard::getTraitCount() const
{
	return isBound() ? m_pHeader->nTraitCount : 0;
}

bool NMDRenownBlackboard::getTrait( uint32_t nSubjectId, uint32_t nLocationId,
                                    uint32_t nTraitId, nmdRenownTrait_t& out ) const
{
	const uint32_t nIndex = findTraitIndex( nSubjectId, nLocationId, nTraitId );
	if ( nIndex == UINT32_MAX ) {
		return false;
	}
	out = m_pTraits[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::getTraitByIndex( uint32_t nIndex, nmdRenownTrait_t& out ) const
{
	if ( nIndex >= getTraitCount() ) {
		return false;
	}
	out = m_pTraits[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::canSetTrait( uint32_t nSubjectId, uint32_t nLocationId,
                                       uint32_t nTraitId ) const
{
	nmdRenownLocation_t location = {};
	if ( nSubjectId == 0 || nTraitId == 0 || !getLocation( nLocationId, location ) ) {
		return false;
	}
	return findTraitIndex( nSubjectId, nLocationId, nTraitId ) != UINT32_MAX ||
	       getTraitCount() < m_Capacity.nTraits;
}

bool NMDRenownBlackboard::setTrait( const nmdRenownTrait_t& value )
{
	nmdRenownLocation_t location = {};
	if ( !isBound() || value.nSubjectId == 0 || value.nTraitId == 0 ||
	     !getLocation( value.nLocationId, location ) ) {
		return false;
	}
	const uint32_t nExisting = findTraitIndex( value.nSubjectId, value.nLocationId, value.nTraitId );
	if ( nExisting != UINT32_MAX ) {
		m_pTraits[ nExisting ] = value;
	} else {
		if ( m_pHeader->nTraitCount >= m_Capacity.nTraits ) {
			return false;
		}
		const uint32_t nNew = m_pHeader->nTraitCount;
		m_pTraits[ nNew ] = value;
		if ( !insertTraitIndex( nNew ) ) {
			return false;
		}
		++m_pHeader->nTraitCount;
	}
	++m_pHeader->nRevision;
	return true;
}

uint32_t NMDRenownBlackboard::getPatternCount() const
{
	return isBound() ? m_pHeader->nPatternCount : 0;
}

bool NMDRenownBlackboard::getPattern( uint32_t nSubjectId, uint32_t nLocationId,
                                      uint8_t nType, nmdRenownPattern_t& out ) const
{
	const uint32_t nIndex = findPatternIndex( nSubjectId, nLocationId, nType );
	if ( nIndex == UINT32_MAX ) {
		return false;
	}
	out = m_pPatterns[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::getPatternByIndex( uint32_t nIndex, nmdRenownPattern_t& out ) const
{
	if ( nIndex >= getPatternCount() ) {
		return false;
	}
	out = m_pPatterns[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::canSetPattern( uint32_t nSubjectId, uint32_t nLocationId,
                                         uint8_t nType ) const
{
	nmdRenownLocation_t location = {};
	if ( nSubjectId == 0 || !getLocation( nLocationId, location ) ) {
		return false;
	}
	return findPatternIndex( nSubjectId, nLocationId, nType ) != UINT32_MAX ||
	       getPatternCount() < m_Capacity.nPatterns;
}

bool NMDRenownBlackboard::setPattern( const nmdRenownPattern_t& value )
{
	nmdRenownLocation_t location = {};
	if ( !isBound() || value.nSubjectId == 0 ||
	     !getLocation( value.nLocationId, location ) ) {
		return false;
	}
	const uint32_t nExisting = findPatternIndex( value.nSubjectId,
	                                             value.nLocationId, value.nIncidentType );
	if ( nExisting != UINT32_MAX ) {
		m_pPatterns[ nExisting ] = value;
	} else {
		if ( m_pHeader->nPatternCount >= m_Capacity.nPatterns ) {
			return false;
		}
		const uint32_t nNew = m_pHeader->nPatternCount;
		m_pPatterns[ nNew ] = value;
		if ( !insertPatternIndex( nNew ) ) {
			return false;
		}
		++m_pHeader->nPatternCount;
	}
	++m_pHeader->nRevision;
	return true;
}

uint32_t NMDRenownBlackboard::getIncidentCount() const
{
	return isBound() ? m_pHeader->nIncidentCount : 0;
}

bool NMDRenownBlackboard::getIncidentByAge( uint32_t nAgeIndex,
                                            nmdRenownIncidentRecord_t& out ) const
{
	if ( nAgeIndex >= getIncidentCount() ) {
		return false;
	}
	const uint32_t nIndex = ( m_pHeader->nIncidentHead + nAgeIndex ) % m_Capacity.nIncidents;
	out = m_pIncidents[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::findIncident( uint64_t nIncidentId,
                                        nmdRenownIncidentRecord_t& out, uint32_t& nPhysicalIndex ) const
{
	for ( uint32_t i = 0; i < getIncidentCount(); ++i ) {
		const uint32_t nIndex = ( m_pHeader->nIncidentHead + i ) % m_Capacity.nIncidents;
		if ( m_pIncidents[ nIndex ].incident.nIncidentId == nIncidentId ) {
			out = m_pIncidents[ nIndex ];
			nPhysicalIndex = nIndex;
			return true;
		}
	}
	return false;
}

bool NMDRenownBlackboard::setIncidentAt( uint32_t nPhysicalIndex,
                                         const nmdRenownIncidentRecord_t& value )
{
	if ( !isBound() || nPhysicalIndex >= m_Capacity.nIncidents ) {
		return false;
	}
	m_pIncidents[ nPhysicalIndex ] = value;
	++m_pHeader->nRevision;
	return true;
}

bool NMDRenownBlackboard::appendIncident( const nmdRenownIncidentRecord_t& value )
{
	if ( !isBound() || value.incident.nIncidentId <= m_pHeader->nLastIncidentId ) {
		return false;
	}
	uint32_t nIndex = 0;
	if ( m_pHeader->nIncidentCount < m_Capacity.nIncidents ) {
		nIndex = ( m_pHeader->nIncidentHead + m_pHeader->nIncidentCount ) % m_Capacity.nIncidents;
		++m_pHeader->nIncidentCount;
	} else {
		nIndex = m_pHeader->nIncidentHead;
		m_pHeader->nIncidentHead = ( m_pHeader->nIncidentHead + 1 ) % m_Capacity.nIncidents;
	}
	m_pIncidents[ nIndex ] = value;
	m_pHeader->nLastIncidentId = value.incident.nIncidentId;
	++m_pHeader->nRevision;
	return true;
}

uint32_t NMDRenownBlackboard::getRumorCount() const
{
	return isBound() ? m_pHeader->nRumorCount : 0;
}

bool NMDRenownBlackboard::getRumorAt( uint32_t nIndex, nmdRenownRumor_t& out ) const
{
	if ( nIndex >= getRumorCount() ) {
		return false;
	}
	out = m_pRumors[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::setRumorAt( uint32_t nIndex, const nmdRenownRumor_t& value )
{
	if ( nIndex >= getRumorCount() ) {
		return false;
	}
	m_pRumors[ nIndex ] = value;
	++m_pHeader->nRevision;
	return true;
}

bool NMDRenownBlackboard::appendRumor( const nmdRenownRumor_t& value )
{
	if ( !isBound() || m_pHeader->nRumorCount >= m_Capacity.nRumors ) {
		return false;
	}
	m_pRumors[ m_pHeader->nRumorCount++ ] = value;
	++m_pHeader->nRevision;
	return true;
}

bool NMDRenownBlackboard::removeLastRumor()
{
	if ( getRumorCount() == 0 ) {
		return false;
	}
	--m_pHeader->nRumorCount;
	++m_pHeader->nRevision;
	return true;
}

uint32_t NMDRenownBlackboard::getSeenCount() const
{
	return isBound() ? m_pHeader->nSeenCount : 0;
}

bool NMDRenownBlackboard::getSeenAt( uint32_t nIndex, nmdRenownSeen_t& out ) const
{
	if ( nIndex >= getSeenCount() ) {
		return false;
	}
	out = m_pSeen[ nIndex ];
	return true;
}

bool NMDRenownBlackboard::findSeen( uint64_t nRumorId, uint32_t nLocationId,
                                    nmdRenownSeen_t& out, uint32_t& nIndex ) const
{
	for ( uint32_t i = 0; i < getSeenCount(); ++i ) {
		if ( m_pSeen[ i ].nRumorId == nRumorId && m_pSeen[ i ].nLocationId == nLocationId ) {
			out = m_pSeen[ i ];
			nIndex = i;
			return true;
		}
	}
	return false;
}

bool NMDRenownBlackboard::setSeenAt( uint32_t nIndex, const nmdRenownSeen_t& value )
{
	if ( nIndex >= getSeenCount() ) {
		return false;
	}
	m_pSeen[ nIndex ] = value;
	++m_pHeader->nRevision;
	return true;
}

bool NMDRenownBlackboard::appendSeen( const nmdRenownSeen_t& value )
{
	if ( !isBound() || m_pHeader->nSeenCount >= m_Capacity.nSeen ) {
		return false;
	}
	m_pSeen[ m_pHeader->nSeenCount++ ] = value;
	++m_pHeader->nRevision;
	return true;
}

void NMDRenownBlackboard::removeSeenForRumor( uint64_t nRumorId )
{
	uint32_t nWrite = 0;
	for ( uint32_t i = 0; i < getSeenCount(); ++i ) {
		if ( m_pSeen[ i ].nRumorId != nRumorId ) {
			m_pSeen[ nWrite++ ] = m_pSeen[ i ];
		}
	}
	if ( nWrite != m_pHeader->nSeenCount ) {
		m_pHeader->nSeenCount = nWrite;
		++m_pHeader->nRevision;
	}
}

void NMDRenownBlackboard::clearRuntimeState()
{
	if ( !isBound() ) {
		return;
	}
	m_pHeader->nProfileCount = 0;
	m_pHeader->nTraitCount = 0;
	m_pHeader->nPatternCount = 0;
	m_pHeader->nIncidentCount = 0;
	m_pHeader->nIncidentHead = 0;
	m_pHeader->nRumorCount = 0;
	m_pHeader->nSeenCount = 0;
	m_pHeader->nLastIncidentId = 0;
	m_pHeader->nLastRumorId = 0;
	m_pHeader->nWorldMinute = 0;
	++m_pHeader->nRevision;
	memset( m_pProfileIndex, 0, m_nProfileIndexCapacity * sizeof( uint32_t ) );
	memset( m_pTraitIndex, 0, m_nTraitIndexCapacity * sizeof( uint32_t ) );
	memset( m_pPatternIndex, 0, m_nPatternIndexCapacity * sizeof( uint32_t ) );
}

bool NMDRenownBlackboard::rebuildIndices()
{
	if ( !isBound() ) {
		return false;
	}
	memset( m_pProfileIndex, 0, m_nProfileIndexCapacity * sizeof( uint32_t ) );
	memset( m_pTraitIndex, 0, m_nTraitIndexCapacity * sizeof( uint32_t ) );
	memset( m_pPatternIndex, 0, m_nPatternIndexCapacity * sizeof( uint32_t ) );
	for ( uint32_t i = 0; i < getProfileCount(); ++i ) {
		if ( !insertProfileIndex( i ) ) {
			return false;
		}
	}
	for ( uint32_t i = 0; i < getTraitCount(); ++i ) {
		if ( !insertTraitIndex( i ) ) {
			return false;
		}
	}
	for ( uint32_t i = 0; i < getPatternCount(); ++i ) {
		if ( !insertPatternIndex( i ) ) {
			return false;
		}
	}
	return true;
}
