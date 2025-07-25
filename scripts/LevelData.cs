using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Godot;
using Steamworks;

public partial class LevelData : Node2D {
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

	protected virtual void OnResourcesFinishedLoading() {
	}
	protected virtual void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}
		
		Renown.Entity player = PlayerScene.Instantiate<Renown.Entity>();
		( player as NetworkPlayer ).MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( userId );
		player.Call( "SetOwnerId", (ulong)userId );
		Players.Add( userId, player );
		PlayerList.AddChild( player );
	}
	protected virtual void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}
		
		Console.PrintLine(
			string.Format( "{0} has faded away...", ( Players[ userId ] as NetworkPlayer ).MultiplayerData.Username )
		);
		PlayerList.CallDeferred( Node.MethodName.RemoveChild, Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
		SteamLobby.Instance.RemovePlayer( userId );
	}

	public virtual void ApplyShadowQuality() {
		static void nodeIterator( Godot.Collections.Array<Node> children ) {
			for ( int i = 0; i < children.Count; i++ ) {
				nodeIterator( children[ i ].GetChildren() );

				if ( children[ i ] is Light2D light && light != null ) {
					light.SetDeferred( "shadow_enabled", true );
					switch ( SettingsData.GetShadowFilterQuality() ) {
					case ShadowFilterQuality.Off:
						light.SetDeferred( "shadow_filter", (long)Light2D.ShadowFilterEnum.None );
						light.SetDeferred( "shadow_filter_smooth", 0.0f );
						break;
					case ShadowFilterQuality.Low:
						light.SetDeferred( "shadow_filter", (long)Light2D.ShadowFilterEnum.Pcf5 );
						light.SetDeferred( "shadow_filter_smooth", 0.10f );
						break;
					case ShadowFilterQuality.High:
						light.SetDeferred( "shadow_filter", (long)Light2D.ShadowFilterEnum.Pcf13 );
						light.SetDeferred( "shadow_filter_smooth", 0.10f );
						break;
					};
				}
			}
		}
		nodeIterator( GetChildren() );
		
		switch ( SettingsData.GetShadowQuality() ) {
		case ShadowQuality.Low:
			RenderingServer.CanvasSetShadowTextureSize( 256 );
			break;
		case ShadowQuality.Medium:
			RenderingServer.CanvasSetShadowTextureSize( 1024 );
			break;
		case ShadowQuality.High:
			RenderingServer.CanvasSetShadowTextureSize( 2048 );
			break;
		case ShadowQuality.Ultra:
			RenderingServer.CanvasSetShadowTextureSize( 8192 );
			break;
		};
	}

	public override void _EnterTree() {
		base._EnterTree();

		Instance = this;
	}
	public override void _Ready() {
		base._Ready();

		Name = "LevelData";

		GetTree().CurrentScene = this;

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			ThisPlayer.SetupSplitScreen( 0 );
		}

		ResourcesLoadingFinished += OnResourcesFinishedLoading;
		PlayerRespawn += () => { ThisPlayer.BlockInput( false ); };

		if ( SettingsData.GetNetworkingEnabled() && GameConfiguration.GameMode != GameMode.ChallengeMode ) {
			SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
			SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
		}

		PhysicsServer2D.SetActive( true );

		SetProcess( false );
		SetProcessInternal( false );
		
		PauseMenu = ResourceLoader.Load<PackedScene>( "res://scenes/menus/pause_menu.tscn" ).Instantiate<PauseMenu>();
		PauseMenu.Hide();
		PauseMenu.Name = "PauseMenu";
		PauseMenu.Connect( "LeaveLobby", Callable.From( SteamLobby.Instance.LeaveLobby ) );
		AddChild( PauseMenu );

		DebrisFactory debrisFactory = ResourceCache.GetScene( "res://scenes/effects/debris_factory.tscn" ).Instantiate<DebrisFactory>();
		AddChild( debrisFactory );

		BulletShellMesh bulletShellMesh = ResourceCache.GetScene( "res://scenes/effects/bullet_shell_mesh.tscn" ).Instantiate<BulletShellMesh>();
		AddChild( bulletShellMesh );

		BloodParticleFactory bloodParticleFactory = ResourceCache.GetScene( "res://scenes/effects/blood_particle_factory.tscn" ).Instantiate<BloodParticleFactory>();
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
				};

				process.PriorityClass = ProcessPriorityClass.AboveNormal;
			}
		} catch ( Exception ) {
		}
	}
};