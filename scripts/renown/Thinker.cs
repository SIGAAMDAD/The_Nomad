using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Renown {
	public partial class Relationship : Resource {
		public enum Type {
			Acquaintance,
			Friend,
			
			Sibling,
			Parent,
			Guardian,
			
			Lover,
			Engaged,
			Spouse,
			
			Dislikes,
			Enemy,
			RapBeef
		};
		
		public static int REQUIRED_SCORE_DISLIKES = 50;
		public static int REQUIRED_SCORE_ENEMY = 35;
		public static int REQUIRED_SCORE_RAPBEEFs = 10;
		
		private Thinker Thinker1;
		private Thinker Thinker2;
		private Type Status;
		private int Score = 50;
//		[Export]
//		private EventHistory History;
		
		public Relationship( Thinker thinker1, Thinker thinker2, Type type ) {
			Thinker1 = thinker1;
			Thinker2 = thinker2;
			Status = type;
			Score = 50;
		}
		public Relationship() {
		}

		private void EvaluateRelationStatus() {
			if ( Score > 50 ) {
				Status = Type.Acquaintance;
			} else if ( Score <= 50 ) {
				Status = Type.Dislikes;
				if ( Score < 35 ) {
					Status = Type.Enemy;
				}
				if ( Score < 10 ) {
					Status = Type.RapBeef;
				}
			}
		}
		
		public Thinker GetPerson1() {
			return Thinker1;
		}
		public Thinker GetPerson2() {
			return Thinker2;
		}
		public void DecreaseScore( int nAmount ) {
			Score -= nAmount;
			EvaluateRelationStatus();
		}
		public void IncreaseScore( int nAmount ) {
			Score += nAmount;
			EvaluateRelationStatus();
		}
		public Type GetStatus() {
			return Status;
		}
		public int GetScore() {
			return Score;
		}
//		public EventHistory GetHistory() {
//			return History;
//		}
	};
	
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : CharacterBody2D {
		public enum Occupation {
			None,
			
			Blacksmith,
			
			Mercenary,
			Politician,
			
			Count
		};
		
		[Export]
		private string BotName;
		[Export]
		private Godot.Collections.Array<Relationship> SetRelations; // preset relationships
		[Export]
		private Occupation Job;
//		[Export]
//		private EventHistory History;
//		[Export]
//		private Godot.Collections.Array<Trait> Traits;
		[Export]
		private uint Age = 0; // in years
		[Export]
		private Settlement Location = null;
		
		[ExportCategory("Stats")]
		[Export]
		private uint Strength;
		[Export]
		private uint Intelligence;
		[Export]
		private uint Constitution;
		[Export]
		private float Health = 100.0f;
		[Export]
		private bool HasMetPlayer = false;
		[Export]
		private Godot.Collections.Dictionary<string, bool> Personality;
		
		private Timer ThinkInterval;
		private Agent Agent;
		private List<Relationship> Relations;
		private NavigationAgent2D Navigation;
		private Godot.Vector2 AngleDir;
		private Godot.Vector2 NextPathPosition;

		private uint Money = 0;
		private uint Pay = 0;
		
		// called when entering a shop
//		public void SetCurrentShop( Shop shop ) {
//			Agent.State[ "Vendor" ] = shop;
//		}
		// called when entering a settlement
		public void SetCurrentSettlement( Settlement location ) {
			Agent.State[ "Location" ] = location;
		}
		
		// TODO: personality generation
		
		private List<MountainGoap.Action> GenerateActions() {
			List<MountainGoap.Action> actions = new List<MountainGoap.Action>();
			
			// action cost is basically how willing the bot is willing to execute said action
			
			//
			// generic actions
			//
			
			MountainGoap.Action GotoNode = new MountainGoap.Action(
				name: "GotoNode",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( !MoveAlongPath() ) {
						return ExecutionStatus.Failed;
					}
					if ( (bool)agent.State[ "TargetReached" ] || (float)agent.State[ "TargetDistance" ] < 10.0f ) {
						agent.State[ "TargetReached" ] = false;
						agent.State[ "TargetDistance" ] = 0.0f;
						return ExecutionStatus.Succeeded;
					}
					return ExecutionStatus.Executing;
				},
				cost: 20,
				costCallback: ( MountainGoap.Action action, ConcurrentDictionary<string, object> currentState ) => {
					return (float)currentState[ "TargetDistance" ];
				},
				preconditions: new Dictionary<string, object>{
					{ "TargetReached", false }
				},
				comparativePreconditions: null,
//				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
//					{ "TargetDistance", new ComparisonValuePair{ Value = 1.0f, Operator = ComparisonOperator.GreaterThan } }
//				},
				postconditions: new Dictionary<string, object>{
					{ "TargetReached", true }
				}
			);
			actions.Add( GotoNode );
			
			/*
			MountainGoap.Action BecomeMercenary = new MountainGoap.Action(
				name: "BecomeMercenary",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( Settlement.GetMercenaryMaster() == null ) {
						return ExecutionStatus.Failed;
					}
				},
			);
			actions.Add( BecomeMercenary );
			*/
			
			MountainGoap.Action BecomeBlacksmith = new MountainGoap.Action(
				
			);
			
			MountainGoap.Action FindWork = new MountainGoap.Action(
				name: "FindWork",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					switch ( (Occupation)agent.State[ "Job" ] ) {
					case Occupation.Mercenary:
						if ( !(bool)agent.State[ "HasContract" ] ) {
							
						} else {
							
						}
						break;
					};
					return ExecutionStatus.Succeeded;
				},
				cost: 20.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "NeedsMoney", true }
				}
			);
			
			/*
			MountainGoap.Action FindShelter = new MountainGoap.Action(
				name: "FindShelter",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if (  )
				},
				cost: 0.0f,
			);
			actions.Add( FindShelter );
			
			MountainGoap.Action BuyFood = new MountainGoap.Action(
				name: "BuyFood",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					uint nMoney = (uint)agent.State[ "Money" ];
					Shop shop = (Shop)agent.State[ "Vendor" ];
					
					if ( shop.GetResourcePrice() > nMoney ) {
						return ExecutionStatus.Failed;
					}
					nMoney -= shop.Buy();
					agent.State[ "Money" ] = nMoney;
					agent.State[ "FoodType" ] = ( (FoodShop)shop ).GetFoodType();
					
					return ExecutionStatus.Succeeded;
				},
				cost: 1.0f,
				costCallback: ( MountainGoap.Action action, ConcurrentDictionary<string, object> currentState ) => {
					return ( (Shop)agent.Memory[ "FoodVendor" ] ).GlobalPosition.DistanceTo( GlobalPosition );
				},
				preconditions: new Dictionary<string, object>{
					{ "HasFood", false },
					{ "AtFoodVendor", false },
					{ "TargetReached", true }
				},
				comparativePreconditions: null,
				postconditions: new Dictionary<string, object>{
					{ "HasFood", true },
					{ "AtFoodVendor", true }
				}
				
			);
			actions.Add( BuyFood );
			
			MountainGoap.Action Eat = new MountainGoap.Action(
				name: "Eat",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( EatDurationTimer.IsStopped() ) {
						EatDurationTimer.Start();
						return ExecutionStatus.Executing;
					} else if ( EatDurationTimer.TimeLeft > 0.0f ) {
						return ExecutionStatus.Executing;
					}
					
					float food = (float)agent.State[ "Food" ];
					Food type = (Food)agent.State[ "FoodType" ];
					food += type.GetValue();
					
					agent.State[ "FoodType" ] = null;
					agent.State[ "Food" ] = food;
					agent.State[ "HasFood" ] = false;
					return ExecutionStatus.Succeeded;
				},
				cost: 5.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "HasFood", true }
				},
				comparativePreconditions: null,
				postconditions: new Dictionary<string, object>{
					{ "HasFood", false }
				}
			);
			actions.Add( Eat );
			*/
			
			//
			// personality influenced actions
			//
			
			MountainGoap.Action RobSomeone;
			if ( HasPersonality( "Greedy" ) ) {
				RobSomeone = new MountainGoap.Action(
					name: "RobSomeone",
					null,
					executor: ( Agent agent, MountainGoap.Action action ) => {
						return ExecutionStatus.Executing;
					},
					cost: 20.0f,
					costCallback: null,
					preconditions: new Dictionary<string, object>{
					},
					comparativePreconditions: null,
					postconditions: new Dictionary<string, object>{
					}
				);
			}
			else {
				RobSomeone = new MountainGoap.Action(
					name: "RobSomeone"
				);
			}
			actions.Add( RobSomeone );
			
			MountainGoap.Action HaveSex;
			if ( HasPersonality( "Horny" ) ) {
				HaveSex = new MountainGoap.Action(
					name: "HaveSex",
					null,
					executor: ( Agent agent, MountainGoap.Action action ) => {
						// check first if the bot has a relationship
						
						return ExecutionStatus.Executing;
					}
				);
			}
			else {
				
			}

			return actions;
		}
		private List<BaseGoal> GenerateGoals() {
			List<BaseGoal> goals = new List<BaseGoal>();
			
			//
			// goals that apply to anyone
			//
			Goal SurviveGoal = new Goal(
				name: "Survive",
				weight: 95.0f,
				desiredState: new Dictionary<string, object>{
					
				}
			);
			
			ComparativeGoal GetFood = new ComparativeGoal(
				name: "GetFood",
				weight: 95.0f,
				desiredState: new Dictionary<string, ComparisonValuePair>{
					{ "Food", new ComparisonValuePair{ Value = 50.0f, Operator = ComparisonOperator.GreaterThanOrEquals } },
					{ "AtFoodVendor", new ComparisonValuePair{ Value = true, Operator = ComparisonOperator.Equals } }
				}
			);
			goals.Add( GetFood );
			
			ComparativeGoal GetWater = new ComparativeGoal(
				name: "GetWater",
				weight: 96.0f,
				desiredState: new Dictionary<string, ComparisonValuePair>{
					{ "Water", new ComparisonValuePair{ Value = 50.0f, Operator = ComparisonOperator.GreaterThanOrEquals } }
				}
			);
			goals.Add( GetWater );
			
			ComparativeGoal GetJob = new ComparativeGoal(
				name: "GetJob",
				weight: 50.0f,
				desiredState: new Dictionary<string, ComparisonValuePair>{
					{ "Job", new ComparisonValuePair{ Value = (uint)Occupation.None, Operator = ComparisonOperator.GreaterThan } }
				}
			);
			goals.Add( GetJob );
			
			//
			// personality based goals
			//
			
			ComparativeGoal GetMoreMoney;
			if ( HasPersonality( "Greedy" ) ) {
				GetMoreMoney = new ComparativeGoal(
					name: "GetMoreMoney",
					weight: 80.0f,
					desiredState: new Dictionary<string, ComparisonValuePair>{
//						{ "Money", new ComparisonValuePair{ Value =  } }
					}
				);
			}
			else {
				GetMoreMoney = new ComparativeGoal();
			}
			goals.Add( GetMoreMoney );
			
			Goal GetDrunk;
			if ( HasPersonality( "Alcoholic" ) ) {
				GetDrunk = new Goal(
					name: "GetDrunk",
					weight: 80.0f,
					desiredState: new Dictionary<string, object>{
						{ "Drunk", false }
					}
				);
			}
			else {
				GetDrunk = new Goal(
					name: "GetDrunk",
					weight: 40.0f,
					desiredState: new Dictionary<string, object>{
						{ "Drunk", false }
					}
				);
			}
			goals.Add( GetDrunk );
			
			if ( HasPersonality( "Daredevil" ) ) {
				
			}
			else {
			
			}
			
			if ( HasPersonality( "Horny" ) ) {
				// ...yep
				Goal HaveSexGoal = new Goal(
					name: "HaveSex",
					weight: 80.0f,
					desiredState: new Dictionary<string, object>{
						{ "RecentlyHadSex", false }
					}
				);
			}
			else {
				Goal HaveSexGoal = new Goal(
					name: "HaveSex",
					weight: Age < 30 ? 70.0f : 40.0f,
					desiredState: new Dictionary<string, object>{
						{ "RecentlyHadSex", false }
					}
				);
			}
			
			return goals;
		}
		
		private void OnThinkIntervalTimeout() {
			Agent.Step();
		}
		private bool MoveAlongPath() {
			if ( GlobalPosition == NextPathPosition ) {
				NextPathPosition = Navigation.GetNextPathPosition();
			}
//			AngleDir = GlobalPosition.DirectionTo( nextPathPosition ) * MovementSpeed;
//			Animations.Play( "move" );
			Velocity = AngleDir;
			return MoveAndSlide();
		}
		private void OnNavigationTargetReached() {
			Agent.State[ "TargetReached" ] = true;
		}
		
		public override void _Ready() {
			ThinkInterval = GetNode<Timer>( "ThinkInterval" );
			ThinkInterval.Connect( "timeout", Callable.From( OnThinkIntervalTimeout ) );
			ThinkInterval.WaitTime = 2.5f;
			ThinkInterval.Autostart = true;
			
			for ( int i = 0; i < SetRelations.Count; i++ ) {
				Relations.Add( SetRelations[i] );
			}
			
			Navigation = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			Navigation.Connect( "target_reached", Callable.From( OnNavigationTargetReached ) );
			
			List<BaseGoal> goals = GenerateGoals();
			List<MountainGoap.Action> actions = GenerateActions();
			List<Sensor> sensors = new List<Sensor>{
				new Sensor(
					runCallback: ( Agent agent ) => {
						if ( (float)agent.State[ "Food" ] < 50.0f ) {
							agent.State[ "TargetReached" ] = false;
						}
					}
				)
			};
			
			Agent = new Agent(
				name: "RenownThinker_" + BotName,
				state: new ConcurrentDictionary<string, object>{
					{ "Drunk", false },
					{ "Water", 100.0f },
					{ "Food", 100.0f },
					{ "Job", (uint)Job },
					{ "Age", Age },
					{ "Money", Money },
					{ "AtFoodVendor", false },
					{ "AtWeaponsVendor", false },
					{ "AtProvisionsVendor", false },
					{ "RecentlyHadSex", false }
				},
				memory: new Dictionary<string, object>{
				},
				goals: goals,
				actions: actions,
				sensors: sensors
			);
			
			Agent.Plan();
		}
		
		public bool HasPersonality( string trait ) {
			return Personality[ trait ];
		}
		public void SetBlackbook( string key, object value ) {
			Agent.Memory[ key ] = value;
		}
		public object GetBlackbook( string key ) {
			return Agent.Memory[ key ];
		}
		
		public void Save( FileAccess file ) {
		}
		public void Load( FileAccess file ) {
		}
		
		public List<Relationship> GetRelationships() {
			return Relations;
		}
		
		public Relationship GetRelation( Thinker thinker ) {
			for ( int i = 0; i < Relations.Count; i++ ) {
				if ( Relations[i].GetPerson1() == thinker || Relations[i].GetPerson2() == thinker ) {
					return Relations[i];
				}
			}
			return null;
		}
		
		public void MeetPlayer() {
			if ( HasMetPlayer ) {
				
			}
		}
		public void MeetThinker( Thinker thinker ) {
			if ( GetRelation( thinker ) != null ) {
				return; // already met
			}
			Relations.Add( new Relationship( this, thinker, Relationship.Type.Acquaintance ) );
		}
		public void AddMoney( uint nAmount ) {
			Money += nAmount;
		}
		
		//
		// the juicy stuff
		//
		private int DetermineGrudgeSeverity( Relationship relation ) {
			if ( relation.GetScore() > 50 ) {
				return 0;
			}
			
			switch ( relation.GetStatus() ) {
			case Relationship.Type.Dislikes:
//				return ( 40 + ( Relationship.REQUIRED_SCORE_DISLIKES - relation.GetScore() ) ) + relation.GetHistory().GetGrudgeScore();
			case Relationship.Type.Enemy:
//				return ( 80 + ( Relationship.REQUIRED_SCORE_ENEMY - relation.GetScore() ) ) + relation.GetHistory().GetGrudgeScore();
			case Relationship.Type.RapBeef:
//				
			default:
				break;
			};
			
			return 0;
		}
		public void DecreaseRelationship( Thinker thinker, int nAmount ) {
			Relationship relation = GetRelation( thinker );
			if ( relation == null ) {
				return;
			}
			relation.DecreaseScore( nAmount );
//			GD.DebugPrint( "thinker " + Name + " decreased relationship with " + thinker.Name + " by " + nAmount.ToString() );
		}
		public void IncreaseRelationship( Thinker thinker, int nAmount ) {
			Relationship relation = GetRelation( thinker );
			if ( relation == null ) {
				return;
			}
			relation.IncreaseScore( nAmount );
		}
	};
};