/**************************************************************************/
/*  collision_polygon3d.hpp                                               */
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
#include <godot_cpp/variant/color.hpp>
#include <godot_cpp/variant/packed_vector2_array.hpp>

#include <godot_cpp/core/class_db.hpp>

#include <type_traits>

namespace godot {

class CollisionPolygon3D : public Node3D {
	GDEXTENSION_CLASS(CollisionPolygon3D, Node3D)

public:
	void set_depth(float p_depth);
	float get_depth() const;
	void set_polygon(const PackedVector2Array &p_polygon);
	PackedVector2Array get_polygon() const;
	void set_disabled(bool p_disabled);
	bool is_disabled() const;
	void set_debug_color(const Color &p_color);
	Color get_debug_color() const;
	void set_enable_debug_fill(bool p_enable);
	bool get_enable_debug_fill() const;
	void set_margin(float p_margin);
	float get_margin() const;

protected:
	template <typename T, typename B>
	static void register_virtuals() {
		Node3D::register_virtuals<T, B>();
	}

public:
};

} // namespace godot

