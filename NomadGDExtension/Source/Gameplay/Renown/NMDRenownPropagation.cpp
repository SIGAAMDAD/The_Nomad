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

#include <limits.h>

bool NMDRenownSystem::rumorLess( const nmdRenownRumor_t &a, const nmdRenownRumor_t &b )
{
	if ( a.nDeliveryMinute != b.nDeliveryMinute )
	{
		return a.nDeliveryMinute < b.nDeliveryMinute;
	}
	if ( a.nRumorId != b.nRumorId )
	{
		return a.nRumorId < b.nRumorId;
	}
	return a.nDestinationLocationId < b.nDestinationLocationId;
}

bool NMDRenownSystem::queueRumor( const nmdRenownRumor_t &rumor )
{
	if ( !m_pBlackboard->appendRumor( rumor ) )
	{
		return false;
	}
	insertRumorHeap( m_pBlackboard->getRumorCount() - 1 );
	return true;
}

void NMDRenownSystem::insertRumorHeap( uint32_t nIndex )
{
	while ( nIndex != 0 )
	{
		const uint32_t nParent = ( nIndex - 1 ) / 2;
		nmdRenownRumor_t parent = {};
		nmdRenownRumor_t child = {};
		m_pBlackboard->getRumorAt( nParent, parent );
		m_pBlackboard->getRumorAt( nIndex, child );
		if ( !rumorLess( child, parent ) )
		{
			break;
		}
		m_pBlackboard->setRumorAt( nParent, child );
		m_pBlackboard->setRumorAt( nIndex, parent );
		nIndex = nParent;
	}
}

void NMDRenownSystem::repairRumorHeap( uint32_t nIndex )
{
	const uint32_t nCount = m_pBlackboard->getRumorCount();
	while ( nIndex < nCount )
	{
		const uint32_t nLeft = nIndex * 2u + 1u;
		if ( nLeft >= nCount )
		{
			return;
		}
		const uint32_t nRight = nLeft + 1u;
		uint32_t nBest = nLeft;
		nmdRenownRumor_t left = {};
		nmdRenownRumor_t right = {};
		m_pBlackboard->getRumorAt( nLeft, left );
		if ( nRight < nCount )
		{
			m_pBlackboard->getRumorAt( nRight, right );
			if ( rumorLess( right, left ) )
			{
				nBest = nRight;
			}
		}
		nmdRenownRumor_t parent = {};
		nmdRenownRumor_t child = {};
		m_pBlackboard->getRumorAt( nIndex, parent );
		m_pBlackboard->getRumorAt( nBest, child );
		if ( !rumorLess( child, parent ) )
		{
			return;
		}
		m_pBlackboard->setRumorAt( nIndex, child );
		m_pBlackboard->setRumorAt( nBest, parent );
		nIndex = nBest;
	}
}

void NMDRenownSystem::heapifyRumors()
{
	const uint32_t nCount = m_pBlackboard->getRumorCount();
	for ( uint32_t i = nCount / 2; i != 0; --i )
	{
		repairRumorHeap( i - 1 );
	}
}

bool NMDRenownSystem::popRumor( nmdRenownRumor_t &out )
{
	if ( !m_pBlackboard->getRumorAt( 0, out ) )
	{
		return false;
	}
	const uint32_t nCount = m_pBlackboard->getRumorCount();
	if ( nCount != 1 )
	{
		nmdRenownRumor_t last = {};
		m_pBlackboard->getRumorAt( nCount - 1, last );
		m_pBlackboard->setRumorAt( 0, last );
	}
	m_pBlackboard->removeLastRumor();
	if ( nCount != 1 )
	{
		repairRumorHeap( 0 );
	}
	return true;
}

bool NMDRenownSystem::hasPendingRumor( uint64_t nRumorId ) const
{
	for ( uint32_t i = 0; i < m_pBlackboard->getRumorCount(); ++i )
	{
		nmdRenownRumor_t rumor = {};
		m_pBlackboard->getRumorAt( i, rumor );
		if ( rumor.nRumorId == nRumorId )
		{
			return true;
		}
	}
	return false;
}

uint32_t NMDRenownSystem::computeTravelMinutes( const nmdRenownEdge_t &edge,
	uint32_t nSourceScoreQ8 ) const
{
	const uint32_t nTier = static_cast<uint32_t>( getTierForScore( nSourceScoreQ8 ) );
	const uint32_t nSpeedQ8 = m_Rules.nTierSpeedQ8[nTier];
	const uint64_t nScaled = ( static_cast<uint64_t>( edge.nTravelMinutes ) * 256u +
		nSpeedQ8 - 1u ) / nSpeedQ8;
	return static_cast<uint32_t>( nScaled > UINT32_MAX ? UINT32_MAX :
		( nScaled == 0 ? 1 : nScaled ) );
}

nmdRenownResult_t NMDRenownSystem::startBleed( uint32_t nSubjectId,
	uint32_t nSourceLocationId, uint32_t nSourceScoreQ8, uint32_t nScoreQ8,
	uint32_t nTraitId, uint16_t nTraitEvidence, bool bTraitNegative,
	uint8_t nRumorFlags )
{
	nmdRenownLocation_t source = {};
	if ( !m_pBlackboard->getLocation( nSourceLocationId, source ) )
	{
		return NMD_RENOWN_INVALID_LOCATION;
	}
	if ( source.nEdgeCount == 0 )
	{
		return NMD_RENOWN_OK;
	}
	const uint64_t nRumorId = m_pBlackboard->reserveRumorId();
	if ( nRumorId == 0 )
	{
		return NMD_RENOWN_CAPACITY_EXCEEDED;
	}
	nmdRenownSeen_t origin = {};
	origin.nRumorId = nRumorId;
	origin.nLocationId = nSourceLocationId;
	origin.nAppliedRenownQ8 = nScoreQ8;
	origin.nAppliedTraitEvidence = nTraitEvidence;
	origin.nBestHopCount = 0;
	m_pBlackboard->appendSeen( origin );

	for ( uint32_t i = 0; i < source.nEdgeCount; ++i )
	{
		nmdRenownEdge_t edge = {};
		m_pBlackboard->getEdgeByIndex( source.nFirstEdge + i, edge );
		nmdRenownRumor_t rumor = {};
		rumor.nRumorId = nRumorId;
		rumor.nSubjectId = nSubjectId;
		rumor.nDestinationLocationId = edge.nDestinationLocationId;
		rumor.nRenownQ8 = scaleQ8( nScoreQ8, edge.nTransmissionQ8 );
		rumor.nTraitId = nTraitId;
		rumor.nTraitEvidence = static_cast<uint16_t>(
			scaleQ8( nTraitEvidence, edge.nTransmissionQ8 ) );
		rumor.nTraitNegative = bTraitNegative ? 1 : 0;
		rumor.nFlags = nRumorFlags;
		rumor.nHopCount = 1;
		const uint32_t nTravel = computeTravelMinutes( edge, nSourceScoreQ8 );
		rumor.nDeliveryMinute = addSaturated( m_pBlackboard->getWorldMinute(), nTravel );
		if ( rumor.nRenownQ8 != 0 || rumor.nTraitEvidence != 0 )
		{
			queueRumor( rumor );
		}
	}
	if ( !hasPendingRumor( nRumorId ) )
	{
		m_pBlackboard->removeSeenForRumor( nRumorId );
	}
	return NMD_RENOWN_OK;
}

bool NMDRenownSystem::canDeliverRumor( const nmdRenownRumor_t &rumor ) const
{
	nmdRenownSeen_t seen = {};
	uint32_t nIndex = 0;
	const bool bExists = m_pBlackboard->findSeen( rumor.nRumorId,
		rumor.nDestinationLocationId, seen, nIndex );
	const uint32_t nDeltaScore = rumor.nRenownQ8 > seen.nAppliedRenownQ8 ?
		rumor.nRenownQ8 - seen.nAppliedRenownQ8 : 0;
	const uint16_t nDeltaTrait = rumor.nTraitEvidence > seen.nAppliedTraitEvidence ?
		static_cast<uint16_t>( rumor.nTraitEvidence - seen.nAppliedTraitEvidence ) : 0;
	const bool bBetterPath = !bExists || rumor.nHopCount < seen.nBestHopCount;
	if ( nDeltaScore == 0 && nDeltaTrait == 0 && !bBetterPath )
	{
		return true;
	}
	const nmdRenownCapacities_t &capacity = m_pBlackboard->getCapacities();
	if ( !bExists && m_pBlackboard->getSeenCount() >= capacity.nSeen )
	{
		return false;
	}
	if ( nDeltaScore != 0 && !m_pBlackboard->canSetProfile( rumor.nSubjectId,
		rumor.nDestinationLocationId ) )
	{
		return false;
	}
	if ( nDeltaTrait != 0 && rumor.nTraitId != 0 && rumor.nTraitNegative == 0 &&
		!m_pBlackboard->canSetTrait( rumor.nSubjectId,
			rumor.nDestinationLocationId, rumor.nTraitId ) )
	{
		return false;
	}
	if ( rumor.nHopCount >= m_Rules.nMaxRumorHops )
	{
		return true;
	}
	nmdRenownLocation_t location = {};
	if ( !m_pBlackboard->getLocation( rumor.nDestinationLocationId, location ) )
	{
		return false;
	}
	// The incoming packet frees one heap slot before forwarding begins.
	return location.nEdgeCount <= capacity.nRumors &&
		m_pBlackboard->getRumorCount() - 1u <= capacity.nRumors - location.nEdgeCount;
}

nmdRenownResult_t NMDRenownSystem::processRumor( const nmdRenownRumor_t &rumor )
{
	nmdRenownSeen_t seen = {};
	uint32_t nSeenIndex = 0;
	const bool bSeen = m_pBlackboard->findSeen( rumor.nRumorId,
		rumor.nDestinationLocationId, seen, nSeenIndex );
	const uint32_t nDeltaScore = rumor.nRenownQ8 > seen.nAppliedRenownQ8 ?
		rumor.nRenownQ8 - seen.nAppliedRenownQ8 : 0;
	const uint16_t nDeltaTrait = rumor.nTraitEvidence > seen.nAppliedTraitEvidence ?
		static_cast<uint16_t>( rumor.nTraitEvidence - seen.nAppliedTraitEvidence ) : 0;
	const bool bBetterPath = !bSeen || rumor.nHopCount < seen.nBestHopCount;
	if ( nDeltaScore == 0 && nDeltaTrait == 0 && !bBetterPath )
	{
		return NMD_RENOWN_NO_CHANGE;
	}
	const uint32_t nDay = m_pBlackboard->getWorldMinute() / NMD_RENOWN_MINUTES_PER_DAY;
	if ( nDeltaScore != 0 )
	{
		applyRenownChange( rumor.nSubjectId, rumor.nDestinationLocationId,
			nDeltaScore, ( rumor.nFlags & NMD_RENOWN_RUMOR_HISTORICAL ) != 0, nDay );
	}
	if ( nDeltaTrait != 0 )
	{
		const uint16_t nFlags = ( rumor.nFlags & NMD_RENOWN_RUMOR_TRAIT_PERMANENT ) != 0 ?
			NMD_RENOWN_TRAIT_PERMANENT : 0;
		applyTraitChange( rumor.nSubjectId, rumor.nDestinationLocationId,
			rumor.nTraitId, nDeltaTrait, rumor.nTraitNegative != 0, nFlags, nDay );
	}
	seen.nRumorId = rumor.nRumorId;
	seen.nLocationId = rumor.nDestinationLocationId;
	if ( rumor.nRenownQ8 > seen.nAppliedRenownQ8 )
	{
		seen.nAppliedRenownQ8 = rumor.nRenownQ8;
	}
	if ( rumor.nTraitEvidence > seen.nAppliedTraitEvidence )
	{
		seen.nAppliedTraitEvidence = rumor.nTraitEvidence;
	}
	if ( bBetterPath )
	{
		seen.nBestHopCount = rumor.nHopCount;
	}
	if ( bSeen )
	{
		m_pBlackboard->setSeenAt( nSeenIndex, seen );
	}
	else
	{
		m_pBlackboard->appendSeen( seen );
	}

	if ( rumor.nHopCount >= m_Rules.nMaxRumorHops )
	{
		return NMD_RENOWN_OK;
	}
	nmdRenownLocation_t location = {};
	m_pBlackboard->getLocation( rumor.nDestinationLocationId, location );
	const uint32_t nLocalScore = getRenownQ8( rumor.nSubjectId, rumor.nDestinationLocationId );
	for ( uint32_t i = 0; i < location.nEdgeCount; ++i )
	{
		nmdRenownEdge_t edge = {};
		m_pBlackboard->getEdgeByIndex( location.nFirstEdge + i, edge );
		nmdRenownRumor_t next = rumor;
		next.nDestinationLocationId = edge.nDestinationLocationId;
		next.nRenownQ8 = scaleQ8( rumor.nRenownQ8, edge.nTransmissionQ8 );
		next.nTraitEvidence = static_cast<uint16_t>(
			scaleQ8( rumor.nTraitEvidence, edge.nTransmissionQ8 ) );
		++next.nHopCount;
		const uint32_t nTravel = computeTravelMinutes( edge, nLocalScore );
		next.nDeliveryMinute = addSaturated( m_pBlackboard->getWorldMinute(), nTravel );
		if ( next.nRenownQ8 != 0 || next.nTraitEvidence != 0 )
		{
			queueRumor( next );
		}
	}
	return NMD_RENOWN_OK;
}

nmdRenownResult_t NMDRenownSystem::advanceToWorldMinute( uint32_t nWorldMinute )
{
	if ( m_pBlackboard == NULL )
	{
		return NMD_RENOWN_BAD_ARGUMENT;
	}
	if ( nWorldMinute < m_pBlackboard->getWorldMinute() )
	{
		return NMD_RENOWN_TIME_REVERSED;
	}
	while ( m_pBlackboard->getRumorCount() != 0 )
	{
		nmdRenownRumor_t rumor = {};
		m_pBlackboard->getRumorAt( 0, rumor );
		if ( rumor.nDeliveryMinute > nWorldMinute )
		{
			break;
		}
		if ( !canDeliverRumor( rumor ) )
		{
			return NMD_RENOWN_CAPACITY_EXCEEDED;
		}
		const uint32_t nDeliveryDay = rumor.nDeliveryMinute / NMD_RENOWN_MINUTES_PER_DAY;
		degradeProfilesToDay( nDeliveryDay );
		degradeTraitsToDay( nDeliveryDay );
		m_pBlackboard->setWorldMinute( rumor.nDeliveryMinute );
		popRumor( rumor );
		processRumor( rumor );
		if ( !hasPendingRumor( rumor.nRumorId ) )
		{
			m_pBlackboard->removeSeenForRumor( rumor.nRumorId );
		}
	}
	const uint32_t nEndDay = nWorldMinute / NMD_RENOWN_MINUTES_PER_DAY;
	degradeProfilesToDay( nEndDay );
	degradeTraitsToDay( nEndDay );
	m_pBlackboard->setWorldMinute( nWorldMinute );
	return NMD_RENOWN_OK;
}
