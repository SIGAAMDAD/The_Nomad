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

#include "NMDRenownSystem.h"

uint32_t renownMix( uint32_t nHash, uint32_t nValue )
{
	for ( uint32_t i = 0; i < 4; ++i ) {
		nHash ^= ( nValue >> ( 8u * i ) ) & 255u;
		nHash *= 16777619u;
	}
	return nHash;
}

bool containsLocation( const nmdRenownLocation_t *pLocations, uint32_t nCount, uint32_t nId )
{
	uint32_t nLow = 0;
	uint32_t nHigh = nCount;
	while ( nLow < nHigh ) {
		const uint32_t nMid = nLow + ( nHigh - nLow ) / 2;
		if ( pLocations[ nMid ].nLocationId < nId ) {
			nLow = nMid + 1;
		} else {
			nHigh = nMid;
		}
	}
	return nLow < nCount && pLocations[ nLow ].nLocationId == nId;
}

uint16_t magnitude16( int16_t nValue )
{
	const int32_t nWide = nValue;
	return static_cast<uint16_t>( nWide < 0 ? -nWide : nWide );
}

NMDRenownSystem::NMDRenownSystem( void )
    : m_pBlackboard( NULL )
{
	m_Rules = getDefaultRules();
}

nmdRenownRules_t NMDRenownSystem::getDefaultRules()
{
	nmdRenownRules_t rules = {};
	rules.nDecayGraceDays = 7;
	rules.nDecayBaseQ8 = 64;         // 0.25 renown/day after grace.
	rules.nDecayAccelerationQ8 = 32; // Additional 0.125/day of inactivity.
	rules.nTraitGraceDays = 7;
	rules.nTraitDecayPerDay = 2;
	rules.nPatternInterval = 12;
	rules.nPatternBonusQ8 = 256;
	rules.nMaxRumorHops = 6;
	rules.nTierSpeedQ8[ 0 ] = 256;
	rules.nTierSpeedQ8[ 1 ] = 256;
	rules.nTierSpeedQ8[ 2 ] = 256;
	rules.nTierSpeedQ8[ 3 ] = 320;
	rules.nTierSpeedQ8[ 4 ] = 448;
	rules.nTierSpeedQ8[ 5 ] = 640;
	return rules;
}

nmdRenownTier_t NMDRenownSystem::getTierForScore( uint32_t nRenownQ8 )
{
	if ( nRenownQ8 >= 1500u * NMD_RENOWN_Q8_ONE ) {
		return NMD_RENOWN_TIER_THE_NOMAD;
	}
	if ( nRenownQ8 >= 900u * NMD_RENOWN_Q8_ONE ) {
		return NMD_RENOWN_TIER_WANDERING_WARRIOR;
	}
	if ( nRenownQ8 >= 500u * NMD_RENOWN_Q8_ONE ) {
		return NMD_RENOWN_TIER_LOCAL_LEGEND;
	}
	if ( nRenownQ8 >= 250u * NMD_RENOWN_Q8_ONE ) {
		return NMD_RENOWN_TIER_HERO_FROM_THE_HILLS;
	}
	if ( nRenownQ8 >= 50u * NMD_RENOWN_Q8_ONE ) {
		return NMD_RENOWN_TIER_TAVERN_TALK;
	}
	return NMD_RENOWN_TIER_WHISPER_IN_THE_WINDS;
}

uint32_t NMDRenownSystem::calculateRulesFingerprint( const nmdRenownRules_t& rules )
{
	uint32_t nHash = 2166136261u;
	nHash = renownMix( nHash, rules.nDecayGraceDays );
	nHash = renownMix( nHash, rules.nDecayBaseQ8 );
	nHash = renownMix( nHash, rules.nDecayAccelerationQ8 );
	nHash = renownMix( nHash, rules.nTraitGraceDays );
	nHash = renownMix( nHash, rules.nTraitDecayPerDay );
	nHash = renownMix( nHash, rules.nPatternInterval );
	nHash = renownMix( nHash, rules.nPatternBonusQ8 );
	nHash = renownMix( nHash, rules.nMaxRumorHops );
	for ( uint32_t i = 0; i < NMD_RENOWN_TIER_COUNT; ++i ) {
		nHash = renownMix( nHash, rules.nTierSpeedQ8[ i ] );
	}
	return nHash;
}

bool NMDRenownSystem::bind( NMDRenownBlackboard *pBlackboard,
                            const nmdRenownRules_t& rules )
{
	unbind();
	if ( pBlackboard == NULL || !pBlackboard->isBound() ||
	     rules.nPatternInterval == 0 || rules.nMaxRumorHops == 0 ) {
		return false;
	}
	for ( uint32_t i = 0; i < NMD_RENOWN_TIER_COUNT; ++i ) {
		if ( rules.nTierSpeedQ8[ i ] == 0 ) {
			return false;
		}
	}
	const uint32_t nHash = calculateRulesFingerprint( rules );
	const nmdRenownArenaHeader_t header = pBlackboard->getHeader();
	if ( header.nRulesFingerprint != 0 && header.nRulesFingerprint != nHash ) {
		return false;
	}
	m_Rules = rules;
	m_pBlackboard = pBlackboard;
	pBlackboard->setRulesFingerprint( nHash );
	return true;
}

void NMDRenownSystem::unbind()
{
	m_pBlackboard = NULL;
}

nmdRenownResult_t NMDRenownSystem::configureGraph( const nmdRenownLocation_t *pLocations,
                                                   uint32_t nLocationCount, const nmdRenownEdge_t *pEdges, uint32_t nEdgeCount )
{
	if ( m_pBlackboard == NULL || pLocations == NULL || nLocationCount == 0 ||
	     ( pEdges == NULL && nEdgeCount != 0 ) ) {
		return NMD_RENOWN_BAD_ARGUMENT;
	}
	const nmdRenownArenaHeader_t state = m_pBlackboard->getHeader();
	if ( state.nProfileCount != 0 || state.nTraitCount != 0 ||
	     state.nPatternCount != 0 || state.nIncidentCount != 0 ||
	     state.nRumorCount != 0 || state.nSeenCount != 0 ||
	     state.nLastIncidentId != 0 ) {
		return NMD_RENOWN_BAD_ARGUMENT;
	}
	uint32_t nNextEdge = 0;
	uint32_t nHash = 2166136261u;
	for ( uint32_t i = 0; i < nLocationCount; ++i ) {
		const nmdRenownLocation_t& location = pLocations[ i ];
		if ( location.nLocationId == 0 || ( i > 0 && pLocations[ i - 1 ].nLocationId >= location.nLocationId ) ||
		     location.nFirstEdge != nNextEdge ||
		     location.nEdgeCount > nEdgeCount - nNextEdge ) {
			return NMD_RENOWN_BAD_ARGUMENT;
		}
		nNextEdge += location.nEdgeCount;
		nHash = renownMix( nHash, location.nLocationId );
		nHash = renownMix( nHash, location.nEdgeCount );
		nHash = renownMix( nHash, location.nFlags );
	}
	if ( nNextEdge != nEdgeCount ) {
		return NMD_RENOWN_BAD_ARGUMENT;
	}
	for ( uint32_t i = 0; i < nEdgeCount; ++i ) {
		if ( pEdges[ i ].nTravelMinutes == 0 || pEdges[ i ].nTransmissionQ8 > 256 ||
		     !containsLocation( pLocations, nLocationCount,
		                        pEdges[ i ].nDestinationLocationId ) ) {
			return NMD_RENOWN_BAD_ARGUMENT;
		}
		nHash = renownMix( nHash, pEdges[ i ].nDestinationLocationId );
		nHash = renownMix( nHash, pEdges[ i ].nTravelMinutes );
		nHash = renownMix( nHash, pEdges[ i ].nTransmissionQ8 );
		nHash = renownMix( nHash, pEdges[ i ].nFlags );
	}
	if ( !m_pBlackboard->setLocationGraph( pLocations, nLocationCount,
	                                       pEdges, nEdgeCount, nHash ) ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	return NMD_RENOWN_OK;
}

uint32_t NMDRenownSystem::scaleQ8( uint32_t nValue, uint32_t nFactorQ8 )
{
	const uint64_t nResult = ( static_cast<uint64_t>( nValue ) * nFactorQ8 + 128u ) / 256u;
	return nResult > UINT32_MAX ? UINT32_MAX : static_cast<uint32_t>( nResult );
}

uint32_t NMDRenownSystem::addSaturated( uint32_t nA, uint32_t nB )
{
	return nB > UINT32_MAX - nA ? UINT32_MAX : nA + nB;
}

uint16_t NMDRenownSystem::addEvidence( uint16_t nCurrent, uint16_t nDelta,
                                       bool bNegative )
{
	if ( bNegative ) {
		return nDelta >= nCurrent ? 0 : static_cast<uint16_t>( nCurrent - nDelta );
	}
	const uint32_t nSum = static_cast<uint32_t>( nCurrent ) + nDelta;
	return static_cast<uint16_t>( nSum > UINT16_MAX ? UINT16_MAX : nSum );
}

uint16_t NMDRenownSystem::getCertainty( uint16_t nVisibilityQ8,
                                        uint16_t nAttributionQ8 ) const
{
	const uint32_t nProduct = static_cast<uint32_t>( nVisibilityQ8 ) * nAttributionQ8;
	return static_cast<uint16_t>( ( nProduct + 128u ) / 256u );
}

uint32_t NMDRenownSystem::getRenownQ8( uint32_t nSubjectId, uint32_t nLocationId ) const
{
	nmdRenownProfile_t profile = {};
	return m_pBlackboard != NULL && m_pBlackboard->getProfile( nSubjectId,
	                                                           nLocationId, profile )
	           ? profile.nRenownQ8
	           : 0;
}

uint16_t NMDRenownSystem::getRenown( uint32_t nSubjectId, uint32_t nLocationId ) const
{
	const uint32_t nInteger = getRenownQ8( nSubjectId, nLocationId ) >> 8;
	return static_cast<uint16_t>( nInteger > UINT16_MAX ? UINT16_MAX : nInteger );
}

nmdRenownTier_t NMDRenownSystem::getTier( uint32_t nSubjectId, uint32_t nLocationId ) const
{
	return getTierForScore( getRenownQ8( nSubjectId, nLocationId ) );
}

uint16_t NMDRenownSystem::getTraitEvidence( uint32_t nSubjectId,
                                            uint32_t nLocationId, uint32_t nTraitId ) const
{
	nmdRenownTrait_t trait = {};
	return m_pBlackboard != NULL && m_pBlackboard->getTrait( nSubjectId,
	                                                         nLocationId, nTraitId, trait )
	           ? trait.nEvidence
	           : 0;
}

nmdRenownResult_t NMDRenownSystem::preflightContribution( uint32_t nSubjectId,
                                                          uint32_t nLocationId, uint32_t nScoreQ8, uint32_t nTraitId,
                                                          uint16_t nTraitEvidence, bool bTraitNegative, uint32_t nSourceScoreQ8 ) const
{
	if ( nScoreQ8 != 0 && !m_pBlackboard->canSetProfile( nSubjectId, nLocationId ) ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	if ( nTraitId != 0 && nTraitEvidence != 0 && !bTraitNegative &&
	     !m_pBlackboard->canSetTrait( nSubjectId, nLocationId, nTraitId ) ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	nmdRenownLocation_t location = {};
	if ( !m_pBlackboard->getLocation( nLocationId, location ) ) {
		return NMD_RENOWN_INVALID_LOCATION;
	}
	if ( nSourceScoreQ8 < 500u * 256u ||
	     ( nScoreQ8 == 0 && nTraitEvidence == 0 ) || location.nEdgeCount == 0 ) {
		return NMD_RENOWN_OK;
	}
	const nmdRenownCapacities_t& capacity = m_pBlackboard->getCapacities();
	if ( location.nEdgeCount > capacity.nRumors ||
	     m_pBlackboard->getRumorCount() > capacity.nRumors - location.nEdgeCount ||
	     m_pBlackboard->getSeenCount() >= capacity.nSeen ||
	     m_pBlackboard->getHeader().nLastRumorId == UINT64_MAX ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	return NMD_RENOWN_OK;
}

void NMDRenownSystem::applyRenownChange( uint32_t nSubjectId, uint32_t nLocationId,
                                         uint32_t nAmountQ8, bool bHistorical, uint32_t nWorldDay )
{
	nmdRenownProfile_t profile = {};
	if ( !m_pBlackboard->getProfile( nSubjectId, nLocationId, profile ) ) {
		profile.nSubjectId = nSubjectId;
		profile.nLocationId = nLocationId;
	}
	profile.nRenownQ8 = addSaturated( profile.nRenownQ8, nAmountQ8 );
	profile.nLastGainDay = nWorldDay;
	profile.nLastUpdateDay = nWorldDay;
	++profile.nRevision;
	if ( bHistorical ) {
		static const uint32_t s_TierFloorsQ8[ NMD_RENOWN_TIER_COUNT ] = {
			0u, 50u * 256u, 250u * 256u, 500u * 256u,
			900u * 256u, 1500u * 256u
		};
		const uint32_t nTier = static_cast<uint32_t>( getTierForScore( profile.nRenownQ8 ) );
		if ( s_TierFloorsQ8[ nTier ] > profile.nHistoricalFloorQ8 ) {
			profile.nHistoricalFloorQ8 = s_TierFloorsQ8[ nTier ];
		}
	}
	m_pBlackboard->setProfile( profile );
}

void NMDRenownSystem::applyTraitChange( uint32_t nSubjectId, uint32_t nLocationId, uint32_t nTraitId, uint16_t nMagnitude, bool bNegative, uint16_t nFlags, uint32_t nWorldDay )
{
	if ( nTraitId == 0 || nMagnitude == 0 ) {
		return;
	}

	nmdRenownTrait_t trait = {};
	if ( !m_pBlackboard->getTrait( nSubjectId, nLocationId, nTraitId, trait ) ) {
		if ( bNegative ) {
			return;
		}
		trait.nSubjectId = nSubjectId;
		trait.nLocationId = nLocationId;
		trait.nTraitId = nTraitId;
	}

	const uint16_t nNextEvidence = addEvidence( trait.nEvidence, nMagnitude, bNegative );
	if ( nNextEvidence == trait.nEvidence && nFlags == 0 ) {
		return;
	}

	trait.nEvidence = nNextEvidence;
	trait.nLastUpdateDay = nWorldDay;

	if ( !bNegative ) {
		trait.nLastGainDay = nWorldDay;
		trait.nFlags |= nFlags;
	}

	m_pBlackboard->setTrait( trait );
}

nmdRenownResult_t NMDRenownSystem::applyReport( nmdRenownIncidentRecord_t& record, uint16_t nCertaintyQ8 )
{
	if ( nCertaintyQ8 <= record.nAppliedCertaintyQ8 ) {
		return NMD_RENOWN_NO_CHANGE;
	}

	const nmdRenownIncident_t& event = record.incident;
	const uint32_t nTotalScore = addSaturated( event.nBaseRenownQ8, record.nPatternBonusQ8 );
	const uint32_t nTargetScore = scaleQ8( nTotalScore, nCertaintyQ8 );
	const uint32_t nDeltaScore = nTargetScore - record.nAppliedRenownQ8;
	const uint16_t nTargetEvidence = static_cast<uint16_t>( scaleQ8( magnitude16( event.nTraitEvidence ), nCertaintyQ8 ) );
	const uint16_t nDeltaEvidence = static_cast<uint16_t>( nTargetEvidence - record.nAppliedTraitMagnitude );

	const uint32_t nLocalScore = addSaturated( getRenownQ8( event.nSubjectId, event.nLocationId ), nDeltaScore );
	const nmdRenownResult_t nPreflight = preflightContribution( event.nSubjectId, event.nLocationId, nDeltaScore, event.nTraitId, nDeltaEvidence, event.nTraitEvidence < 0, nLocalScore );
	if ( nPreflight != NMD_RENOWN_OK ) {
		return nPreflight;
	}
	const uint32_t nDay = m_pBlackboard->getWorldMinute() / NMD_RENOWN_MINUTES_PER_DAY;
	if ( nDeltaScore != 0 ) {
		applyRenownChange( event.nSubjectId, event.nLocationId, nDeltaScore,
		                   ( event.nFlags & NMD_RENOWN_INCIDENT_HISTORICAL ) != 0, nDay );
	}
	if ( nDeltaEvidence != 0 ) {
		const uint16_t nTraitFlags = ( event.nFlags & NMD_RENOWN_INCIDENT_TRAIT_PERMANENT ) != 0 ? NMD_RENOWN_TRAIT_PERMANENT : 0;
		applyTraitChange( event.nSubjectId, event.nLocationId, event.nTraitId,
		                  nDeltaEvidence, event.nTraitEvidence < 0, nTraitFlags, nDay );
	}
	if ( nLocalScore >= 500u * 256u && ( nDeltaScore != 0 || nDeltaEvidence != 0 ) ) {
		uint8_t nRumorFlags = NMD_RENOWN_RUMOR_NONE;
		if ( ( event.nFlags & NMD_RENOWN_INCIDENT_HISTORICAL ) != 0 ) {
			nRumorFlags |= NMD_RENOWN_RUMOR_HISTORICAL;
		}
		if ( ( event.nFlags & NMD_RENOWN_INCIDENT_TRAIT_PERMANENT ) != 0 ) {
			nRumorFlags |= NMD_RENOWN_RUMOR_TRAIT_PERMANENT;
		}
		const nmdRenownResult_t nBleed = startBleed( event.nSubjectId, event.nLocationId, nLocalScore, nDeltaScore, event.nTraitId, nDeltaEvidence, event.nTraitEvidence < 0, nRumorFlags );
		if ( nBleed != NMD_RENOWN_OK ) {
			return nBleed;
		}
	}
	record.nAppliedRenownQ8 = nTargetScore;
	record.nAppliedTraitMagnitude = nTargetEvidence;
	record.nAppliedCertaintyQ8 = nCertaintyQ8;
	return NMD_RENOWN_OK;
}

nmdRenownResult_t NMDRenownSystem::submitIncident( const nmdRenownIncident_t& incident )
{
	if ( m_pBlackboard == NULL || incident.nIncidentId == 0 || incident.nSubjectId == 0 ||
	     incident.nType >= NMD_RENOWN_INCIDENT_TYPE_COUNT ||
	     incident.nVisibilityQ8 > 256 || incident.nAttributionQ8 > 256 ||
	     ( incident.nTraitEvidence != 0 && incident.nTraitId == 0 ) ) {
		return NMD_RENOWN_BAD_ARGUMENT;
	}
	if ( incident.nWorldMinute != m_pBlackboard->getWorldMinute() ) {
		return NMD_RENOWN_TIME_REVERSED;
	}
	if ( incident.nIncidentId <= m_pBlackboard->getHeader().nLastIncidentId ) {
		return NMD_RENOWN_DUPLICATE_INCIDENT;
	}
	nmdRenownLocation_t location = {};
	if ( !m_pBlackboard->getLocation( incident.nLocationId, location ) ) {
		return NMD_RENOWN_INVALID_LOCATION;
	}
	if ( !m_pBlackboard->canSetPattern( incident.nSubjectId,
	                                    incident.nLocationId, incident.nType ) ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}

	nmdRenownPattern_t pattern = {};
	if ( !m_pBlackboard->getPattern( incident.nSubjectId,
	                                 incident.nLocationId, incident.nType, pattern ) ) {
		pattern.nSubjectId = incident.nSubjectId;
		pattern.nLocationId = incident.nLocationId;
		pattern.nIncidentType = incident.nType;
	}
	if ( pattern.nIncidentCount == UINT32_MAX ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	++pattern.nIncidentCount;
	pattern.nLastIncidentDay = incident.nWorldMinute / NMD_RENOWN_MINUTES_PER_DAY;

	nmdRenownIncidentRecord_t record = {};
	record.incident = incident;
	record.nPatternBonusQ8 = pattern.nIncidentCount % m_Rules.nPatternInterval == 0 ? m_Rules.nPatternBonusQ8 : 0;
	const uint16_t nCertainty = getCertainty( incident.nVisibilityQ8,
	                                          incident.nAttributionQ8 );
	const uint32_t nScore = scaleQ8( addSaturated( incident.nBaseRenownQ8,
	                                               record.nPatternBonusQ8 ),
	                                 nCertainty );
	const uint16_t nEvidence = static_cast<uint16_t>( scaleQ8(
	    magnitude16( incident.nTraitEvidence ), nCertainty ) );
	const uint32_t nSourceScore = addSaturated( getRenownQ8( incident.nSubjectId,
	                                                         incident.nLocationId ),
	                                            nScore );
	if ( m_pBlackboard->getCapacities().nIncidents == 0 ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	const nmdRenownResult_t nPreflight = preflightContribution( incident.nSubjectId,
	                                                            incident.nLocationId, nScore, incident.nTraitId, nEvidence,
	                                                            incident.nTraitEvidence < 0, nSourceScore );
	if ( nPreflight != NMD_RENOWN_OK ) {
		return nPreflight;
	}

	// Incident sequence and pattern mutation are committed after preflight.
	m_pBlackboard->setPattern( pattern );
	const nmdRenownResult_t nApplied = applyReport( record, nCertainty );
	if ( nApplied != NMD_RENOWN_OK && nApplied != NMD_RENOWN_NO_CHANGE ) {
		return nApplied;
	}
	if ( !m_pBlackboard->appendIncident( record ) ) {
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	return NMD_RENOWN_OK;
}

nmdRenownResult_t NMDRenownSystem::reportIncident( uint64_t nIncidentId, uint16_t nVisibilityQ8, uint16_t nAttributionQ8 )
{
	if ( m_pBlackboard == NULL || nVisibilityQ8 > 256 || nAttributionQ8 > 256 ) {
		return NMD_RENOWN_BAD_ARGUMENT;
	}

	nmdRenownIncidentRecord_t record = {};
	uint32_t nIndex = 0;
	if ( !m_pBlackboard->findIncident( nIncidentId, record, nIndex ) ) {
		return NMD_RENOWN_MISSING_INCIDENT;
	}

	const nmdRenownResult_t nResult = applyReport( record, getCertainty( nVisibilityQ8, nAttributionQ8 ) );
	if ( nResult == NMD_RENOWN_OK ) {
		m_pBlackboard->setIncidentAt( nIndex, record );
	}

	return nResult;
}
