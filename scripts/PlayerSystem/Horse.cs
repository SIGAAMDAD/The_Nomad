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
using ResourceCache;

public partial class Horse : CharacterBody2D {
	private Player User;
	private AnimatedSprite2D Body;

	private Area2D MountArea;

	private AudioStreamPlayer2D AudioChannel;
	private AudioStreamPlayer2D SnortChannel;

	[Signal]
	public delegate void PlayerMountHorseEventHandler();

	private void OnMountAreaBodyShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			SetDeferred( "global_position", player.GlobalPosition );
			MountArea.SetDeferred( "monitoring", false );

			AudioChannel.Stream = AudioCache.GetStream( "res://sounds/env/mount_horse.ogg" );
			AudioChannel.Play();
			
			EmitSignalPlayerMountHorse();
		}
	}

	public override void _Ready() {
		base._Ready();

		Body = GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );

		SnortChannel = GetNode<AudioStreamPlayer2D>( "SnortChannel" );
		SnortChannel.Connect( "finished", Callable.From( () => {
			if ( Velocity == Godot.Vector2.Zero ) {
				return;
			}
			SnortChannel.Stream = AudioCache.GetStream( string.Format( "res://sounds/env/horse_snort_{0}.ogg", RNJesus.IntRange( 0, 2 ) ) );
			SnortChannel.Play();
		} ) );

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.Connect( "finished", Callable.From( () => {
			if ( Velocity == Godot.Vector2.Zero ) {
				return;
			}
			AudioChannel.Stream = AudioCache.GetStream( "res://sounds/env/gallop_gravel.ogg" );
			AudioChannel.Play();
		} ) );

		MountArea = GetNode<Area2D>( "MountArea" );
		MountArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnMountAreaBodyShapeEntered ) );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( ( Engine.GetProcessFrames() % 20 ) != 0 ) {
			return;
		}

		if ( Velocity == Godot.Vector2.Zero ) {
			return;
		}

		if ( !AudioChannel.Playing ) {
			AudioChannel.Stream = AudioCache.GetStream( "res://sounds/env/gallop_gravel.ogg" );
			AudioChannel.Play();

			SnortChannel.Stream = AudioCache.GetStream( string.Format( "res://sounds/env/horse_snort_{0}.ogg", RNJesus.IntRange( 0, 2 ) ) );
			SnortChannel.Play();
		}
	}
};