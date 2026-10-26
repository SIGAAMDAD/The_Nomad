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

#ifndef __NMD_RENOWN_TYPES_H__
#define __NMD_RENOWN_TYPES_H__

#include <stdint.h>
#include <stddef.h>

#define NMD_RENOWN_Q8_ONE                   256u
#define NMD_RENOWN_MINUTES_PER_DAY          1440u
#define NMD_RENOWN_TIER_COUNT               6u
#define NMD_RENOWN_INVALID_ID               0u
#define NMD_RENOWN_STORAGE_VERSION          1u
#define NMD_RENOWN_STORAGE_MAGIC            0x4E4D4452u
#define NMD_RENOWN_SNAPSHOT_MAGIC           0x524E4D44u
#define NMD_RENOWN_SNAPSHOT_VERSION         1u

// Renown locations are compiled from NMD_LOCATION_FLAG_TRACK_RENOWN.
// The existing location flag's numeric bit is resolved at integration time.
typedef enum nmdRenownTier_e : uint8_t
{
	NMD_RENOWN_TIER_WHISPER_IN_THE_WINDS = 0,
	NMD_RENOWN_TIER_TAVERN_TALK,
	NMD_RENOWN_TIER_HERO_FROM_THE_HILLS,
	NMD_RENOWN_TIER_LOCAL_LEGEND,
	NMD_RENOWN_TIER_WANDERING_WARRIOR,
	NMD_RENOWN_TIER_THE_NOMAD
} nmdRenownTier_t;

typedef enum nmdRenownIncidentType_e : uint8_t
{
	NMD_RENOWN_INCIDENT_KILL = 0,
	NMD_RENOWN_INCIDENT_RESCUE,
	NMD_RENOWN_INCIDENT_CONTRACT,
	NMD_RENOWN_INCIDENT_DISCOVERY,
	NMD_RENOWN_INCIDENT_OTHER,
	NMD_RENOWN_INCIDENT_TYPE_COUNT
} nmdRenownIncidentType_t;

typedef enum nmdRenownResult_e : uint8_t
{
	NMD_RENOWN_OK = 0,
	NMD_RENOWN_NO_CHANGE,
	NMD_RENOWN_BAD_ARGUMENT,
	NMD_RENOWN_INVALID_LOCATION,
	NMD_RENOWN_CAPACITY_EXCEEDED,
	NMD_RENOWN_DUPLICATE_INCIDENT,
	NMD_RENOWN_MISSING_INCIDENT,
	NMD_RENOWN_TIME_REVERSED,
	NMD_RENOWN_CORRUPT_SNAPSHOT,
	NMD_RENOWN_CONTENT_MISMATCH
} nmdRenownResult_t;

typedef enum nmdRenownIncidentFlags_e : uint16_t
{
	NMD_RENOWN_INCIDENT_NONE = 0,
	NMD_RENOWN_INCIDENT_HISTORICAL = 1u << 0,
	NMD_RENOWN_INCIDENT_TRAIT_PERMANENT = 1u << 1
} nmdRenownIncidentFlags_t;

typedef enum nmdRenownRumorFlags_e : uint8_t
{
	NMD_RENOWN_RUMOR_NONE = 0,
	NMD_RENOWN_RUMOR_HISTORICAL = 1u << 0,
	NMD_RENOWN_RUMOR_TRAIT_PERMANENT = 1u << 1
} nmdRenownRumorFlags_t;

typedef enum nmdRenownTraitFlags_e : uint16_t
{
	NMD_RENOWN_TRAIT_NONE = 0,
	NMD_RENOWN_TRAIT_PERMANENT = 1u << 0
} nmdRenownTraitFlags_t;

typedef struct nmdRenownProfile_s
{
	uint32_t nSubjectId;
	uint32_t nLocationId;
	uint32_t nRenownQ8;
	uint32_t nHistoricalFloorQ8;
	uint32_t nLastGainDay;
	uint32_t nLastUpdateDay;
	uint32_t nRevision;
	uint32_t nFlags;
} nmdRenownProfile_t;

typedef struct nmdRenownTrait_s
{
	uint32_t nSubjectId;
	uint32_t nLocationId;
	uint32_t nTraitId;
	uint32_t nLastGainDay;
	uint32_t nLastUpdateDay;
	uint16_t nEvidence;
	uint16_t nFlags;
} nmdRenownTrait_t;

typedef struct nmdRenownPattern_s
{
	uint32_t nSubjectId;
	uint32_t nLocationId;
	uint32_t nLastIncidentDay;
	uint32_t nIncidentCount;
	uint8_t nIncidentType;
	uint8_t nReserved[3];
} nmdRenownPattern_t;

typedef struct nmdRenownIncident_s
{
	uint64_t nIncidentId;
	uint32_t nSubjectId;
	uint32_t nLocationId;
	uint32_t nWorldMinute;
	uint32_t nBaseRenownQ8;
	uint32_t nTraitId;
	int16_t nTraitEvidence;
	uint16_t nVisibilityQ8;
	uint16_t nAttributionQ8;
	uint16_t nFlags;
	uint8_t nType;
	uint8_t nReserved;
} nmdRenownIncident_t;

typedef struct nmdRenownIncidentRecord_s
{
	nmdRenownIncident_t incident;
	uint32_t nPatternBonusQ8;
	uint32_t nAppliedRenownQ8;
	uint16_t nAppliedCertaintyQ8;
	uint16_t nAppliedTraitMagnitude;
} nmdRenownIncidentRecord_t;

typedef struct nmdRenownLocation_s
{
	uint32_t nLocationId;
	uint32_t nFirstEdge;
	uint16_t nEdgeCount;
	uint16_t nFlags;
} nmdRenownLocation_t;

typedef struct nmdRenownEdge_s
{
	uint32_t nDestinationLocationId;
	uint32_t nTravelMinutes;
	uint16_t nTransmissionQ8;
	uint16_t nFlags;
} nmdRenownEdge_t;

typedef struct nmdRenownRumor_s
{
	uint64_t nRumorId;
	uint32_t nSubjectId;
	uint32_t nDestinationLocationId;
	uint32_t nDeliveryMinute;
	uint32_t nRenownQ8;
	uint32_t nTraitId;
	uint16_t nTraitEvidence;
	uint16_t nHopCount;
	uint8_t nTraitNegative;
	uint8_t nFlags;
	uint8_t nReserved[2];
} nmdRenownRumor_t;

typedef struct nmdRenownSeen_s
{
	uint64_t nRumorId;
	uint32_t nLocationId;
	uint32_t nAppliedRenownQ8;
	uint16_t nAppliedTraitEvidence;
	uint16_t nBestHopCount;
} nmdRenownSeen_t;

typedef struct nmdRenownCapacities_s
{
	uint32_t nProfiles;
	uint32_t nTraits;
	uint32_t nPatterns;
	uint32_t nIncidents;
	uint32_t nRumors;
	uint32_t nSeen;
	uint32_t nLocations;
	uint32_t nEdges;
} nmdRenownCapacities_t;

typedef struct nmdRenownRules_s
{
	uint32_t nDecayGraceDays;
	uint32_t nDecayBaseQ8;
	uint32_t nDecayAccelerationQ8;
	uint32_t nTraitGraceDays;
	uint16_t nTraitDecayPerDay;
	uint16_t nPatternInterval;
	uint32_t nPatternBonusQ8;
	uint16_t nMaxRumorHops;
	uint16_t nReserved;
	uint16_t nTierSpeedQ8[NMD_RENOWN_TIER_COUNT];
} nmdRenownRules_t;

typedef struct nmdRenownArenaHeader_s
{
	uint64_t nLastIncidentId;
	uint64_t nLastRumorId;
	uint32_t nMagic;
	uint32_t nVersion;
	uint32_t nWorldMinute;
	uint32_t nRevision;
	uint32_t nProfileCount;
	uint32_t nTraitCount;
	uint32_t nPatternCount;
	uint32_t nIncidentCount;
	uint32_t nIncidentHead;
	uint32_t nRumorCount;
	uint32_t nSeenCount;
	uint32_t nLocationCount;
	uint32_t nEdgeCount;
	uint32_t nGraphFingerprint;
	uint32_t nRulesFingerprint;
	uint32_t nCapacityFingerprint;
} nmdRenownArenaHeader_t;

static_assert( sizeof( nmdRenownProfile_t ) == 32, "Renown profile ABI" );
static_assert( sizeof( nmdRenownTrait_t ) == 24, "Renown trait ABI" );
static_assert( sizeof( nmdRenownPattern_t ) == 20, "Renown pattern ABI" );
static_assert( sizeof( nmdRenownIncident_t ) == 40, "Renown incident ABI" );
static_assert( sizeof( nmdRenownIncidentRecord_t ) == 56, "Renown incident record ABI" );
static_assert( sizeof( nmdRenownLocation_t ) == 12, "Renown location ABI" );
static_assert( sizeof( nmdRenownEdge_t ) == 12, "Renown edge ABI" );
static_assert( sizeof( nmdRenownRumor_t ) == 40, "Renown rumor ABI" );
static_assert( sizeof( nmdRenownSeen_t ) == 24, "Renown dedup ABI" );
static_assert( sizeof( nmdRenownArenaHeader_t ) == 80, "Renown header ABI" );

#endif // __NMD_RENOWN_TYPES_H__
