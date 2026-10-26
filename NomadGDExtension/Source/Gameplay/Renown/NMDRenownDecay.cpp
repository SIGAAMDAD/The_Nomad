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

uint32_t inactiveDaysAfterGrace( uint32_t nToday, uint32_t nGainDay, uint32_t nGraceDays )
{
	if ( nToday <= nGainDay || nToday - nGainDay <= nGraceDays ) {
		return 0;
	}
	return nToday - nGainDay - nGraceDays;
}

// Computes accelerated daily loss over (nFrom, nTo] without daily loops.
uint32_t decayBetween( uint32_t nFrom, uint32_t nTo, uint32_t nBase, uint32_t nAcceleration )
{
	if ( nTo <= nFrom ) {
		return 0;
	}

	const uint64_t nCount = static_cast<uint64_t>( nTo - nFrom );
	const uint64_t nOldTriangle = static_cast<uint64_t>( nFrom ) * ( nFrom == 0 ? 0 : static_cast<uint64_t>( nFrom - 1 ) ) / 2;
	const uint64_t nNewTriangle = static_cast<uint64_t>( nTo ) * ( static_cast<uint64_t>( nTo ) - 1 ) / 2;
	const uint64_t nSteps = nNewTriangle - nOldTriangle;

	if ( nBase != 0 && nCount > UINT32_MAX / nBase ) {
		return UINT32_MAX;
	}
	if ( nAcceleration != 0 && nSteps > UINT32_MAX / nAcceleration ) {
		return UINT32_MAX;
	}

	const uint64_t nLoss = nCount * nBase + nSteps * nAcceleration;

	return nLoss > UINT32_MAX ? UINT32_MAX : static_cast<uint32_t>( nLoss );
}

uint64_t NMDRenownSystem::cumulativeDecayQ8( uint32_t nDays, uint32_t nBaseQ8, uint32_t nAccelerationQ8 )
{
	const uint64_t nTriangle = static_cast<uint64_t>( nDays ) * ( nDays == 0 ? 0 : static_cast<uint64_t>( nDays - 1 ) ) / 2;

	if ( nAccelerationQ8 != 0 && nTriangle > UINT64_MAX / nAccelerationQ8 ) {
		return UINT64_MAX;
	}

	const uint64_t nAccelerated = nTriangle * nAccelerationQ8;
	const uint64_t nBase = static_cast<uint64_t>( nDays ) * nBaseQ8;

	return nBase > UINT64_MAX - nAccelerated ? UINT64_MAX : nBase + nAccelerated;
}

void NMDRenownSystem::degradeProfilesToDay( uint32_t nWorldDay )
{
	for ( uint32_t i = 0; i < m_pBlackboard->getProfileCount(); ++i ) {
		nmdRenownProfile_t profile;
		memset( &profile, 0, sizeof( profile ) );

		m_pBlackboard->getProfileByIndex( i, profile );

		if ( nWorldDay <= profile.nLastUpdateDay ) {
			continue;
		}

		const uint32_t nOldInactive = inactiveDaysAfterGrace( profile.nLastUpdateDay, profile.nLastGainDay, m_Rules.nDecayGraceDays );
		const uint32_t nNewInactive = inactiveDaysAfterGrace( nWorldDay, profile.nLastGainDay, m_Rules.nDecayGraceDays );
		const uint32_t nLoss = decayBetween( nOldInactive, nNewInactive,
		                                     m_Rules.nDecayBaseQ8, m_Rules.nDecayAccelerationQ8 );

		if ( nLoss != 0 ) {
			const uint32_t nAvailable = profile.nRenownQ8 > profile.nHistoricalFloorQ8 ? profile.nRenownQ8 - profile.nHistoricalFloorQ8 : 0;
			profile.nRenownQ8 -= nLoss < nAvailable ? nLoss : nAvailable;
			++profile.nRevision;
		}

		profile.nLastUpdateDay = nWorldDay;
		m_pBlackboard->setProfile( profile );
	}
}

void NMDRenownSystem::degradeTraitsToDay( uint32_t nWorldDay )
{
	for ( uint32_t i = 0; i < m_pBlackboard->getTraitCount(); ++i ) {
		nmdRenownTrait_t trait = {};
		m_pBlackboard->getTraitByIndex( i, trait );
		if ( nWorldDay <= trait.nLastUpdateDay ) {
			continue;
		}
		if ( ( trait.nFlags & NMD_RENOWN_TRAIT_PERMANENT ) == 0 ) {
			const uint32_t nOldInactive = inactiveDaysAfterGrace(
			    trait.nLastUpdateDay, trait.nLastGainDay, m_Rules.nTraitGraceDays );
			const uint32_t nNewInactive = inactiveDaysAfterGrace(
			    nWorldDay, trait.nLastGainDay, m_Rules.nTraitGraceDays );
			const uint64_t nLoss = static_cast<uint64_t>(
			                           nNewInactive - nOldInactive ) *
			                       m_Rules.nTraitDecayPerDay;
			if ( nLoss != 0 ) {
				trait.nEvidence = nLoss >= trait.nEvidence ? 0 : static_cast<uint16_t>( trait.nEvidence - nLoss );
			}
		}
		trait.nLastUpdateDay = nWorldDay;
		m_pBlackboard->setTrait( trait );
	}
}
