#ifndef __NMD_PLAYER_H__
#define __NMD_PLAYER_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <godot_cpp/classes/node3d.hpp>
#include <Framework/Core/Containers/NMDStrings.h>
#include "NMDPlayerStatsManager.h"

class NMDPlayer : public godot::Node3D
{
public:
	NMDPlayer( void );
	~NMDPlayer();

private:
	static void _bind_methods( void );

private:
	NMDPlayerStatsManager *m_pStatsManager;
};

#endif // __NMD_PLAYER_H__
