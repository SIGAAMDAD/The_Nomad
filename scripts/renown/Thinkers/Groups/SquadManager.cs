using Godot;
using System.Collections.Generic;
using Renown.World;

namespace Renown.Thinkers.Groups {
	public class SquadManager {
		private static List<Squad> GroupCache = new List<Squad>();

		private static Squad CreateGroup( Faction faction ) {
			Squad group = new Squad( faction );
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
		public static Squad GetGroup( Faction faction, Godot.Vector2 position ) {
			Squad current = null;
			for ( int i = 0; i < GroupCache.Count; i++ ) {
				if ( GroupCache[ i ].Members.Count < Squad.MaxSquadMembers && GroupCache[ i ].Faction == faction ) {
					current = GroupCache[ i ];
					break;
				}
			}
			current ??= CreateGroup( faction );
			return current;
		}
	};
};