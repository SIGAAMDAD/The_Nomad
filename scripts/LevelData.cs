/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Godot;
using ImGuiNET;
using Steamworks;
using Steam;
using Menus;
using Multiplayer;
using ResourceCache;

public partial class LevelData : Node2D {
	[Export]
	protected NavigationRegion2D GlobalNavigationMesh;

	protected PauseMenu PauseMenu;

	protected Thread ResourceLoadThread;
	protected Thread SceneLoadThread;
	
	protected Dictionary<CSteamID, Renown.Entity> Players = null;
	protected PackedScene PlayerScene = null;

	protected static readonly string[] DeathQuotes = [
		"\"Add a step forward to it.\"\nA Spartan mother to her son when he complained his sword was too short.",
		"\"By Heracles! A man's valor is dead.\"\nArchidamus when he saw a catapult fire for the first time.",
		"\"Pain leaves you as soon as it's done teaching you.\"\nBruce Lee",
		"\"The difficult we do immediately. The impossible takes a little longer.\"\nCharles Alexandre de Calonne",
		"\"I am not a product of my cicumstances, I am a product of my decisions.\"\nStephen Covey",
		"\"No one is more hated than the ones who speak the truth.\"\nPlato",
		"\"A person who has the same views of the world when they're 50 as when they are 30 has wasted 20 years of their life.\"\nMuhammad Ali",
		"\"It's hard to be a person who never gives up.\"\nBabe Ruth",
		"\"The most difficult thing is the decision to act, the rest is merely tenacity.\"\nAmelia Earhart",
		"\"The happiness of your life depends upon the quality of your thoughts.\"\nMarcus Aurelius",
		"\"...Don't allow your emotions to overpower your intelligence.\"\nMorgan Freeman",
		"\"Set a goal so bit that you can't achieve it until you grow into the person who can\"\nZig Ziglar",
		"\"Before you embark on a journey of revenge, you should first dig two graves.\"\nConfucius",
		"\"War does not determine who is right, only who is left.\"\nBertrand Russell",
		"\"War is the science of destruction.\n\"John Abbott",
		"\"Freedom is something that dies unless it's used.\"\nHunter S. Thompson",
		"\"Someone has to die in order that the rest of us should value life more.\"\nVirginia Woolf",
		"\"A thing is not necessarily true because a man dies for it.\"\nOscar Wilde",
		"\"When you go through hardships and decide not to surrender, that is strength.\"\nMahatma Gandhi",
		"\"You must not fear death, my lads; defy her, and you drive her into the enemy's ranks.\"\nNapoleon Bonaparte",
		"\"I am more afraid of our own mistakes than our enemies' design.\"\nPericles",
		"\"Success is not final, failure is not fatal: it is the courage to continue that counts.\"\nWinston Churchill",
		"\"They died hard, those savage men - like wounded wolves at bay. They were filthy, and they were lousy, and they stunk. And I loved them.\"\nGeneral Douglas MacArthur",
		"\"There's no honorable way to kill, no gentle way to destroy. There is nothing good in war. Except its ending.\"\nAbraham Lincoln",
		"\"The death of one man is a tragedy. The death of millions is a statistic.\"\nJoseph Stalin",
		"\"Death solves all problems - no man, no problem.\"\nJoseph Stalin",
		"\"All wars are civil wars, because all men are brothers.\"\nFrancois Fenelon",
		"\"Older men declare war. But it is the youth that must fight and die.\"\nHerbert Hoover",
		"\"A soldier will fight long and hard for a bit of colored ribbon.\"\nNapolean Bonaparte",
		"\"They wrote in the old days that it is sweet and fitting to die for one's country. But in modern war, there is nothing sweet nor fitting in your dying. You will die like a dog for no good reason.\"\nErnest Hemingway",
		"\"When you have to kill a man it costs nothing to be polite.\"\nWinston Churchill",
		"\"History will be kind to me for I intend to write it.\"\nWinston Churchill",
		"\"When you get to the end of your rope, tie a knot and hang on.\"\nFranklin D. Roosevelt",
		"\"Never interrupt your enemy when he is making a mistake.\"\nNapolean Bonaparte",
		"\"It is better to die on your feet than to live on your knees!\"\nEmiliano Zapata",
		"\"War is a series of catastrophes which result in victory.\"\nGeorges Clemenceau",
		"\"In war, truth is the first casualty.\"\nAeschylus",
		"\"War is delightful to those who have not yet experienced it.\"\nErasmus",
		"\"I know not with what weapons World War III will be fought, but World War IV will be fought with sticks and stones.\"\nAlbert Einstein"
	];

	[Export]
	public Player ThisPlayer;
	[Export]
	protected Node PlayerList = null;

	public static LevelData Instance;

	[Signal]
	public delegate void ResourcesLoadingFinishedEventHandler();
	[Signal]
	public delegate void PlayerRespawnEventHandler();
	[Signal]
	public delegate void HellbreakerBeginEventHandler();
	[Signal]
	public delegate void HellbreakerFinishedEventHandler();

	[Signal]
	public delegate void ExitLevelEventHandler();

	/*
	===============
	OnResourcesFinishedLoading
	===============
	*/
	protected virtual void OnResourcesFinishedLoading() {
	}

	/*
	===============
	OnPlayerJoined
	===============
	*/
	protected virtual void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( $"Adding {steamId} to game..." );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}

		Renown.Entity player = PlayerScene.Instantiate<Renown.Entity>();
		( player as NetworkPlayer ).MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( userId );
		player.Call( NetworkPlayer.MethodName.SetOwner, (ulong)userId );
		Players.Add( userId, player );
		PlayerList.AddChild( player );
	}

	/*
	===============
	OnPlayerLeft
	===============
	*/
	protected virtual void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}

		Console.PrintLine(
			$"{( Players[ userId ] as NetworkPlayer ).MultiplayerData.Username} has faded away..."
		);
		PlayerList.CallDeferred( Node.MethodName.RemoveChild, Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
		SteamLobby.Instance.RemovePlayer( userId );
	}

	/*
	===============
	_EnterTree
	===============
	*/
	public override void _EnterTree() {
		base._EnterTree();

		Instance = this;
		EntityManager.Init();
	}

	/*
	===============
	_ExitTree
	===============
	*/
	public override void _ExitTree() {
		base._ExitTree();

		EmitSignalExitLevel();
	}

	/*
	===============
	_Ready
	===============
	*/
	/// <summary>
	/// godot initialization override
	/// </summary>
	public override void _Ready() {
		base._Ready();

		Name = "LevelData";

		GetTree().CurrentScene = this;

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			ThisPlayer.SetupSplitScreen( 0 );
		}

		ResourcesLoadingFinished += OnResourcesFinishedLoading;
		PlayerRespawn += () => { ThisPlayer.BlockInput( false ); };

		if ( SettingsData.EnableNetworking && GameConfiguration.GameMode != GameMode.ChallengeMode ) {
			SteamLobby.Instance.Connect( SteamLobby.SignalName.ClientJoinedLobby, Callable.From<ulong>( OnPlayerJoined ) );
			SteamLobby.Instance.Connect( SteamLobby.SignalName.ClientLeftLobby, Callable.From<ulong>( OnPlayerLeft ) );
		}

		GodotServerManager.InitServers( GlobalNavigationMesh.GetNavigationMap() );

		SetProcess( false );
		SetProcessInternal( false );

		PauseMenu = ResourceLoader.Load<PackedScene>( "res://scenes/menus/pause_menu.tscn" ).Instantiate<PauseMenu>();
		PauseMenu.Hide();
		PauseMenu.Name = "PauseMenu";
		PauseMenu.Connect( PauseMenu.SignalName.LeaveLobby, Callable.From( SteamLobby.Instance.LeaveLobby ) );
		AddChild( PauseMenu );

		DebrisFactory debrisFactory = SceneCache.GetScene( "res://scenes/effects/debris_factory.tscn" ).Instantiate<DebrisFactory>();
		AddChild( debrisFactory );

		BulletShellMesh bulletShellMesh = SceneCache.GetScene( "res://scenes/effects/bullet_shell_mesh.tscn" ).Instantiate<BulletShellMesh>();
		AddChild( bulletShellMesh );

		BloodParticleFactory bloodParticleFactory = SceneCache.GetScene( "res://scenes/effects/blood_particle_factory.tscn" ).Instantiate<BloodParticleFactory>();
		AddChild( bloodParticleFactory );

		//
		// force the game to run at the highest priority possible
		//
		try {
			using ( Process process = Process.GetCurrentProcess() ) {
				process.PriorityBoostEnabled = true;

				switch ( OS.GetName() ) {
					case "Linux":
					case "Windows":
						process.ProcessorAffinity = System.Environment.ProcessorCount;
						break;
				}

				process.PriorityClass = ProcessPriorityClass.AboveNormal;
			}
		} catch ( Exception ) {
		}
	}
};