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

#ifndef __NMD_STORAGE_UNIT_H__
#define __NMD_STORAGE_UNIT_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <EASTL/fixed_vector.h>
#include <Framework/Core/Containers/NMDStrings.h>

typedef enum nmdStorageUnitType_e : uint8_t
{
	NMD_STORAGE_UNIT_BACKPACK,
	NMD_STORAGE_UNIT_DROPPED_ITEM_CACHE,
	NMD_STORAGE_UNIT_ITEM_CACHE,

	NMD_STORAGE_UNIT_COUNT
} nmdStorageUnitType_t;

/// @brief a generic base type for a unit of in-game world storage.
class NMDStorageUnit
{
public:
	NMDStorageUnit( void );
	virtual ~NMDStorageUnit();

	virtual nmdStorageUnitType_t getType( void ) const = 0;

	virtual uint32_t getItemQuantity( const NMDStringAtom& itemId ) const;
	virtual bool containsItem( const NMDStringAtom& itemId ) const;

	virtual bool addItems( const NMDStringAtom& itemId, uint32_t nQuantity = 1 ) = 0;
	virtual bool removeItems( const NMDStringAtom& itemId, uint32_t nQuantity = 1 ) = 0;

protected:
	eastl::fixed_hash_map<NMDStringAtom, uint32_t, 1024> m_ItemPool;
};

NOMAD_FORCE_INLINE uint32_t NMDStorageUnit::getItemQuantity( const NMDStringAtom& itemId ) const
{
	const auto& pItemPtr =  m_ItemPool.find( itemId );
	return pItemPtr != m_ItemPool.cend() ? pItemPtr->second : 0;
}

NOMAD_FORCE_INLINE bool NMDStorageUnit::containsItem( const NMDStringAtom& itemId ) const
{
	return m_ItemPool.find( itemId ) != m_ItemPool.cend();
}

#endif // __NMD_STORAGE_UNIT_H__
