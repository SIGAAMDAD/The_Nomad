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

#ifndef __NMD_LOCATION_3D_H__
#define __NMD_LOCATION_3D_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDLocationSystem.h"

#include <godot_cpp/classes/area3d.hpp>
#include <godot_cpp/classes/node3d.hpp>
#include <godot_cpp/variant/node_path.hpp>
#include <godot_cpp/variant/string_name.hpp>

// Streamable 3D detection node. All persistent data remains in NMDLocationSystem::GetDatabase().
// Child Area3D shapes are authored in Godot's 3D editor.
class NMDLocation3D : public godot::Node3D
{
	GDCLASS( NMDLocation3D, godot::Node3D )

public:
	NMDLocation3D();
	~NMDLocation3D() = default;

	void _ready() override;
	void _exit_tree() override;

	void set_location_id( int32_t nId );
	int32_t get_location_id() const;

	void set_system_path( const godot::NodePath& path );
	godot::NodePath get_system_path() const;

	void set_visit_area_path( const godot::NodePath& path );
	godot::NodePath get_visit_area_path() const;

	void set_discovery_area_path( const godot::NodePath& path );
	godot::NodePath get_discovery_area_path() const;

	void set_player_group( const godot::StringName& group );
	godot::StringName get_player_group() const;

protected:
	static void _bind_methods();
	NMDLocationSystem *GetLocationSystem( void ) const;

private:
	bool IsPlayerBody( godot::Node3D *pBody ) const;
	void _on_visit_body_entered( godot::Node3D *pBody );
	void _on_visit_body_exited( godot::Node3D *pBody );
	void _on_discovery_body_entered( godot::Node3D *pBody );

	godot::NodePath m_SystemPath;
	godot::NodePath m_VisitAreaPath;
	godot::NodePath m_DiscoveryAreaPath;
	godot::StringName m_PlayerGroup;

	NMDLocationSystem *m_pSystem;
	godot::Area3D *m_pVisitArea;
	godot::Area3D *m_pDiscoveryArea;

	int32_t m_nLocationId;
	bool m_bPlayerInside;
};

#endif // __NMD_LOCATION_3D_H__
