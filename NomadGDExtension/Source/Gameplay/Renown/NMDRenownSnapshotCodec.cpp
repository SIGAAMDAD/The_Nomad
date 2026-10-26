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

#include "NMDRenownSnapshotCodec.h"

#include <string.h>
#include <limits.h>

static const size_t NMD_SNAPSHOT_HEADER_BYTES = 72;
static const size_t NMD_PROFILE_BYTES = 32;
static const size_t NMD_TRAIT_BYTES = 24;
static const size_t NMD_PATTERN_BYTES = 20;
static const size_t NMD_INCIDENT_BYTES = 50;
static const size_t NMD_RUMOR_BYTES = 34;
static const size_t NMD_SEEN_BYTES = 20;

struct nmdSnapshotHeader_t
{
	uint32_t nMagic;
	uint32_t nVersion;
	uint32_t nBytes;
	uint32_t nChecksum;
	uint32_t nGraphFingerprint;
	uint32_t nRulesFingerprint;
	uint32_t nWorldMinute;
	uint32_t nProfileCount;
	uint32_t nTraitCount;
	uint32_t nPatternCount;
	uint32_t nIncidentCount;
	uint32_t nRumorCount;
	uint32_t nSeenCount;
	uint32_t nRevision;
	uint64_t nLastIncidentId;
	uint64_t nLastRumorId;
};

struct nmdSnapshotLayout_t
{
	size_t nProfiles;
	size_t nTraits;
	size_t nPatterns;
	size_t nIncidents;
	size_t nRumors;
	size_t nSeen;
	size_t nTotal;
};

class NMDByteWriter
{
public:
	explicit NMDByteWriter( void *pData ) : m_pData( static_cast<uint8_t *>( pData ) ), m_nOffset( 0 )
	{
	}
	void put8( uint8_t nValue )
	{
		m_pData[ m_nOffset++ ] = nValue;
	}
	void put16( uint16_t nValue )
	{
		put8( static_cast<uint8_t>( nValue ) );
		put8( static_cast<uint8_t>( nValue >> 8 ) );
	}
	void put32( uint32_t nValue )
	{
		put16( static_cast<uint16_t>( nValue ) );
		put16( static_cast<uint16_t>( nValue >> 16 ) );
	}
	void put64( uint64_t nValue )
	{
		put32( static_cast<uint32_t>( nValue ) );
		put32( static_cast<uint32_t>( nValue >> 32 ) );
	}
	size_t getOffset() const
	{
		return m_nOffset;
	}
private:
	uint8_t *m_pData;
	size_t m_nOffset;
};

class NMDByteReader
{
public:
	explicit NMDByteReader( const void *pData ) : m_pData( static_cast<const uint8_t *>( pData ) ), m_nOffset( 0 )
	{
	}
	void seek( size_t nOffset )
	{
		m_nOffset = nOffset;
	}
	uint8_t get8()
	{
		return m_pData[ m_nOffset++ ];
	}
	uint16_t get16()
	{
		const uint16_t nA = get8();
		const uint16_t nB = get8();
		return static_cast<uint16_t>( nA | ( nB << 8 ) );
	}
	uint32_t get32()
	{
		const uint32_t nA = get16();
		const uint32_t nB = get16();
		return nA | ( nB << 16 );
	}
	uint64_t get64()
	{
		const uint64_t nA = get32();
		const uint64_t nB = get32();
		return nA | ( nB << 32 );
	}
	size_t getOffset() const
	{
		return m_nOffset;
	}
private:
	const uint8_t *m_pData;
	size_t m_nOffset;
};

bool addBytes( size_t& nOffset, uint32_t nCount, size_t nStride )
{
	if ( nCount > ( SIZE_MAX - nOffset ) / nStride ) {
		return false;
	}
	nOffset += nCount * nStride;
	return true;
}

bool calculateLayout( const nmdSnapshotHeader_t& header, nmdSnapshotLayout_t& layout )
{
	size_t nOffset = NMD_SNAPSHOT_HEADER_BYTES;
	layout.nProfiles = nOffset;
	if ( !addBytes( nOffset, header.nProfileCount, NMD_PROFILE_BYTES ) )
		return false;
	layout.nTraits = nOffset;
	if ( !addBytes( nOffset, header.nTraitCount, NMD_TRAIT_BYTES ) )
		return false;
	layout.nPatterns = nOffset;
	if ( !addBytes( nOffset, header.nPatternCount, NMD_PATTERN_BYTES ) )
		return false;
	layout.nIncidents = nOffset;
	if ( !addBytes( nOffset, header.nIncidentCount, NMD_INCIDENT_BYTES ) )
		return false;
	layout.nRumors = nOffset;
	if ( !addBytes( nOffset, header.nRumorCount, NMD_RUMOR_BYTES ) )
		return false;
	layout.nSeen = nOffset;
	if ( !addBytes( nOffset, header.nSeenCount, NMD_SEEN_BYTES ) )
		return false;
	layout.nTotal = nOffset;
	return true;
}

bool buffersOverlap( const void *pA, size_t nA, const void *pB, size_t nB )
{
	const uintptr_t nStartA = reinterpret_cast<uintptr_t>( pA );
	const uintptr_t nStartB = reinterpret_cast<uintptr_t>( pB );
	if ( nA > UINTPTR_MAX - nStartA || nB > UINTPTR_MAX - nStartB ) {
		return true;
	}
	return nStartA < nStartB + nB && nStartB < nStartA + nA;
}

uint32_t checksumSnapshot( const uint8_t *pData, size_t nBytes )
{
	uint32_t nHash = 2166136261u;
	for ( size_t i = 0; i < nBytes; ++i ) {
		if ( i >= 12 && i < 16 ) {
			continue;
		}
		nHash ^= pData[ i ];
		nHash *= 16777619u;
	}
	return nHash;
}

void writeChecksum( uint8_t *pData, uint32_t nValue )
{
	for ( uint32_t i = 0; i < 4; ++i ) {
		pData[ 12 + i ] = static_cast<uint8_t>( nValue >> ( i * 8 ) );
	}
}

void writeHeader( NMDByteWriter& writer, const nmdSnapshotHeader_t& header )
{
	writer.put32( header.nMagic );
	writer.put32( header.nVersion );
	writer.put32( header.nBytes );
	writer.put32( header.nChecksum );
	writer.put32( header.nGraphFingerprint );
	writer.put32( header.nRulesFingerprint );
	writer.put32( header.nWorldMinute );
	writer.put32( header.nProfileCount );
	writer.put32( header.nTraitCount );
	writer.put32( header.nPatternCount );
	writer.put32( header.nIncidentCount );
	writer.put32( header.nRumorCount );
	writer.put32( header.nSeenCount );
	writer.put32( header.nRevision );
	writer.put64( header.nLastIncidentId );
	writer.put64( header.nLastRumorId );
}

nmdSnapshotHeader_t readHeader( NMDByteReader& reader )
{
	nmdSnapshotHeader_t header = {};
	header.nMagic = reader.get32();
	header.nVersion = reader.get32();
	header.nBytes = reader.get32();
	header.nChecksum = reader.get32();
	header.nGraphFingerprint = reader.get32();
	header.nRulesFingerprint = reader.get32();
	header.nWorldMinute = reader.get32();
	header.nProfileCount = reader.get32();
	header.nTraitCount = reader.get32();
	header.nPatternCount = reader.get32();
	header.nIncidentCount = reader.get32();
	header.nRumorCount = reader.get32();
	header.nSeenCount = reader.get32();
	header.nRevision = reader.get32();
	header.nLastIncidentId = reader.get64();
	header.nLastRumorId = reader.get64();
	return header;
}

void writeProfile( NMDByteWriter& writer, const nmdRenownProfile_t& p )
{
	writer.put32( p.nSubjectId );
	writer.put32( p.nLocationId );
	writer.put32( p.nRenownQ8 );
	writer.put32( p.nHistoricalFloorQ8 );
	writer.put32( p.nLastGainDay );
	writer.put32( p.nLastUpdateDay );
	writer.put32( p.nRevision );
	writer.put32( p.nFlags );
}

nmdRenownProfile_t readProfile( NMDByteReader& reader )
{
	nmdRenownProfile_t p = {};
	p.nSubjectId = reader.get32();
	p.nLocationId = reader.get32();
	p.nRenownQ8 = reader.get32();
	p.nHistoricalFloorQ8 = reader.get32();
	p.nLastGainDay = reader.get32();
	p.nLastUpdateDay = reader.get32();
	p.nRevision = reader.get32();
	p.nFlags = reader.get32();
	return p;
}

void writeTrait( NMDByteWriter& writer, const nmdRenownTrait_t& t )
{
	writer.put32( t.nSubjectId );
	writer.put32( t.nLocationId );
	writer.put32( t.nTraitId );
	writer.put32( t.nLastGainDay );
	writer.put32( t.nLastUpdateDay );
	writer.put16( t.nEvidence );
	writer.put16( t.nFlags );
}

nmdRenownTrait_t readTrait( NMDByteReader& reader )
{
	nmdRenownTrait_t t = {};
	t.nSubjectId = reader.get32();
	t.nLocationId = reader.get32();
	t.nTraitId = reader.get32();
	t.nLastGainDay = reader.get32();
	t.nLastUpdateDay = reader.get32();
	t.nEvidence = reader.get16();
	t.nFlags = reader.get16();
	return t;
}

void writePattern( NMDByteWriter& writer, const nmdRenownPattern_t& p )
{
	writer.put32( p.nSubjectId );
	writer.put32( p.nLocationId );
	writer.put32( p.nLastIncidentDay );
	writer.put32( p.nIncidentCount );
	writer.put8( p.nIncidentType );
	writer.put8( 0 );
	writer.put8( 0 );
	writer.put8( 0 );
}

nmdRenownPattern_t readPattern( NMDByteReader& reader )
{
	nmdRenownPattern_t p = {};
	p.nSubjectId = reader.get32();
	p.nLocationId = reader.get32();
	p.nLastIncidentDay = reader.get32();
	p.nIncidentCount = reader.get32();
	p.nIncidentType = reader.get8();
	reader.get8();
	reader.get8();
	reader.get8();
	return p;
}

void writeIncident( NMDByteWriter& writer, const nmdRenownIncidentRecord_t& record )
{
	const nmdRenownIncident_t& i = record.incident;
	writer.put64( i.nIncidentId );
	writer.put32( i.nSubjectId );
	writer.put32( i.nLocationId );
	writer.put32( i.nWorldMinute );
	writer.put32( i.nBaseRenownQ8 );
	writer.put32( i.nTraitId );
	writer.put16( static_cast<uint16_t>( i.nTraitEvidence ) );
	writer.put16( i.nVisibilityQ8 );
	writer.put16( i.nAttributionQ8 );
	writer.put16( i.nFlags );
	writer.put8( i.nType );
	writer.put8( 0 );
	writer.put32( record.nPatternBonusQ8 );
	writer.put32( record.nAppliedRenownQ8 );
	writer.put16( record.nAppliedCertaintyQ8 );
	writer.put16( record.nAppliedTraitMagnitude );
}

nmdRenownIncidentRecord_t readIncident( NMDByteReader& reader )
{
	nmdRenownIncidentRecord_t record = {};
	nmdRenownIncident_t& i = record.incident;
	i.nIncidentId = reader.get64();
	i.nSubjectId = reader.get32();
	i.nLocationId = reader.get32();
	i.nWorldMinute = reader.get32();
	i.nBaseRenownQ8 = reader.get32();
	i.nTraitId = reader.get32();
	const uint32_t nTraitBits = reader.get16();
	const int32_t nTraitSigned = nTraitBits > INT16_MAX ? static_cast<int32_t>( nTraitBits ) - 65536 : static_cast<int32_t>( nTraitBits );
	i.nTraitEvidence = static_cast<int16_t>( nTraitSigned );
	i.nVisibilityQ8 = reader.get16();
	i.nAttributionQ8 = reader.get16();
	i.nFlags = reader.get16();
	i.nType = reader.get8();
	reader.get8();
	record.nPatternBonusQ8 = reader.get32();
	record.nAppliedRenownQ8 = reader.get32();
	record.nAppliedCertaintyQ8 = reader.get16();
	record.nAppliedTraitMagnitude = reader.get16();
	return record;
}

void writeRumor( NMDByteWriter& writer, const nmdRenownRumor_t& r )
{
	writer.put64( r.nRumorId );
	writer.put32( r.nSubjectId );
	writer.put32( r.nDestinationLocationId );
	writer.put32( r.nDeliveryMinute );
	writer.put32( r.nRenownQ8 );
	writer.put32( r.nTraitId );
	writer.put16( r.nTraitEvidence );
	writer.put16( r.nHopCount );
	writer.put8( r.nTraitNegative );
	writer.put8( r.nFlags );
}

nmdRenownRumor_t readRumor( NMDByteReader& reader )
{
	nmdRenownRumor_t r = {};
	r.nRumorId = reader.get64();
	r.nSubjectId = reader.get32();
	r.nDestinationLocationId = reader.get32();
	r.nDeliveryMinute = reader.get32();
	r.nRenownQ8 = reader.get32();
	r.nTraitId = reader.get32();
	r.nTraitEvidence = reader.get16();
	r.nHopCount = reader.get16();
	r.nTraitNegative = reader.get8();
	r.nFlags = reader.get8();
	return r;
}

void writeSeen( NMDByteWriter& writer, const nmdRenownSeen_t& seen )
{
	writer.put64( seen.nRumorId );
	writer.put32( seen.nLocationId );
	writer.put32( seen.nAppliedRenownQ8 );
	writer.put16( seen.nAppliedTraitEvidence );
	writer.put16( seen.nBestHopCount );
}

nmdRenownSeen_t readSeen( NMDByteReader& reader )
{
	nmdRenownSeen_t s = {};
	s.nRumorId = reader.get64();
	s.nLocationId = reader.get32();
	s.nAppliedRenownQ8 = reader.get32();
	s.nAppliedTraitEvidence = reader.get16();
	s.nBestHopCount = reader.get16();
	return s;
}

uint32_t hashKey( uint64_t nA, uint32_t nB, uint32_t nC )
{
	uint32_t nHash = static_cast<uint32_t>( nA ) ^ static_cast<uint32_t>( nA >> 32 );
	nHash ^= nB * 0x9e3779b9u;
	nHash ^= nC * 0x85ebca6bu;
	nHash ^= nHash >> 16;
	nHash *= 0x7feb352du;
	nHash ^= nHash >> 15;
	nHash *= 0x846ca68bu;
	return nHash ^ ( nHash >> 16 );
}

bool rumorLess( const nmdRenownRumor_t& a, const nmdRenownRumor_t& b )
{
	if ( a.nDeliveryMinute != b.nDeliveryMinute ) {
		return a.nDeliveryMinute < b.nDeliveryMinute;
	}
	if ( a.nRumorId != b.nRumorId ) {
		return a.nRumorId < b.nRumorId;
	}
	return a.nDestinationLocationId < b.nDestinationLocationId;
}

size_t NMDRenownSnapshotCodec::getSnapshotBytes( const NMDRenownBlackboard& blackboard )
{
	if ( !blackboard.isBound() ) {
		return 0;
	}
	const nmdRenownArenaHeader_t h = blackboard.getHeader();
	nmdSnapshotHeader_t snapshot = {};
	snapshot.nProfileCount = h.nProfileCount;
	snapshot.nTraitCount = h.nTraitCount;
	snapshot.nPatternCount = h.nPatternCount;
	snapshot.nIncidentCount = h.nIncidentCount;
	snapshot.nRumorCount = h.nRumorCount;
	snapshot.nSeenCount = h.nSeenCount;
	nmdSnapshotLayout_t layout = {};
	if ( !calculateLayout( snapshot, layout ) || layout.nTotal > UINT32_MAX ) {
		return 0;
	}
	return layout.nTotal;
}

bool NMDRenownSnapshotCodec::writeSnapshot( const NMDRenownBlackboard& blackboard,
                                            void *pOutput, size_t nCapacity, size_t& nBytesWritten )
{
	nBytesWritten = getSnapshotBytes( blackboard );
	if ( nBytesWritten == 0 || pOutput == NULL || nCapacity < nBytesWritten ||
	     buffersOverlap( pOutput, nBytesWritten, blackboard.m_pArena, blackboard.m_nBytes ) ) {
		return false;
	}
	const nmdRenownArenaHeader_t h = blackboard.getHeader();
	nmdSnapshotHeader_t snapshot = {};
	snapshot.nMagic = NMD_RENOWN_SNAPSHOT_MAGIC;
	snapshot.nVersion = NMD_RENOWN_SNAPSHOT_VERSION;
	snapshot.nBytes = static_cast<uint32_t>( nBytesWritten );
	snapshot.nGraphFingerprint = h.nGraphFingerprint;
	snapshot.nRulesFingerprint = h.nRulesFingerprint;
	snapshot.nWorldMinute = h.nWorldMinute;
	snapshot.nProfileCount = h.nProfileCount;
	snapshot.nTraitCount = h.nTraitCount;
	snapshot.nPatternCount = h.nPatternCount;
	snapshot.nIncidentCount = h.nIncidentCount;
	snapshot.nRumorCount = h.nRumorCount;
	snapshot.nSeenCount = h.nSeenCount;
	snapshot.nRevision = h.nRevision;
	snapshot.nLastIncidentId = h.nLastIncidentId;
	snapshot.nLastRumorId = h.nLastRumorId;
	NMDByteWriter writer( pOutput );
	writeHeader( writer, snapshot );
	for ( uint32_t i = 0; i < h.nProfileCount; ++i ) {
		nmdRenownProfile_t profile = {};
		blackboard.getProfileByIndex( i, profile );
		writeProfile( writer, profile );
	}
	for ( uint32_t i = 0; i < h.nTraitCount; ++i ) {
		nmdRenownTrait_t trait = {};
		blackboard.getTraitByIndex( i, trait );
		writeTrait( writer, trait );
	}
	for ( uint32_t i = 0; i < h.nPatternCount; ++i ) {
		nmdRenownPattern_t pattern = {};
		blackboard.getPatternByIndex( i, pattern );
		writePattern( writer, pattern );
	}
	for ( uint32_t i = 0; i < h.nIncidentCount; ++i ) {
		nmdRenownIncidentRecord_t incident = {};
		blackboard.getIncidentByAge( i, incident );
		writeIncident( writer, incident );
	}
	for ( uint32_t i = 0; i < h.nRumorCount; ++i ) {
		nmdRenownRumor_t rumor = {};
		blackboard.getRumorAt( i, rumor );
		writeRumor( writer, rumor );
	}
	for ( uint32_t i = 0; i < h.nSeenCount; ++i ) {
		nmdRenownSeen_t seen = {};
		blackboard.getSeenAt( i, seen );
		writeSeen( writer, seen );
	}
	if ( writer.getOffset() != nBytesWritten ) {
		return false;
	}
	uint8_t *pBytes = static_cast<uint8_t *>( pOutput );
	writeChecksum( pBytes, checksumSnapshot( pBytes, nBytesWritten ) );
	return true;
}

namespace {
	bool validateProfiles( NMDRenownBlackboard& blackboard,
	                       NMDByteReader& reader, const nmdSnapshotHeader_t& header,
	                       const nmdSnapshotLayout_t& layout, uint32_t *pSlots, uint32_t nSlotCount )
	{
		memset( pSlots, 0, nSlotCount * sizeof( uint32_t ) );
		const uint32_t nMask = nSlotCount - 1;
		for ( uint32_t i = 0; i < header.nProfileCount; ++i ) {
			const size_t nOffset = layout.nProfiles + i * NMD_PROFILE_BYTES;
			reader.seek( nOffset );
			const nmdRenownProfile_t p = readProfile( reader );
			nmdRenownLocation_t location = {};
			const uint32_t nDay = header.nWorldMinute / NMD_RENOWN_MINUTES_PER_DAY;
			if ( p.nSubjectId == 0 || !blackboard.getLocation( p.nLocationId, location ) ||
			     p.nRenownQ8 < p.nHistoricalFloorQ8 || p.nLastGainDay > nDay ||
			     p.nLastUpdateDay > nDay || p.nLastUpdateDay < p.nLastGainDay ) {
				return false;
			}
			uint32_t nSlot = hashKey( p.nSubjectId, p.nLocationId, 0 ) & nMask;
			for ( uint32_t nProbe = 0; nProbe < nSlotCount; ++nProbe ) {
				const uint32_t nPrev = pSlots[ nSlot ];
				if ( nPrev == 0 ) {
					pSlots[ nSlot ] = i + 1;
					break;
				}
				reader.seek( layout.nProfiles + ( nPrev - 1 ) * NMD_PROFILE_BYTES );
				const nmdRenownProfile_t previous = readProfile( reader );
				if ( previous.nSubjectId == p.nSubjectId && previous.nLocationId == p.nLocationId ) {
					return false;
				}
				nSlot = ( nSlot + 1 ) & nMask;
			}
		}
		return true;
	}

	bool validateTraits( NMDRenownBlackboard& blackboard,
	                     NMDByteReader& reader, const nmdSnapshotHeader_t& header,
	                     const nmdSnapshotLayout_t& layout, uint32_t *pSlots, uint32_t nSlotCount )
	{
		memset( pSlots, 0, nSlotCount * sizeof( uint32_t ) );
		const uint32_t nMask = nSlotCount - 1;
		for ( uint32_t i = 0; i < header.nTraitCount; ++i ) {
			reader.seek( layout.nTraits + i * NMD_TRAIT_BYTES );
			const nmdRenownTrait_t t = readTrait( reader );
			nmdRenownLocation_t location = {};
			const uint32_t nDay = header.nWorldMinute / NMD_RENOWN_MINUTES_PER_DAY;
			if ( t.nSubjectId == 0 || t.nTraitId == 0 ||
			     !blackboard.getLocation( t.nLocationId, location ) ||
			     t.nLastGainDay > nDay || t.nLastUpdateDay > nDay ||
			     t.nLastUpdateDay < t.nLastGainDay ||
			     ( t.nFlags & ~NMD_RENOWN_TRAIT_PERMANENT ) != 0 ) {
				return false;
			}
			uint32_t nSlot = hashKey( t.nSubjectId, t.nLocationId, t.nTraitId ) & nMask;
			for ( uint32_t nProbe = 0; nProbe < nSlotCount; ++nProbe ) {
				const uint32_t nPrev = pSlots[ nSlot ];
				if ( nPrev == 0 ) {
					pSlots[ nSlot ] = i + 1;
					break;
				}
				reader.seek( layout.nTraits + ( nPrev - 1 ) * NMD_TRAIT_BYTES );
				const nmdRenownTrait_t previous = readTrait( reader );
				if ( previous.nSubjectId == t.nSubjectId && previous.nLocationId == t.nLocationId &&
				     previous.nTraitId == t.nTraitId ) {
					return false;
				}
				nSlot = ( nSlot + 1 ) & nMask;
			}
		}
		return true;
	}

	bool validatePatterns( NMDRenownBlackboard& blackboard,
	                       NMDByteReader& reader, const nmdSnapshotHeader_t& header,
	                       const nmdSnapshotLayout_t& layout, uint32_t *pSlots, uint32_t nSlotCount )
	{
		memset( pSlots, 0, nSlotCount * sizeof( uint32_t ) );
		const uint32_t nMask = nSlotCount - 1;
		for ( uint32_t i = 0; i < header.nPatternCount; ++i ) {
			reader.seek( layout.nPatterns + i * NMD_PATTERN_BYTES );
			const nmdRenownPattern_t p = readPattern( reader );
			nmdRenownLocation_t location = {};
			if ( p.nSubjectId == 0 || p.nIncidentType >= NMD_RENOWN_INCIDENT_TYPE_COUNT ||
			     p.nIncidentCount == 0 ||
			     !blackboard.getLocation( p.nLocationId, location ) ||
			     p.nLastIncidentDay > header.nWorldMinute / NMD_RENOWN_MINUTES_PER_DAY ) {
				return false;
			}
			uint32_t nSlot = hashKey( p.nSubjectId, p.nLocationId, p.nIncidentType ) & nMask;
			for ( uint32_t nProbe = 0; nProbe < nSlotCount; ++nProbe ) {
				const uint32_t nPrev = pSlots[ nSlot ];
				if ( nPrev == 0 ) {
					pSlots[ nSlot ] = i + 1;
					break;
				}
				reader.seek( layout.nPatterns + ( nPrev - 1 ) * NMD_PATTERN_BYTES );
				const nmdRenownPattern_t previous = readPattern( reader );
				if ( previous.nSubjectId == p.nSubjectId && previous.nLocationId == p.nLocationId &&
				     previous.nIncidentType == p.nIncidentType ) {
					return false;
				}
				nSlot = ( nSlot + 1 ) & nMask;
			}
		}
		return true;
	}

	bool validateIncidents( NMDRenownBlackboard& blackboard, NMDByteReader& reader,
	                        const nmdSnapshotHeader_t& header, const nmdSnapshotLayout_t& layout )
	{
		uint64_t nPreviousId = 0;
		uint32_t nPreviousMinute = 0;
		for ( uint32_t i = 0; i < header.nIncidentCount; ++i ) {
			reader.seek( layout.nIncidents + i * NMD_INCIDENT_BYTES );
			const nmdRenownIncidentRecord_t rec = readIncident( reader );
			const nmdRenownIncident_t& event = rec.incident;
			nmdRenownLocation_t location = {};
			if ( event.nIncidentId <= nPreviousId || event.nIncidentId > header.nLastIncidentId ||
			     event.nSubjectId == 0 || event.nType >= NMD_RENOWN_INCIDENT_TYPE_COUNT ||
			     event.nWorldMinute < nPreviousMinute || event.nWorldMinute > header.nWorldMinute ||
			     event.nVisibilityQ8 > 256 || event.nAttributionQ8 > 256 ||
			     rec.nAppliedCertaintyQ8 > 256 ||
			     ( event.nTraitEvidence != 0 && event.nTraitId == 0 ) ||
			     !blackboard.getLocation( event.nLocationId, location ) ) {
				return false;
			}
			const uint32_t nTotal = static_cast<uint32_t>(
			    static_cast<uint64_t>( event.nBaseRenownQ8 ) + rec.nPatternBonusQ8 > UINT32_MAX ? UINT32_MAX : static_cast<uint64_t>( event.nBaseRenownQ8 ) + rec.nPatternBonusQ8 );
			const uint64_t nCredited = ( static_cast<uint64_t>( nTotal ) *
			                                 rec.nAppliedCertaintyQ8 +
			                             128u ) /
			                           256u;
			if ( rec.nAppliedRenownQ8 > nCredited ||
			     rec.nAppliedTraitMagnitude > 32768u ) {
				return false;
			}
			nPreviousId = event.nIncidentId;
			nPreviousMinute = event.nWorldMinute;
		}
		return true;
	}

	bool validateRumors( NMDRenownBlackboard& blackboard, NMDByteReader& reader,
	                     const nmdSnapshotHeader_t& header, const nmdSnapshotLayout_t& layout )
	{
		for ( uint32_t i = 0; i < header.nRumorCount; ++i ) {
			reader.seek( layout.nRumors + i * NMD_RUMOR_BYTES );
			const nmdRenownRumor_t r = readRumor( reader );
			nmdRenownLocation_t location = {};
			if ( r.nRumorId == 0 || r.nRumorId > header.nLastRumorId ||
			     r.nSubjectId == 0 || r.nTraitNegative > 1 || r.nHopCount == 0 ||
			     ( r.nFlags & ~( NMD_RENOWN_RUMOR_HISTORICAL |
			                     NMD_RENOWN_RUMOR_TRAIT_PERMANENT ) ) != 0 ||
			     r.nDeliveryMinute < header.nWorldMinute ||
			     ( r.nTraitId == 0 && r.nTraitEvidence != 0 ) ||
			     !blackboard.getLocation( r.nDestinationLocationId, location ) ) {
				return false;
			}
			if ( i != 0 ) {
				reader.seek( layout.nRumors + ( ( i - 1 ) / 2 ) * NMD_RUMOR_BYTES );
				const nmdRenownRumor_t parent = readRumor( reader );
				if ( rumorLess( r, parent ) ) {
					return false;
				}
			}
		}
		return true;
	}

	bool validateSeen( NMDRenownBlackboard& blackboard, NMDByteReader& reader,
	                   const nmdSnapshotHeader_t& header, const nmdSnapshotLayout_t& layout,
	                   uint32_t *pSlots, uint32_t nSlotCount )
	{
		memset( pSlots, 0, nSlotCount * sizeof( uint32_t ) );
		const uint32_t nMask = nSlotCount - 1;
		for ( uint32_t i = 0; i < header.nSeenCount; ++i ) {
			reader.seek( layout.nSeen + i * NMD_SEEN_BYTES );
			const nmdRenownSeen_t s = readSeen( reader );
			nmdRenownLocation_t location = {};
			if ( s.nRumorId == 0 || s.nRumorId > header.nLastRumorId ||
			     !blackboard.getLocation( s.nLocationId, location ) ) {
				return false;
			}
			uint32_t nSlot = hashKey( s.nRumorId, s.nLocationId, 0 ) & nMask;
			for ( uint32_t nProbe = 0; nProbe < nSlotCount; ++nProbe ) {
				const uint32_t nPrev = pSlots[ nSlot ];
				if ( nPrev == 0 ) {
					pSlots[ nSlot ] = i + 1;
					break;
				}
				reader.seek( layout.nSeen + ( nPrev - 1 ) * NMD_SEEN_BYTES );
				const nmdRenownSeen_t previous = readSeen( reader );
				if ( previous.nRumorId == s.nRumorId && previous.nLocationId == s.nLocationId ) {
					return false;
				}
				nSlot = ( nSlot + 1 ) & nMask;
			}
		}
		return true;
	}
} // namespace

nmdRenownResult_t NMDRenownSnapshotCodec::readSnapshot( NMDRenownBlackboard& blackboard,
                                                        const void *pInput, size_t nBytes )
{
	if ( !blackboard.isBound() || pInput == NULL || nBytes < NMD_SNAPSHOT_HEADER_BYTES ||
	     buffersOverlap( pInput, nBytes, blackboard.m_pArena, blackboard.m_nBytes ) ) {
		return NMD_RENOWN_BAD_ARGUMENT;
	}
	NMDByteReader reader( pInput );
	const nmdSnapshotHeader_t h = readHeader( reader );
	nmdSnapshotLayout_t layout = {};
	if ( h.nMagic != NMD_RENOWN_SNAPSHOT_MAGIC ||
	     h.nVersion != NMD_RENOWN_SNAPSHOT_VERSION ||
	     !calculateLayout( h, layout ) || layout.nTotal != nBytes ||
	     h.nBytes != nBytes ) {
		return NMD_RENOWN_CORRUPT_SNAPSHOT;
	}
	const nmdRenownArenaHeader_t world = blackboard.getHeader();
	if ( h.nGraphFingerprint != world.nGraphFingerprint ||
	     h.nRulesFingerprint != world.nRulesFingerprint ) {
		return NMD_RENOWN_CONTENT_MISMATCH;
	}
	const nmdRenownCapacities_t& capacity = blackboard.getCapacities();
	if ( h.nProfileCount > capacity.nProfiles || h.nTraitCount > capacity.nTraits ||
	     h.nPatternCount > capacity.nPatterns || h.nIncidentCount > capacity.nIncidents ||
	     h.nRumorCount > capacity.nRumors || h.nSeenCount > capacity.nSeen ||
	     ( h.nIncidentCount > 0 && h.nLastIncidentId == 0 ) ||
	     ( h.nRumorCount > 0 && h.nLastRumorId == 0 ) ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	if ( checksumSnapshot( static_cast<const uint8_t *>( pInput ), nBytes ) != h.nChecksum ) {
		return NMD_RENOWN_CORRUPT_SNAPSHOT;
	}
	uint32_t *pSlots = blackboard.m_pValidationSlots;
	const uint32_t nSlots = blackboard.m_nValidationSlotCount;
	if ( !validateProfiles( blackboard, reader, h, layout, pSlots, nSlots ) ||
	     !validateTraits( blackboard, reader, h, layout, pSlots, nSlots ) ||
	     !validatePatterns( blackboard, reader, h, layout, pSlots, nSlots ) ||
	     !validateIncidents( blackboard, reader, h, layout ) ||
	     !validateRumors( blackboard, reader, h, layout ) ||
	     !validateSeen( blackboard, reader, h, layout, pSlots, nSlots ) ) {
		return NMD_RENOWN_CORRUPT_SNAPSHOT;
	}

	// All structural checks have passed. Storage writes cannot fail after
	// capacity/uniqueness preflight. No world records were touched before here.
	blackboard.clearRuntimeState();
	for ( uint32_t i = 0; i < h.nProfileCount; ++i ) {
		reader.seek( layout.nProfiles + i * NMD_PROFILE_BYTES );
		blackboard.setProfile( readProfile( reader ) );
	}
	for ( uint32_t i = 0; i < h.nTraitCount; ++i ) {
		reader.seek( layout.nTraits + i * NMD_TRAIT_BYTES );
		blackboard.setTrait( readTrait( reader ) );
	}
	for ( uint32_t i = 0; i < h.nPatternCount; ++i ) {
		reader.seek( layout.nPatterns + i * NMD_PATTERN_BYTES );
		blackboard.setPattern( readPattern( reader ) );
	}
	for ( uint32_t i = 0; i < h.nIncidentCount; ++i ) {
		reader.seek( layout.nIncidents + i * NMD_INCIDENT_BYTES );
		blackboard.appendIncident( readIncident( reader ) );
	}
	for ( uint32_t i = 0; i < h.nRumorCount; ++i ) {
		reader.seek( layout.nRumors + i * NMD_RUMOR_BYTES );
		blackboard.appendRumor( readRumor( reader ) );
	}
	for ( uint32_t i = 0; i < h.nSeenCount; ++i ) {
		reader.seek( layout.nSeen + i * NMD_SEEN_BYTES );
		blackboard.appendSeen( readSeen( reader ) );
	}
	blackboard.setLastIncidentId( h.nLastIncidentId );
	blackboard.setLastRumorId( h.nLastRumorId );
	blackboard.setWorldMinute( h.nWorldMinute );
	blackboard.setRevision( h.nRevision );
	return NMD_RENOWN_OK;
}
