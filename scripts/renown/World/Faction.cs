using System.Collections.Generic;
using Godot;

namespace Renown.World {
	public enum AIAlignment {
		NeutralEvil,
		ChaoticEvil,
		LawfulEvil,

		Nuetral,
		ChaoticNeutral,
		LawfulNeutral,

		NeutralGood,
		ChaoticGood,
		LawfulGood,

		Count
	};
	
	public enum MessageType : uint {
		Truce,
		War,
		
		Contract,
		
		// asking for money, creates a new debt
		MoneyRequest,
		
		// giving money to another faction
		MoneyDeposit,
		
		Count
	};
	public partial class Faction : Node, Object {
		public static DataCache<Faction> Cache = null;

		[Export]
		protected StringName FactionName;
		[Export]
		protected string Description;
		[Export]
		protected AIAlignment PrimaryAlignment;
		[Export]
		protected Entity Leader;
		[Export]
		protected Godot.Collections.Array<Entity> MemberList;
		[Export]
		protected float Money = 0.0f;
		[Export]
		protected int RenownScore = 0;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Debts = null;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Relations = null;
		[Export]
		protected WorldArea Location = null;

		protected HashSet<Trait> TraitCache = null;
		protected Dictionary<Object, float> DebtCache = null;
		protected Dictionary<Object, float> RelationCache = null;
		
		protected HashSet<Faction> WarList = null;
		
		protected System.Threading.Thread ThinkThread = null;
		protected bool Quit = false;
		protected int ThreadSleep = Constants.THREADSLEEP_FACTION_PLAYER_AWAY;
		protected System.Threading.ThreadPriority Importance = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;

		[Signal]
		public delegate void EarnTraitEventHandler( Node self, Trait trait );
		[Signal]
		public delegate void LoseTraitEventHandler( Node self, Trait trait );
		[Signal]
		public delegate void MeetEntityEventHandler( Entity other, Node self );
		[Signal]
		public delegate void MeetFactionEventHandler( Faction faction, Node self );
		[Signal]
		public delegate void IncreaseRelationEventHandler( Node other, Node self, float nAmount );
		[Signal]
		public delegate void DecreaseRelationEventHandler( Node other, Node self, float nAmount );
		[Signal]
		public delegate void IncreaseRenownEventHandler( Node self, int nAmount );
		[Signal]
		public delegate void GainMoneyEventHandler( Node entity, float nAmount );
		[Signal]
		public delegate void LoseMoneyEventHandler( Node entity, float nAmount );

		public StringName GetFactionName() => FactionName;
		public AIAlignment GetAlignment() => PrimaryAlignment;
		public Entity GetLeader() => Leader;
	
	#region Object
		public NodePath GetHash() => GetPath();

		public StringName GetObjectName() => GetFactionName();

		public virtual void PayWorker( float nIncomeTax, float nAmount, Thinker thinker ) {
			Settlement settlement = thinker.GetLocation() as Settlement;
			
			settlement.GetGovernment().IncreaseMoney( nIncomeTax );
			thinker.IncreaseMoney( nAmount );
			DecreaseMoney( nAmount );
		}

		public float GetMoney() => Money;
		public virtual void DecreaseMoney( float nAmount ) {
			Money -= nAmount;
			EmitSignalLoseMoney( this, nAmount );
		}
		public virtual void IncreaseMoney( float nAmount ) {
			Money += nAmount;
			EmitSignalGainMoney( this, nAmount );
		}

		public HashSet<Trait> GetTraits() => TraitCache;

		/// <summary>
		/// returns true if the entity has the given trait
		/// </summary>
		public bool HasTrait( Trait trait ) => TraitCache.Contains( trait );

		/// <summary>
		/// checks if the given trait conflicts with this entity's values
		/// </summary>
		public bool HasConflictingTrait( Trait other ) {
			foreach ( var trait in TraitCache ) {
				if ( trait.Conflicts( other ) ) {
					return true;
				}
			}
			return false;
		}
		public List<Trait> GetConflictingTraits( Trait other ) {
			List<Trait> traits = new List<Trait>();
			foreach ( var trait in TraitCache ) {
				if ( trait.Conflicts( other ) ) {
					traits.Add( trait );
				}
			}
			return traits;
		}
		public List<Trait> GetAgreeableTraits( Trait other ) {
			List<Trait> traits = new List<Trait>();
			foreach ( var trait in TraitCache ) {
				if ( trait.Agrees( other ) ) {
					traits.Add( trait );
				}
			}
			return traits;
		}
		public virtual void AddTrait( Trait trait ) {
			EmitSignalEarnTrait( this, trait );
			TraitCache.Add( trait );
		}
		public virtual void RemoveTrait( Trait trait ) {
			EmitSignalLoseTrait( this, trait );
			TraitCache.Remove( trait );
		}

		public virtual void Meet( Object other ) {
			RelationCache.Add( other, 0.0f );

			if ( other is Entity entity && entity != null ) { 
				EmitSignalMeetEntity( entity, this );
			} else {
				if ( other is Faction faction && faction != null ) {
					EmitSignalMeetFaction( faction, this );
					return;
				}
				Console.PrintError( "Entity.Meet: node isn't an entity or faction!" );
			}
		}
		public float GetRelationScore( Object other ) => RelationCache.TryGetValue( other, out float score ) ? score : 0.0f;
		public void RelationIncrease( Object other, float nAmount ) {
			RelationCache.TryAdd(other, 0.0f);
			RelationCache[ other ] += nAmount;
		}
		public void RelationDecrease( Object other, float nAmount ) {
			RelationCache.TryAdd(other, 0.0f);
			RelationCache[ other ] -= nAmount;
		}
		public virtual void DetermineRelationStatus( Object other ) {
			float score = RelationCache[ other ];
			int renownScore = other.GetRenownScore();

			// TODO: write some way of using renown to determine if the entity knows all this stuff about the other one

			/*
			HashSet<Trait> traitList = other.GetTraits();
			foreach ( var trait in traitList ) {
				List<Trait> conflicting = GetConflictingTraits( trait );
				for ( int i = 0; i < conflicting.Count; i++ ) {
					score -= conflicting[i].GetNegativeRelationScore( trait );
				}

				List<Trait> agreeables = GetAgreeableTraits( trait );
				for ( int i = 0; i < agreeables.Count; i++ ) {
					score += conflicting[i].GetPositiveRelationScore( trait );
				}
			}
			*/
			
			RelationCache[ other ] = score;
		}
		public RelationStatus GetRelationStatus( Object other ) {
			float score = GetRelationScore( other );
			
			if ( score < -100.0f ) {
				return RelationStatus.KendrickAndDrake;
			}
			if ( score < -50.0f ) {
				return RelationStatus.Hates;
			}
			if ( score < 0.0f ) {
				return RelationStatus.Dislikes;
			}
			if ( score > 25.0f ) {
				return RelationStatus.Friends;
			}
			if ( score > 100.0f ) {
				return RelationStatus.GoodFriends;
			}
			return RelationStatus.Neutral;
		}

		public int GetRenownScore() => RenownScore;
		public void AddRenown( int nAmount ) {
			EmitSignalIncreaseRenown( this, nAmount );
		}
	#endregion

		public virtual void Save() {
			int counter;
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( GetPath() );

			writer.SaveUInt( nameof( PrimaryAlignment ), (uint)PrimaryAlignment );
			writer.SaveString( nameof( Leader ), Leader != null ? Leader.GetPath() : "nil" );
			
			counter = 0;
			writer.SaveInt( "DebtCount", DebtCache.Count );
			foreach ( var debt in DebtCache ) {
				writer.SaveString( string.Format( "DebtNode{0}", counter ), debt.Key.GetHash() );
				writer.SaveFloat( string.Format( "DebtValue{0}", counter ), debt.Value );
				counter++;
			}
			
			counter = 0;
			writer.SaveInt( "RelationCount", RelationCache.Count );
			foreach ( var relation in RelationCache ) {
				writer.SaveString( string.Format( "DebtNode{0}", counter ), relation.Key.GetHash() );
				writer.SaveFloat( string.Format( "DebtValue{0}", counter ), relation.Value );
				counter++;
			}
			writer.Flush();
		}
		public virtual void Load() {
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			PrimaryAlignment = (AIAlignment)reader.LoadUInt( "Alignment" );
//			Leader = (Thinker)GetTree().CurrentScene.GetNode( reader.LoadString( "leader" ) );
			
			int debtCount = reader.LoadInt( "DebtCount" );
		}
		
		/*
		public virtual void ReceiveMessenger( Messenger actor ) {
			// start a relation link if we haven't already
			Node sender = actor.GetSender();
			
			switch ( actor.GetMessageType() ) {
			case MessageType.War:
				RelationDecrease( sender, 80.0f );
				break;
			};
		}
		public virtual void SendMessenger( Faction destination, MessageType nType ) {
			Messenger actor = new Messenger( this, destination, nType );
		}
		*/
		public bool CanJoin( Entity member ) {
			return true;
		}
		public void MemberJoin( Entity member ) {
			member.Connect( "Die", Callable.From<Entity, Entity>( OnMemberDeath ) );
			member.SetFaction( this );
		}
		public void MemberLeave( Entity member ) {
			member.Disconnect( "Die", Callable.From<Entity, Entity>( OnMemberDeath ) );
			member.SetFaction( null );
		}
		
		private void OnMemberDeath( Entity killer, Entity member ) {
			if ( member.GetFaction() != this ) {
				// warning?
				return;
			}
			
			float amount = 0.0f;
			int favor = member.GetFactionImportance();
			if ( favor > 50 ) {
				// leader
				amount = favor;
			} else if ( favor > 0 ) {
				amount = ( favor * 0.001f );
			} else if ( favor < 0 ) {
				amount = -favor;
			}
			RelationDecrease( killer, amount );
			if ( RelationCache[ killer ] < -80.0f ) {
				// bounty
				/*
				if ( !BountyList.ContainsKey( killer ) ) {
					BountyList.Add( killer, new Bounty( this, killer, -RelationCache[ killer ] ) );
				} else {
					// increase bounty on target's head exponentially
					BountyList[ killer ].Increase( -RelationCache[ killer ] );
				}
				*/
			}
		}
		/*
		public void OnBountyCompleted( Bounty bounty, Entity entity ) {
			if ( Money - bounty.GetAmount() < 0.0f ) {
				AddDebt( entity, bounty.GetAmount() );
			} else {
				entity.IncreaseMoney( bounty.GetAmount() );
			}
			Money -= bounty.GetAmount();
//			ContractManager.Remove( bounty );
		}
		*/

		public override void _ExitTree() {
			base._ExitTree();

			Quit = true;
		}
		public override void _Ready() {
			base._Ready();
			
			if ( !IsInGroup( "Factions" ) ) {
				AddToGroup( "Factions" );
			}
			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			} else {
				RelationCache = new Dictionary<Object, float>( Relations != null ? Relations.Count : 0 );
				foreach ( var relation in Relations ) {
					if ( relation.Key is Faction faction && faction != null ) {
						RelationCache.Add( faction, relation.Value );
					} else if ( relation.Key is Entity entity && entity != null ) {
						RelationCache.Add( entity, relation.Value );
					} else {
						Console.PrintError( string.Format( "Faction._Ready: relation key {0} isn't a renown object!", relation.Key != null ? relation.Key.GetPath() : "nil" ) );
					}
				}

				DebtCache = new Dictionary<Object, float>( Debts != null ? Debts.Count : 0 );
				foreach ( var debt in Debts ) {
					if ( debt.Key is Faction faction && faction != null ) {
						DebtCache.Add( faction, debt.Value );
					} else if ( debt.Key is Entity entity && entity != null ) {
						DebtCache.Add( entity, debt.Value );
					} else {
						Console.PrintError( string.Format( "Faction._Ready: debt key {0} isn't a renown object!", debt.Key != null ? debt.Key.GetPath() : "nil" ) );
					}
				}
			}

			// this isn't an entity
			ProcessMode = ProcessModeEnum.Disabled;

			Location.PlayerEntered += () => {
				ThreadSleep = Constants.THREADSLEEP_FACTION_PLAYER_IN_AREA;
				Importance = Constants.THREAD_IMPORTANCE_PLAYER_IN_AREA;

				ThinkThread.Priority = Importance;
			};
			Location.PlayerExited += () => {
				if ( Location.GetBiome().IsPlayerHere() ) {
					ThreadSleep = Constants.THREADSLEEP_FACTION_PLAYER_IN_BIOME;
					Importance = Constants.THREAD_IMPORTANCE_PLAYER_IN_BIOME;
				} else {
					ThreadSleep = Constants.THREADSLEEP_FACTION_PLAYER_AWAY;
					Importance = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;
				}

				ThinkThread.Priority = Importance;
			};

			ThinkThread = new System.Threading.Thread( RenownProcess, 512*1024 );
			ThinkThread.Priority = Importance;
			ThinkThread.Start();
		}
		
		protected bool CreateDebt( float nAmount ) {
			// TODO: allow debt with politicians & specific rich mercs outside of faction
			Godot.Collections.Array<Node> factions = GetTree().GetNodesInGroup( "Factions" );
			foreach ( var faction in factions ) {
				Faction loaner = faction as Faction;
				if ( loaner.CanLoanMoney( this, nAmount ) ) {
					AddDebt( loaner, nAmount );
					return true;
				}
			}
			return false;
		}
		public bool CanLoanMoney( Object destination, float nAmount ) {
			if ( !RelationCache.TryGetValue( destination, out float score ) ) {
				RelationCache.Add( destination, -10.0f );
				return false; // they don't have an active relation with said node, so start off on a bad note
			}
			
			// if we hate them, don't give them any money
			if ( score < 0.0f ) {
				RelationCache[ destination ] -= 10.0f;
				return false;
			}
			// TODO: implement malicious debt creation
			// TODO: implement debt counter over time mechanic
			
			if ( Money - nAmount < 0.0f ) {
				// we don't have enough in the reserves
				// but if we're good enough friends, then create a new
				// debt with a different node
				if ( score >= 50.0f && CreateDebt( nAmount ) ) {
					return true;
				}
				return false;
			}
			Money -= nAmount;
			
			// if we have enough money and the relation is neutral or friendly, give them money
			return true;
		}
		protected void AddDebt( Object to, float nAmount ) {
			DebtCache.TryAdd( to, 0.0f );
			DebtCache[ to ] += nAmount;
		}
		protected void PayDebt( Object to, float nAmount ) {
			// sanity check
			if ( !DebtCache.TryGetValue( to, out float amount ) ) {
				Console.PrintError(
					string.Format( "Faction.PayDebt: debt from {0} to {1} doesn't exist!", GetPath(), to.GetHash() )
				);
				return;
			}
			amount -= nAmount;
			if ( amount <= 0.0f ) {
				// paid in full, remove it
				DebtCache.Remove( to );
			} else {
				DebtCache[ to ] = amount;
			}
			Money -= nAmount;
		}
		
		protected void UpdateWarStatus( Faction faction ) {
		}
		protected void UpdateRelations() {
			foreach ( var relation in RelationCache ) {
				Faction faction = relation.Key as Faction;
				if ( faction != null ) {
					if ( WarList.Contains( faction ) ) {
						UpdateWarStatus( faction );
					}
				}
			}
		}
		protected void UpdateDebts() {
			float amount = 0.0f;
			
			foreach ( var debt in DebtCache ) {
				// start the debt payment at the debt's value
				
				if ( Money > debt.Value - ( Money * 0.25f ) ) {
					PayDebt( debt.Key, debt.Value );
				}
			}
		}
		protected virtual void RenownProcess() {
			while ( !Quit ) {
				System.Threading.Thread.Sleep( ThreadSleep );

				UpdateRelations();
				UpdateDebts();
			}
		}
	};
};
