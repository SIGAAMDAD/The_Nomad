#ifndef __NMD_PLAYER_STATS_MANAGER_H__
#define __NMD_PLAYER_STATS_MANAGER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

typedef enum nmdPlayerStat_e : uint8_t
{
	NMD_PLAYER_BASE_STAT_MIN = 0,

	NMD_PLAYER_BASE_STAT_BASE_MOVEMENT_SPEED = NMD_PLAYER_BASE_STAT_MIN,
	NMD_PLAYER_BASE_STAT_INVENTORY_WEIGHT,
	NMD_PLAYER_BASE_STAT_ENCUMBRANCE_THRESHOLD,
	NMD_PLAYER_BASE_STAT_BASE_DASH_SPEED,
	NMD_PLAYER_BASE_STAT_BASE_HEALTH,
	NMD_PLAYER_BASE_STAT_BASE_RAGE,
	NMD_PLAYER_BASE_STAT_BASE_SANITY,

	NMD_PLAYER_BASE_STAT_MAX = NMD_PLAYER_BASE_STAT_BASE_SANITY,

	NMD_PLAYER_BASE_STAT_COUNT,

	NMD_PLAYER_DERIVED_STAT_MIN = 0,

	NMD_PLAYER_DERIVED_STAT_MOVEMENT_SPEED_MULTIPLIER = NMD_PLAYER_DERIVED_STAT_MIN,
	NMD_PLAYER_DERIVED_STAT_EFFECTIVE_MOVEMENT_SPEED,
	NMD_PLAYER_DERIVED_STAT_DASH_SPEED_MULTIPLIER,
	NMD_PLAYER_DERIVED_STAT_EFFECTIVE_DASH_SPEED,
	NMD_PLAYER_DERIVED_STAT_EFFECTIVE_HEALTH_MAX,
	NMD_PLAYER_DERIVED_STAT_EFFECTIVE_RAGE_MAX,
	NMD_PLAYER_DERIVED_STAT_EFFECTIVE_SANITY_MAX,
	NMD_PLAYER_DERIVED_STAT_SANITY_DRAIN_MULTIPLIER,
	NMD_PLAYER_DERIVED_STAT_BULLET_TIME_DRAIN_RATE,

	NMD_PLAYER_DERIVED_STAT_MAX = NMD_PLAYER_DERIVED_STAT_BULLET_TIME_DRAIN_RATE,

	NMD_PLAYER_DERIVED_STAT_COUNT
} nmdPlayerStat_t;

typedef struct alignas( 64 ) nmdPlayerStatsHot_s
{
	float fHealth;
	float fRage;
	float fMovementSpeed;
	float fDashSpeed;
	float fInventoryWeight;

	float fMaxHealth;
	float fMaxRage;

	float fSanity;
	float fMaxSanity;

	float fBaseMovementSpeed;
	float fBaseDashSpeed;
	float fEncumbranceThreshold;

	float fMovementSpeedMultiplier;
	float fDashSpeedMultiplier;

	const uint16_t iDirtyDerivedMask = static_cast<uint16_t>( ( 1U << NMD_PLAYER_DERIVED_STAT_COUNT ) - 1U );
} nmdPlayerStatsHot_t;

static_assert( sizeof( nmdPlayerStatsHot_t ) == 64 );
static_assert( alignof( nmdPlayerStatsHot_t ) == 64 );

typedef struct nmdPlayerStatsCold_s
{
	float fBaseHealth;
	float fBaseRage;
	float fBaseSanity;

	float fSanityDrainMultiplier;
	float fBulletTimeDrainRate;
} nmdPlayerStatsCold_t;

typedef uint16_t nmdPlayerStatMask_t;

static_assert(
	NMD_PLAYER_DERIVED_STAT_COUNT <= 16,
	"Derived stat count exceeds bitmask capacity (16)"
);

#define NMD_DERIVED_STAT_BIT( stat ) ( static_cast<nmdPlayerStatMask_t>( 1u << static_cast<uint8_t>( stat ) ) )

class NMDPlayerStatsManager
{
public:
	NMDPlayerStatsManager( void );
	~NMDPlayerStatsManager();

	float getBaseStat( nmdPlayerStat_t eStat ) const;
	void setBaseStatValue( nmdPlayerStat_t eStat, float fValue );

private:
	eastl::array<float, NMD_PLAYER_BASE_STAT_COUNT> m_szStatValues;

	nmdPlayerStat_t m_szBaseToDerived[ NMD_PLAYER_BASE_STAT_COUNT ][ NMD_PLAYER_DERIVED_STAT_COUNT ];
	nmdPlayerStat_t m_szDerivedToDerived[ NMD_PLAYER_DERIVED_STAT_COUNT ][ NMD_PLAYER_DERIVED_STAT_COUNT ];

	//
	// dependency graphs
	//

	float m_szPlayerBaseStats[ NMD_PLAYER_BASE_STAT_COUNT ];
	float m_szPlayerDerivedStats[ NMD_PLAYER_DERIVED_STAT_COUNT ];
};

#endif // __NMD_PLAYER_STATS_MANAGER_H__
