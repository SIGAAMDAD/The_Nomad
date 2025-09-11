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
using Renown.Thinkers.GoapCache;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Renown.Thinkers {
	public partial class MobBase : Thinker {
		protected virtual float MAX_VIEW_DISTANCE { get; set; } = 180.0f;
		protected virtual float SIGHT_DETECTION_SPEED { get; set; } = 1.0f;
		protected virtual float ANGLE_BETWEEN_RAYS { get; set; } = Mathf.DegToRad( 8.0f );
		protected virtual float VIEW_ANGLE_AMOUNT { get; set; } = Mathf.DegToRad( 80.0f );

		protected virtual string AGENT_NAME { get; }
		protected virtual string AGENT_ID { get; }

		public readonly float[] SightLines;

		public float Health { get; protected set; } = 0.0f;

		public AnimatedSprite2D? HeadAnimations { get; protected set; }
		public AnimatedSprite2D? TorsoAnimations { get; protected set; }

		public Vector2 LookDir { get; protected set; }
		public float LookAngle { get; protected set; }
		public float AimAngle { get; protected set; }

		public float SightDetectionAmount { get; protected set; }
		public float SightDetectionTime { get; protected set; }

		public Rid Rid { get; protected set; }

		public Entity? Target { get; protected set; }

		public Vector2 LastTargetPosition { get; protected set; }
		public Timer LoseInterestTimer { get; protected set; }
		public bool CanSeeTarget { get; protected set; }
		public MobAwareness Awareness { get; protected set; }

		public Entity SightTarget { get; protected set; }

		public MountainGoap.Agent Agent { get; protected set; }

		public NavigationAgent2D NavAgent { get; protected set; }
		public Rid NavAgentRID { get; protected set; }

		public MobBase() {
			int rayCount = (int)( VIEW_ANGLE_AMOUNT / ANGLE_BETWEEN_RAYS );
			SightLines = new float[ rayCount ];
			for ( int i = 0; i < rayCount; i++ ) {
				SightLines[ i ] = ANGLE_BETWEEN_RAYS * ( i - rayCount / 2.0f );
			}
		}

		/*
		===============
		IsAlert
		===============
		*/
		public bool IsAlert() {
			return Awareness == MobAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
		}

		/*
		===============
		IsSuspicious
		===============
		*/
		public bool IsSuspicious() {
			return Awareness == MobAwareness.Suspicious || SightDetectionAmount >= SightDetectionTime * 0.25f;
		}


		/*
		===============
		SetSuspicious
		===============
		*/
		public virtual void SetSuspicious() {
			Awareness = MobAwareness.Suspicious;
			//			Bark( BarkType.Confusion, Group.Members.Count > 0 ? BarkType.CheckItOut : BarkType.Count );
		}

		/*
		===============
		SetAlert
		===============
		*/
		public virtual void SetAlert() {
			if ( Awareness != MobAwareness.Alert ) {
				//				SetNavigationTarget( NodeCache.FindClosestCover( GlobalPosition, Target.GlobalPosition ).GlobalPosition );
			}
			Target = SightTarget;
			Bark( BarkType.TargetSpotted );
			Awareness = MobAwareness.Alert;
		}

		/*
		===============
		UpdateDetectionColor
		===============
		*/
		public virtual void UpdateDetectionColor() {
		}

		/*
		===============
		Bark
		===============
		*/
		public virtual void Bark( BarkType barkType ) {
		}

		/*
		===============
		SetNavigationTarget
		===============
		*/
		public virtual void SetNavigationTarget( in Vector2 targetPosition ) {
		}

		/*
		===============
		CheckSight
		===============
		*/
		protected virtual void CheckSight() {
			Entity? sightTarget = null;
			Thinker? thinker = this as Thinker;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				RayIntersectionInfo info = GodotServerManager.CheckRayCast( HeadAnimations.GlobalPosition, SightLines[ i ], MAX_VIEW_DISTANCE, Rid );
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
						SightDetectionAmount -= SightDetectionAmount * (float)thinker.GetProcessDeltaTime();
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
				}
				UpdateDetectionColor();
				return;
			}

			if ( sightTarget != null ) {
				if ( sightTarget.Health <= 0.0f && sightTarget.Faction == thinker.Faction ) {
					Bark( BarkType.ManDown );
					Awareness = MobAwareness.Alert;
				} else if ( sightTarget.Faction != thinker.Faction ) {
					SightTarget = sightTarget;
					LastTargetPosition = sightTarget.GlobalPosition;
					CanSeeTarget = true;

					LookDir = thinker.GlobalPosition.DirectionTo( SightTarget.GlobalPosition );
					LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
					AimAngle = LookAngle;

					if ( Awareness >= MobAwareness.Suspicious ) {
						// if we're already suspicious, then detection rate increases as we're more alert
						SightDetectionAmount += SIGHT_DETECTION_SPEED * 2.0f * (float)thinker.GetProcessDeltaTime();
					} else {
						SightDetectionAmount += SIGHT_DETECTION_SPEED * (float)thinker.GetProcessDeltaTime();
					}
				}
			}

			if ( IsAlert() ) {
				SetAlert();
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			UpdateDetectionColor();
		}

		/*
		===============
		GetStateDictionary
		===============
		*/
		protected virtual ConcurrentDictionary<string, object?> GetStateDictionary() {
			return new ConcurrentDictionary<string, object?>();
		}

		/*
		===============
		GetMemoryDictionary
		===============
		*/
		protected virtual Dictionary<string, object?> GetMemoryDictionary() {
			return new Dictionary<string, object?>();
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			Agent = new MountainGoap.Agent(
				name: AGENT_NAME,
				state: null,
				memory: null,
				goals: GoapAllocator.GetGoalList( AGENT_ID ),
				actions: GoapAllocator.GetActionList( AGENT_ID ),
				sensors: GoapAllocator.GetSensorList( AGENT_ID ),
				costMaximum: float.MaxValue,
				stepMaximum: 24
			);
		}
	};
};