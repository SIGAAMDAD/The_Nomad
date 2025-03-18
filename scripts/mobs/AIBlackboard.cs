using System.Collections.Generic;

public class AIBlackboard {
	private Godot.Vector2 GuardPosition = Godot.Vector2.Zero;
	private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
	private Godot.Vector2 SightPosition = Godot.Vector2.Zero;
	private Godot.Vector2 GotoPosition = Godot.Vector2.Zero;
	private MobBase.Awareness Alertness = MobBase.Awareness.Relaxed;
	private int BodyCount = 0;
	private Godot.CharacterBody2D Target = null;
	private List<MobBase> SeenBodies = new List<MobBase>();
	private bool Investigating = false;
	private bool CanSeeTarget = false;
	private bool TargetReached = true;
	private float Fear = 0.0f;
	private float TargetDistance = 0.0f;
	private bool HasTarget = false;
	private int Stims = 0;
	
	public AIBlackboard() {
	}

	public void SetStims( int nStims ) { Stims = nStims; }
	public int GetStims() { return Stims; }

	public void SetHasTarget( bool bHasTarget ) { HasTarget = bHasTarget; }
	public bool GetHasTarget() { return HasTarget; }

	public void SetGotoPosition( Godot.Vector2 position ) { GotoPosition = position; }
	public Godot.Vector2 GetGotoPosition() { return GotoPosition; }

	public void SetTargetDistance( float distance ) { TargetDistance = distance; }
	public float GetTargetDistance() { return TargetDistance; }

	public void SetFear( float fear ) { Fear = fear; }
	public float GetFear() { return Fear; }

	public void SetTargetReached( bool bTargetReached ) { TargetReached = bTargetReached; }
	public bool GetTargetReached() { return TargetReached; }

	public void SetSightPosition( Godot.Vector2 sightPosition ) { SightPosition = sightPosition; }
	public Godot.Vector2 GetSightPosition() { return SightPosition; }

	public void SetCanSeeTarget( bool bCanSeeTarget ) { CanSeeTarget = bCanSeeTarget; }
	public bool GetCanSeeTarget() { return CanSeeTarget; }

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