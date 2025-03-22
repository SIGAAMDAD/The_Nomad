using Godot;

public partial class AISquad : Node2D {
	private static int MAX_SQUAD_MEMBERS = 20;
	
	private int SquadID = -1;

	private NavigationRegion2D SquadRegion;
	private MobBase[] SquadMembers = new MobBase[ MAX_SQUAD_MEMBERS ];
	private int NumMembersAlive = 0;
	private int SquadMemberCount = 0;
	
	private MobBase SquadLeader = null;
//	private AIActivity Activity = null;
	private CharacterBody2D SquadTarget = null;
	
	private Timer CheckInTimer = null;
	/*

	[Signal]
	public delegate void ReportToSquadEventHandler( bool bPanic );
	
	public void Report() {
		System.Threading.Interlocked.Add( ref NumMembersAlive, 1 );
	}
	public MobBase GetLeader() {
		return SquadLeader;
	}
	private void OnLeaderBarkFinished() {
		switch ( SquadLeader.GetLastBark() ) {
		case BarkType.Report: {
			EmitSignal( "ReportToSquad", false );
			if ( NumMembersAlive < SquadMemberCount ) {
				SquadLeader.Bark( BarkType.CheckItOut );
			}
			break; }
		case BarkType.ReportPanic: {
			EmitSignal( "ReportToSquad", true );
			if ( NumMembersAlive < SquadMemberCount ) {
				SquadLeader.Bark( BarkType.Curse, BarkType.CheckItOutPanic );
			}
			break; }
		};
	}
	private void OnCheckInTimerTimeout() {
		if ( SquadLeader == null || SquadLeader.IsDead() ) {
			return;
		}
		NumMembersAlive = 0;
		SquadLeader.Bark( BarkType.Report );
	}
	
	public AISquad() {
		CheckInTimer = new Timer();
		CheckInTimer.WaitTime = 30.0f;
		CheckInTimer.Autostart = true;
		CheckInTimer.Connect( "timeout", Callable.From( OnCheckInTimerTimeout ) );
	}
	public void Init() {
		SquadMemberCount = 0;
		SquadLeader = null;
	}
	
	public int GetNumSquadMembers() {
		return SquadMemberCount;
	}
	public bool AddSquadMember( MobBase member ) {
		if ( SquadMemberCount < MAX_SQUAD_MEMBERS ) {
			if ( SquadMemberCount == 0 ) {
				SquadLeader = member;
				SquadLeader.BarkChannel.Finished += OnLeaderBarkFinished;
			}
			SquadMembers[ SquadMemberCount ] = member;
			SquadMemberCount++;
			return true;
		}
		return false;
	}
	public void MergeSquad() {
		
	}
	public void UpdateSquad() {
		
	}
	public void NotifyTarget( CharacterBody2D target ) {
		SquadTarget = target;
		
		for ( int i = 0; i < SquadMemberCount; i++ ) {
			SquadMembers[i].GetBlackboard().SetTarget( target );
		}
	}
	*/
};