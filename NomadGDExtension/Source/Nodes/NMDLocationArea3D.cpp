#include "NMDLocationArea3D.h"
#include "Framework/Core/Memory/NMDIntrusivePtr.h"
#include "Gameplay/NMDWorldLocation.h"
#include "godot_cpp/classes/node3d.hpp"
#include "godot_cpp/variant/callable.hpp"
#include "godot_cpp/variant/callable_method_pointer.hpp"

NMDLocationArea3D::NMDLocationArea3D( const NMDIntrusivePtr<NMDWorldLocation>& pOwner )
	: m_pOwner( pOwner )
{
	connect( "body_shape_entered", callable_mp( this, &NMDLocationArea3D::onBodyEntered ) );
}

NMDLocationArea3D::~NMDLocationArea3D()
{
}

void NMDLocationArea3D::onBodyEntered( const godot::RID& bodyRid, godot::Node3D *pBody, int bodyShapeIndex, int localShapeIndex )
{
	m_pOwner->triggerDiscover();
}
