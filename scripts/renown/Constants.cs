using System.Collections.Generic;
using Godot;
using Renown.Thinkers;

namespace Renown {
	public static class Constants {
		public static readonly int THREAD_GROUP_BIOMES = 0;
		public static readonly float DAMAGE_VELOCITY = 420.0f;

		public static readonly int JobChance_MercenaryGovernment = 60;
		public static readonly Dictionary<OccupationType, Dictionary<SocietyRank, int>> JobChances_SocioEconomicStatus = new Dictionary<OccupationType, Dictionary<SocietyRank, int>>{
			{
				OccupationType.None,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 72 },
					{ SocietyRank.Middle, 28 },
					{ SocietyRank.Upper, 0 }
				}
			},
			{
				OccupationType.Farmer,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 80 },
					{ SocietyRank.Middle, 20 },
					{ SocietyRank.Upper, 0 }
				}
			},
			{
				OccupationType.Industry,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 80 },
					{ SocietyRank.Middle, 20 },
					{ SocietyRank.Upper, 0 }
				}
			},
			{
				OccupationType.Bandit,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 70 },
					{ SocietyRank.Middle, 28 },
					{ SocietyRank.Upper, 2 }
				}
			},
			{
				OccupationType.Blacksmith,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 30 },
					{ SocietyRank.Middle, 70 },
					{ SocietyRank.Upper, 0 }
				}
			},
			{
				OccupationType.Gunsmith,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 30 },
					{ SocietyRank.Middle, 70 },
					{ SocietyRank.Upper, 0 }
				}
			},
			{
				OccupationType.Mercenary,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 44 },
					{ SocietyRank.Middle, 50 },
					{ SocietyRank.Upper, 6 }
				}
			},
			{
				OccupationType.Merchant,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 10 },
					{ SocietyRank.Middle, 60 },
					{ SocietyRank.Upper, 20 }
				}
			},
			{
				OccupationType.Politician,
				new Dictionary<SocietyRank, int>{
					{ SocietyRank.Lower, 0 },
					{ SocietyRank.Middle, 10 },
					{ SocietyRank.Upper, 90 }
				}
			},
		};

		public static readonly int THREAD_GROUP_THINKERS = 1;
		public static readonly int THREAD_GROUP_FACTIONS = 2;

		public static readonly int THREADSLEEP_THINKER_PLAYER_IN_AREA = 500;
		public static readonly int THREADSLEEP_THINKER_PLAYER_IN_BIOME = 1000;
		public static readonly int THREADSLEEP_THINKER_PLAYER_AWAY = 10000;

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