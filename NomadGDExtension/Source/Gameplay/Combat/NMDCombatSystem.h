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

#ifndef __NMD_COMBAT_SYSTEM_H__
#define __NMD_COMBAT_SYSTEM_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

class NMDCombatSystem
{
public:
	NMDCombatSystem( void );
	NMDCombatSystem( const NMDCombatSystem& ) = delete;
	NMDCombatSystem( NMDCombatSystem&& ) = delete;
	~NMDCombatSystem();
private:
};

#endif // __NMD_COMBAT_SYSTEM_H__
