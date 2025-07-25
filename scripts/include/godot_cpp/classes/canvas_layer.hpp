/**************************************************************************/
/*  canvas_layer.hpp                                                      */
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

#include <godot_cpp/classes/node.hpp>
#include <godot_cpp/variant/rid.hpp>
#include <godot_cpp/variant/transform2d.hpp>
#include <godot_cpp/variant/vector2.hpp>

#include <godot_cpp/core/class_db.hpp>

#include <type_traits>

namespace godot {

class CanvasLayer : public Node {
	GDEXTENSION_CLASS(CanvasLayer, Node)

public:
	void set_layer(int32_t p_layer);
	int32_t get_layer() const;
	void set_visible(bool p_visible);
	bool is_visible() const;
	void show();
	void hide();
	void set_transform(const Transform2D &p_transform);
	Transform2D get_transform() const;
	Transform2D get_final_transform() const;
	void set_offset(const Vector2 &p_offset);
	Vector2 get_offset() const;
	void set_rotation(float p_radians);
	float get_rotation() const;
	void set_scale(const Vector2 &p_scale);
	Vector2 get_scale() const;
	void set_follow_viewport(bool p_enable);
	bool is_following_viewport() const;
	void set_follow_viewport_scale(float p_scale);
	float get_follow_viewport_scale() const;
	void set_custom_viewport(Node *p_viewport);
	Node *get_custom_viewport() const;
	RID get_canvas() const;

protected:
	template <typename T, typename B>
	static void register_virtuals() {
		Node::register_virtuals<T, B>();
	}

public:
};

} // namespace godot

