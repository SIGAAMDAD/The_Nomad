using System.Collections.Generic;
using Godot;

public partial class ExplosionFactory : Node2D {
	private partial class Explosion : Node2D {
		public readonly AnimatedSprite2D Animation;
		public readonly AudioStreamPlayer2D AudioChannel;
		
		[Signal]
		public delegate void FinishedEventHandler();

		public Explosion( SpriteFrames frames ) {
			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.Stream = ResourceCache.GetSound( "res://sounds/env/explosion.ogg" );
			AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AudioChannel.Finished += EmitSignalFinished;
			CallDeferred( "add_child", AudioChannel );

			Animation = new AnimatedSprite2D();
			Animation.SpriteFrames = frames;
			CallDeferred( "add_child", Animation );
		}

		public override void _Ready() {
			base._Ready();

			Animation.CallDeferred( "play" );
			AudioChannel.CallDeferred( "play" );
		}
	};

	private HashSet<Explosion> Explosions = new HashSet<Explosion>();
	private static SpriteFrames Frames;

	private static ExplosionFactory Instance;

	public override void _Ready() {
		base._Ready();

		Instance = this;

		ZIndex = 4;
		Frames = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/explosion.tres" );
	}

	private static void RemoveExplosion( Explosion explosion ) {
		Instance.RemoveChild( explosion );
		Instance.Explosions.Remove( explosion );
		explosion.QueueFree();
	}

	public static void AddExplosion( Godot.Vector2 position ) {
		Explosion explosion = new Explosion( Frames );
		explosion.Finished += () => { RemoveExplosion( explosion ); };
		Instance.CallDeferred( "add_child", explosion );
		Instance.Explosions.Add( explosion );
	}
};