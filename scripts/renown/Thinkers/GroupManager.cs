using System.Collections.Generic;
using Renown.World;

namespace Renown.Thinkers {
	public class GroupManager {
		private static List<ThinkerGroup> GroupCache = new List<ThinkerGroup>();

		private static ThinkerGroup CreateGroup( GroupType nType, Faction faction ) {
			ThinkerGroup group = null;

			switch ( nType ) {
			case GroupType.Military:
				group = new MilitarySquad( faction );
				break;
			case GroupType.Bandit:
				group = new BanditGroup( faction );
				break;
			default:
				Console.PrintError( "GroupManager.CreateGroup: Type isn't valid!" );
				return null;
			};

			GroupCache.Add( group );
			return group;
		}

		/// <summary>
		/// finds the nearest group in the faction with given type
		/// </summary>
		/// <param>Group type</param>
		/// <param>Faction</param>
		/// <param>Thinker's position</param>
		/// <returns>ThinkerGroup</returns>
		public static ThinkerGroup GetGroup( GroupType nType, Faction faction, Godot.Vector2 position ) {
			ThinkerGroup current = null;
			float bestDistance = float.MaxValue;
			for ( int i = 0; i < GroupCache.Count; i++ ) {
				if ( GroupCache[i].GetFaction() == faction && GroupCache[i].GetGroupType() == nType ) {
					float distance = position.DistanceTo( GroupCache[i].GetLeader().GlobalPosition );
					if ( distance < bestDistance ) {
						bestDistance = distance;
						current = GroupCache[i];
					}
				}
			}
			if ( current == null ) {
				current = CreateGroup( nType, faction );
			}
			return current;
		}
	};
};