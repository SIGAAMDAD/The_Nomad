using System.Collections.Generic;
using Godot;
using Renown.Thinkers;

namespace Renown.World {
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
		public StringName FactionName { get; private set; }
		[Export]
		public StringName Description { get; private set; }
		[Export]
		public AIAlignment PrimaryAlignment { get; private set; }
		[Export]
		public Entity Leader { get; private set; }
		[Export]
		protected Godot.Collections.Array<Entity> MemberList;
		[Export]
		public float Money { get; private set; } = 0.0f;
		[Export]
		public int RenownScore { get; private set; } = 0;
		[Export]
		public Godot.Collections.Dictionary<Trait, float> TraitScores { get; private set; } = null;
		[ Export ]
		protected Godot.Collections.Dictionary<Node, float> Debts = null;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Relations = null;
		[Export]
		protected WorldArea Location = null;

		protected Dictionary<Object, float> DebtCache = null;
		protected Dictionary<Object, float> RelationCache = null;
		
		protected HashSet<Faction> WarList = null;

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
		public delegate void IncreaseRelationEventHandler( Node other, Node self, float amount );
		[Signal]
		public delegate void DecreaseRelationEventHandler( Node other, Node self, float amount );
		[Signal]
		public delegate void IncreaseRenownEventHandler( Node self, int amount );
		[Signal]
		public delegate void GainMoneyEventHandler( Node entity, float amount );
		[Signal]
		public delegate void LoseMoneyEventHandler( Node entity, float amount );

		/*
		===============
		GetHash
		===============
		*/
		public NodePath GetHash() {
			return GetPath();
		}

		/*
		===============
		GetObjectName
		===============
		*/
		public StringName GetObjectName() {
			return FactionName;
		}

		/*
		===============
		GetRenownScore
		===============
		*/
		public int GetRenownScore() {
			return RenownScore;
		}

		/*
		===============
		GetMoney
		===============
		*/
		public float GetMoney() {
			return Money;
		}

		/*
		===============
		DecreaseMoney
		===============
		*/
		public virtual void DecreaseMoney( float amount ) {
			Money -= amount;
			EmitSignalLoseMoney( this, amount );
		}

		/*
		===============
		IncreaseMoney
		===============
		*/
		public virtual void IncreaseMoney( float amount ) {
			Money += amount;
			EmitSignalGainMoney( this, amount );
		}

		/*
		===============
		HasConflictingTrait
		===============
		*/
		/// <summary>
		/// checks if the given trait conflicts with this entity's values
		/// </summary>
		public bool HasConflictingTrait( Trait other, float score ) {
			foreach ( var trait in TraitScores ) {
				if ( trait.Key.Conflicts( other, trait.Value, score ) ) {
					return true;
				}
			}
			return false;
		}

		/*
		===============
		GetConflictingTraits
		===============
		*/
		public List<Trait> GetConflictingTraits( Trait other, float score ) {
			List<Trait> traits = new List<Trait>();
			foreach ( var trait in TraitScores ) {
				if ( trait.Key.Conflicts( other, trait.Value, score ) ) {
					traits.Add( trait.Key );
				}
			}
			return traits;
		}

		/*
		===============
		GetAgreeableTraits
		===============
		*/
		public List<Trait> GetAgreeableTraits( Trait other ) {
			List<Trait> traits = new List<Trait>();
			foreach ( var trait in TraitScores ) {
				if ( trait.Key.Agrees( other ) ) {
					traits.Add( trait.Key );
				}
			}
			return traits;
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

		/*
		===============
		GetRelationScore
		===============
		*/
		public float GetRelationScore( Object other ) {
			return RelationCache.TryGetValue( other, out float score ) ? score : 0.0f;
		}

		/*
		===============
		RelationIncrease
		===============
		*/
		public void RelationIncrease( Object other, float amount ) {
			RelationCache.TryAdd( other, 0.0f );
			RelationCache[ other ] += amount;
		}

		/*
		===============
		RelationDecrease
		===============
		*/
		public void RelationDecrease( Object other, float amount ) {
			RelationCache.TryAdd( other, 0.0f );
			RelationCache[ other ] -= amount;
		}
		
		/*
		===============
		DetermineRelationStatus
		===============
		*/
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
		
		/*
		===============
		GetRelationStatus
		===============
		*/
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

		/*
		===============
		AddRenown
		===============
		*/
		public void AddRenown( int amount ) {
			RenownScore += amount;
			EmitSignalIncreaseRenown( this, amount );
		}

		public virtual void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter );
			int counter;

			writer.SaveUInt( nameof( PrimaryAlignment ), (uint)PrimaryAlignment );
			writer.SaveInt( nameof( RenownScore ), RenownScore );
			writer.SaveFloat( nameof( Money ), Money );
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
				writer.SaveString( string.Format( "RelationNode{0}", counter ), relation.Key.GetHash() );
				writer.SaveFloat( string.Format( "RelationValue{0}", counter ), relation.Value );
				counter++;
			}
		}
		public virtual void Load() {
			SaveSystem.SaveSectionReader? reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			PrimaryAlignment = (AIAlignment)reader.LoadUInt( nameof( PrimaryAlignment ) );
			RenownScore = reader.LoadInt( nameof( RenownScore ) );
			Money = reader.LoadFloat( nameof( Money ) );
//			Leader = (Thinker)GetTree().CurrentScene.GetNode( reader.LoadString( "leader" ) );

			int debtCount = reader.LoadInt( "DebtCount" );
			DebtCache = new Dictionary<Object, float>( debtCount );
			for ( int i = 0; i < debtCount; i++ ) {
				DebtCache.Add( (Object)GetNode( reader.LoadString( $"DebtNode{i}" ) ), reader.LoadFloat( $"DebtValue{i}" ) );
			}

			int relationCount = reader.LoadInt( "RelationCount" );
			RelationCache = new Dictionary<Object, float>( relationCount );
			for ( int i = 0; i < relationCount; i++ ) {
				RelationCache.Add( (Object)GetNode( reader.LoadString( $"RelationNode{i}" ) ), reader.LoadFloat( $"RelationValue{i}" ) );
			}
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
		
		private void OnMemberDeath( Entity killer, Entity member ) {
			if ( member.Faction != this ) {
				// warning?
				return;
			}

			float amount = 0.0f;
			int favor = member.FactionImportance;
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

		public override void _EnterTree() {
			base._EnterTree();
			
			if ( !IsInGroup( "Factions" ) ) {
				AddToGroup( "Factions" );
			}
			if ( ArchiveSystem.IsLoaded() ) {
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
		}
		
		protected bool CreateDebt( float amount ) {
			// TODO: allow debt with politicians & specific rich mercs outside of faction
			Godot.Collections.Array<Node> factions = GetTree().GetNodesInGroup( "Factions" );
			foreach ( var faction in factions ) {
				Faction loaner = faction as Faction;
				if ( loaner.CanLoanMoney( this, amount ) ) {
					AddDebt( loaner, amount );
					return true;
				}
			}
			return false;
		}
		public bool CanLoanMoney( Object destination, float amount ) {
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
			
			if ( Money - amount < 0.0f ) {
				// we don't have enough in the reserves
				// but if we're good enough friends, then create a new
				// debt with a different node
				if ( score >= 50.0f && CreateDebt( amount ) ) {
					return true;
				}
				return false;
			}
			Money -= amount;
			
			// if we have enough money and the relation is neutral or friendly, give them money
			return true;
		}
		protected void AddDebt( Object to, float nAmount ) {
			DebtCache.TryAdd( to, 0.0f );
			DebtCache[ to ] += nAmount;
		}
		protected void PayDebt( Object to, float debtAmount ) {
			// sanity check
			if ( !DebtCache.TryGetValue( to, out float amount ) ) {
				Console.PrintError(
					string.Format( "Faction.PayDebt: debt from {0} to {1} doesn't exist!", GetPath(), to.GetHash() )
				);
				return;
			}
			amount -= debtAmount;
			if ( amount <= 0.0f ) {
				// paid in full, remove it
				DebtCache.Remove( to );
			} else {
				DebtCache[ to ] = amount;
			}
			Money -= debtAmount;
		}
	};
};
