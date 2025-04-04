using Godot;
using Renown;
using Renown.World;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// the base parent class which all living renown entities inherit from
/// implements most common renown events
/// </summary>
public partial class Entity : CharacterBody2D {
	[ExportCategory("Base Stats")]
	[Export]
	protected float Health;
	[Export]
	protected int RenownScore = 0;
	[Export]
	protected float Money = 0.0f;
	[Export]
	protected int WarCrimeCount = 0;
	[Export]
	private TraitType[] Traits = null;
	[Export]
	private Godot.Collections.Dictionary<Node, float> Relations = null;
	[Export]
	private Godot.Collections.Dictionary<Node, float> Debts = null;
	
	[Export]
	protected WorldArea Location;
	
	[ExportCategory("Faction")]
	[Export]
	protected Faction Faction;
	[Export]
	protected int FactionImportance;
	
	protected HashSet<Trait> TraitCache = null;
	protected Dictionary<Node, float> RelationCache = null;
	protected Dictionary<Node, float> DebtCache = null;
	
	[Signal]
	public delegate void DamageEventHandler( Entity source, Entity target, float nAmount );
	
	//
	// renown events
	//
	[Signal]
	public delegate void DieEventHandler( Entity source, Entity target );
	[Signal]
	public delegate void JoinFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void LeaveFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void FactionPromotionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void FactionDemotionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void GainMoneyEventHandler( Entity entity, float nAmount );
	[Signal]
	public delegate void LoseMoneyEventHandler( Entity entity, float nAmount );
	[Signal]
	public delegate void CommitWarCrimeEventHandler( Entity entity, WarCrimeType nType );
	[Signal]
	public delegate void EarnTraitEventHandler( Entity entity, Trait trait );
	[Signal]
	public delegate void LoseTraitEventHandler( Entity entity, Trait trait );
	[Signal]
	public delegate void MeetEntityEventHandler( Entity other, Entity entity );
	[Signal]
	public delegate void MeetFactionEventHandler( Faction faction, Entity entity );
	[Signal]
	public delegate void RelationIncreaseEventHandler( Node other, Entity entity, float nAmount );
	[Signal]
	public delegate void RelationDecreaseEventHandler( Node other, Entity entity, float nAmount );
	[Signal]
	public delegate void StartContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void CompleteContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void FailedContractEventHandler( Contract contract, Entity entity );
	[Signal]
	public delegate void CanceledContractEventHandler( Contract contract, Entity entity );
	
	public virtual void Damage( Entity source, float nAmount ) {
		EmitSignal( "Damage", source, this, nAmount );
		Health -= nAmount;
		
		if ( Health <= 0.0f ) {
			EmitSignal( "Die", source, this );
		}
	}
	
	public float GetHealth() => Health;
	
	public float GetMoney() => Money;
	public virtual void DecreaseMoney( float nAmount ) {
		Money -= nAmount;
		EmitSignal( "LoseMoney", this, nAmount );
	}
	public virtual void IncreaseMoney( float nAmount ) {
		Money += nAmount;
		EmitSignal( "GainMoney", this, nAmount );
	}
	
	public int GetWarCrimeCount() => WarCrimeCount;
	public virtual void CommitWarCrime( WarCrimeType nType ) {
		EmitSignal( "CommitWarCrime", this, nType );
		WarCrimeCount++;
	}
	
	public Faction GetFaction() => Faction;
	public virtual void SetFaction( Faction faction ) {
		// sanity
		if ( Faction == faction ) {
			Console.PrintWarning( "Entity.SetFaction: same faction" );
			return;
		}
		if ( faction != null ) {
			CallDeferred( "emit_signal", "JoinFaction", faction, this );
		} else {
			CallDeferred( "emit_signal", "LeaveFaction", Faction, this );
		}
		Faction = faction;
	}
	
	public int GetFactionImportance() => FactionImportance;
	public virtual void DecreaseFactionImportance( int nAmount ) {
		// sanity
		if ( Faction == null ) {
			Console.PrintError( "Entity.DecreaseFactionImportance: no faction" );
			return;
		}
		FactionImportance -= nAmount;
		EmitSignal( "FactionDemotion", Faction, this );
	}
	public virtual void IncreaseFactionImportance( int nAmount ) {
		// sanity
		if ( Faction == null ) {
			Console.PrintError( "Entity.IncreaseFactionImportance: no faction" );
			return;
		}
		FactionImportance += nAmount;
		EmitSignal( "FactionPromotion", Faction, this );
	}
	
	public int GetRenownScore() => RenownScore;
	public void AddRenown( int nAmount ) {
		RenownScore += nAmount;
	}
	
	/// <summary>
	/// returns true if the entity has the given trait
	/// </summary>
	public bool HasTrait( Trait trait ) => TraitCache.Contains( trait );
	
	/// <summary>
	/// checks if the given trait conflicts with this entity's values
	/// </summary>
	public bool HasConflictingTrait( Trait other ) {
		foreach ( var trait in TraitCache ) {
			if ( trait.Conflicts( other ) ) {
				return bool;
			}
		}
		return false;
	}
	public List<Trait> GetConflicingTraits( Trait other ) {
		List<Trait> traits = new List<Trait>();
		foreach ( var trait in TraitCache ) {
			if ( trait.Conflicts( other ) ) {
				traits.Add( trait );
			}
		}
		return traits;
	}
	public List<Trait> GetAgreeableTraits( Trait other ) {
		List<Trait> traits = new List<Trait>();
		foreach ( var trait in TraitCache ) {
			if ( trait.Agrees( other ) ) {
				traits.Add( trait );
			}
		}
		return traits;
	}
	public virtual void AddTrait( Trait trait ) {
		EmitSignal( "EarnTrait", this, trait );
		TraitCache.Add( trait );
	}
	public virtual void RemoveTrait( TraitType nType ) {
		EmitSignal( "LoseTrait", this, nType );
		TraitCache.Remove( nType );
	}
	
	protected virtual void DetermineRelationStatus( Node other ) {
		float score = RelationCache[ other ];
		int renownScore = (int)other.Call( "GetRenownScore" );
		
		// TODO: write some way of using renown to determine if the entity knows all this stuff about the other one
		
		if ( Faction.GetRelationStatus( other ) >= RelationStatus.Hates ) {
			score -= Faction.GetRelationScore( other );
		}
		
		HashSet<Trait> traitList = other.GetTraits();
		foreach ( var trait in traitList ) {
			List<Trait> conflicting = GetConflictingTraits( trait );
			for ( int i = 0; i < conflicting.Count; i++ ) {
				score -= conflicting[i].GetNegativeRelationScore( trait );
			}
			
			List<Trait> agreeables = GetAgreeableTraits( trait );
			for ( int i = 0; i < agreeables.Count; i++ ) {
				score += conflicting[i].GetPositiveRelationScore( trait );
			}
		}
		
		RelationCache[ other ] = score;
	}
	protected virtual void Meet( Node other ) {
		RelationCache.Add( other, 0.0f );
		
		Entity entity = other as Entity;
		if ( entity != null ) { 
			EmitSinal( "MeetEntity", entity, this );
		} else {
			Faction faction = other as Faction;
			if ( faction == null ) {
				Console.PushError( "Entity.Meet: node isn't an entity or faction!" );
				return;
			}
			EmitSignal( "MeetFaction", faction, this );
		}
	}
	public bool HasRelation( Node other ) => RelationCache.ContainsKey( other );
	public virtual void RelationIncrease( Node other, float nAmount ) {
		if ( !RelationCache.TryGetValue( other, out float score ) ) {
			Meet( other );
		}
		score += nAmount;
		EmitSignal( "RelationIncrease", other, this, nAmount );
		RelationCache[ other ] = score;
	}
	public virtual void RelationDecrease( Node other, float nAmount ) {
		if ( !RelationCache.TryGetValue( other, out float score ) ) {
			Meet( other );
		}
		score -= nAmount;
		EmitSignal( "RelationDecrease", other, this, nAmount );
		RelationCache[ other ] = score;
	}
	
	public override void _Ready() {
		base._Ready();
		
		TraitCache = new HashSet<Trait>( Traits.Length );
		for ( int i = 0; i < Traits.Length; i++ ) {
//			FIXME:
//			TraitCache.Add( new Trait(  ) );
		}
		
		RelationCache = new Dictionary<Node, float>( Relations.Count );
		foreach ( var relation in Relations ) {
			RelationCache.Add( relation.Key, relation.Value );
		}
		
		DebtCache = new Dictionary<Node, float>( Debts.Count );
		foreach ( var debt in Debts ) {
			DebtCache.Add( debt.Key, debt.Value );
		}
	}
};
