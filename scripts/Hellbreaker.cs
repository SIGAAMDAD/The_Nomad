using System.Collections.Generic;
using Godot;

public partial class Hellbreaker : Node2D {
	private List<HellbreakerExit> Exits;
	private AudioStreamPlayer Theme;

	private Tween MusicTween;
	private Sprite2D Spawn;

	public bool Activate( Player player ) {
		for ( int i = 0; i < Exits.Count; i++ ) {
			if ( !Exits[i].IsUsed() ) {
				MusicTween = CreateTween();
				MusicTween.CallDeferred( "tween_property", Theme, "volume_db", SettingsData.GetMusicVolumeLinear(), 4.5f );
				Theme.CallDeferred( "play" );
				player.SetDeferred( "global_position", Spawn.GlobalPosition );
				return true;
			}
		}
		return false;
	}

	private void OnHellbreakerExitUsed( HellbreakerExit exit ) {
		MusicTween = CreateTween();
		MusicTween.TweenProperty( Theme, "volume_db", -20.0f, 4.5f );
		MusicTween.Finished += Theme.Stop;
	}
	public void Start( Player player ) {
		//
		// create a sort of teleport effect with fire
		//

		player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/bonfire_enter.ogg" ) );

		player.GetTorsoAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetTorsoAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		Tween TorsoTween = CreateTween();
		TorsoTween.TweenProperty( player.GetTorsoAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		TorsoTween.Connect( "finished", Callable.From( () => { player.GetTorsoAnimation().Material = null; } ) );

		player.GetLegsAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetLegsAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		Tween LegsTween = CreateTween();
		LegsTween.TweenProperty( player.GetLegsAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		LegsTween.Connect( "finished", Callable.From( () => { player.GetLegsAnimation().Material = null; } ) );

		player.GetLeftArmAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetLeftArmAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		Tween LeftArmTween = CreateTween();
		LeftArmTween.TweenProperty( player.GetLeftArmAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		LeftArmTween.Connect( "finished", Callable.From( () => { player.GetLeftArmAnimation().Material = null; } ) );

		player.GetRightArmAnimation().Material = ResourceLoader.Load<Material>( "res://resources/materials/teleport.tres" );
		player.GetRightArmAnimation().Material.Set( "shader_parameter/progress", 1.0f );
		Tween RightArmTween = CreateTween();
		RightArmTween.TweenProperty( player.GetRightArmAnimation().Material, "shader_parameter/progress", 0.0f, 3.0f );
		RightArmTween.Connect( "finished", Callable.From( () => { player.GetRightArmAnimation().Material = null; } ) );
	}

	public override void _Ready() {
		base._Ready();

		Theme = GetNode<AudioStreamPlayer>( "Music" );
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
