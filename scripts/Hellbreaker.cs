using System.Collections.Generic;
using Godot;
using Renown.Thinkers;
using Menus;

public partial class Hellbreaker : Node2D {
	private static List<HellbreakerExit> Exits;
	private AudioStreamPlayer Theme;

	private Tween MusicTween;
	private Sprite2D Spawn;

	private Godot.Vector2 DeathPosition;

	public static Player ThisPlayer;
	public static bool Active = false;
	public static List<Thinker> Demons;

	[Export]
	private HellbreakerSpawner[] Spawners;

	private void SpawnDemons() {
	}
	public static bool CanActivate() {
		if ( Active ) {
			return false;
		}
		for ( int i = 0; i < Exits.Count; i++ ) {
			if ( !Exits[ i ].IsUsed() ) {
				return true;
			}
		}
		return false;
	}
	public bool Activate( Player player ) {
		for ( int i = 0; i < Exits.Count; i++ ) {
			if ( !Exits[i].IsUsed() ) {
				Demons = new List<Thinker>();
				ThisPlayer = player;

				return true;
			}
		}
		Console.PrintLine( "All hellbreaker exits used, respawning to checkpoint..." );

		for ( int i = 0; i < Exits.Count; i++ ) {
			Exits[i].Reset();
		}

		Godot.Collections.Array<Node> spawns = GetTree().GetNodesInGroup( "HellbreakerSpawns" );
		for ( int i = 0; i < spawns.Count; i++ ) {
			spawns[i].Call( "Reset" );
		}

		return false;
	}

	private void OnPlayerRespawn() {
		for ( int i = 0; i < Exits.Count; i++ ) {
			Exits[i].Reset();
		}

		Godot.Collections.Array<Node> spawns = GetTree().GetNodesInGroup( "HellbreakerSpawns" );
		for ( int i = 0; i < spawns.Count; i++ ) {
			spawns[i].Call( "Reset" );
		}
	}

	private void OnHellbreakerExitUsed( HellbreakerExit exit ) {
		MusicTween = CreateTween();
		MusicTween.TweenProperty( Theme, "volume_db", -20.0f, 4.5f );
		MusicTween.Finished += Theme.Stop;

		ThisPlayer.SetDeferred( "global_position", DeathPosition );
		ThisPlayer.SetDeferred( "SetHealth", 100.0f );
		ThisPlayer.SetDeferred( "SetRage", 60.0f );

		/*
		ThisPlayer.GetTorsoAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		ThisPlayer.GetTorsoAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		*/
		ThisPlayer.TorsoAnimation.CallDeferred( "play", "default" );
		ThisPlayer.TorsoAnimation.Show();
		/*
		Tween TorsoTween = CreateTween();
		TorsoTween.TweenProperty( ThisPlayer.GetTorsoAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		TorsoTween.Connect( "finished", Callable.From( () => { ThisPlayer.GetTorsoAnimation().Material = null; } ) );
		*/

		/*
		ThisPlayer.GetLegsAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		ThisPlayer.GetLegsAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		*/
		ThisPlayer.LegAnimation.CallDeferred( "play", "idle" );
		ThisPlayer.LegAnimation.Show();
		/*
		Tween LegsTween = CreateTween();
		LegsTween.TweenProperty( ThisPlayer.GetLegsAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		LegsTween.Connect( "finished", Callable.From( () => { ThisPlayer.GetLegsAnimation().Material = null; } ) );
		*/

		/*
		ThisPlayer.GetLeftArmAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		ThisPlayer.GetLeftArmAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		*/
		ThisPlayer.ArmLeft.Animations.CallDeferred( "play", "idle" );
		ThisPlayer.ArmLeft.Animations.Show();
		//Tween LeftArmTween = CreateTween();
		//LeftArmTween.TweenProperty( ThisPlayer.GetLeftArmAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		//LeftArmTween.Connect( "finished", Callable.From( () => { ThisPlayer.GetLeftArmAnimation().Material = null; } ) );

		/*
		ThisPlayer.GetRightArmAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		ThisPlayer.GetRightArmAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		*/
		ThisPlayer.ArmRight.Animations.CallDeferred( "play", "idle" );
		ThisPlayer.ArmRight.Animations.Show();
		//Tween RightArmTween = CreateTween();
		//RightArmTween.TweenProperty( ThisPlayer.GetRightArmAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		//RightArmTween.Connect( "finished", Callable.From( () => { ThisPlayer.GetRightArmAnimation().Material = null; ThisPlayer.CallDeferred( "BlockInput", false ); } ) );

		Active = false;

		for ( int i = 0; i < Spawners.Length; i++ ) {
			Spawners[ i ].Call( "Clear" );
		}

		GetParent<ChallengeLevel>().ExitHellbreaker();
	}
	public void Start( Player player ) {
		//
		// create a sort of teleport effect with fire
		//

		Show();
		ProcessMode = ProcessModeEnum.Pausable;

		Theme.CallDeferred( "play" );
		Theme.SetDeferred( "parameters/looping", true );

		/*
		MusicTween = CreateTween();
		MusicTween.CallDeferred( "tween_property", Theme, "volume_db", SettingsData.GetMusicVolumeLinear(), 4.5f );

		DeathPosition = player.GlobalPosition;
		player.SetDeferred( "global_position", Spawn.GlobalPosition );

		Active = true;

		CallDeferred( "SpawnDemons" );

		player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/bonfire_enter.ogg" ) );

		player.GetTorsoAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetTorsoAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		player.GetTorsoAnimation().CallDeferred( "play", "default" );
		player.GetTorsoAnimation().Show();
		Tween TorsoTween = CreateTween();
		TorsoTween.TweenProperty( player.GetTorsoAnimation().Material, "shader_parameter/progress", 0.0f, 1.0f );
		TorsoTween.Connect( "finished", Callable.From( () => { player.GetTorsoAnimation().Material = null; } ) );

		player.GetLegsAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetLegsAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		player.GetLegsAnimation().CallDeferred( "play", "idle" );
		player.GetLegsAnimation().Show();
		Tween LegsTween = CreateTween();
		LegsTween.TweenProperty( player.GetLegsAnimation().Material, "shader_parameter/progress", 0.0f, 1.0f );
		LegsTween.Connect( "finished", Callable.From( () => { player.GetLegsAnimation().Material = null; } ) );

		player.GetLeftArmAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetLeftArmAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		player.GetLeftArmAnimation().CallDeferred( "play", "idle" );
		player.GetLeftArmAnimation().Show();
		Tween LeftArmTween = CreateTween();
		LeftArmTween.TweenProperty( player.GetLeftArmAnimation().Material, "shader_parameter/progress", 0.0f, 1.0f );
		LeftArmTween.Connect( "finished", Callable.From( () => { player.GetLeftArmAnimation().Material = null; } ) );

		player.GetRightArmAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetRightArmAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		player.GetRightArmAnimation().CallDeferred( "play", "idle" );
		player.GetRightArmAnimation().Show();
		Tween RightArmTween = CreateTween();
		RightArmTween.TweenProperty( player.GetRightArmAnimation().Material, "shader_parameter/progress", 0.0f, 1.0f );
		RightArmTween.Connect( "finished", Callable.From( () => { player.GetRightArmAnimation().Material = null; } ) );

		player.CallDeferred( "SetHealth", 100.0f );
		player.CallDeferred( "SetRage", 60.0f );
		*/
	}

	public override void _Ready() {
		base._Ready();

		LevelData.Instance.PlayerRespawn += OnPlayerRespawn;

		Theme = GetNode<AudioStreamPlayer>( "Music" );
		Theme.VolumeDb = SettingsData.GetMusicVolumeLinear();

		Spawn = GetNode<Sprite2D>( "Spawn" );

		Exits = new List<HellbreakerExit>();

		Godot.Collections.Array<Node> exits = GetTree().GetNodesInGroup( "HellbreakerExits" );
		for ( int i = 0; i < exits.Count; i++ ) {
			HellbreakerExit exit = exits[i] as HellbreakerExit;
			exit.Connect( "Used", Callable.From<HellbreakerExit>( OnHellbreakerExitUsed ) );
			Exits.Add( exit );
		}
	}
};
