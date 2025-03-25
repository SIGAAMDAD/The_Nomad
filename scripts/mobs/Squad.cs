using System.Diagnostics.CodeAnalysis;
using Godot;

public partial class Squad : Node2D {
	private int SquadID = -1;

	private CollisionShape2D SquadAABB;
	private System.Collections.Generic.List<MobBase> SquadMembers = new System.Collections.Generic.List<MobBase>();
	private int SquadMemberCount = 0;

	private bool SquadMemberDied = false;

	public Squad() {
	}

	public int GetNumSquadMembers() {
		return SquadMemberCount;
	}
	public void AddSquadMember( MobBase member ) {

	}
	public void HandleSquadDeaths() {
		bool bDead = false;
		int iMember;

		for ( iMember = 0; iMember < SquadMemberCount; ++iMember ) {
			if ( SquadMembers[iMember].GetHealth() <= 0.0f ) {
				SquadMembers[iMember] = null;
				bDead = true;
				break;
			}
		}

		if ( !bDead ) {
			return;
		}

		int iDead = iMember;
		for ( iMember = iDead; iMember < SquadMemberCount - 1; ++iMember ) {
			// keep track of body counts
//			int nBodyCount = (int)SquadMembers[iMember].GetBlackboard( "BodyCount" );
//			SquadMembers[iMember].SetBlackboard( "BodyCount", nBodyCount + 1 );
		}
	}
};