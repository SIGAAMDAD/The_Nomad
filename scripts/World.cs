using System.Threading;
using System.Collections.Generic;
using Steamworks;
using Godot;
using Renown.World;
using Renown;
using System.Diagnostics;
using System;

public partial class World : LevelData {
	[Export]
	private Resource MainQuest;
	[Export]
	private Godot.Collections.Dictionary<string, Variant> State;

	private static Dictionary<string, object> ObjectivesState;

	public static void SetObjectiveState( string key, object value ) {
		if ( !ObjectivesState.ContainsKey( key ) ) {
			Console.PrintError( string.Format( "World.SetObjectiveState: invalid ObjectiveState key \"{0}\"", key ) );
			return;
		}
		ObjectivesState[ key ] = value;
	}

	private void OnConditionQueryRequested( string queryType, string key, Variant value, Resource requester ) {
		switch ( queryType ) {
		case "State":
			if ( ObjectivesState.TryGetValue( key, out object compare ) ) {
				if ( compare is bool boolValue ) {
					Questify.SetConditionCompleted( requester, boolValue == value.AsBool() );
				} else if ( compare is float floatValue ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, floatValue == value.AsDouble() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, floatValue != value.AsDouble() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, floatValue < value.AsDouble() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, floatValue <= value.AsDouble() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, floatValue > value.AsDouble() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, floatValue >= value.AsDouble() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, floatValue == value.AsDouble() );
					}
				} else if ( compare is int intValue ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, intValue == value.AsInt32() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, intValue != value.AsInt32() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, intValue < value.AsInt32() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, intValue <= value.AsInt32() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, intValue > value.AsInt32() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, intValue >= value.AsInt32() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, intValue == value.AsInt32() );
					}
				} else if ( compare is string stringValue ) {
					Questify.SetConditionCompleted( requester, stringValue == value.AsString() );
				} else if ( compare is Vector2 vectorValue ) {
					Questify.SetConditionCompleted( requester, vectorValue == value.AsVector2() );
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
	private void OnQuestCompleted( Resource questResource ) {
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

	public override void _Ready() {
		base._Ready();

		SceneLoadThread = new Thread( () => {
			PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" );
			Faction.Cache = new DataCache<Faction>( this, "Factions" );
			Settlement.Cache = new DataCache<Settlement>( this, "Settlements" );
			WorldArea.Cache = new DataCache<WorldArea>( this, "Locations" );

			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				return;
			}

			Faction.Cache.Load();
			Settlement.Cache.Load();
			WorldArea.Cache.Load();
		} );

		ResourceLoadThread = new Thread( () => { ResourceCache.Cache( this, SceneLoadThread ); } );
		ResourceLoadThread.Start();

		Questify.StartQuest( Questify.Instantiate( MainQuest ) );
		Questify.ConnectConditionQueryRequested( OnConditionQueryRequested );
		Questify.ConnectQuestObjectiveCompleted( OnConditionObjectiveCompleted );
		Questify.ConnectQuestCompleted( OnQuestCompleted );
	}
};
