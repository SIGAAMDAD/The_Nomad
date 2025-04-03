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
	public partial class Faction : Node {
		public static DataCache<Faction> Cache = null;

		[Export]
		protected StringName FactionName;
		[Export]
		protected string Description;
		[Export]
		protected AIAlignment PrimaryAlignment;
		[Export]
		protected Thinker Leader;
		[Export]
		protected Thinker[] MemberList;
		[Export]
		protected NavigationRegion2D[] TerritoryBounds;
		[Export]
		protected float Reserves = 0.0f;
		[Export]
		protected Godot.Collections.Dictionary<Node2D, float> ExistingDebts;
		[Export]
		protected Godot.Collections.Dictionary<Node2D, float> ExistingRelations;
		
		protected Dictionary<Node, float> DebtList = null;
		protected Dictionary<Node, float> RelationList = null;
		
		protected HashSet<Faction> WarList = null;
		protected Dictionary<CharacterBody2D, Bounty> BountyList = null;
		
		private Thread WorkThread = null;
		private object LockObject = new object();

		public StringName GetFactionName() => FactionName;
		public AIAlignment GetAlignment() => PrimaryAlignment;
		public Thinker GetLeader() => Leader;

		public virtual void Save() {
			int counter;
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( GetPath() );

			writer.SaveUInt( "alignment", (uint)PrimaryAlignment );
			writer.SaveString( "leader", Leader != null ? Leader.GetPath() : "nil" );
			
			counter = 0;
			writer.SaveInt( "debt_count", DebtList.Count );
			foreach ( var debt in DebtList ) {
				string key = string.Format( "debt_{0}", counter );
				writer.SaveString( key + "_node", debt.Key.GetPath() );
				writer.SaveFloat( key + "_amount", debt.Value );
				counter++;
			}
			
			counter = 0;
			writer.SaveInt( "relation_count", RelationList.Count );
			foreach ( var relation in RelationList ) {
				string key = string.Format( "relation_{0}", counter );
				writer.SaveString( key + "_node", relation.Key.GetPath() );
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
			Leader = Thinker.Cache.SearchCache( reader.LoadString( "leader" ) );
			
			int debtCount = reader.LoadInt( "" );
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
			
		}
		public void MemberJoin( Entity member ) {
			member.Connect( "Die", Callable.From<CharacterBody2D, CharacterBody2D>( OnMemberDeath ) );
			member.SetFaction( this );
		}
		public void MemberLeave( Entity member ) {
			member.Disconnect( "Die", Callable.From<CharacterBody2D, CharacterBody2D>( OnMemberDeath ) );
			member.SetFaction( null );
		}
		
		public float GetRelationScore( Node other ) => RelationList.TryGetValue( other, out float score ) ? score : 0.0f;
		public void RelationIncrease( Node other, float nAmount ) {
			if ( !RelationList.ContainsKey( other ) ) {
				RelationList.Add( other, 0.0f );
			}
			RelationList[ other ] += nAmount;
		}
		public void RelationDecrease( Node other, float nAmount ) {
			if ( !RelationList.ContainsKey( other ) ) {
				RelationList.Add( other, 0.0f );
			}
			RelationList[ other ] -= nAmount;
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
			if ( RelationList[ killer ] < -80.0f ) {
				// bounty
				if ( !BountyList.ContainsKey( killer ) ) {
//					BountyList.Add( killer, new Bounty( this, killer, -RelationList[ killer ] ) );
				} else {
					// increase bounty on target's head exponentially
//					BountyList[ killer ].Increase( -RelationList[ killer ] );
				}
			}
		}
		public void OnBountyCompleted( Bounty bounty, Entity entity ) {
			if ( Reserves - bounty.GetAmount() < 0.0f ) {
				AddDebt( entity, bount.GetAmount() );
			} else {
				entity.AddMoney( bounty.GetAmount() );
			}
			Reserves -= bounty.GetAmount();
//			ContractManager.Remove( bounty );
		}
		
		public override void _Ready() {
			base._Ready();
			
			if ( !IsInGroup( "Factions" ) ) {
				AddToGroup( "Factions" );
			}
			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			} else {
				DebtList = new Dictionary<Node, float>( ExistingDebts.Count );
				RelationList = new Dictionary<Node, float>( ExistingRelations.Count );
			}
			
			WorkThread = new Thread( Think );
			WorkThread.Start();
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 120 ) != 0 ) {
				return;
			}
			
			base._Process( delta );
			
			lock ( LockObject ) {
				// allow it to run again
				Monitor.Pulse( LockObject );
			}
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
		public bool CanLoanMoney( Node destination, float nAmount ) {
			if ( !RelationList.TryGetValue( destination, out float score ) ) {
				RelationList.Add( destination, -10.0f );
				return false; // they don't have an active relation with said node, so start off on a bad note
			}
			
			// if we hate them, don't give them any money
			if ( score < 0.0f ) {
				RelationList[ destination ] -= 10.0f;
				return false;
			}
			// TODO: implement malicious debt creation
			// TODO: implement debt counter over time mechanic
			
			if ( Reserves - nAmount < 0.0f ) {
				// we don't have enough in the reserves
				// but if we're good enough friends, then create a new
				// debt with a different node
				if ( score >= 50.0f && CreateDebt( nAmount ) ) {
					return true;
				}
				return false;
			}
			Reserves -= nAmount;
			
			// if we have enough money and the relation is neutral or friendly, give them money
			return true;
		}
		private void AddDebt( Node to, float nAmount ) {
			if ( !DebtList.ContainsKey( to ) ) {
				DebtList.Add( to, 0.0f );
			}
			DebtList[ to ] += nAmount;
		}
		private void PayDebt( Node to, float nAmount ) {
			// sanity check
			if ( !DebtList.TryGetValue( to, out float amount ) ) {
				Console.PrintError(
					string.Format( "Faction.PayDebt: debt from {0} to {1} doesn't exist!", GetPath(), to.GetPath() )
				);
				return;
			}
			amount -= nAmount;
			if ( amount <= 0.0f ) {
				// paid in full, remove it
				DebtList.Remove( to );
			} else {
				DebtList[ to ] = amount;
			}
			Reserves -= nAmount;
		}
		
		private void UpdateWarStatus( Faction faction ) {
		}
		private void UpdateRelations() {
			foreach ( var relation in RelationList ) {
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
			
			foreach ( var debt in DebtList ) {
				// start the debt payment at the debt's value
				
				if ( Reserves > debt.Value - ( Reserves * 0.25f ) ) {
					PayDebt( debt.Key, debt.Value );
				}
			}
		}
		private void Think() {
			lock ( LockObject ) {
				// wait for frame sync
				Monitor.Wait( LockObject );
			}
			
			UpdateRelations();
			UpdateDebts();
			
			SubThink();
		}
		protected virtual void SubThink() {
		}
	};
};
