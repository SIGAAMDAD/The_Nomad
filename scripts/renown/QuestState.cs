using Godot;
using System.Collections.Generic;

namespace Renown {
	public class QuestState {
		private static Dictionary<string, int> ValueIntCache = null;
		private static Dictionary<string, float> ValueFloatCache = null;
		private static Dictionary<string, bool> StateCache = null;
		private static Dictionary<string, bool> ContractFlagsCache = null;

		private static void OnConditionQueryRequested( string type, string key, Variant value, Resource requester ) {
			if ( type == "ContractFlags" ) {
				if ( !ContractFlagsCache.TryGetValue( key, out bool flag ) ) {
					return;
				}
				Questify.SetConditionCompleted( requester, flag == value.AsBool() );
			} else if ( type == "State" ) {
				if ( !StateCache.TryGetValue( key, out bool state ) ) {
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsBool() );
			} else if ( type == "ValueInt" ) {
				if ( !ValueIntCache.TryGetValue( key, out int state ) ) {
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsInt32() );
			} else if ( type == "ValueFloat" ) {
				if ( !ValueFloatCache.TryGetValue( key, out float state ) ) {
					return;
				}
				Questify.SetConditionCompleted( requester, state == (float)value.AsDouble() );
			}
		}
		private static void OnQuestCompleted( Resource quest ) {
		}
		private static void OnQuestObjectiveCompleted( Resource quest, Resource objective ) {
		}

		public static void StartContract( Resource quest, ContractFlags flags, Dictionary<string, bool> state ) {
			ContractFlagsCache?.Clear();
			ContractFlagsCache =  new Dictionary<string, bool>{
				{ "Massacre", ( flags & ContractFlags.Massacre ) != 0 },
				{ "Silent", ( flags & ContractFlags.Silent ) != 0 },
				{ "Ghost", ( flags & ContractFlags.Ghost ) != 0 },
				{ "Clean", ( flags & ContractFlags.Clean ) != 0 }
			};

			StateCache?.Clear();
			StateCache = state;

			ValueIntCache?.Clear();
			ValueIntCache = new Dictionary<string, int>{
				{ "TimesSpotted", 0 },
				{ "TimesAlerted", 0 },
				{ "EnemiesRemaining", 0 },
				{ "CollateralScore", 0 }
			};

			ValueFloatCache?.Clear();
			ValueFloatCache = new Dictionary<string, float>{
			};

			Resource contract = Questify.Instantiate( quest );
			Questify.ConnectConditionQueryRequested( new System.Action<string, string, Variant, Resource>( OnConditionQueryRequested ) );
			Questify.ConnectQuestCompleted( new System.Action<Resource>( OnQuestCompleted ) );
			Questify.ConnectQuestObjectiveCompleted( new System.Action<Resource, Resource>( OnQuestObjectiveCompleted ) );
			Questify.StartQuest( contract );
		}
	};
};