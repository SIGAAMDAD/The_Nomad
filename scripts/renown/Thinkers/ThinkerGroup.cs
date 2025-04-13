using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public enum GroupType : uint {
		Military,

		// for later...
		Riot,
		Protest,
		
		Count
	};
	public enum GroupEvent : uint {
		TargetChanged,

		Count
	};

	public class ThinkerGroup {
		private List<Thinker> Thinkers;
		private Thinker Leader;
		private Faction Faction;
		private GroupType Type;

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

		public void NotifyGroup( GroupEvent nEventType, Thinker source ) {
			for ( int i = 0; i < Thinkers.Count; i++ ) {
				Thinkers[i].Notify( nEventType, source );
			}
		}
		public void AddThinker( Thinker thinker ) {
			if ( Thinkers.Count == 0 ) {
				Leader = thinker;
				Console.PrintLine( "Assigning leader..." );
			}
			Thinkers.Add( thinker );
		}
	};
};