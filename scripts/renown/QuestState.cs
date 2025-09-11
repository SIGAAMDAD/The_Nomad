/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using System.Collections.Generic;

namespace Renown {
	public partial class QuestState : Node {
		private static Dictionary<string, int> ValueIntCache = null;
		private static Dictionary<string, float> ValueFloatCache = null;
		private static Dictionary<string, bool> StateCache = null;
		private static Dictionary<string, bool> ContractFlagsCache = null;

		private static QuestState Instance;

		[Signal]
		public delegate void QuestCompletedEventHandler( Resource questResource );
		[Signal]
		public delegate void QuestStartedEventHandler( Resource questResource );
		[Signal]
		public delegate void QuestObjectiveCompletedEventHandler( Resource questResource, Resource objective );

		/*
		===============
		StartContract
		===============
		*/
		public static void StartContract( Resource quest, Contracts.Flags flags, Dictionary<string, bool> state ) {
			ContractFlagsCache?.Clear();
			ContractFlagsCache = new Dictionary<string, bool>{
				{ "Massacre", ( flags & Contracts.Flags.Massacre ) != 0 },
				{ "Silent", ( flags & Contracts.Flags.Silent ) != 0 },
				{ "Ghost", ( flags & Contracts.Flags.Ghost ) != 0 },
				{ "Clean", ( flags & Contracts.Flags.Clean ) != 0 }
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
			ValueFloatCache = new Dictionary<string, float> {
			};

			Resource contract = Questify.Instantiate( quest );
			Questify.StartQuest( contract );
		}

		/*
		===============
		StartQuest
		===============
		*/
		public static void StartQuest( Resource quest, in Godot.Collections.Dictionary<string, bool> state ) {
			ContractFlagsCache?.Clear();
			ValueIntCache?.Clear();
			ValueFloatCache?.Clear();
			StateCache?.Clear();

			StateCache = new Dictionary<string, bool>( state.Count );
			foreach ( var value in state ) {
				StateCache.TryAdd( value.Key, value.Value );
			}
		}

		/*
		===============
		CheckQuestState
		===============
		*/
		private static void CheckQuestState( in Resource requester, in string key, in Variant value ) {
			if ( !StateCache.TryGetValue( key, out bool state ) ) {
				Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such StateVar {0}!", key ) );
				return;
			}
			Questify.SetConditionCompleted( requester, state == value.AsBool() );
		}

		/*
		===============
		CheckContractFlag
		===============
		*/
		private static void CheckContractFlag( in Resource requester, in string key, in Variant value ) {
			if ( !ContractFlagsCache.TryGetValue( key, out bool state ) ) {
				Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such ContractFlag {0}!", key ) );
				return;
			}
			Questify.SetConditionCompleted( requester, state == value.AsBool() );
		}

		/*
		===============
		CheckIntegerState
		===============
		*/
		private static void CheckIntegerState( in Resource requester, in string key, in Variant value ) {
			if ( !ValueIntCache.TryGetValue( key, out int state ) ) {
				Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such IntState {0}!", key ) );
				return;
			}
			Questify.SetConditionCompleted( requester, state == value.AsInt32() );
		}

		/*
		===============
		CheckFloatState
		===============
		*/
		private static void CheckFloatState( in Resource requester, in string key, in Variant value ) {
			if ( !ValueFloatCache.TryGetValue( key, out float state ) ) {
				Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such FloatState {0}!", key ) );
				return;
			}
			Questify.SetConditionCompleted( requester, state == value.AsDouble() );
		}

		/*
		===============
		OnConditionQueryRequested
		===============
		*/
		private static void OnConditionQueryRequested( string type, string key, Variant value, Resource requester ) {
			switch ( type ) {
				case "State":
					CheckQuestState( in requester, in key, in value );
					break;
				case "ContractFlags":
					CheckContractFlag( in requester, in key, in value );
					break;
				case "Int":
					CheckIntegerState( in requester, in key, in value );
					break;
				case "Float":
					CheckFloatState( in requester, in key, in value );
					break;
				default:
					Console.PrintError( $"QuestState.OnConditionQueryRequested: invalid condition type {type}" );
					break;
			}
		}

		/*
		===============
		OnQuestCompleted
		===============
		*/
		private static void OnQuestCompleted( Resource quest ) {
			Instance.EmitSignalQuestCompleted( quest );
		}

		/*
		===============
		OnQuestObjectiveCompleted
		===============
		*/
		private static void OnQuestObjectiveCompleted( Resource quest, Resource objective ) {
			Instance.EmitSignalQuestObjectiveCompleted( quest, objective );
		}

		/*
		===============
		OnQuestStarted
		===============
		*/
		private static void OnQuestStarted( Resource quest ) {
			Instance.EmitSignalQuestStarted( quest );
		}

		/*
		===============
		_Ready
		===============
		*/
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