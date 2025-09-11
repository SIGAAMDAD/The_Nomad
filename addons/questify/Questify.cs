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
using System;
using System.Runtime.CompilerServices;

public static class Questify {
	private static readonly NodePath nodePath = new NodePath( "/root/Questify" );
	private static Node instance;
	public static Node Instance {
		get {
			if ( instance == null ) {
				instance = ( (SceneTree)Engine.GetMainLoop() ).Root.GetNode( nodePath );
			}
			return instance;
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Resource Instantiate( Resource questResource ) {
		return (Resource)questResource.Call( FuncName.Instantiate );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void StartQuest( Resource quest ) {
		Instance.Call( FuncName.StartQuest, quest );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetConditionCompleted( Resource questCondition, bool complete ) {
		questCondition.Call( FuncName.SetConditionCompleted, complete );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void ToggleUpdatePolling( bool updatePolling ) {
		Instance.Call( FuncName.ToggleUpdatePolling, updatePolling );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void UpdateQuests() {
		Instance.Call( FuncName.UpdateQuests );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Godot.Collections.Array<Resource> GetQuests() {
		return Instance.Call( FuncName.GetQuests ).AsGodotArray<Resource>();
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Godot.Collections.Array<Resource> GetActiveQuests() {
		return Instance.Call( FuncName.GetActiveQuests ).As<Godot.Collections.Array<Resource>>();
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Godot.Collections.Array<Resource> GetCompletedQuests() {
		return Instance.Call( FuncName.GetCompletedQuests ).As<Godot.Collections.Array<Resource>>();
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetQuests( Godot.Collections.Array<Resource> quests ) {
		Instance.Call( FuncName.SetQuests, quests );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Godot.Collections.Array Serialize() {
		return Instance.Call( FuncName.Serialize ).As<Godot.Collections.Array>();
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Deserialize( Godot.Collections.Array data ) {
		Instance.Call( FuncName.Deserialize, data );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static string GetResourcePath( Resource quest ) {
		return quest.Call( FuncName.GetResourcePath ).As<string>();
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Clear() {
		Instance.Call( FuncName.Clear );
	}

	/// <summary>
	/// Connect action to "quest_started" signal.
	/// </summary>
	/// <param name="action"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void ConnectQuestStarted( Action<Resource> action ) {
		Instance.Connect( SignalName.QuestStarted, Callable.From( action ) );
	}

	/// <summary>
	/// Connect action to "quest_condition_query_requested" signal.
	/// </summary>
	/// <param name="action"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void ConnectConditionQueryRequested( Action<string, string, Variant, Resource> action ) {
		Instance.Connect( SignalName.ConditionQueryRequested, Callable.From( action ) );
	}

	/// <summary>
	/// Connect action to "quest_objective_added" signal.
	/// </summary>
	/// <param name="action"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void ConnectQuestObjectiveAdded( Action<Resource, Resource> action ) {
		Instance.Connect( SignalName.QuestObjectiveAdded, Callable.From( action ) );
	}

	/// <summary>
	/// Connect action to "quest_objective_completed" signal.
	/// </summary>
	/// <param name="action"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void ConnectQuestObjectiveCompleted( Action<Resource, Resource> action ) {
		Instance.Connect( SignalName.QuestObjectiveCompleted, Callable.From( action ) );
	}

	/// <summary>
	/// Connect action to "quest_completed" signal.
	/// </summary>
	/// <param name="action"></param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void ConnectQuestCompleted( Action<Resource> action ) {
		Instance.Connect( SignalName.QuestCompleted, Callable.From( action ) );
	}

	private static class FuncName {
		public static readonly StringName StartQuest = "start_quest";
		public static readonly StringName Instantiate = "instantiate";
		public static readonly StringName SetConditionCompleted = "set_completed";
		public static readonly StringName ToggleUpdatePolling = "toggle_update_polling";
		public static readonly StringName UpdateQuests = "update_quests";
		public static readonly StringName GetQuests = "get_quests";
		public static readonly StringName GetActiveQuests = "get_active_quests";
		public static readonly StringName GetCompletedQuests = "get_completed_quests";
		public static readonly StringName SetQuests = "set_quests";
		public static readonly StringName Serialize = "serialize";
		public static readonly StringName Deserialize = "deserialize";
		public static readonly StringName GetResourcePath = "get_resource_path";
		public static readonly StringName Clear = "clear";
	};
	private static class SignalName {
		public static readonly StringName QuestStarted = "quest_started";
		public static readonly StringName ConditionQueryRequested = "condition_query_requested";
		public static readonly StringName QuestObjectiveAdded = "quest_objective_added";
		public static readonly StringName QuestObjectiveCompleted = "quest_objective_completed";
		public static readonly StringName QuestCompleted = "quest_completed";
	};
};
