using Godot;
using System.Collections.Generic;

namespace StorySystem {
	public partial class QuestManager : Node {
		private Dictionary<string, int> ValueIntCache = null;
		private Dictionary<string, float> ValueFloatCache = null;
		private Dictionary<string, bool> StateCache = null;
		private Dictionary<string, bool> ContractFlagsCache = null;

		private static QuestManager Instance;

		[Signal]
		public delegate void QuestCompletedEventHandler( Resource questResource );
		[Signal]
		public delegate void QuestStartedEventHandler( Resource questResource );
		[Signal]
		public delegate void QuestObjectiveCompletedEventHandler( Resource questResource, Resource objective );

		private static void OnConditionQueryRequested( string type, string key, Variant value, Resource requester ) {
			switch ( type ) {
			case "State": {
				if ( !Instance.StateCache.TryGetValue( key, out bool state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such StateVar {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsBool() );
				break; }
			case "ContractFlags": {
				if ( !Instance.ContractFlagsCache.TryGetValue( key, out bool state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such ContractFlag {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsBool() );
				break; }
			case "Int": {
				if ( !Instance.ValueIntCache.TryGetValue( key, out int state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such IntState {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsInt32() );
				break; }
			case "Float": {
				if ( !Instance.ValueFloatCache.TryGetValue( key, out float state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such FloatState {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsDouble() );
				break; }
			default:
				Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: invalid condition type {0}", type ) );
				break;
			};
		}
		private static void OnQuestCompleted( Resource quest ) {
			Instance.EmitSignalQuestCompleted( quest );
		}
		private static void OnQuestObjectiveCompleted( Resource quest, Resource objective ) {
			Instance.EmitSignalQuestObjectiveCompleted( quest, objective );
		}
		private static void OnQuestStarted( Resource quest ) {
			Instance.EmitSignalQuestStarted( quest );
		}

		public static void StartContract( Resource quest, ContractFlags flags, Dictionary<string, bool> state ) {
			Instance.ContractFlagsCache?.Clear();
			Instance.ContractFlagsCache = new Dictionary<string, bool>{
				{ "Massacre", ( flags & ContractFlags.Massacre ) != 0 },
				{ "Silent", ( flags & ContractFlags.Silent ) != 0 },
				{ "Ghost", ( flags & ContractFlags.Ghost ) != 0 },
				{ "Clean", ( flags & ContractFlags.Clean ) != 0 }
			};

			Instance.StateCache?.Clear();
			Instance.StateCache = state;

			Instance.ValueIntCache?.Clear();
			Instance.ValueIntCache = new Dictionary<string, int>{
				{ "TimesSpotted", 0 },
				{ "TimesAlerted", 0 },
				{ "EnemiesRemaining", 0 },
				{ "CollateralScore", 0 }
			};

			Instance.ValueFloatCache?.Clear();
			Instance.ValueFloatCache = new Dictionary<string, float> {
			};

			Resource contract = Questify.Instantiate( quest );
			Questify.StartQuest( contract );
		}

		public override void _Ready() {
			base._Ready();

			Console.PrintLine( "Initializing QuestState..." );

			Instance = this;

			Questify.ConnectConditionQueryRequested( new System.Action<string, string, Variant, Resource>( OnConditionQueryRequested ) );
			Questify.ConnectQuestCompleted( new System.Action<Resource>( OnQuestCompleted ) );
			Questify.ConnectQuestObjectiveCompleted( new System.Action<Resource, Resource>( OnQuestObjectiveCompleted ) );
			Questify.ConnectQuestStarted( new System.Action<Resource>( OnQuestStarted ) );
		}
	};
};