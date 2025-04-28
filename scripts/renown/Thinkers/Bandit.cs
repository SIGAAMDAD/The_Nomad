using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Bandit : Thinker {
		private enum State : uint {
			Guarding,
			Attacking,
			Investigating,

			Count
		};

		private readonly float AngleBetweenRays = Mathf.DegToRad( 12.0f );

		[Export]
		private float ViewAngleAmount = 0.0f;
		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionTime = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		private float MaxViewDistance = 0.0f;
		[Export]
		private Resource DefaultWeapon;
		[Export]
		private Resource DefaultAmmo;

		private float SightDetectionAmount = 0.0f;

		private State CurrentState;

		// combat variables
		private Timer AimTimer;
		private Timer AttackTimer;
		private RayCast2D AimLine;
		private bool Aiming = false;

		private Timer LoseInterestTimer;
		private Timer ChangeInvestigationAngleTimer;
		private Tween ChangeLookAngleTweener;

		private WeaponEntity Weapon;
		private AmmoEntity Ammo;

		private MobAwareness Awareness = MobAwareness.Relaxed;

		private AudioStreamPlayer2D BarkChannel;

		private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
		private bool CanSeeTarget = false;
		private RayCast2D[] SightLines = null;
		private Tween MeleeTween;

		private Line2D DetectionMeter;

		private AnimatedSprite2D HeadAnimations;
		private AnimatedSprite2D ArmAnimations;

		private int Fear = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private BarkType LastBark = BarkType.Count;
		private BarkType SequencedBark = BarkType.Count;

		private AIPatrolRoute PatrolRoute;
		private AINodeCache NodeCache;

		public AIPatrolRoute GetPatrolRoute() {
			return PatrolRoute;
		}
		private float RandomFloat( float min, float max ) {
			return (float)( min + Random.NextDouble() * ( min - max ) );
		}

		public override void SetLocation( WorldArea location ) {
			base.SetLocation( location );
			NodeCache = location.GetNodeCache();
		}

		private void SetSuspicious() {
			if ( Awareness != MobAwareness.Suspicious ) {
				if ( !CanSeeTarget ) {
					// start patrolling near the last known position
					PatrolRoute = NodeCache.FindClosestRoute( LastTargetPosition );
				} else {
					SetNavigationTarget( LastTargetPosition );
				}
			}
			Awareness = MobAwareness.Suspicious;
			CurrentState = State.Investigating;
			Bark( BarkType.Confusion, Squad.GetMemberCount() > 0 ? BarkType.CheckItOut : BarkType.Count );
		}
		private void SetAlert() {
			if ( Awareness != MobAwareness.Alert ) {
				// increase alertness
				MaxViewDistance += MaxViewDistance * 0.015f;
				ViewAngleAmount += Mathf.DegToRad( 15.0f );

				for ( int i = 0; i < SightLines.Length; i++ ) {
					HeadAnimations.CallDeferred( "remove_child", SightLines[i] );
					SightLines[i].CallDeferred( "queue_Free" );
				}

				GenerateRayCasts();
			}
			Bark( BarkType.TargetSpotted );
			Awareness = MobAwareness.Alert;
		}

		private bool IsValidTarget( Entity target ) => target.GetFaction() != Faction;
		private AudioStream GetBarkResource( BarkType bark ) {
			switch ( bark ) {
			case BarkType.ManDown:
				return ResourceCache.ManDown[ Random.Next( 0, ResourceCache.ManDown.Length - 1 ) ];
			case BarkType.MenDown2:
				return ResourceCache.ManDown2;
			case BarkType.MenDown3:
				return ResourceCache.ManDown3;
			case BarkType.TargetSpotted:
				return ResourceCache.TargetSpotted[ Random.Next( 0, ResourceCache.TargetSpotted.Length - 1 ) ];
			case BarkType.TargetPinned:
				return ResourceCache.TargetPinned[ Random.Next( 0, ResourceCache.TargetPinned.Length - 1 ) ];
			case BarkType.TargetRunning:
				return ResourceCache.TargetRunning[ Random.Next( 0, ResourceCache.TargetRunning.Length - 1 ) ];
			case BarkType.Confusion:
				return ResourceCache.Confusion[ Random.Next( 0, ResourceCache.Confusion.Length - 1 ) ];
			case BarkType.Alert:
				return ResourceCache.Alert[ Random.Next( 0, ResourceCache.Alert.Length - 1 ) ];
			case BarkType.OutOfTheWay:
				return ResourceCache.OutOfTheWay[ Random.Next( 0, ResourceCache.OutOfTheWay.Length - 1 ) ];
			case BarkType.NeedBackup:
				return ResourceCache.NeedBackup[ Random.Next( 0, ResourceCache.NeedBackup.Length - 1 ) ];
			case BarkType.SquadWiped:
				return ResourceCache.SquadWiped;
			case BarkType.Curse:
				return ResourceCache.Curse[ Random.Next( 0, ResourceCache.Curse.Length - 1 ) ];
			case BarkType.CheckItOut:
				return ResourceCache.CheckItOut[ Random.Next( 0, ResourceCache.CheckItOut.Length - 1 ) ];
			case BarkType.Quiet:
				return ResourceCache.Quiet[ Random.Next( 0, ResourceCache.Quiet.Length - 1 ) ];
			case BarkType.Unstoppable:
				return ResourceCache.Unstoppable;
			case BarkType.Count:
			default:
				break;
			};
			return null;
		}
		private void Bark( BarkType bark, BarkType sequenced = BarkType.Count ) {
			if ( Health <= 0.0f || LastBark == bark ) {
				return;
			}
			LastBark = bark;
			SequencedBark = sequenced;

			PlaySound( BarkChannel, GetBarkResource( bark ) );
		}
		private void GenerateRayCasts() {
			int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
			SightLines = new RayCast2D[ rayCount ];
			for ( int i = 0; i < rayCount; i++ ) {
				RayCast2D ray = new RayCast2D();
				float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
				ray.SetDeferred( "target_position", Godot.Vector2.Right.Rotated( angle ) * MaxViewDistance );
				ray.SetDeferred( "enabled", true );
				ray.SetDeferred( "collision_mask", 2 );
				HeadAnimations.CallDeferred( "add_child", ray );
				SightLines[i] = ray;
			}
		}
		private void SetDetectionColor() {
			if ( SightDetectionAmount > SightDetectionTime ) {
				SightDetectionAmount = SightDetectionTime;
			}
			switch ( Awareness ) {
			case MobAwareness.Relaxed:
				if ( SightDetectionAmount == 0.0f ) {
					DetectionColor.R = 1.0f;
					DetectionColor.G = 1.0f;
					DetectionColor.B = 1.0f;
				} else {
					DetectionColor.R = 0.0f;
					DetectionColor.G = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
					DetectionColor.B = 1.0f;
				}
				break;
			case MobAwareness.Suspicious:
				DetectionColor.R = 0.0f;
				DetectionColor.G = 0.0f;
				DetectionColor.B = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
				break;
			case MobAwareness.Alert:
				DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
				DetectionColor.G = 0.0f;
				DetectionColor.B = 0.0f;
				break;
			};
			DetectionMeter.SetDeferred( "default_color", DetectionColor );
		}
		private void OnChangeInvestigationAngleTimerTimeout() {
			float angle = RandomFloat( 0.0f, 360.0f );
			if ( angle > 135.0f && angle < 225.0f ) {
				angle = 225.0f;
			}
			ChangeLookAngleTweener.CallDeferred( "tween_property", HeadAnimations, "global_rotation", angle, 1.2f );
			ChangeInvestigationAngleTimer.CallDeferred( "start" );
		}
		private void OnLoseInterestTimerTimeout() {
		}

		public override void _Ready() {
			base._Ready();

			ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );

			CurrentState = State.Guarding;

			Squad = GroupManager.GetGroup( GroupType.Military, Faction, GlobalPosition );
			Squad.AddThinker( this );

			LoseInterestTimer = new Timer();
			LoseInterestTimer.WaitTime = LoseInterestTime;
			LoseInterestTimer.Connect( "timeout", Callable.From( OnLoseInterestTimerTimeout ) );
			AddChild( LoseInterestTimer );

			AttackTimer = new Timer();
			AttackTimer.OneShot = true;
			AttackTimer.Connect( "timeout", Callable.From( () => { Aiming = false; } ) );
			AddChild( AttackTimer );

			ChangeInvestigationAngleTimer = new Timer();
			ChangeInvestigationAngleTimer.OneShot = true;
			ChangeInvestigationAngleTimer.WaitTime = 3.5f;
			ChangeInvestigationAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigationAngleTimerTimeout ) );
			AddChild( ChangeInvestigationAngleTimer );

			MeleeTween = CreateTween();
			ChangeLookAngleTweener = CreateTween();

			GenerateRayCasts();
		}

		private void OnAimTimerTimeout() {
			if ( AimLine.GetCollider() is GodotObject collision && collision != null ) {
				if ( collision is Entity entity && entity != null ) {
					if ( entity.GetFaction() == Faction ) {
						if ( GetRelationStatus( entity ) > World.RelationStatus.Dislikes || Fear >= 80 ) {
							// if we hate their guts, don't bother warning them
							// or, if we're scared
							Weapon.Use( WeaponEntity.Properties.TwoHandedFirearm, out float soundLevel, false );
						}
					} else {
						Weapon.Use( WeaponEntity.Properties.TwoHandedFirearm, out float soundLevel, false );
					}
				}
			}
		}
		public override void PickupWeapon( WeaponEntity weapon ) {
			// TODO: evaluate if we actually want it
			if ( Weapon != null ) {
				return;
			}
			Weapon = weapon;
			
			if ( ( Weapon.GetProperties() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				Weapon.SetUseMode( WeaponEntity.Properties.TwoHandedFirearm );
				AttackTimer.SetDeferred( "wait_time", Weapon.GetUseTime() * 3.0f );
				AimTimer.SetDeferred( "wait_time", Weapon.GetReloadTime() * 3.0f );
			}
		}

		private void AttackMelee() {
			MeleeTween.CallDeferred( "tween_property", ArmAnimations, "global_rotation", 0.0f, Weapon.GetWeight() );
		}

		protected override void ProcessAnimations() {
			if ( !Visible ) {
				return;
			}

			if ( Velocity != Godot.Vector2.Zero ) {
				HeadAnimations.CallDeferred( "play", "move" );
				ArmAnimations.CallDeferred( "play", "move" );
				BodyAnimations.CallDeferred( "play", "move" );
			} else {

			}
		}
		protected override void Think() {
			CheckSight();

			switch ( CurrentState ) {
			case State.Attacking:
				break;
			case State.Guarding:
				break;
			};
		}

		private void CheckSight() {
			Entity sightTarget = null;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				sightTarget = SightLines[i].GetCollider() as Entity;
				if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
					break;
				}
			}

			if ( sightTarget == null && SightDetectionAmount > 0.0f ) {
				// out of sight, but we got something
				switch ( Awareness ) {
				case MobAwareness.Relaxed:
					SightDetectionAmount -= SightDetectionAmount * (float)GetProcessDeltaTime();
					if ( SightDetectionAmount < 0.0f ) {
						SightDetectionAmount = 0.0f;
					}
					break;
				case MobAwareness.Suspicious:
					SetSuspicious();
					break;
				};
			}
		}

		protected override void InitRenownStats() {
			if ( !IsPremade ) {
				Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Cities" );
				Family = FamilyCache.GetFamily( nodes[ Random.Next( 0, nodes.Count - 1 ) ] as Settlement, (SocietyRank)Random.Next( 0, (int)SocietyRank.Count ) );
				FirstName = NameGenerator.GenerateName();
				BotName = string.Format( "{0} {1}", FirstName, Family.GetFamilyName() );
				Name = string.Format( "{0}{1}{2}", this, FirstName, Family.GetFamilyName() );
			}

			InitBaseStats();

			GD.Print( "Generated Mercenary " + this + ":" );
			GD.Print( "\t[Renown Data]:" );
			GD.Print( "\t\tBotName: " + BotName );
			GD.Print( "\t\tBirthPlace: " + BirthPlace.GetAreaName() );
			GD.Print( "\t\tFamily: " + Family.GetFamilyName() );
		}
	};
};