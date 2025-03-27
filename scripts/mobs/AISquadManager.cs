using Godot;
using System.Collections.Generic;

public class AISquadManager {
	private static System.Collections.Generic.List<AISquad> SquadList =
		new System.Collections.Generic.List<AISquad>();
	
	public static AISquad GetOrCreateSquad( MobBase mob, System.Object faction ) {
		// TODO: make faction a part of this
		for ( int i = 0; i < SquadList.Count; i++ ) {
//			if ( SquadList[i].AddSquadMember( mob ) ) {
//				return SquadList[i];
//			}
		}
		
		GD.Print( "Creating new AISquad..." );
		SquadList.Add( new AISquad() );
//		SquadList[ SquadList.Count - 1 ].AddSquadMember( mob );
		return SquadList[ SquadList.Count - 1 ];
	}
	public static void DisbandSquad( AISquad squad ) {
		GD.Print( "Disbanding AISquad " + squad + "..." );
		SquadList.Remove( squad );
	}
};