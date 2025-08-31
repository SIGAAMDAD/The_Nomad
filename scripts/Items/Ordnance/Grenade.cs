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
using Renown;
using System;

public partial class Grenade : CharacterBody2D {
	protected enum DetonationType {
		Trigger,
		Timer
	};

	[Export]
	public Resource Data;

	protected Sprite2D Icon;
	protected DetonationType Type;
	protected float Damage = 0.0f;
	protected float Radius = 0.0f;
	protected Curve DamageFalloff;
	protected Entity User;

	protected float MoveVelocity = 0.0f;
	protected float MoveAngle = 0.0f;

	protected Area2D ExplosionArea;

	protected virtual void ApplyEntityDamage( in Entity entity ) {
		float distance = GlobalPosition.DistanceTo( entity.GlobalPosition ) / Radius;
		entity.Damage( User, Damage * DamageFalloff.SampleBaked( distance ) );
	}
	protected virtual void CalculateAreaDamage() {
		Godot.Collections.Array<Node2D> nodes = ExplosionArea.GetOverlappingBodies();

		for ( int i = 0; i < nodes.Count; i++ ) {
			if ( nodes[ i ] is Entity entity && entity != null ) {
				ApplyEntityDamage( entity );
			}
		}
	}
	public void OnBlowup() {
		ExplosionArea.SetDeferred( Area2D.PropertyName.Monitoring, true );

		// could this "technically" give players a single frame to escape the blast radius?
		CallDeferred( MethodName.CalculateAreaDamage );
	}
	public void Use( in Entity user, float throwAngle, float velocity = 72.0f ) {
		User = user;
		MoveVelocity = velocity;
		MoveAngle = throwAngle;

		SetPhysicsProcess( true );
	}

	protected Area2D CreateArea( float radius ) {
		CircleShape2D shape = new CircleShape2D();
		shape.Radius = 8.0f;

		CollisionShape2D collision = new CollisionShape2D();
		collision.Shape = shape;

		Area2D area = new Area2D();
		area.CollisionLayer = (uint)( PhysicsLayer.SpecialHitboxes | PhysicsLayer.Player | PhysicsLayer.SpriteEntity );
		area.CollisionMask = (uint)( PhysicsLayer.SpecialHitboxes | PhysicsLayer.Player | PhysicsLayer.SpriteEntity );
		area.AddChild( collision );

		return area;
	}

	public override void _Ready() {
		base._Ready();

		if ( Data == null ) {
			Console.PrintError( string.Format( "Grenade._Ready: a null ItemDefinition isn't valid!" ) );
			QueueFree();
			return;
		}

		Icon = GetNode<Sprite2D>( "Icon" );
		Icon.Texture = (Texture2D)Data.Get( "icon" );

		Godot.Collections.Dictionary properties = Data.Get( "properties" ).AsGodotDictionary();

		Type = (DetonationType)properties[ "type" ].AsUInt32();
		Damage = properties[ "damage" ].AsSingle();
		Radius = properties[ "range" ].AsSingle();
		DamageFalloff = (Curve)properties[ "damage_falloff" ];

		ExplosionArea = CreateArea( Radius );
		ExplosionArea.Monitoring = false;
		AddChild( ExplosionArea );

		if ( Type == DetonationType.Timer ) {
			Timer timer = new Timer();
			timer.WaitTime = properties[ "delay" ].AsSingle();
			timer.OneShot = true;
			timer.Connect( Timer.SignalName.Timeout, Callable.From( OnBlowup ) );
			AddChild( timer );
		} else if ( Type == DetonationType.Trigger ) {
			//
			// create a "pressure plate"
			//

			Area2D trigger = CreateArea( 8.0f );
			trigger.Monitoring = false;
			AddChild( trigger );
		}
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		Godot.Vector2 velocity = Velocity;
		velocity.X = MoveVelocity * (float)Math.Cos( MoveAngle );
		velocity.Y = MoveVelocity * (float)Math.Sin( MoveAngle );
		Velocity = velocity;
		MoveVelocity -= 200.0f * (float)GetPhysicsProcessDeltaTime();
	}
};