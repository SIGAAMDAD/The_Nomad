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

#ifndef __NMD_GAMEPLAY_CLASS_REGISTRATION_H__
#define __NMD_GAMEPLAY_CLASS_REGISTRATION_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDWorldStateHost.h"
#include "World/NMDLocationSystem.h"
#include "Story/NMDQuest.h"
#include "Story/NMDQuestObjective.h"
#include "World/NMDLocation3D.h"
#include "World/NMDWorldLocation.h"
#include "Simulation/NMDCalendarSystem.h"
#include <godot_cpp/core/class_db.hpp>

inline void NMDRegisterGameplayClasses( void )
{
	GDREGISTER_CLASS( NMDWorldStateHost );
	GDREGISTER_CLASS( NMDLocationSystem );
	GDREGISTER_CLASS( NMDQuest );
	GDREGISTER_CLASS( NMDQuestObjective );
	GDREGISTER_CLASS( NMDLocation3D );
	GDREGISTER_CLASS( NMDWorldLocation );
	GDREGISTER_CLASS( NMDCalendarSystem );
}

// Compatibility with the original registration entry point.
inline void NMDRegisterLocationClasses( void )
{
	NMDRegisterGameplayClasses();
}

#endif // __NMD_GAMEPLAY_CLASS_REGISTRATION_H__
