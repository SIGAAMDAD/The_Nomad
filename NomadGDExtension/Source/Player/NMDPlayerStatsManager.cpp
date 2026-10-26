#include "NMDPlayerStatsManager.h"

NMDPlayerStatsManager::NMDPlayerStatsManager( void )
{
	m_szBaseToDerived[ NMD_PLAYER_BASE_STAT_INVENTORY_WEIGHT ][ 0 ] = NMD_PLAYER_DERIVED_STAT_MOVEMENT_SPEED_MULTIPLIER;
}

NMDPlayerStatsManager::~NMDPlayerStatsManager()
{
}

void NMDPlayerStatsManager::setBaseStatValue( nmdPlayerStat_t eStat, float fValue )
{
	if ( eStat < NMD_PLAYER_BASE_STAT_MIN || eStat > NMD_PLAYER_BASE_STAT_MAX ) {
		return;
	}

	float *pStat = &m_szPlayerBaseStats[ eStat ];
	if ( *pStat == fValue ) {
		return;
	}

	const float fOldValue = *pStat;
	*pStat = fValue;
}
