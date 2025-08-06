using Godot;
using ImGuiNET;
using MountainGoap;
using Renown.Thinkers.Groups;
using Renown.World;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Renown.Thinkers {
	public partial class Bandit : Thinker {
		private enum State : uint {
			Goto,
			Animate,
			UseSmartObject,

			Count
		};

		private readonly struct SightLine {
			public readonly float Angle;

			public SightLine( float angle ) {
				Angle = angle;
			}
		};

		private static readonly float AngleBetweenRays = Mathf.DegToRad( 8.0f );
		private static readonly float ViewAngleAmount = Mathf.DegToRad( 80.0f );
		private static readonly float MaxViewDistance = 180.0f;

		private static readonly int ChallengeMode_Score = 10;

		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionTime = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		private AINodeCache NodeCache;

		private float SightDetectionAmount = 0.0f;

		private MountainGoap.Agent Agent;

		private Hitbox HeadHitbox;

		private Formations.Formation Formation;

		public bool HitHead { get; private set; }

		private State CurrentState;

		private Godot.Vector2 StartPosition;
		private float StartHealth;

		// combat variables
		private Timer AimTimer;
		private Timer AttackTimer;
		private bool Aiming = false;
		private Line2D AttackMeter;
		private float AttackMeterProgress = 0.0f;
		private Godot.Vector2 AttackMeterFull;
		private Godot.Vector2 AttackMeterDone;

		private Squad Group;

		public Entity SightTarget { get; private set; } = null;

		private Timer LoseInterestTimer;
		private Timer ChangeInvestigationAngleTimer;
		private Timer TargetMovedTimer;

		private WeaponEntity Weapon;
		private AmmoEntity Ammo;
		private AmmoStack AmmoStack;

		private MobAwareness Awareness = MobAwareness.Relaxed;

		private AudioStreamPlayer2D BarkChannel;

		public Godot.Vector2 LastTargetPosition { get; private set; } = Godot.Vector2.Zero;
		private float[] SightLines = null;
		public bool CanSeeTarget { get; private set; } = false;
		private Tween MeleeTween;

		public Entity Target { get; private set; }

		private Line2D DetectionMeter;

		// if we have fear, move slower
		private float SpeedDegrade = 1.0f;

		private int Fear = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private BarkType LastBark = BarkType.Count;
		private BarkType SequencedBark = BarkType.Count;

		private void GenerateRayCasts() {
			int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
			SightLines = new float[ rayCount ];
			for ( int i = 0; i < rayCount; i++ ) {
				SightLines[ i ] = AngleBetweenRays * ( i - rayCount / 2.0f );
			}
		}

		protected override void SetNavigationTarget( Godot.Vector2 position ) {
			base.SetNavigationTarget( position );
			CurrentState = State.Goto;
		}

		public override void Alert( Entity source ) {
			LastTargetPosition = source.GlobalPosition;

			SightDetectionAmount = Mathf.Lerp( SightDetectionAmount, SightDetectionTime, 1.0f / GlobalPosition.DistanceTo( source.GlobalPosition ) );
			LookDir = GlobalPosition.DirectionTo( source.GlobalPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			if ( IsAlert() ) {
				SetAlert();
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			} else {
				Bark( BarkType.Confusion );
			}

			SetNavigationTarget( LastTargetPosition );

			SetFear( Fear + 10 );
		}

		public Godot.Vector2 GetInvestigationPosition() => LastTargetPosition;

		public override void AddStatusEffect( string effectName ) {
			base.AddStatusEffect( effectName );

			if ( Target is Player && Fear >= 80 && effectName == "status_burning" ) {
				SteamAchievements.ActivateAchievement( "ACH_DANCE_DANCE_DANCE" );
			}
		}

		public bool IsAlert() => Awareness == MobAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
		public bool IsSuspicious() => Awareness == MobAwareness.Suspicious || SightDetectionAmount >= SightDetectionTime * 0.25f;

		public override void SetLocation( in WorldArea location ) {
			base.SetLocation( location );
			NodeCache = location.GetNodeCache();
		}

		public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 && stream != ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" )
				&& stream != ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) ) {
				return;
			}
			base.PlaySound( channel, stream );
		}

		public override void Damage( in Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );
			PlaySound( AudioChannel, ResourceCache.Pain[ RNJesus.IntRange( 0, ResourceCache.Pain.Length - 1 ) ] );

			Godot.Vector2 knockbackDirection = source.GlobalPosition.DirectionTo( GlobalPosition ).Normalized();
			const float knockbackAmount = 60.0f;

			Velocity = knockbackDirection * knockbackAmount;

			AimTimer.Stop();
			Aiming = false;

			if ( Health <= 0.0f ) {
				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );

				HeadHitbox.SetDeferred( Area2D.PropertyName.Monitoring, false );
				HeadHitbox.GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );

				DetectionMeter.CallDeferred( MethodName.Hide );

				AimTimer.Stop();
				Aiming = false;

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );

				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;

				HeadAnimations.CallDeferred( MethodName.Hide );
				ArmAnimations.CallDeferred( MethodName.Hide );
				if ( !HitHead ) {
					CallDeferred( MethodName.PlaySound, AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" ) );
					BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "die_low" );
				}
				return;
			}

			if ( source.GetFaction() == Faction ) {
				// "CEASEFIRE!"
				//				Bark( BarkType.Ceasefire );
			}

			if ( Awareness == MobAwareness.Alert ) {

			} else {
				SetAlert();
			}

			float angle = RNJesus.FloatRange( 0, 360.0f );
			HeadAnimations.GlobalRotation = angle;
			ArmAnimations.GlobalRotation = angle;

			Target = source;
			LastTargetPosition = source.GlobalPosition;

			Awareness = MobAwareness.Alert;
			SetNavigationTarget( LastTargetPosition );
		}

		private void SetSuspicious() {
			Awareness = MobAwareness.Suspicious;
			Bark( BarkType.Confusion, Group.Members.Count > 0 ? BarkType.CheckItOut : BarkType.Count );
		}
		private void SetAlert() {
			if ( Awareness != MobAwareness.Alert ) {
				//				SetNavigationTarget( NodeCache.FindClosestCover( GlobalPosition, Target.GlobalPosition ).GlobalPosition );
			}
			Target = SightTarget;
			Bark( BarkType.TargetSpotted );
			Awareness = MobAwareness.Alert;
		}

		private void SetFear( int nAmount ) {
			Fear = nAmount;
		}

		private AudioStream GetBarkResource( BarkType bark ) {
			switch ( bark ) {
			case BarkType.ManDown:
				return ResourceCache.ManDown[ RNJesus.IntRange( 0, ResourceCache.ManDown.Length - 1 ) ];
			case BarkType.MenDown2:
				return ResourceCache.ManDown2;
			case BarkType.MenDown3:
				return ResourceCache.ManDown3;
			case BarkType.TargetSpotted:
				return ResourceCache.TargetSpotted[ RNJesus.IntRange( 0, ResourceCache.TargetSpotted.Length - 1 ) ];
			case BarkType.TargetPinned:
				return ResourceCache.TargetPinned[ RNJesus.IntRange( 0, ResourceCache.TargetPinned.Length - 1 ) ];
			case BarkType.TargetRunning:
				return ResourceCache.TargetRunning[ RNJesus.IntRange( 0, ResourceCache.TargetRunning.Length - 1 ) ];
			case BarkType.Confusion:
				return ResourceCache.Confusion[ RNJesus.IntRange( 0, ResourceCache.Confusion.Length - 1 ) ];
			case BarkType.Alert:
				return ResourceCache.Alert[ RNJesus.IntRange( 0, ResourceCache.Alert.Length - 1 ) ];
			case BarkType.OutOfTheWay:
				return ResourceCache.OutOfTheWay[ RNJesus.IntRange( 0, ResourceCache.OutOfTheWay.Length - 1 ) ];
			case BarkType.NeedBackup:
				return ResourceCache.NeedBackup[ RNJesus.IntRange( 0, ResourceCache.NeedBackup.Length - 1 ) ];
			case BarkType.SquadWiped:
				return ResourceCache.SquadWiped;
			case BarkType.Curse:
				return ResourceCache.Curse[ RNJesus.IntRange( 0, ResourceCache.Curse.Length - 1 ) ];
			case BarkType.CheckItOut:
				return ResourceCache.CheckItOut[ RNJesus.IntRange( 0, ResourceCache.CheckItOut.Length - 1 ) ];
			case BarkType.Quiet:
				return ResourceCache.Quiet[ RNJesus.IntRange( 0, ResourceCache.Quiet.Length - 1 ) ];
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
			DetectionMeter.SetDeferred( Line2D.PropertyName.DefaultColor, DetectionColor );
		}
		private void OnLoseInterestTimerTimeout() {
			// if we have lost interest, go back to starting position
			Awareness = MobAwareness.Suspicious;
			SetNavigationTarget( StartPosition );

			if ( Fear > 60 ) {
				Bark( BarkType.Curse, BarkType.Quiet );
			}

			SetFear( Fear + 20 );
		}

		public void OnHeadHit( Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			HitHead = true;
			CallDeferred( MethodName.PlaySound, AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) );
			BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "die_high" );
			Damage( source, Health );
		}

		private void OnRestartCheckpoint() {
			SetDeferred( PropertyName.GlobalPosition, StartPosition );
			Awareness = MobAwareness.Relaxed;
			Health = StartHealth;
			Flags = 0;
			SightDetectionAmount = 0.0f;
			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, false );
		}
		private void ResetAttackMeter() {
			AttackMeterProgress = AttackMeterFull.X;
			AttackMeter.Points[ 1 ] = AttackMeterFull;
			AttackMeter.Show();
		}
		private void UpdateAttackMeter() {
			AttackMeter.Points[ 1 ].X = AttackMeterProgress;
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			if ( source is Player ) {
				if ( GameConfiguration.GameMode == GameMode.ChallengeMode || GameConfiguration.GameMode == GameMode.JohnWick ) {
					if ( BodyAnimations.Animation == "die_high" ) {
						FreeFlow.AddKill( KillType.Headshot, ChallengeMode_Score );
					} else if ( BodyAnimations.Animation == "die_low" ) {
						FreeFlow.AddKill( KillType.Bodyshot, ChallengeMode_Score );
					}
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				AddToGroup( "Enemies" );

				LevelData.Instance.PlayerRespawn += OnRestartCheckpoint;
			}

			StartPosition = GlobalPosition;
			StartHealth = Health;

			HeadHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			HeadHitbox.Hit += OnHeadHit;

			CurrentState = State.Animate;

//			Group = SquadManager.GetGroup( GroupType.Bandit, Faction, GlobalPosition );
			Group.AddMember( this );

			LoseInterestTimer = new Timer();
			LoseInterestTimer.Name = "LoseInterestTimer";
			LoseInterestTimer.WaitTime = LoseInterestTime;
			LoseInterestTimer.OneShot = true;
			LoseInterestTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnLoseInterestTimerTimeout ) );
			AddChild( LoseInterestTimer );

			ChangeInvestigationAngleTimer = new Timer();
			ChangeInvestigationAngleTimer.Name = "ChangeInvestigationAngleTimer";
			ChangeInvestigationAngleTimer.WaitTime = 2.0f;
			ChangeInvestigationAngleTimer.OneShot = true;
			ChangeInvestigationAngleTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => {
				float angle;
				if ( RNJesus.IntRange( 0, 99 ) > 49 ) {
					angle = RNJesus.FloatRange( -80.0f, 80.0f );
				} else {
					angle = RNJesus.FloatRange( 100.0f, 260.0f );
				}
				angle = Mathf.DegToRad( angle );
				LookAngle = angle;
				AimAngle = angle;
				if ( !CanSeeTarget ) {
					ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Start );
				}
			} ) );

			AttackMeter = GetNode<Line2D>( "AttackMeter" );
			AttackMeterDone = AttackMeter.Points[ 0 ];
			AttackMeter.Points[ 1 ] = AttackMeterFull;
			AttackMeterProgress = AttackMeterFull.X;

			AttackTimer = new Timer();
			AttackTimer.Name = "AttackTimer";
			AttackTimer.OneShot = true;
			AttackTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => { Aiming = false; } ) );
			AddChild( AttackTimer );

			AimTimer = new Timer();
			AimTimer.Name = "AimTimer";
			AimTimer.WaitTime = 2.5f;
			AimTimer.OneShot = true;
			AimTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnAimTimerTimeout ) );
			AddChild( AimTimer );

			BarkChannel = new AudioStreamPlayer2D();
			BarkChannel.Name = "BarkChannel";
			BarkChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( BarkChannel );

			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );

			Weapon = new WeaponEntity();
			Weapon.Name = "Weapon";
			Weapon.Data = ResourceLoader.Load( "res://resources/weapons/firearms/desert_carbine.tres" );

			Ammo = new AmmoEntity();
			Ammo.Name = "Ammo";
			Ammo.Data = ResourceLoader.Load( "res://resources/ammo/556_ammo.tres" );
			AddChild( Ammo );

			AmmoStack = new AmmoStack();
			AmmoStack.SetType( Ammo );
			AmmoStack.Amount = (int)Ammo.Data.Get( "max_stack" );

			AddChild( Weapon );

			Weapon.TriggerPickup( this );
			Weapon.SetOwner( this );
			Weapon.SetReserve( AmmoStack );
			Weapon.SetAmmo( Ammo );

			TargetMovedTimer = new Timer();
			TargetMovedTimer.Name = "TargetMovedTimer";
			TargetMovedTimer.WaitTime = 5.0f;
			TargetMovedTimer.OneShot = true;
			TargetMovedTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => { Bark( BarkType.TargetPinned ); } ) );
			AddChild( TargetMovedTimer );

			switch ( Direction ) {
			case DirType.North:
				LookDir = Godot.Vector2.Up;
				break;
			case DirType.East:
				LookDir = Godot.Vector2.Right;
				break;
			case DirType.South:
				LookDir = Godot.Vector2.Down;
				break;
			case DirType.West:
				LookDir = Godot.Vector2.Left;
				BodyAnimations.FlipH = true;
				break;
			};
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			List<MountainGoap.BaseGoal> goals = new List<MountainGoap.BaseGoal>() {
				new MountainGoap.ComparativeGoal(
					name: "SurviveGoal",
					weight: 1.0f,
					desiredState: new Dictionary<string, ComparisonValuePair>{
						{ "Health", new ComparisonValuePair { Value = 25.0f, Operator = ComparisonOperator.GreaterThan } }
					}
				),
				new MountainGoap.Goal(
					name: "ShootAtTargetGoal",
					weight: 0.9f,
					desiredState: new Dictionary<string, object>{
						{ "HasAmmo", true },
						{ "PlayerVisible", true },
						{ "Target", null }
					}
				),
				new MountainGoap.ComparativeGoal(
					name: "IdleGoal",
					weight: 0.7f,
					desiredState: new Dictionary<string, ComparisonValuePair>{
						{ "Fear", new ComparisonValuePair { Value = 10, Operator = ComparisonOperator.LessThanOrEquals } },
						{ "Awareness", new ComparisonValuePair { Value = MobAwareness.Suspicious, Operator = ComparisonOperator.LessThan } }
					}
				),
				new MountainGoap.ComparativeGoal(
					name: "FollowGoal",
					weight: 0.7f,
					desiredState: new Dictionary<string, ComparisonValuePair>{
						{ "DistanceToTarget", new ComparisonValuePair { Value = 72.0f, Operator = ComparisonOperator.LessThan } }
					}
				)
			};
			List<MountainGoap.Action> actions = new List<MountainGoap.Action> {
				new MountainGoap.Action(
					name: "AimAction",
					permutationSelectors: null,
					executor: ( agent, action ) => {
						RayIntersectionInfo info = GodotServerManager.CheckRayCast( ArmAnimations.GlobalPosition, AimAngle, Ammo.Range, GetRid() );
						if ( info.Collider is Entity entity && entity.GetFaction() == Faction ) {
							agent.State[ "ClearLineOfFire" ] = false;
							Bark( BarkType.OutOfTheWay );
							return ExecutionStatus.Failed;
						}
						agent.State[ "ClearLineOfFire" ] = true;
						if ( AmmoStack.Amount == 0 ) {
							return ExecutionStatus.NotPossible;
						}
						if ( AimTimer.TimeLeft == 0.0f && (bool)Agent.State[ "Aiming" ] ) {
							Agent.State[ "Aiming" ] = false;
							return ExecutionStatus.Succeeded;
						} else if ( AimTimer.TimeLeft > AimTimer.WaitTime / 4.0f ) {
							Vector2 direction = GlobalPosition.DirectionTo( LastTargetPosition );
							AimAngle = Mathf.Atan2( direction.Y, direction.X );
							LookAngle = AimAngle;
						}
						return ExecutionStatus.Executing;
					},
					cost: 1.0f,
					costCallback: ( agent, currentState ) => {
						return 1.0f;
					},
					preconditions: new Dictionary<string, object>{
						{ "Aiming", true },
						{ "HasAmmo", true }
					},
					comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
						{ "Fear", new ComparisonValuePair { Value = 80, Operator = ComparisonOperator.LessThan } }
					}
				),
				new MountainGoap.Action(
					name: "ShootWeaponAction",
					permutationSelectors: null,
					executor: ( agent, action ) => {
						if ( AmmoStack.Amount == 0 ) {
							Bark( BarkType.Curse );
							return ExecutionStatus.Failed;
						}
						return ExecutionStatus.Executing;
					},
					cost: 1.0f,
					costCallback: ( agent, currentState ) => {
						return 1.0f;
					},
					preconditions: new Dictionary<string, object>{
						{ "ClearLineOfFire", true },
						{ "Aiming", false },
					}
				),
				new MountainGoap.Action(
					name: "ReloadAction",
					permutationSelectors: null,
					executor: ( agent, action ) => {
						if ( Weapon.WeaponTimer.TimeLeft > 0.0f && Weapon.CurrentState == WeaponEntity.WeaponState.Reload ) {
							return ExecutionStatus.Executing;
						}
						return ExecutionStatus.Succeeded;
					},
					cost: 1.0f,
					costCallback: ( agent, currentState ) => {
						return 1.0f;
					},
					preconditions: new Dictionary<string, object>{
						{ "HasAmmo", false }
					},
					comparativePreconditions: null,
					postconditions: new Dictionary<string, object>{
						{ "HasAmmo", true }
					}
				),
				new MountainGoap.Action(
					name: "CoverAction",
					permutationSelectors: null,
					executor: ( agent, action ) => {
						return ExecutionStatus.Executing;
					},
					cost: 1.0f,
					costCallback: null, // TODO: make cost determined by the distance
					preconditions: null
				)
			};

			Agent = new MountainGoap.Agent(
				name: "Bandit",
				state: new ConcurrentDictionary<string, object> {
					{ "Health", Health },
					{ "HasAmmo", false },
					{ "PlayerVisible", CanSeeTarget },
					{ "PlayerInRange", GlobalPosition.DistanceTo( LastTargetPosition ) < 200.0f },
					{ "UnderFire", false },
					{ "HasCover", false },
					{ "IsAlerted", false },
					{ "Awareness", MobAwareness.Relaxed },
					{ "SquadSize", 0 },
					{ "WeaponState", Weapon.CurrentState },
					{ "Target", null },
					{ "ClearLineOfFire", false },
					{ "SquadTactic", Group.CurrentTactic }
				},
				memory: new Dictionary<string, object> {
					{ "WarnedFriendlies", false },
					{ "AimTime", 0.0f }
				},
				goals: goals,
				actions: actions
			);

			GenerateRayCasts();
		}

		private void OnAimTimerTimeout() {
			RayIntersectionInfo collision = GodotServerManager.CheckRayCast( GlobalPosition, AimAngle, Ammo.Range, GetRid() );
			if ( collision.Collider is Entity entity && entity != null && entity.GetFaction() == Faction ) {
				Bark( BarkType.OutOfTheWay );
				SetNavigationTarget( GlobalPosition + new Godot.Vector2( Godot.Vector2.Right.X + 50.0f, Godot.Vector2.Right.Y + 20.0f ) );
				return;
			}

			AttackTimer.Start();
			Weapon.SetAttackAngle( AimAngle );
			Weapon.CallDeferred( WeaponEntity.MethodName.SetUseMode, (uint)WeaponEntity.Properties.TwoHandedFirearm );
			Weapon.CallDeferred( WeaponEntity.MethodName.UseDeferred, (uint)WeaponEntity.Properties.TwoHandedFirearm );
		}
		public override void PickupWeapon( WeaponEntity weapon ) {
			// TODO: evaluate if we actually want it
			Weapon = weapon;

			if ( ( Weapon.PropertyBits & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				Weapon.SetUseMode( WeaponEntity.Properties.TwoHandedFirearm );
				AttackTimer.SetDeferred( Timer.PropertyName.WaitTime, Weapon.UseTime );
			}
		}

		private void AttackMelee() {
			MeleeTween = CreateTween();
			MeleeTween.CallDeferred( Tween.MethodName.TweenProperty, ArmAnimations, PropertyName.GlobalRotation, 0.0f, Weapon.Weight );
		}

		protected override void ProcessAnimations() {
			base.ProcessAnimations();
		}

		private void SyncGOAPState() {
			Agent.State[ "Health" ] = Health;
			Agent.State[ "Fear" ] = Fear;
			Agent.State[ "Awareness" ] = Awareness;
			Agent.State[ "SquadSize" ] = Group.Members.Count;
			Agent.State[ "HasAmmo" ] = AmmoStack.Amount > 0;
			Agent.State[ "PlayerVisible" ] = CanSeeTarget;
			Agent.State[ "WeaponState" ] = Weapon.CurrentState;
			if ( Fear > 80 ) {
				Agent.State[ "ClearLineOfFire" ] = true;
			}
		}
		protected override void Think() {
			SyncGOAPState();
			CheckSight();

			/*
			if ( Awareness == MobAwareness.Relaxed ) {
				return;
			}

			// do we have a target?
			LookAtTarget();

			CheckParry();

			// can we see the target?
			if ( CanSeeTarget ) {

				CheckAttack();

			} else {

				if ( CurrentState == State.Investigating ) {
					// are we at the last known position?
					if ( GlobalPosition == LastTargetPosition ) {

						// start the lose interest timer
						if ( LoseInterestTimer.IsStopped() ) {
							LoseInterestTimer.CallDeferred( Timer.MethodName.Start );
						}
						if ( ChangeInvestigationAngleTimer.IsStopped() ) {
							ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Start );
						}

						// cycle
						return;
					} else {
						SetNavigationTarget( LastTargetPosition );
					}
				}

				// investigate the last known position
				if ( CurrentState != State.Investigating ) {
					Awareness = MobAwareness.Suspicious;
					SetNavigationTarget( LastTargetPosition );
					CurrentState = State.Investigating;

					// cycle
					return;
				}
			}
			*/
		}

		public void LookAtTarget() {
			Godot.Vector2 position = LastTargetPosition;
			if ( CanSeeTarget && SightTarget != null ) {
				ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Stop );
				position = SightTarget.GlobalPosition;
			}
			LookDir = GlobalPosition.DirectionTo( position );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}

		private void CheckParry() {
			if ( AimTimer.TimeLeft > 0.0f && Target is Player player && player != null ) {
				if ( ( player.GetFlags() & Player.PlayerFlags.LightParrying ) != 0 ) {

					// shoot early
					AimTimer.Stop();
					OnAimTimerTimeout();
				}
			}
		}

		private void CheckAttack() {
			// are we in range?
			if ( GlobalPosition.DistanceTo( LastTargetPosition ) > 200.0f ) {

				LookAtTarget();

				// if not, stop aiming and move in closer
				Aiming = false;
				AimTimer.CallDeferred( Timer.MethodName.Stop );

				const float interp = 0.5f;
				Godot.Vector2 position1 = GlobalPosition;
				Godot.Vector2 position2 = LastTargetPosition;
				SetNavigationTarget( new Godot.Vector2( position1.X * ( 1 - interp ) + position2.X * interp, position1.Y * ( 1 - interp ) + position2.Y * interp ) );

				// cycle
				return;
			}

			// are we about to shoot?
			if ( Aiming ) {
				CallDeferred( MethodName.UpdateAttackMeter );

				// allow adjustments until we have 15% time left
				if ( AimTimer.TimeLeft > AimTimer.WaitTime * 0.15f ) {
					LookAtTarget();

					CallDeferred( MethodName.StopMoving );
				}
			} else {

				// if not, start
				CallDeferred( MethodName.ResetAttackMeter );

				Tween Tweener = CreateTween();
				Tweener.CallDeferred( Tween.MethodName.TweenProperty, this, "AttackMeterProgress", AttackMeterDone.X, AimTimer.WaitTime );
				Tweener.CallDeferred( Tween.MethodName.Connect, Tween.SignalName.Finished, Callable.From( AttackMeter.Hide ) );

				AimTimer.CallDeferred( Timer.MethodName.Start );
				Aiming = true;

				CallDeferred( MethodName.StopMoving );
			}
		}

		public void CheckSight() {
			Entity sightTarget = null;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				RayIntersectionInfo info = GodotServerManager.CheckRayCast( HeadAnimations.GlobalPosition, SightLines[ i ], MaxViewDistance, GetRid() );
				sightTarget = info.Collider as Entity;
				if ( sightTarget != null ) {
					break;
				} else {
					sightTarget = null;
				}
			}

			if ( SightDetectionAmount >= SightDetectionTime * 0.25f && SightDetectionAmount < SightDetectionTime * 0.90f && sightTarget == null ) {
				SetSuspicious();
				SetNavigationTarget( LastTargetPosition );
				if ( LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.Start();
				}
			}

			CanSeeTarget = sightTarget != null;

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
				case MobAwareness.Alert:
					SetAlert();
					break;
				};
				SetDetectionColor();
				return;
			}

			if ( sightTarget != null ) {
				if ( sightTarget.GetHealth() <= 0.0f && sightTarget.GetFaction() == Faction ) {
					Bark( BarkType.ManDown );

					Awareness = MobAwareness.Alert;
				} else if ( sightTarget.GetFaction() != Faction ) {
					SightTarget = sightTarget;
					LastTargetPosition = sightTarget.GlobalPosition;
					CanSeeTarget = true;

					LookDir = GlobalPosition.DirectionTo( SightTarget.GlobalPosition );
					LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
					AimAngle = LookAngle;

					if ( Awareness >= MobAwareness.Suspicious ) {
						// if we're already suspicious, then detection rate increases as we're more alert
						SightDetectionAmount += SightDetectionSpeed * 2.0f * (float)GetProcessDeltaTime();
					} else {
						SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
					}
				}
			}

			if ( IsAlert() ) {
				SetAlert();
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			SetDetectionColor();
		}
	};
};