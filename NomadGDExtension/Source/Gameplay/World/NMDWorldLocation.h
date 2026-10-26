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

#ifndef __NMD_WORLD_LOCATION_H__
#define __NMD_WORLD_LOCATION_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDLocation3D.h"

// Legacy scene class retained for authored NMDWorldLocation nodes.
// Location progress is owned exclusively by NMDWorldDatabase.
class NMDWorldLocation : public NMDLocation3D
{
	GDCLASS( NMDWorldLocation, NMDLocation3D )

public:
	NMDWorldLocation( void );
	~NMDWorldLocation();

	const NMDStringAtom *getId( void ) const;
	const NMDStringAtom *getDescription( void ) const;
	void triggerDiscover( void );
	bool isCompleted( void ) const;
	nmdLocationStatus_t getStatus( void ) const;
	void setStatus( nmdLocationStatus_t eStatus );
	NMDLocationStatusChangedEvent getLocationStatusChangedEvent( void ) const;

protected:
	static void _bind_methods( void );
	int32_t get_status( void ) const;
};

#endif // __NMD_WORLD_LOCATION_H__
