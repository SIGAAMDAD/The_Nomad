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

#ifndef __NMD_INVENTORY_H__
#define __NMD_INVENTORY_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDStorageUnit.h"

class NMDInventory : public NMDStorageUnit
{
public:
	NMDInventory( void );
	~NMDInventory();

	virtual nmdStorageUnitType_t getType( void ) const override;

	float getTotalWeight( void ) const;

private:
	float m_nTotalItemWeight;
};

NOMAD_FORCE_INLINE nmdStorageUnitType_t NMDInventory::getType( void ) const
{
	return NMD_STORAGE_UNIT_BACKPACK;
}

#endif // __NMD_INVENTORY_H__
