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

#ifndef __NMD_STORYLINE_INSTANCE_H__
#define __NMD_STORYLINE_INSTANCE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "../World/NMDWorldDatabase.h"
#include "NMDStorylineSystem.h"

// Non-owning runtime view of a registered storyline. No duplicate quest pool.
class NMDStorylineInstance
{
public:
	NMDStorylineInstance( void ) : m_pDatabase( NULL ),
		m_pSystem( NULL ), m_hStory{ NMD_WORLD_HANDLE_INVALID }
	{
	}

	void bind( NMDWorldDatabase *pDatabase, NMDStorylineSystem *pSystem,
		nmdWorldHandle_t hStory )
	{
		m_pDatabase = pDatabase;
		m_pSystem = pSystem;
		m_hStory = hStory;
	}

	bool active( void ) const
	{
		nmdWorldStoryState_t state = {};
		return m_pDatabase != NULL && m_pDatabase->getStory( m_hStory, state ) &&
			( state.nFlags & NMD_STORY_FLAG_ACTIVE ) != 0u;
	}

	bool start( void )
	{
		return m_pSystem != NULL && m_pSystem->startStory( m_hStory );
	}

	bool setBranch( uint32_t nBranchId )
	{
		return m_pSystem != NULL && m_pSystem->setBranch( m_hStory, nBranchId );
	}

	bool getState( nmdWorldStoryState_t& state ) const
	{
		return m_pDatabase != NULL && m_pDatabase->getStory( m_hStory, state );
	}

	nmdWorldHandle_t getHandle( void ) const { return m_hStory; }

private:
	NMDWorldDatabase *m_pDatabase;
	NMDStorylineSystem *m_pSystem;
	nmdWorldHandle_t m_hStory;
};

#endif // __NMD_STORYLINE_INSTANCE_H__
