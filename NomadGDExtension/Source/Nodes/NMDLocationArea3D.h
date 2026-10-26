#ifndef __NMD_LOCATION_AREA_3D_H__
#define __NMD_LOCATION_AREA_3D_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <godot_cpp/classes/area3d.hpp>
#include "Gameplay/NMDWorldLocation.h"
#include <Framework/Core/Memory/NMDIntrusivePtr.h>
#include <godot_cpp/classes/wrapped.hpp>

class NMDLocationArea3D : public godot::Area3D
{
	GDCLASS( NMDLocationArea3D, godot::Area3D );

public:
	NMDLocationArea3D( const NMDIntrusivePtr<NMDWorldLocation>& pOwner );
	~NMDLocationArea3D();

private:
	//
	// Godot API
	//

	static void _bind_methods( void );

private:
	void onBodyEntered( const godot::RID& bodyRid, godot::Node3D *pBody, int bodyShapeIndex, int localShapeIndex );

	NMDIntrusivePtr<NMDWorldLocation> m_pOwner;
};

#endif // __NMD_LOCATION_AREA_3D_H__
