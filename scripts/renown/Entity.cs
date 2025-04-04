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
	
	protected HashSet<TraitType> TraitCache = null;
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
	public delegate void EarnTraitEventHandler( Entity entity, TraitType nType );
	[Signal]
	public delegate void LoseTraitEventHandler( Entity entity, TraitType nType );
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
	public void DecreaseMoney( float nAmount ) {
		Money -= nAmount;
		EmitSignal( "LoseMoney", this, nAmount );
	}
	public void IncreaseMoney( float nAmount ) {
		Money += nAmount;
		EmitSignal( "GainMoney", this, nAmount );
	}
	
	public int GetWarCrimeCount() => WarCrimeCount;
	public void CommitWarCrime( WarCrimeType nType ) {
		EmitSignal( "CommitWarCrime", this, nType );
		WarCrimeCount++;
	}
	
	public Faction GetFaction() => Faction;
	public void SetFaction( Faction faction ) {
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
	public void DecreaseFactionImportance( int nAmount ) {
		// sanity
		if ( Faction == null ) {
			Console.PrintError( "Entity.DecreaseFactionImportance: no faction" );
			return;
		}
		FactionImportance -= nAmount;
		EmitSignal( "FactionDemotion", Faction, this );
	}
	public void IncreaseFactionImportance( int nAmount ) {
		// sanity
		if ( Faction == null ) {
			Console.PrintError( "Entity.IncreaseFactionImportance: no faction" );
			return;
		}
		FactionImportance += nAmount;
		EmitSignal( "FactionPromotion", Faction, this );
	}
	
	public bool HasTrait( TraitType nType ) => TraitCache.Contains( nType );
	public void AddTrait( TraitType nType ) {
		EmitSignal( "EarnTrait", this, nType );
		TraitCache.Add( nType );
	}
	public void RemoveTrait( TraitType nType ) {
		EmitSignal( "LoseTrait", this, nType );
		TraitCache.Remove( nType );
	}
	
	private void Meet( Node other ) {
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
	public void RelationIncrease( Node other, float nAmount ) {
		if ( !RelationCache.TryGetValue( other, out float score ) ) {
			Meet( other );
		}
		score += nAmount;
		EmitSignal( "RelationIncrease", other, this, nAmount );
		RelationCache[ other ] = score;
	}
	public void RelationDecrease( Node other, float nAmount ) {
		if ( !RelationCache.TryGetValue( other, out float score ) ) {
			Meet( other );
		}
		score -= nAmount;
		EmitSignal( "RelationDecrease", other, this, nAmount );
		RelationCache[ other ] = score;
	}
	
	public override void _Ready() {
		base._Ready();
		
		TraitCache = new HashSet<TraitType>( Traits.Length );
		for ( int i = 0; i < Traits.Length; i++ ) {
			TraitCache.Add( Traits[i] );
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
