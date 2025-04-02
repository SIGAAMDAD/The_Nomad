using System;
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
		protected Godot.Collections.Dictionary<Node2D< float> ExistingDebts;
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
			foreach ( var debt in DebtList ) {
				string key = string.Format( "debt_{0}", counter );
				writer.SaveString( key + "_node", debt.Key.GetPath() );
				writer.SaveFloat( key + "_amount", debt.Value );
				counter++;
			}
			
			counter = 0;
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
		}
		
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
		public void MemberJoin( CharacterBody2D member ) {
			member.Connect( "Die", Callable.From( OnMemberDeath ) );
		}
		public void MemberLeave( CharacterBody2D member ) {
			member.Disconnect( "Die", Callable.From( OnMemberDeath ) );
		}
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
		
		private void OnMemberDeath( CharacterBody2D killer, CharacterBody2D member ) {
			if ( (Faction)member.Call( "GetFaction" ) != this ) {
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
					BountyList.Add( killer, new Bounty( this, killer, -RelationList[ killer ] ) );
				} else {
					// increase bounty on target's head exponentially
					BountyList[ killer ].Increase( -RelationList[ killer ] );
				}
			}
		}
		public void OnBountyCompleted( Bounty bounty, CharacterBody2D completer ) {
			if ( Reserves - bounty.GetAmount() < 0.0f ) {
				DebtList.Add( completer, bounty.GetAmount() );
			} else {
				completer.Call( "AddMoney", bounty.GetAmount() );
			}
//			ContractManager.Remove( bounty );
		}
		
		public override void _Ready() {
			base._Ready();

			if ( SettingsData.GetNetworkingEnabled() ) {
			}
			if ( !IsInGroup( "Factions" ) ) {
				AddToGroup( "Factions" );
			}
			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			} else {
				DebtList = new Dictionary<Node2D, float>( ExistingDebts.Count );
				RelationList = new Dictionary<Node2D, float>( ExistingRelations.Count );
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
				Moniter.Pulse( LockObject );
			}
		}
		
		private int GetNumMembersOfOccupation( Occupation job ) {
			int count = 0;
			foreach ( var member in MemberCache ) {
				if ( member.Value.GetOccupation() == job ) {
					count++;
				}
			}
			return count;
		}
		private void UpdateWarStatus( Faction faction ) {
			// check if we need more fighters
			int mercenaryCount = GetNumMembersOfOccupation( Occupation.Mercenary );
			
			if ( mercenaryCount < 40 ) {
				// hire some more
				
			}
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
		private void Think() {
			lock ( LockObject ) {
				// wait for frame sync
				Moniter.Wait( LockObject );
			}
			
			UpdateRelations();
			
			SubThink();
		}
		protected virtual void SubThink() {
		}
	};
};
