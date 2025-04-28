using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public enum GroupType : uint {
		Military,
		Bandit,

		// for later...
		Riot,
		Protest,
		
		Count
	};
	public enum GroupEvent : uint {
		TargetChanged,

		Count
	};

	public partial class ThinkerGroup : Node, Renown.Object {
		protected List<Thinker> Thinkers;
		protected Thinker Leader;
		protected Faction Faction;
		protected GroupType Type;

		public GroupType GetGroupType() => Type;
		public Faction GetFaction() => Faction;
		public Thinker GetLeader() => Leader;
		public void SetLeader( Thinker leader ) => Leader = leader;
		public int NumGroupMembers() => Thinkers.Count;

		public ThinkerGroup( GroupType nType, Faction faction ) {
			Type = nType;
			Faction = faction;
			Thinkers = new List<Thinker>();
		}

		public void Save() {
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( GetPath() );

			writer.SaveString( "faction", Faction.GetPath() );
			writer.SaveString( "leader", Leader.GetPath() );
			writer.SaveUInt( "type", (uint)Type );
			writer.SaveInt( "member_count", Thinkers.Count );

			for ( int i = 0; i < Thinkers.Count; i++ ) {
				writer.SaveString( "thinker_" + i.ToString(), Thinkers[i].GetPath() );
			}
		}
		public void Load() {
		}

		public NodePath GetHash() => GetPath();
		public StringName GetObjectName() => Name;
		public int GetMemberCount() => Thinkers.Count;

		public float GetMoney() => 0.0f;
		public void DecreaseMoney( float nAmount ) {
		}
		public void IncreaseMoney( float nAmount ) {
		}

		public HashSet<Trait> GetTraits() => null;
		public bool HasTrait( Trait trait ) => false;
		public bool HasConflictingTrait( Trait other ) => false;
		public List<Trait> GetConflictingTraits( Trait other ) => null;
		public List<Trait> GetAgreeableTraits( Trait other ) => null;
		public void AddTrait( Trait trait ) {
		}
		public void RemoveTrait( Trait trait ) {
		}

		public void DetermineRelationStatus( Object other ) {
		}
		public void Meet( Object other ) {
		}
		public void RelationIncrease( Object other, float nAmount ) {
		}
		public void RelationDecrease( Object other, float nAmount ) {
		}
		public float GetRelationScore( Object other ) => 0.0f;
		public RelationStatus GetRelationStatus( Object other ) => RelationStatus.Count;

		public int GetRenownScore() => 0;
		public void AddRenown( int nAmount ) {
		}

		public void NotifyGroup( GroupEvent nEventType, Thinker source ) {
			for ( int i = 0; i < Thinkers.Count; i++ ) {
				Thinkers[i].Notify( nEventType, source );
			}
		}
		public void AddThinker( Thinker thinker ) {
			if ( Thinkers.Count == 0 ) {
				Leader = thinker;
				GD.Print( "Set leader to " + Leader );
			}
			Thinkers.Add( thinker );
		}
	};
};