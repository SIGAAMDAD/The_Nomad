using Godot;
using Renown;

public partial class Bullet : Area2D {
	public AmmoEntity AmmoType;
	public Godot.Vector2 Direction;

	private float Velocity = 0.0f;
	private int InstanceId = 0;

	private void OnCollision( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		float damage = AmmoType.GetDamage();
		if ( body is Entity entity && entity != null && entity != GetParent<WeaponEntity>().GetParent<Entity>() ) {
			float distance = GetParent<WeaponEntity>().GetParent<Entity>().GlobalPosition.DistanceTo( entity.GlobalPosition );
			if ( distance > 20.0f ) {
				// out of bleed range, no healing
//				frameDamage -= damage;
			}
			distance /= AmmoType.GetRange();
			damage *= AmmoType.GetDamageFalloff( distance );
			entity.Damage( GetParent<WeaponEntity>().GetParent<Entity>(), damage );

			AmmoEntity.ExtraEffects effects = AmmoType.GetEffects();
			if ( ( effects & AmmoEntity.ExtraEffects.Incendiary ) != 0 ) {
				entity.AddStatusEffect( "status_burning" );
			} else if ( ( effects & AmmoEntity.ExtraEffects.Explosive ) != 0 ) {
				entity.CallDeferred( MethodName.AddChild, ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		} else if ( body is Area2D parryBox && parryBox != null && parryBox.HasMeta( "ParryBox" ) ) {
			float distance = GetParent<WeaponEntity>().GetParent<Entity>().GlobalPosition.DistanceTo( parryBox.GlobalPosition );
			distance /= AmmoType.GetRange();
			damage *= AmmoType.GetDamageFalloff( distance );
			parryBox.GetParent<Player>().OnParry( this, damage );
		} else if ( body is Hitbox hitbox && hitbox != null && (Entity)hitbox.GetMeta( "Owner" ) != GetParent<Entity>() ) {
			Entity owner = (Entity)hitbox.GetMeta( "Owner" );
			hitbox.OnHit( GetParent<WeaponEntity>().GetParent<Entity>() );
			AmmoEntity.ExtraEffects effects = AmmoType.GetEffects();
			if ( ( effects & AmmoEntity.ExtraEffects.Incendiary ) != 0 ) {
				( (Node2D)hitbox.GetMeta( "Owner" ) as Entity ).AddStatusEffect( "status_burning" );
			} else if ( ( effects & AmmoEntity.ExtraEffects.Explosive ) != 0 ) {
				owner.CallDeferred( MethodName.AddChild, ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		} else {
			AmmoEntity.ExtraEffects effects = AmmoType.GetEffects();
			if ( ( effects & AmmoEntity.ExtraEffects.Explosive ) != 0 ) {
				Explosion explosion = ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>();
				explosion.GlobalPosition = GlobalPosition;
				body.CallDeferred( MethodName.AddChild, explosion );
			}
			DebrisFactory.Create( GlobalPosition );
		}
		CallDeferred( MethodName.QueueFree );
	}

	public override void _Ready() {
		base._Ready();

		BulletShellMesh.AddShellDeferred( GetParent<WeaponEntity>().GetParent<Entity>(), AmmoType.Data );
		InstanceId = BulletMesh.AddBullet( this );

		Velocity = AmmoType.GetVelocity();

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnCollision ) );
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		CallDeferred( "MoveBullet" );
	}
	private void MoveBullet() {
	//	Velocity -= 1.0f * (float)GetPhysicsProcessDeltaTime();
		GlobalPosition += Direction * Velocity * (float)GetPhysicsProcessDeltaTime();
		BulletMesh.SetBulletTransform( AmmoType.Data, InstanceId, GlobalPosition );
	}
};