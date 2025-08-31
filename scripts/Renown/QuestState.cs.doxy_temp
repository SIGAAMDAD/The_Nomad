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

		private static void OnConditionQueryRequested( string type, string key, Variant value, Resource requester ) {
			switch ( type ) {
			case "State": {
				if ( !StateCache.TryGetValue( key, out bool state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such StateVar {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsBool() );
				break; }
			case "ContractFlags": {
				if ( !ContractFlagsCache.TryGetValue( key, out bool state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such ContractFlag {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsBool() );
				break; }
			case "Int": {
				if ( !ValueIntCache.TryGetValue( key, out int state ) ) {
					Console.PrintError( string.Format( "QuestState.OnConditionQueryRequested: no such IntState {0}!", key ) );
					return;
				}
				Questify.SetConditionCompleted( requester, state == value.AsInt32() );
				break; }
			case "Float": {
				if ( !ValueFloatCache.TryGetValue( key, out float state ) ) {
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
			ContractFlagsCache?.Clear();
			ContractFlagsCache = new Dictionary<string, bool>{
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
			ValueFloatCache = new Dictionary<string, float> {
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