using System.Threading;
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
		protected Entity[] MemberList;
		[Export]
		protected float Money = 0.0f;
		[Export]
		protected int RenownScore = 0;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Debts = null;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Relations = null;

		protected HashSet<Trait> TraitCache = null;
		protected Dictionary<Object, float> DebtCache = null;
		protected Dictionary<Object, float> RelationCache = null;
		
		protected HashSet<Faction> WarList = null;
		
		private Thread WorkThread = null;
		private object LockObject = new object();

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
			if ( !RelationCache.ContainsKey( other ) ) {
				RelationCache.Add( other, 0.0f );
			}
			RelationCache[ other ] += nAmount;
		}
		public void RelationDecrease( Object other, float nAmount ) {
			if ( !RelationCache.ContainsKey( other ) ) {
				RelationCache.Add( other, 0.0f );
			}
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

			writer.SaveUInt( "alignment", (uint)PrimaryAlignment );
			writer.SaveString( "leader", Leader != null ? Leader.GetPath() : "nil" );
			
			counter = 0;
			writer.SaveInt( "debt_count", DebtCache.Count );
			foreach ( var debt in DebtCache ) {
				string key = string.Format( "debt_{0}", counter );
				writer.SaveString( key + "_node", debt.Key.GetHash() );
				writer.SaveFloat( key + "_amount", debt.Value );
				counter++;
			}
			
			counter = 0;
			writer.SaveInt( "relation_count", RelationCache.Count );
			foreach ( var relation in RelationCache ) {
				string key = string.Format( "relation_{0}", counter );
				writer.SaveString( key + "_node", relation.Key.GetHash() );
				writer.SaveFloat( key + "_amount", relation.Value );
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

			PrimaryAlignment = (AIAlignment)reader.LoadUInt( "alignment" );
//			Leader = (Thinker)GetTree().CurrentScene.GetNode( reader.LoadString( "leader" ) );
			
			int debtCount = reader.LoadInt( "debt_count" );
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
				amount = (float)favor;
			} else if ( favor > 0 ) {
				amount = ( favor * 0.001f );
			} else if ( favor < 0 ) {
				amount = (float)-favor;
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
		
		public override void _Ready() {
			base._Ready();

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
			
			if ( !IsInGroup( "Factions" ) ) {
				AddToGroup( "Factions" );
			}
			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			}
			
//			WorkThread = new Thread( Think );
//			WorkThread.Start();
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 120 ) != 0 ) {
				return;
			}
			
			base._Process( delta );

			Think();
			
			/*
			lock ( LockObject ) {
				// allow it to run again
				Monitor.Pulse( LockObject );
			}
			*/
		}
		
		private bool CreateDebt( float nAmount ) {
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
		private void AddDebt( Object to, float nAmount ) {
			DebtCache.TryAdd( to, 0.0f );
			DebtCache[ to ] += nAmount;
		}
		private void PayDebt( Object to, float nAmount ) {
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
		
		private void UpdateWarStatus( Faction faction ) {
		}
		private void UpdateRelations() {
			foreach ( var relation in RelationCache ) {
				Faction faction = relation.Key as Faction;
				if ( faction != null ) {
					if ( WarList.Contains( faction ) ) {
						UpdateWarStatus( faction );
					}
				}
			}
		}
		private void UpdateDebts() {
			float amount = 0.0f;
			
			foreach ( var debt in DebtCache ) {
				// start the debt payment at the debt's value
				
				if ( Money > debt.Value - ( Money * 0.25f ) ) {
					PayDebt( debt.Key, debt.Value );
				}
			}
		}
		private void Think() {
			/*
			lock ( LockObject ) {
				// wait for frame sync
				Monitor.Wait( LockObject );
			}
			*/
			
			UpdateRelations();
			UpdateDebts();
			
			SubThink();
		}
		protected virtual void SubThink() {
		}
	};
};
