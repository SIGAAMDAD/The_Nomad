using System.Collections.Generic;
using Godot;

namespace Renown {
	public static class Constants {
		public static readonly int THREAD_GROUP_BIOMES = 0;
		public static readonly float DAMAGE_VELOCITY = 420.0f;

		// change this if you want some other starting contract
		public static readonly StringName StartingQuestPath = "res://resources/quests/kirosla_contract.tres";
		public static readonly ContractFlags StartingQuestFlags = ContractFlags.None;
		public static readonly Dictionary<string, bool> StartingQuestState = new Dictionary<string, bool>{
			{ "TargetAlive", true }
		};
	};
};