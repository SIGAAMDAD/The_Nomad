using System.Collections.Generic;
using Godot;

namespace Renown {
	public static class Constants {
		public static readonly int THREAD_GROUP_BIOMES = 0;
		public static readonly float DAMAGE_VELOCITY = 420.0f;

		public static readonly int THREAD_GROUP_THINKERS = 1;
		public static readonly int THREAD_GROUP_FACTIONS = 2;

		public static readonly int THREADSLEEP_PLAYER_IN_AREA = 500;
		public static readonly int THREADSLEEP_PLAYER_IN_BIOME = 10000;
		public static readonly int THREADSLEEP_PLAYER_AWAY = 15000;

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