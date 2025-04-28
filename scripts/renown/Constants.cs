using System.Collections.Generic;
using Godot;
using Renown.Thinkers;
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

		public static readonly int THREADSLEEP_THINKER_PLAYER_IN_AREA = 30;
		public static readonly int THREADSLEEP_THINKER_PLAYER_IN_BIOME = 120;
		public static readonly int THREADSLEEP_THINKER_PLAYER_AWAY = 1000;

		public static readonly int THREADSLEEP_FACTION_PLAYER_IN_AREA = 1000;
		public static readonly int THREADSLEEP_FACTION_PLAYER_IN_BIOME = 5000;
		public static readonly int THREADSLEEP_FACTION_PLAYER_AWAY = 20000;

		public static readonly System.Threading.ThreadPriority THREAD_IMPORTANCE_PLAYER_IN_AREA = System.Threading.ThreadPriority.Highest;
		public static readonly System.Threading.ThreadPriority THREAD_IMPORTANCE_PLAYER_IN_BIOME = System.Threading.ThreadPriority.BelowNormal;
		public static readonly System.Threading.ThreadPriority THREAD_IMPORTANCE_PLAYER_AWAY = System.Threading.ThreadPriority.Lowest;

		// change this if you want some other starting contract
		public static readonly StringName StartingQuestPath = "res://resources/quests/kirosla_contract.tres";
		public static readonly ContractFlags StartingQuestFlags = ContractFlags.None;
		public static readonly Dictionary<string, bool> StartingQuestState = new Dictionary<string, bool>{
			{ "TargetAlive", true }
		};
	};
};