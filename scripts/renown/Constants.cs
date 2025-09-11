/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown {
	public static class Constants {
		public static readonly float DAMAGE_VELOCITY = 420.0f;

		public static readonly Dictionary<ResourceType, Resource> RawResources = new Dictionary<ResourceType, Resource>{
			{ ResourceType.Metal, ResourceLoader.Load( "res://resources/items/metal.tres" ) },
			{ ResourceType.Wood, ResourceLoader.Load( "res://resources/items/wood.tres" ) },
			{ ResourceType.Drugflower, ResourceLoader.Load( "res://resources/items/drugflower.tres" ) },
			{ ResourceType.Gunpowder, ResourceLoader.Load( "res://resources/items/gunpowder.tres" ) },
			{ ResourceType.Food, ResourceLoader.Load( "res://resources/items/food.tres" ) },
			{ ResourceType.Water, ResourceLoader.Load( "res://resources/items/water.tres" ) }
		};

		public static readonly int THREAD_GROUP_BIOMES = 0;
		public static readonly int THREAD_GROUP_THINKERS = 1;
		public static readonly int THREAD_GROUP_FACTIONS = 2;
		public static readonly int THREAD_GROUP_THINKERS_AWAY = 3;

		public static readonly int THREADSLEEP_THINKER_PLAYER_IN_AREA = 15;
		public static readonly int THREADSLEEP_THINKER_PLAYER_IN_BIOME = 180;
		public static readonly int THREADSLEEP_THINKER_PLAYER_AWAY = 5000;

		public static readonly int THREADSLEEP_FACTION_PLAYER_IN_AREA = 1000;
		public static readonly int THREADSLEEP_FACTION_PLAYER_IN_BIOME = 5000;
		public static readonly int THREADSLEEP_FACTION_PLAYER_AWAY = 20000;

		public static readonly System.Threading.ThreadPriority THREAD_IMPORTANCE_PLAYER_IN_AREA = System.Threading.ThreadPriority.Highest;
		public static readonly System.Threading.ThreadPriority THREAD_IMPORTANCE_PLAYER_IN_BIOME = System.Threading.ThreadPriority.BelowNormal;
		public static readonly System.Threading.ThreadPriority THREAD_IMPORTANCE_PLAYER_AWAY = System.Threading.ThreadPriority.Lowest;

		// change this if you want some other starting contract
		public static readonly StringName StartingQuestPath = "res://resources/quests/war_of_the_wastes/from_eagles_peak.tres";
		public static readonly Contracts.Flags StartingQuestFlags = Contracts.Flags.None;
		public static readonly Dictionary<string, bool> StartingQuestState = new Dictionary<string, bool>{
			{ "TargetAlive", true }
		};
	};
};