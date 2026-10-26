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

#ifndef __NMD_RENOWN_BLACKBOARD_H__
#define __NMD_RENOWN_BLACKBOARD_H__

#include "NMDRenownTypes.h"

/*
===========================================================================
NMDRenownBlackboard

Typed, nonowning view into a partition of NMDWorldDatabase's ONE backing
allocation. Contains no allocation calls and no gameplay logic. After an
arena relocation, the database must call bindExisting() with the new base.
===========================================================================
*/
class NMDRenownBlackboard
{
public:
	NMDRenownBlackboard( void );

	static size_t getRequiredBytes( const nmdRenownCapacities_t& capacity );

	bool initialize( void *pArena, size_t nBytes, const nmdRenownCapacities_t& capacity );
	bool bindExisting( void *pArena, size_t nBytes, const nmdRenownCapacities_t& capacity );
	void unbind( void );
	bool isBound( void ) const;

	const nmdRenownCapacities_t& getCapacities( void ) const;
	nmdRenownArenaHeader_t getHeader( void ) const;
	uint32_t getWorldMinute( void ) const;
	uint32_t getRevision( void ) const;
	void setWorldMinute( uint32_t nMinute );
	void setRulesFingerprint( uint32_t nFingerprint );
	void setLastIncidentId( uint64_t nIncidentId );
	void setLastRumorId( uint64_t nRumorId );
	void setRevision( uint32_t nRevision );
	uint64_t reserveRumorId( void );

	uint32_t getLocationCount( void ) const;
	uint32_t getEdgeCount( void ) const;
	bool getLocationByIndex( uint32_t nIndex, nmdRenownLocation_t& out ) const;
	bool getLocation( uint32_t nLocationId, nmdRenownLocation_t& out ) const;
	bool getEdgeByIndex( uint32_t nIndex, nmdRenownEdge_t& out ) const;
	bool setLocationGraph( const nmdRenownLocation_t *pLocations, uint32_t nLocationCount,
	                       const nmdRenownEdge_t *pEdges, uint32_t nEdgeCount, uint32_t nFingerprint );

	uint32_t getProfileCount() const;
	bool getProfile( uint32_t nSubjectId, uint32_t nLocationId, nmdRenownProfile_t& out ) const;
	bool getProfileByIndex( uint32_t nIndex, nmdRenownProfile_t& out ) const;
	bool canSetProfile( uint32_t nSubjectId, uint32_t nLocationId ) const;
	bool setProfile( const nmdRenownProfile_t& value );

	uint32_t getTraitCount( void ) const;
	bool getTrait( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nTraitId, nmdRenownTrait_t& out ) const;
	bool getTraitByIndex( uint32_t nIndex, nmdRenownTrait_t& out ) const;
	bool canSetTrait( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nTraitId ) const;
	bool setTrait( const nmdRenownTrait_t& value );

	uint32_t getPatternCount() const;
	bool getPattern( uint32_t nSubjectId, uint32_t nLocationId, uint8_t nType, nmdRenownPattern_t& out ) const;
	bool getPatternByIndex( uint32_t nIndex, nmdRenownPattern_t& out ) const;
	bool canSetPattern( uint32_t nSubjectId, uint32_t nLocationId, uint8_t nType ) const;
	bool setPattern( const nmdRenownPattern_t& value );

	uint32_t getIncidentCount() const;
	bool getIncidentByAge( uint32_t nAgeIndex, nmdRenownIncidentRecord_t& out ) const;
	bool findIncident( uint64_t nIncidentId, nmdRenownIncidentRecord_t& out, uint32_t& nPhysicalIndex ) const;
	bool setIncidentAt( uint32_t nPhysicalIndex, const nmdRenownIncidentRecord_t& value );
	bool appendIncident( const nmdRenownIncidentRecord_t& value );

	uint32_t getRumorCount() const;
	bool getRumorAt( uint32_t nIndex, nmdRenownRumor_t& out ) const;
	bool setRumorAt( uint32_t nIndex, const nmdRenownRumor_t& value );
	bool appendRumor( const nmdRenownRumor_t& value );
	bool removeLastRumor();

	uint32_t getSeenCount() const;
	bool getSeenAt( uint32_t nIndex, nmdRenownSeen_t& out ) const;
	bool findSeen( uint64_t nRumorId, uint32_t nLocationId,
	               nmdRenownSeen_t& out, uint32_t& nIndex ) const;
	bool setSeenAt( uint32_t nIndex, const nmdRenownSeen_t& value );
	bool appendSeen( const nmdRenownSeen_t& value );
	void removeSeenForRumor( uint64_t nRumorId );

	// Called only after external snapshot prevalidation.
	void clearRuntimeState();

private:
	bool bind( void *pArena, size_t nBytes, const nmdRenownCapacities_t& capacity );
	bool rebuildIndices();
	uint32_t findProfileIndex( uint32_t nSubjectId, uint32_t nLocationId ) const;
	uint32_t findTraitIndex( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nTraitId ) const;
	uint32_t findPatternIndex( uint32_t nSubjectId, uint32_t nLocationId, uint8_t nType ) const;
	bool insertProfileIndex( uint32_t nIndex );
	bool insertTraitIndex( uint32_t nIndex );
	bool insertPatternIndex( uint32_t nIndex );
	static uint32_t hashKey( uint32_t nA, uint32_t nB, uint32_t nC );
	static uint32_t nextPowerOfTwo( uint32_t nValue );
	static uint32_t capacityFingerprint( const nmdRenownCapacities_t& capacity );

	uint8_t *m_pArena;
	size_t m_nBytes;
	nmdRenownCapacities_t m_Capacity;
	friend class NMDRenownSnapshotCodec;

	nmdRenownArenaHeader_t *m_pHeader;
	nmdRenownProfile_t *m_pProfiles;
	nmdRenownTrait_t *m_pTraits;
	nmdRenownPattern_t *m_pPatterns;
	nmdRenownIncidentRecord_t *m_pIncidents;
	nmdRenownRumor_t *m_pRumors;
	nmdRenownSeen_t *m_pSeen;
	nmdRenownLocation_t *m_pLocations;
	nmdRenownEdge_t *m_pEdges;
	uint32_t *m_pProfileIndex;
	uint32_t *m_pTraitIndex;
	uint32_t *m_pPatternIndex;
	uint32_t *m_pValidationSlots;
	uint32_t m_nProfileIndexCapacity;
	uint32_t m_nTraitIndexCapacity;
	uint32_t m_nPatternIndexCapacity;
	uint32_t m_nValidationSlotCount;
};

#endif // __NMD_RENOWN_BLACKBOARD_H__
