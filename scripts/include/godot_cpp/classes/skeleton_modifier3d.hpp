/**************************************************************************/
/*  skeleton_modifier3d.hpp                                               */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             GODOT ENGINE                               */
/*                        https://godotengine.org                         */
/**************************************************************************/
/* Copyright (c) 2014-present Godot Engine contributors (see AUTHORS.md). */
/* Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  */
/*                                                                        */
/* Permission is hereby granted, free of charge, to any person obtaining  */
/* a copy of this software and associated documentation files (the        */
/* "Software"), to deal in the Software without restriction, including    */
/* without limitation the rights to use, copy, modify, merge, publish,    */
/* distribute, sublicense, and/or sell copies of the Software, and to     */
/* permit persons to whom the Software is furnished to do so, subject to  */
/* the following conditions:                                              */
/*                                                                        */
/* The above copyright notice and this permission notice shall be         */
/* included in all copies or substantial portions of the Software.        */
/*                                                                        */
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        */
/* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     */
/* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. */
/* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   */
/* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   */
/* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      */
/* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 */
/**************************************************************************/

// THIS FILE IS GENERATED. EDITS WILL BE LOST.

#pragma once

#include <godot_cpp/classes/node3d.hpp>

#include <godot_cpp/core/class_db.hpp>

#include <type_traits>

namespace godot {

class Skeleton3D;

class SkeletonModifier3D : public Node3D {
	GDEXTENSION_CLASS(SkeletonModifier3D, Node3D)

public:
	enum BoneAxis {
		BONE_AXIS_PLUS_X = 0,
		BONE_AXIS_MINUS_X = 1,
		BONE_AXIS_PLUS_Y = 2,
		BONE_AXIS_MINUS_Y = 3,
		BONE_AXIS_PLUS_Z = 4,
		BONE_AXIS_MINUS_Z = 5,
	};

	Skeleton3D *get_skeleton() const;
	void set_active(bool p_active);
	bool is_active() const;
	void set_influence(float p_influence);
	float get_influence() const;
	virtual void _process_modification();

protected:
	template <typename T, typename B>
	static void register_virtuals() {
		Node3D::register_virtuals<T, B>();
		if constexpr (!std::is_same_v<decltype(&B::_process_modification), decltype(&T::_process_modification)>) {
			BIND_VIRTUAL_METHOD(T, _process_modification, 3218959716);
		}
	}

public:
};

} // namespace godot

VARIANT_ENUM_CAST(SkeletonModifier3D::BoneAxis);

