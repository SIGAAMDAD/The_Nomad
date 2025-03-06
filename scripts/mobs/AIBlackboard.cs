using System.Collections.Generic;

public class AIBlackboard {
	private Godot.Vector2 GuardPosition = Godot.Vector2.Zero;
	private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
	private MobBase.Awareness Alertness = MobBase.Awareness.Relaxed;
	private int BodyCount = 0;
	private Godot.CharacterBody2D Target = null;
	private List<MobBase> SeenBodies = new List<MobBase>();
	private bool Investigating = false;
	
	public AIBlackboard() {
	}
	
	public void SetInvestigating( bool bInvestigating ) { Investigating = bInvestigating; }
	public bool GetInvestigating() { return Investigating; }
	
	public void SetAwareness( MobBase.Awareness eAwareness ) { Alertness = eAwareness; }
	public MobBase.Awareness GetAwareness() { return Alertness; }
	
	public void SetGuardPosition( Godot.Vector2 position ) { GuardPosition = position; }
	public Godot.Vector2 GetGuardPosition() { return GuardPosition; }
	
	public void SetLastTargetPosition( Godot.Vector2 position ) { LastTargetPosition = position; }
	public Godot.Vector2 GetLastTargetPosition() { return LastTargetPosition; }
	
	public void SetBodyCount( int nBodyCount ) { BodyCount = nBodyCount; }
	public int GetBodyCount() { return BodyCount; }
	
	public void SetTarget( Godot.CharacterBody2D target ) { Target = target; }
	public Godot.CharacterBody2D GetTarget() { return Target; }
	
	public List<MobBase> GetSeenBodies() { return SeenBodies; }
	public void AddSeenBody( MobBase body ) { SeenBodies.Add( body ); }
};