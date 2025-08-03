using System.Threading;
using Steamworks;
using Godot;
using Renown.World;
using Renown;
using System;
using System.Diagnostics;

public partial class World : LevelData {
	[Export]
	private Resource CurrentQuest;
	[Export]
	private Godot.Collections.Dictionary<string, Variant> State;

	public static Stopwatch LoadTime;

	private static Godot.Collections.Dictionary<string, Variant> ObjectivesState;
	private static Resource QuestState;

	public static Godot.Variant GetObjectiveState( string key ) {
		return ObjectivesState[ key ];
	}
	public static void SetObjectiveState( string key, Godot.Variant value ) {
		if ( !ObjectivesState.ContainsKey( key ) ) {
			Console.PrintError( string.Format( "ChallengeLevel.SetObjectiveState: invalid ObjectiveState key \"{0}\"", key ) );
			return;
		}
		Console.PrintLine( string.Format( "Set objective state \"{0}\" to {1}", key, value.ToString() ) );
		ObjectivesState[ key ] = value;
	}

	private void OnConditionQueryRequested( string queryType, string key, Variant value, Resource requester ) {
		switch ( queryType ) {
		case "State":
			if ( ObjectivesState.TryGetValue( key, out Godot.Variant compare ) ) {
				if ( compare.VariantType == Variant.Type.Bool ) {
					Questify.SetConditionCompleted( requester, compare.AsBool() == value.AsBool() );
				} else if ( compare.VariantType == Variant.Type.Float ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, compare.AsDouble() == value.AsDouble() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, compare.AsDouble() != value.AsDouble() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, compare.AsDouble() < value.AsDouble() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, compare.AsDouble() <= value.AsDouble() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, compare.AsDouble() > value.AsDouble() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, compare.AsDouble() >= value.AsDouble() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, compare.AsDouble() == value.AsDouble() );
					}
				} else if ( compare.VariantType == Variant.Type.Int ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, compare.AsInt32() == value.AsInt32() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, compare.AsInt32() != value.AsInt32() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, compare.AsInt32() < value.AsInt32() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, compare.AsInt32() <= value.AsInt32() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, compare.AsInt32() > value.AsInt32() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, compare.AsInt32() >= value.AsInt32() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, compare.AsInt32() == value.AsInt32() );
					}
				} else if ( compare.VariantType == Variant.Type.String ) {
					Questify.SetConditionCompleted( requester, compare.AsString() == value.AsString() );
				} else if ( compare.VariantType == Variant.Type.Vector2 ) {
					Questify.SetConditionCompleted( requester, compare.AsVector2() == value.AsVector2() );
				}
			}
			break;
		default:
			Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid QueryType {0}", queryType ) );
			break;
		};
	}
	private void OnConditionObjectiveCompleted( Resource questResource, Resource questObjective ) {
	}
	private void OnQuestObjectiveAdded( Resource questResource, Resource questObjective ) {
		Console.PrintLine( "Added quest objective..." );
	}
	private void OnQuestCompleted( Resource questResource ) {
		Console.PrintLine( "Finished quest..." );
	}

	protected override void OnResourcesFinishedLoading() {
		SetProcess( true );

		SceneLoadThread.Join();
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		if ( SettingsData.GetNetworkingEnabled() ) {
			SteamLobby.Instance.SetProcess( true );
			SteamLobby.Instance.SetPhysicsProcess( true );
		}

		if ( !SteamLobby.Instance.IsOwner() ) {
			GD.Print( "Adding other players (" + SteamLobby.Instance.LobbyMemberCount + ") to game..." );
			for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
				if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
					continue;
				}
				Renown.Entity player = PlayerScene.Instantiate<Renown.Entity>();
				player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] ) );
				player.Set( "MultiplayerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
				player.Call( "SetOwnerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
				Players.Add( SteamLobby.Instance.LobbyMembers[i], player );
				PlayerList.AddChild( player );
			}
		}

		MercenaryLeaderboard.Init();

		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive );

		LoadTime.Stop();
		Console.PrintLine( "Game loaded in " + LoadTime.Elapsed.Microseconds + " microseconds." );

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}

	private void SpawnPlayer( NetworkPlayer player ) {
		Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Checkpoints" );
		Checkpoint warpPoint = null;
		Godot.Vector2 position = ThisPlayer.GlobalPosition;
		float bestDistance = float.MaxValue;

		for ( int i = 0; i < nodes.Count; i++ ) {
			Checkpoint checkpoint = nodes[i] as Checkpoint;
			float dist = position.DistanceTo( checkpoint.GlobalPosition );
			
			if ( dist < bestDistance ) {
				bestDistance = dist;
				warpPoint = checkpoint;
			}
		}
		player.GlobalPosition = warpPoint.GlobalPosition;
	}
	protected override void OnPlayerJoined( ulong steamId ) {
		base.OnPlayerJoined( steamId );
		SpawnPlayer( Players[ (CSteamID)steamId ] as NetworkPlayer );
	}
	protected override void OnPlayerLeft( ulong steamId ) {
		base.OnPlayerLeft( steamId );
	}

	private void Load() {
		using ( var reader = ArchiveSystem.GetSection( "WorldState" ) ) {
			if ( reader == null ) {
				return;
			}
			Questify.Deserialize( GD.BytesToVarWithObjects( new Span<byte>( reader.LoadByteArray( "QuestState" ) ) ).AsGodotArray() );

			int count = reader.LoadInt( "StateCount" );
			ObjectivesState.Clear();
			foreach ( var state in State ) {
				switch ( state.Value.VariantType ) {
				case Godot.Variant.Type.Int:
					ObjectivesState.Add( state.Key, reader.LoadInt( string.Format( "State_{0}", state.Key ) ) );
					break;
				case Godot.Variant.Type.Float:
					ObjectivesState.Add( state.Key, reader.LoadFloat( string.Format( "State_{0}", state.Key ) ) );
					break;
				case Godot.Variant.Type.Bool:
					ObjectivesState.Add( state.Key, reader.LoadBoolean( string.Format( "State_{0}", state.Key ) ) );
					break;
				case Godot.Variant.Type.String:
					ObjectivesState.Add( state.Key, reader.LoadString( string.Format( "State_{0}", state.Key ) ) );
					break;
				};
			}
		}
	}
	public void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( "WorldState" ) ) {
			writer.SaveByteArray( "QuestState", GD.VarToBytesWithObjects( Questify.Serialize() ) );

			writer.SaveInt( "StateCount", ObjectivesState.Count );
			foreach ( var state in ObjectivesState ) {
				switch ( state.Value.VariantType ) {
				case Godot.Variant.Type.Int:
					writer.SaveInt( string.Format( "State_{0}", state.Key ), (int)state.Value );
					break;
				case Godot.Variant.Type.Float:
					writer.SaveFloat( string.Format( "State_{0}", state.Key ), (float)state.Value );
					break;
				case Godot.Variant.Type.Bool:
					writer.SaveBool( string.Format( "State_{0}", state.Key ), (bool)state.Value );
					break;
				case Godot.Variant.Type.String:
					writer.SaveString( string.Format( "State_{0}", state.Key ), (string)state.Value );
					break;
				};
			}
		}
	}

	private void DisableTutorials() {
		Godot.Collections.Array<Node> nodes = Instance.GetNode( "Tutorials" ).GetChildren();
		for ( int i = 0; i < nodes.Count; i++ ) {
			( nodes[ i ] as InteractionItem ).Hide();
			( nodes[ i ] as InteractionItem ).ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	public override void _Ready() {
		base._Ready();

		AddChild( new BiomeManager() );

		SceneLoadThread = new Thread( () => {
			long[] initTasks = [
				WorkerThreadPool.AddTask( Callable.From( () => { PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" ); } ) ),
				WorkerThreadPool.AddTask( Callable.From( () => { Faction.Cache = new DataCache<Faction>( this, "Factions" ); } ) ),
				WorkerThreadPool.AddTask( Callable.From( () => { Settlement.Cache = new DataCache<Settlement>( this, "Settlements" ); } ) ),
				WorkerThreadPool.AddTask( Callable.From( () => { WorldArea.Cache = new DataCache<WorldArea>( this, "Locations" ); } ) )
			];

			for ( int i = 0; i < initTasks.Length; i++ ) {
				WorkerThreadPool.WaitForTaskCompletion( initTasks[ i ] );
			}

			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				return;
			}

			long[] loadTasks = [
				WorkerThreadPool.AddTask( Callable.From( () => Faction.Cache.Load() ) ),
				WorkerThreadPool.AddTask( Callable.From( () => Settlement.Cache.Load() ) ),
				WorkerThreadPool.AddTask( Callable.From( () => WorldArea.Cache.Load() ) )
			];

			for ( int i = 0; i < loadTasks.Length; i++ ) {
				WorkerThreadPool.WaitForTaskCompletion( loadTasks[ i ] );
			}
		} );

		ResourceLoadThread = new Thread( () => ResourceCache.Cache( this, SceneLoadThread ) );
		ResourceLoadThread.Start();

		if ( !SettingsData.GetTutorialsEnabled() ) {
			DisableTutorials();
		}

		ObjectivesState = new Godot.Collections.Dictionary<string, Variant>();
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		} else {
			foreach ( var state in State ) {
				ObjectivesState.Add( state.Key, state.Value );
			}
		}

		Questify.ToggleUpdatePolling( true );
		QuestState = Questify.Instantiate( CurrentQuest );
		Questify.StartQuest( QuestState );

		AddToGroup( "Archive" );
	}
};
