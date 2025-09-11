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
using Renown.World;
using ResourceCache;
using Menus;
using Items;

namespace Renown.Thinkers {
	/*
	===================================================================================
	
	GatlingGunner
	
	===================================================================================
	*/
	
	public partial class GatlingGunner : MobBase {
		private enum State : uint {
			Guarding,
			Attacking,
			Investigating,

			Count
		};

		private static readonly float EXPLOSION_DAMAGE = 80.0f;
		private static readonly float BULLET_DAMAGE = 3.5f;
		private static readonly float GATLING_GUN_RANGE = 728.0f;

		private static readonly int ChallengeMode_Score = 50;

		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		private AINodeCache NodeCache;

		private State CurrentState;

		private Godot.Vector2 StartPosition = Godot.Vector2.Zero;
		private float StartHealth = 0.0f;

		private bool HitHead = false;

		// combat variables
		private Timer RevTimer;
		private bool Revved = false;
		private bool Shooting = false;
		private AudioStreamPlayer2D GunChannel;

		private Timer ChangeInvestigationAngleTimer;

		private Line2D DetectionMeter;

		// if we have fear, move slower
		private float SpeedDegrade = 1.0f;

		private int Fear = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		public override void Alert( Entity source ) {
			return; // deaf, but they have a mix track playing inside that helmet
		}

		public override void SetLocation( in WorldArea location ) {
			base.SetLocation( location );
			NodeCache = location.NodeCache;
		}

		public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 && stream != AudioCache.GetStream( "res://sounds/mobs/die_low.ogg" )
				&& stream != AudioCache.GetStream( "res://sounds/mobs/die_high.ogg" ) )
			{
				return;
			}
			base.PlaySound( channel, stream );
		}

		public override void Damage( in Entity source, float amount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, amount );
			//			PlaySound( AudioChannel, ResourceCache.Pain[ RNJesus.IntRange( 0, ResourceCache.Pain.Length - 1 ) ] );

			if ( Health <= 0.0f ) {
				DetectionMeter.CallDeferred( Line2D.MethodName.Hide );

				GunChannel.Stop();
				GunChannel.Set( "parameters/looping", false );

				AudioChannel.Stop();
				AudioChannel.Set( "parameters/looping", false );

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );
				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.CallDeferred( AnimatedSprite2D.MethodName.Hide );
				ArmAnimations.CallDeferred( AnimatedSprite2D.MethodName.Hide );
				//				if ( BodyAnimations.Animation != "die_high" ) {
				//					PlaySound( AudioChannel, AudioCache.GetStream( "res://sounds/mobs/die_low.ogg" ) );
				BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "die" );
				//				}

				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
				GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
				return;
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

			CurrentState = State.Attacking;
			SetNavigationTarget( LastTargetPosition );
		}

		private void SetFear( int amount ) {
			Fear = amount;
			if ( Fear >= 100 ) {
				SpeedDegrade = 0.0f;
				ChangeInvestigationAngleTimer.WaitTime = 0.5f;
			} else if ( Fear >= 80 ) {
				SpeedDegrade = 0.25f;
				ChangeInvestigationAngleTimer.WaitTime = 0.90f;
			} else if ( Fear >= 60 ) {
				SpeedDegrade = 0.5f;
				ChangeInvestigationAngleTimer.WaitTime = 1.2f;
			} else {
				SpeedDegrade = 1.0f;
				ChangeInvestigationAngleTimer.WaitTime = 2.0;
			}
		}

		private void OnChangeInvestigationAngleTimerTimeout() {
			float angle = RNJesus.FloatRange( 0, 360.0f );
			LookAngle = angle;
			AimAngle = angle;
			ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Start );
		}

		public void OnHeadHit( Entity source, float nAmount ) {
			//			CallDeferred( "PlaySound", AudioChannel, AudioCache.GetStream( "res://sounds/mobs/die_high.ogg" ) );
			//			BodyAnimations.Play( "die_high" );
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			HitHead = true;
			Damage( source, Health );
		}
		public void OnBackpackHit( Entity source, float nAmount ) {
			CallDeferred( MethodName.BlowupBackpack );
		}

		private void BlowupBackpack() {
			GetNode<Area2D>( "Animations/BodyAnimations/BlowupArea" ).SetDeferred( Area2D.PropertyName.Monitoring, true );

			Explosion explosion = SceneCache.GetScene( "res://scenes/effects/big_explosion.tscn" ).Instantiate<Explosion>();
			explosion.Radius = 72.0f;
			explosion.Damage = EXPLOSION_DAMAGE;
			explosion.Effects = ExtraAmmoEffects.Incendiary;
			AddChild( explosion );

			base.Damage( this, Health );
			DetectionMeter.CallDeferred( Line2D.MethodName.Hide );

			GunChannel.Stop();
			GunChannel.Set( "parameters/looping", false );

			AudioChannel.Stop();
			AudioChannel.Set( "parameters/looping", false );

			GotoPosition = Godot.Vector2.Zero;
			Flags |= ThinkerFlags.Dead;
			HeadAnimations.Hide();
			ArmAnimations.Hide();
			BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "die" );

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
		}

		private void OnRevTimerTimeout() {
			Revved = !Revved;
		}
		private void OnLoseInterestTimerTimeout() {
			GunChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Stop );
			GunChannel.SetDeferred( "parameters/looping", false );

			AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Stop );
			AudioChannel.SetDeferred( "parameters/looping", false );

			ArmAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );

			CallDeferred( MethodName.PlaySound, AudioChannel, AudioCache.GetStream( "res://sounds/mobs/gatling_dissapointed.ogg" ) );
			if ( Shooting || Revved ) {
				CallDeferred( MethodName.PlaySound, GunChannel, AudioCache.GetStream( "res://sounds/mobs/gatling_revdown.ogg" ) );
				RevTimer.CallDeferred( Timer.MethodName.Start );
			}
			Shooting = false;
			Revved = false;

			CurrentState = State.Investigating;
		}

		private void OnPlayerRestart() {
			GlobalPosition = StartPosition;
			Health = StartHealth;

			DetectionMeter.CallDeferred( Line2D.MethodName.Show );
			ArmAnimations.CallDeferred( AnimatedSprite2D.MethodName.Show );
			HeadAnimations.CallDeferred( AnimatedSprite2D.MethodName.Show );

			NavAgent.AvoidanceEnabled = true;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, false );
			GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).SetDeferred( Hitbox.PropertyName.Monitoring, true );

			SetDeferred( PropertyName.CollisionLayer, (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );
			SetDeferred( PropertyName.CollisionMask, (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );

			Flags &= ~ThinkerFlags.Dead;

			Awareness = MobAwareness.Suspicious;

			Target = null;
			SightTarget = null;

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
			}
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			NavAgent.AvoidanceEnabled = false;

			SetDeferred( PropertyName.CollisionLayer, (uint)PhysicsLayer.None );
			SetDeferred( PropertyName.CollisionMask, (uint)PhysicsLayer.None );

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

		private void OnHellbreakerBegin() {
			AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
			NavAgent.ProcessMode = ProcessModeEnum.Disabled;

			ProcessMode = ProcessModeEnum.Disabled;
		}
		private void OnHellbreakerFinished() {
			AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;

			ProcessMode = ProcessModeEnum.Pausable;
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				AddToGroup( "Enemies" );

				LevelData.Instance.PlayerRespawn += OnPlayerRestart;
				LevelData.Instance.HellbreakerBegin += OnHellbreakerBegin;
				LevelData.Instance.HellbreakerFinished += OnHellbreakerFinished;
			}

			GunChannel = new AudioStreamPlayer2D() {
				Name = nameof( GunChannel ),
				VolumeDb = SettingsData.GetEffectsVolumeLinear()
			};
			AddChild( GunChannel );

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );

			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
			//			ArmAnimations.AnimationFinished += OnArmAnimationFinished;

			Hitbox headHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			GameEventBus.ConnectSignal( headHitbox, Hitbox.SignalName.Hit, this, Callable.From<Entity, float>( OnHeadHit ) );

			// the massive oxygen tank on their back, highly dangerous and explosive
			Hitbox backpackHitbox = BodyAnimations.GetNode<Hitbox>( "BackpackHitbox" );
			GameEventBus.ConnectSignal( backpackHitbox, Hitbox.SignalName.Hit, this, Callable.From<Entity, float>( OnBackpackHit ) );

			CurrentState = State.Guarding;

			if ( GameConfiguration.GameMode != GameMode.JohnWick ) {
				NodeCache ??= Location.NodeCache;
			}

			RevTimer = new Timer() {
				Name = nameof( RevTimer ),
				WaitTime = 2.5f,
				OneShot = true
			};
			GameEventBus.ConnectSignal( RevTimer, Timer.SignalName.Timeout, this, OnRevTimerTimeout );
			AddChild( RevTimer );

			LoseInterestTimer = new Timer() {
				Name = nameof( LoseInterestTimer ),
				OneShot = true,
				WaitTime = LoseInterestTime
			};
			GameEventBus.ConnectSignal( LoseInterestTimer, Timer.SignalName.Timeout, this, OnLoseInterestTimerTimeout );
			AddChild( LoseInterestTimer );

			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );

			ChangeInvestigationAngleTimer = new Timer() {
				Name = nameof( ChangeInvestigationAngleTimer ),
				OneShot = true,
				WaitTime = 1.0f
			};
			GameEventBus.ConnectSignal( ChangeInvestigationAngleTimer, Timer.SignalName.Timeout, this, OnChangeInvestigationAngleTimerTimeout );
			AddChild( ChangeInvestigationAngleTimer );

			StartPosition = GlobalPosition;
			StartHealth = Health;

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				ThreadSleep = Constants.THREADSLEEP_THINKER_PLAYER_IN_AREA;
			}

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
					break;
			}
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			if ( GameConfiguration.GameMode == GameMode.JohnWick ) {
				Target = LevelData.Instance.ThisPlayer;
				Awareness = MobAwareness.Alert;
				SightDetectionAmount = SightDetectionTime;
			}
		}

		protected override void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			ArmAnimations.SetDeferred( AnimatedSprite2D.PropertyName.GlobalRotation, AimAngle );
			HeadAnimations.SetDeferred( AnimatedSprite2D.PropertyName.GlobalRotation, LookAngle );

			if ( Velocity.X < 0.0f ) {
				BodyAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipH, true );
			} else if ( Velocity.X > 0.0f ) {
				BodyAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipH, false );
			}

			if ( LookAngle > 225.0f ) {
				HeadAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, true );
			} else if ( LookAngle < 135.0f ) {
				HeadAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, false );
			}

			if ( AimAngle > 225.0f ) {
				ArmAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, true );
			} else if ( AimAngle < 135.0f ) {
				ArmAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, false );
			}

			if ( Velocity != Godot.Vector2.Zero ) {
				HeadAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "move" );
				BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "move" );
			} else {
				//				if ( Awareness == MobAwareness.Relaxed ) {
				//					BodyAnimations.CallDeferred( "play", "calm" );
				//					HeadAnimations.CallDeferred( "hide" );
				//					ArmAnimations.CallDeferred( "hide" );
				//				} else {
				ArmAnimations.CallDeferred( AnimatedSprite2D.MethodName.Show );
				HeadAnimations.CallDeferred( AnimatedSprite2D.MethodName.Show );
				BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
				HeadAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
				//				}
			}
		}
		private void CalcShots() {
			const int numShots = 24;

			for ( int i = 0; i < numShots; i++ ) {
				RayIntersectionInfo info = GodotServerManager.CheckRayCast(
					GlobalPosition,
					Mathf.DegToRad( RNJesus.FloatRange( 0.0f, 60.0f ) ),
					GATLING_GUN_RANGE,
					GetRid()
				);
				
				if ( info.Collider != null ) {
					if ( info.Collider is Entity entity && entity != null ) {
						entity.Damage( this, BULLET_DAMAGE );
					} else {
						DebrisFactory.Create( info.Position );
					}
				}
			}
		}
		protected override void Think() {
			if ( !Visible ) {
				return;
			}

			CheckSight();

			if ( Target != null ) {
				if ( CanSeeTarget && Awareness == MobAwareness.Alert ) {
					CurrentState = State.Attacking;
					ChangeInvestigationAngleTimer.Stop();
				} else {
					//					CurrentState = State.Investigating;
				}
			}

			switch ( CurrentState ) {
				case State.Investigating:
					if ( ChangeInvestigationAngleTimer.IsStopped() ) {
						ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Start );
					}
					if ( Target != null && CanSeeTarget ) {
						CurrentState = State.Attacking;
					}
					break;
				case State.Attacking:
					if ( !CanSeeTarget && LoseInterestTimer.IsStopped() ) {
						LoseInterestTimer.CallDeferred( "start" );
					}
					if ( !Shooting && CanSeeTarget ) {
						CallDeferred( "PlaySound", GunChannel, AudioCache.GetStream( "res://sounds/mobs/gatling_aiming.ogg" ) );
						GunChannel.SetDeferred( "parameters/looping", false );
						AudioChannel.SetDeferred( "parameters/looping", false );
						Shooting = true;
						ArmAnimations.CallDeferred( "play", "aim" );

						SetNavigationTarget( LastTargetPosition );

						LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
						LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
						AimAngle = LookAngle;
					} else if ( Revved && Shooting ) {
						if ( ArmAnimations.Animation != "attack" ) {
							CallDeferred( "PlaySound", GunChannel, AudioCache.GetStream( "res://sounds/mobs/gatling_shooting.ogg" ) );
							GunChannel.SetDeferred( "parameters/looping", true );
							CallDeferred( "PlaySound", AudioChannel, AudioCache.GetStream( string.Format( "res://sounds/mobs/gatling_laughter{0}.ogg", RNJesus.IntRange( 0, 7 ) ) ) );
							AudioChannel.SetDeferred( "parameters/looping", true );
						}
						ArmAnimations.CallDeferred( "play", "attack" );
						CallDeferred( "CalcShots" );
					}
					if ( !Revved && CanSeeTarget ) {
						if ( RevTimer.IsStopped() ) {
							PlaySound( GunChannel, AudioCache.GetStream( "res://sounds/mobs/gatling_revup.ogg" ) );
							RevTimer.CallDeferred( "start" );
						}
						ArmAnimations.CallDeferred( "play", "aim" );
					}
					break;
				case State.Guarding:
					if ( Awareness > MobAwareness.Relaxed ) {
						CurrentState = State.Investigating;
					}
					break;
			}
			;
		}

		protected override bool MoveAlongPath() {
			float movespeed = MovementSpeed;
			if ( Shooting ) {
				MovementSpeed = 100.0f;
			} else {
				MovementSpeed = movespeed;
			}
			base.MoveAlongPath();
			MovementSpeed = movespeed;
			return true;
		}
	};
};