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

#ifndef __NMD_LOCATION_TYPES_H__
#define __NMD_LOCATION_TYPES_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldIds.h"
#include <Framework/Events/NMDEvent.h>

/*
===========================================================================
Location Definitions
===========================================================================
*/

typedef enum nmdLocationStatus_e : uint8_t
{
	NMD_LOCATION_STATUS_UNDISCOVERED = 0,
	NMD_LOCATION_STATUS_DISCOVERED,
	NMD_LOCATION_STATUS_VISITED,
	NMD_LOCATION_STATUS_COMPLETED,

	NMD_LOCATION_STATUS_COUNT
} nmdLocationStatus_t;

#define NMD_LOCATION_INVALID_ID     0xFFFFu
#define NMD_LOCATION_STATUS_MASK    0x0003u
#define NMD_LOCATION_TRAITS_MASK    0xFFFCu
#define NMD_LOCATION_TRAITS_SHIFT   2u

// Bits 0-1 of nStateAndTraits are reserved for nmdLocationStatus_t.
// Retain the original category and hidden bit positions for compiled data.
typedef enum nmdLocationFlags_e : uint16_t
{
	NMD_LOCATION_FLAG_NONE = 0,
	NMD_LOCATION_FLAG_IS_LANDMARK = NOMAD_BIT( 2 ),
	NMD_LOCATION_FLAG_IS_HIDDEN = NOMAD_BIT( 3 ),
	NMD_LOCATION_FLAG_HAS_CLEAR_CONDITIONS = NOMAD_BIT( 4 ),
	NMD_LOCATION_FLAG_IS_HOSTILE = NOMAD_BIT( 5 ),
	NMD_LOCATION_FLAG_IS_INTERIOR = NOMAD_BIT( 6 ),
	NMD_LOCATION_FLAG_FAST_TRAVEL = NOMAD_BIT( 7 ),
	NMD_LOCATION_FLAG_IS_TOWN = NOMAD_BIT( 10 ),
	NMD_LOCATION_FLAG_IS_RUIN = NOMAD_BIT( 11 ),
	NMD_LOCATION_FLAG_IS_CITY = NOMAD_BIT( 12 ),
	NMD_LOCATION_FLAG_IS_CAVE = NOMAD_BIT( 13 ),
	NMD_LOCATION_FLAG_TRACK_RENOWN = NOMAD_BIT( 14 )
} nmdLocationFlags_t;

// Hot, persisted 8-byte location record. Owned by NMDWorldDatabase.
// nId is the stable dense location index; traits occupy bits 2-15.
typedef struct nmdLocationData_s
{
	uint16_t nId;
	uint16_t nStateAndTraits;
	uint16_t nPlayerRenownScore;
	uint16_t nPlayerFearScore;
} nmdLocationData_t;

typedef enum nmdLocationChangeReason_e : uint8_t
{
	NMD_LOCATION_CHANGE_PROXIMITY = 0,
	NMD_LOCATION_CHANGE_ENTERED,
	NMD_LOCATION_CHANGE_GAMEPLAY,
	NMD_LOCATION_CHANGE_COMPLETION_RULE
} nmdLocationChangeReason_t;

// Native location events are PODs small enough to copy directly into a queue.
typedef struct nmdLocationStatusChangedEvent_s
{
	nmdLocationId_t nId;
	nmdLocationStatus_t ePrevious;
	nmdLocationStatus_t eCurrent;
	nmdLocationChangeReason_t eReason;
	uint8_t nReserved;
	uint16_t nTraits;
} nmdLocationStatusChangedEvent_t;

typedef struct nmdLocationPresenceChangedEvent_s
{
	nmdLocationId_t nId;
	uint8_t bInside;
	uint8_t nReserved;
} nmdLocationPresenceChangedEvent_t;

// Versioned save files serialize this logical record; never dump native vectors.
typedef struct nmdLocationSaveRecord_s
{
	nmdLocationId_t nId;
	uint8_t nStatus;
	uint8_t nReserved;
	uint16_t nPlayerRenownScore;
	uint16_t nPlayerFearScore;
} nmdLocationSaveRecord_t;

typedef NMDEvent<nmdLocationStatusChangedEvent_t> *NMDLocationStatusChangedEvent;
typedef NMDEvent<nmdLocationPresenceChangedEvent_t> *NMDLocationPresenceChangedEvent;

static_assert( sizeof( nmdLocationData_t ) == 8, "Location hot record must be 8 bytes" );
static_assert( sizeof( nmdLocationStatusChangedEvent_t ) == 8, "Location event must be 8 bytes" );
static_assert( sizeof( nmdLocationPresenceChangedEvent_t ) == 4, "Presence event must be 4 bytes" );
static_assert( sizeof( nmdLocationSaveRecord_t ) == 8, "Location save record must be 8 bytes" );

#endif // __NMD_LOCATION_TYPES_H__
