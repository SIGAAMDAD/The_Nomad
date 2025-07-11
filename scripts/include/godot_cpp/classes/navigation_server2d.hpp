/**************************************************************************/
/*  navigation_server2d.hpp                                               */
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

#include <godot_cpp/classes/ref.hpp>
#include <godot_cpp/core/object.hpp>
#include <godot_cpp/variant/callable.hpp>
#include <godot_cpp/variant/packed_vector2_array.hpp>
#include <godot_cpp/variant/rect2.hpp>
#include <godot_cpp/variant/rid.hpp>
#include <godot_cpp/variant/transform2d.hpp>
#include <godot_cpp/variant/typed_array.hpp>
#include <godot_cpp/variant/vector2.hpp>

#include <godot_cpp/core/class_db.hpp>

#include <type_traits>

namespace godot {

class NavigationMeshSourceGeometryData2D;
class NavigationPathQueryParameters2D;
class NavigationPathQueryResult2D;
class NavigationPolygon;
class Node;

class NavigationServer2D : public Object {
	GDEXTENSION_CLASS(NavigationServer2D, Object)

	static NavigationServer2D *singleton;

public:
	static NavigationServer2D *get_singleton();

	TypedArray<RID> get_maps() const;
	RID map_create();
	void map_set_active(const RID &p_map, bool p_active);
	bool map_is_active(const RID &p_map) const;
	void map_set_cell_size(const RID &p_map, float p_cell_size);
	float map_get_cell_size(const RID &p_map) const;
	void map_set_use_edge_connections(const RID &p_map, bool p_enabled);
	bool map_get_use_edge_connections(const RID &p_map) const;
	void map_set_edge_connection_margin(const RID &p_map, float p_margin);
	float map_get_edge_connection_margin(const RID &p_map) const;
	void map_set_link_connection_radius(const RID &p_map, float p_radius);
	float map_get_link_connection_radius(const RID &p_map) const;
	PackedVector2Array map_get_path(const RID &p_map, const Vector2 &p_origin, const Vector2 &p_destination, bool p_optimize, uint32_t p_navigation_layers = 1);
	Vector2 map_get_closest_point(const RID &p_map, const Vector2 &p_to_point) const;
	RID map_get_closest_point_owner(const RID &p_map, const Vector2 &p_to_point) const;
	TypedArray<RID> map_get_links(const RID &p_map) const;
	TypedArray<RID> map_get_regions(const RID &p_map) const;
	TypedArray<RID> map_get_agents(const RID &p_map) const;
	TypedArray<RID> map_get_obstacles(const RID &p_map) const;
	void map_force_update(const RID &p_map);
	uint32_t map_get_iteration_id(const RID &p_map) const;
	void map_set_use_async_iterations(const RID &p_map, bool p_enabled);
	bool map_get_use_async_iterations(const RID &p_map) const;
	Vector2 map_get_random_point(const RID &p_map, uint32_t p_navigation_layers, bool p_uniformly) const;
	void query_path(const Ref<NavigationPathQueryParameters2D> &p_parameters, const Ref<NavigationPathQueryResult2D> &p_result, const Callable &p_callback = Callable());
	RID region_create();
	void region_set_enabled(const RID &p_region, bool p_enabled);
	bool region_get_enabled(const RID &p_region) const;
	void region_set_use_edge_connections(const RID &p_region, bool p_enabled);
	bool region_get_use_edge_connections(const RID &p_region) const;
	void region_set_enter_cost(const RID &p_region, float p_enter_cost);
	float region_get_enter_cost(const RID &p_region) const;
	void region_set_travel_cost(const RID &p_region, float p_travel_cost);
	float region_get_travel_cost(const RID &p_region) const;
	void region_set_owner_id(const RID &p_region, uint64_t p_owner_id);
	uint64_t region_get_owner_id(const RID &p_region) const;
	bool region_owns_point(const RID &p_region, const Vector2 &p_point) const;
	void region_set_map(const RID &p_region, const RID &p_map);
	RID region_get_map(const RID &p_region) const;
	void region_set_navigation_layers(const RID &p_region, uint32_t p_navigation_layers);
	uint32_t region_get_navigation_layers(const RID &p_region) const;
	void region_set_transform(const RID &p_region, const Transform2D &p_transform);
	Transform2D region_get_transform(const RID &p_region) const;
	void region_set_navigation_polygon(const RID &p_region, const Ref<NavigationPolygon> &p_navigation_polygon);
	int32_t region_get_connections_count(const RID &p_region) const;
	Vector2 region_get_connection_pathway_start(const RID &p_region, int32_t p_connection) const;
	Vector2 region_get_connection_pathway_end(const RID &p_region, int32_t p_connection) const;
	Vector2 region_get_closest_point(const RID &p_region, const Vector2 &p_to_point) const;
	Vector2 region_get_random_point(const RID &p_region, uint32_t p_navigation_layers, bool p_uniformly) const;
	Rect2 region_get_bounds(const RID &p_region) const;
	RID link_create();
	void link_set_map(const RID &p_link, const RID &p_map);
	RID link_get_map(const RID &p_link) const;
	void link_set_enabled(const RID &p_link, bool p_enabled);
	bool link_get_enabled(const RID &p_link) const;
	void link_set_bidirectional(const RID &p_link, bool p_bidirectional);
	bool link_is_bidirectional(const RID &p_link) const;
	void link_set_navigation_layers(const RID &p_link, uint32_t p_navigation_layers);
	uint32_t link_get_navigation_layers(const RID &p_link) const;
	void link_set_start_position(const RID &p_link, const Vector2 &p_position);
	Vector2 link_get_start_position(const RID &p_link) const;
	void link_set_end_position(const RID &p_link, const Vector2 &p_position);
	Vector2 link_get_end_position(const RID &p_link) const;
	void link_set_enter_cost(const RID &p_link, float p_enter_cost);
	float link_get_enter_cost(const RID &p_link) const;
	void link_set_travel_cost(const RID &p_link, float p_travel_cost);
	float link_get_travel_cost(const RID &p_link) const;
	void link_set_owner_id(const RID &p_link, uint64_t p_owner_id);
	uint64_t link_get_owner_id(const RID &p_link) const;
	RID agent_create();
	void agent_set_avoidance_enabled(const RID &p_agent, bool p_enabled);
	bool agent_get_avoidance_enabled(const RID &p_agent) const;
	void agent_set_map(const RID &p_agent, const RID &p_map);
	RID agent_get_map(const RID &p_agent) const;
	void agent_set_paused(const RID &p_agent, bool p_paused);
	bool agent_get_paused(const RID &p_agent) const;
	void agent_set_neighbor_distance(const RID &p_agent, float p_distance);
	float agent_get_neighbor_distance(const RID &p_agent) const;
	void agent_set_max_neighbors(const RID &p_agent, int32_t p_count);
	int32_t agent_get_max_neighbors(const RID &p_agent) const;
	void agent_set_time_horizon_agents(const RID &p_agent, float p_time_horizon);
	float agent_get_time_horizon_agents(const RID &p_agent) const;
	void agent_set_time_horizon_obstacles(const RID &p_agent, float p_time_horizon);
	float agent_get_time_horizon_obstacles(const RID &p_agent) const;
	void agent_set_radius(const RID &p_agent, float p_radius);
	float agent_get_radius(const RID &p_agent) const;
	void agent_set_max_speed(const RID &p_agent, float p_max_speed);
	float agent_get_max_speed(const RID &p_agent) const;
	void agent_set_velocity_forced(const RID &p_agent, const Vector2 &p_velocity);
	void agent_set_velocity(const RID &p_agent, const Vector2 &p_velocity);
	Vector2 agent_get_velocity(const RID &p_agent) const;
	void agent_set_position(const RID &p_agent, const Vector2 &p_position);
	Vector2 agent_get_position(const RID &p_agent) const;
	bool agent_is_map_changed(const RID &p_agent) const;
	void agent_set_avoidance_callback(const RID &p_agent, const Callable &p_callback);
	bool agent_has_avoidance_callback(const RID &p_agent) const;
	void agent_set_avoidance_layers(const RID &p_agent, uint32_t p_layers);
	uint32_t agent_get_avoidance_layers(const RID &p_agent) const;
	void agent_set_avoidance_mask(const RID &p_agent, uint32_t p_mask);
	uint32_t agent_get_avoidance_mask(const RID &p_agent) const;
	void agent_set_avoidance_priority(const RID &p_agent, float p_priority);
	float agent_get_avoidance_priority(const RID &p_agent) const;
	RID obstacle_create();
	void obstacle_set_avoidance_enabled(const RID &p_obstacle, bool p_enabled);
	bool obstacle_get_avoidance_enabled(const RID &p_obstacle) const;
	void obstacle_set_map(const RID &p_obstacle, const RID &p_map);
	RID obstacle_get_map(const RID &p_obstacle) const;
	void obstacle_set_paused(const RID &p_obstacle, bool p_paused);
	bool obstacle_get_paused(const RID &p_obstacle) const;
	void obstacle_set_radius(const RID &p_obstacle, float p_radius);
	float obstacle_get_radius(const RID &p_obstacle) const;
	void obstacle_set_velocity(const RID &p_obstacle, const Vector2 &p_velocity);
	Vector2 obstacle_get_velocity(const RID &p_obstacle) const;
	void obstacle_set_position(const RID &p_obstacle, const Vector2 &p_position);
	Vector2 obstacle_get_position(const RID &p_obstacle) const;
	void obstacle_set_vertices(const RID &p_obstacle, const PackedVector2Array &p_vertices);
	PackedVector2Array obstacle_get_vertices(const RID &p_obstacle) const;
	void obstacle_set_avoidance_layers(const RID &p_obstacle, uint32_t p_layers);
	uint32_t obstacle_get_avoidance_layers(const RID &p_obstacle) const;
	void parse_source_geometry_data(const Ref<NavigationPolygon> &p_navigation_polygon, const Ref<NavigationMeshSourceGeometryData2D> &p_source_geometry_data, Node *p_root_node, const Callable &p_callback = Callable());
	void bake_from_source_geometry_data(const Ref<NavigationPolygon> &p_navigation_polygon, const Ref<NavigationMeshSourceGeometryData2D> &p_source_geometry_data, const Callable &p_callback = Callable());
	void bake_from_source_geometry_data_async(const Ref<NavigationPolygon> &p_navigation_polygon, const Ref<NavigationMeshSourceGeometryData2D> &p_source_geometry_data, const Callable &p_callback = Callable());
	bool is_baking_navigation_polygon(const Ref<NavigationPolygon> &p_navigation_polygon) const;
	RID source_geometry_parser_create();
	void source_geometry_parser_set_callback(const RID &p_parser, const Callable &p_callback);
	PackedVector2Array simplify_path(const PackedVector2Array &p_path, float p_epsilon);
	void free_rid(const RID &p_rid);
	void set_debug_enabled(bool p_enabled);
	bool get_debug_enabled() const;

protected:
	template <typename T, typename B>
	static void register_virtuals() {
		Object::register_virtuals<T, B>();
	}

	~NavigationServer2D();

public:
};

} // namespace godot

