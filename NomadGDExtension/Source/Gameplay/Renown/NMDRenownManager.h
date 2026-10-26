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

#ifndef __NMD_RENOWN_MANAGER_H__
#define __NMD_RENOWN_MANAGER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <godot_cpp/classes/ref_counted.hpp>
#include <EASTL/fixed_vector.h>
#include <EASTL/fixed_hash_map.h>
#include <Framework/Core/Containers/NMDStrings.h>
#include <godot_cpp/classes/wrapped.hpp>
#include <godot_cpp/core/object.hpp>

typedef enum nmdRenownTrait_e : uint8_t
{
	NMD_TRAIT_MERCY,
	NMD_TRAIT_HONESTY,
	NMD_TRAIT_WARCRIMINALITY,
	NMD_TRAIT_RELIABILITY,

	NMD_TRAIT_COUNT
} nmdRenownTrait_t;

typedef struct nmdRegionRenown_s
{
	uint32_t szPlayerTraitScores[ NMD_TRAIT_COUNT ];
	uint32_t nPlayerRenownScore;
} nmdRegionRenown_t;

typedef struct nmdRenownData_s
{
	uint16_t szPlayerTraitScores[ NMD_TRAIT_COUNT ];
} nmdRenownData_t;

class NMDRenownManager : public godot::RefCounted
{
	GDCLASS( NMDRenownManager, godot::RefCounted );

public:
	NMDRenownManager( void );
	~NMDRenownManager();

	uint32_t getRenownForRegion( const NMDStringAtom& regionId ) const;
	uint32_t getTraitScoreForRegion( const NMDStringAtom& regionId, nmdRenownTrait_t eTrait ) const;

protected:
	//
	// Godot API
	//

	static void _bind_methods( void );

private:
	//
	// Player renown scores
	//

	uint32_t *m_pszRenownScores;
	uint32_t *m_pszRenownTraitScores;
};

#endif // __NMD_RENOWN_MANAGER_H__
