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

#ifndef __NMD_RENOWN_SYSTEM_H__
#define __NMD_RENOWN_SYSTEM_H__

#include "NMDRenownBlackboard.h"

/*
===========================================================================
NMDRenownSystem

Owns decisions, not memory. The blackboard borrows a segment allocated by
NMDWorldDatabase; calendar callers advance this system with absolute time.
===========================================================================
*/
class NMDRenownSystem
{
public:
	NMDRenownSystem( void );

	static nmdRenownRules_t getDefaultRules( void );
	static nmdRenownTier_t getTierForScore( uint32_t nRenownQ8 );
	static uint32_t calculateRulesFingerprint( const nmdRenownRules_t &rules );

	bool bind( NMDRenownBlackboard *pBlackboard, const nmdRenownRules_t &rules );
	void unbind( void );

	// The caller compiles ONLY LOIs with their renown-tracking flag set.
	// Locations MUST be sorted by nLocationId and edge ranges contiguous.
	nmdRenownResult_t configureGraph( const nmdRenownLocation_t *pLocations, uint32_t nLocationCount, const nmdRenownEdge_t *pEdges, uint32_t nEdgeCount );

	// Globally monotonic incident IDs: preserves dedup across journal compaction.
	// Incident time must match the world minute last advanced by the calendar.
	nmdRenownResult_t submitIncident( const nmdRenownIncident_t &incident );

	// Later witness reports may increase confidence while incident is retained.
	nmdRenownResult_t reportIncident( uint64_t nIncidentId, uint16_t nVisibilityQ8, uint16_t nAttributionQ8 );

	// Process rumor deliveries in chronological order, then closed-form decay.
	nmdRenownResult_t advanceToWorldMinute( uint32_t nWorldMinute );

	uint32_t getRenownQ8( uint32_t nSubjectId, uint32_t nLocationId ) const;
	uint16_t getRenown( uint32_t nSubjectId, uint32_t nLocationId ) const;
	nmdRenownTier_t getTier( uint32_t nSubjectId, uint32_t nLocationId ) const;
	uint16_t getTraitEvidence( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nTraitId ) const;

private:
	static uint32_t scaleQ8( uint32_t nValue, uint32_t nFactorQ8 );
	static uint32_t addSaturated( uint32_t nA, uint32_t nB );
	static uint16_t addEvidence( uint16_t nCurrent, uint16_t nDelta, bool bNegative );
	uint16_t getCertainty( uint16_t nVisibilityQ8, uint16_t nAttributionQ8 ) const;

	nmdRenownResult_t applyReport( nmdRenownIncidentRecord_t &record, uint16_t nCertaintyQ8 );
	nmdRenownResult_t preflightContribution( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nScoreQ8, uint32_t nTraitId, uint16_t nTraitEvidence, bool bTraitNegative, uint32_t nSourceScoreQ8 ) const;
	void applyTraitChange( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nTraitId, uint16_t nMagnitude, bool bNegative, uint16_t nFlags, uint32_t nWorldDay );
	void applyRenownChange( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nAmountQ8, bool bHistorical, uint32_t nWorldDay );

	nmdRenownResult_t startBleed( uint32_t nSubjectId, uint32_t nSourceLocationId, uint32_t nSourceScoreQ8, uint32_t nScoreQ8, uint32_t nTraitId, uint16_t nTraitEvidence, bool bTraitNegative, uint8_t nRumorFlags );
	nmdRenownResult_t processRumor( const nmdRenownRumor_t &rumor );
	bool canDeliverRumor( const nmdRenownRumor_t &rumor ) const;
	bool hasPendingRumor( uint64_t nRumorId ) const;
	bool queueRumor( const nmdRenownRumor_t &rumor );
	bool popRumor( nmdRenownRumor_t &out );
	void heapifyRumors( void );
	void insertRumorHeap( uint32_t nIndex );
	void repairRumorHeap( uint32_t nIndex );
	static bool rumorLess( const nmdRenownRumor_t &a, const nmdRenownRumor_t &b );
	uint32_t computeTravelMinutes( const nmdRenownEdge_t &edge, uint32_t nSourceScoreQ8 ) const;

	void degradeProfilesToDay( uint32_t nWorldDay );
	void degradeTraitsToDay( uint32_t nWorldDay );
	static uint64_t cumulativeDecayQ8( uint32_t nDays, uint32_t nBaseQ8, uint32_t nAccelerationQ8 );

	NMDRenownBlackboard *m_pBlackboard;
	nmdRenownRules_t m_Rules;
};

#endif // __NMD_RENOWN_SYSTEM_H__
