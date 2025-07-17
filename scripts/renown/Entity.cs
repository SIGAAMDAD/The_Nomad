using Godot;
using Renown.World;
using System;
using System.Collections.Generic;

/// <summary>
/// the base parent class which all living renown entities inherit from
/// implements most common renown events
/// </summary>
namespace Renown {
	public partial class Entity : CharacterBody2D, Object {
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
		protected Godot.Collections.Array<TraitType> Traits = new Godot.Collections.Array<TraitType>();
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Relations = null;
		[Export]
		protected Godot.Collections.Dictionary<Node, float> Debts = null;

		[Export]
		protected WorldArea Location;

		[ExportCategory("Faction")]
		[Export]
		protected Faction Faction;
		[Export]
		protected int FactionImportance;

		protected Dictionary<string, StatusEffect> StatusEffects = new Dictionary<string, StatusEffect>();

		protected HashSet<Trait> TraitCache = null;
		protected HashSet<RenownValue> RelationCache = null;
		protected HashSet<RenownValue> DebtCache = null;

		protected List<LightData> LightSources = new List<LightData>();
		protected float LightAmount = 0.0f;

		protected readonly object Lock = new object();

		[Signal]
		public delegate void DamagedEventHandler( Entity source, Entity target, float nAmount );

		//
		// renown events
		//
		[Signal]
		public delegate void DieEventHandler( Entity source, Entity target );

		public WorldArea GetLocation() => Location;
		public virtual void SetLocation( in WorldArea location ) => Location = location;

		public virtual void AddLightSource( in LightData lightSource ) => LightSources.Add( lightSource );
		public virtual void RemoveLightSource( in LightData lightSource ) => LightSources.Remove( lightSource );
		public float GetLightAmount() => LightAmount;

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		public virtual void ClearStatusEffects() {
			foreach ( var effect in StatusEffects ) {
				effect.Value.Stop();
			}
			StatusEffects.Clear();
		}
		public virtual void AddStatusEffect( string effectName ) {
			if ( StatusEffects.TryGetValue( effectName, out StatusEffect data ) ) {
				data.ResetTimer();
				return;
			}
			StatusEffect effect = ResourceCache.GetScene( "res://scenes/status_effects/" + effectName + ".tscn" ).Instantiate<StatusEffect>();
			effect.SetVictim( this );
			StatusEffects.Add( effectName, effect );
			effect.Timeout += () => {
				CallDeferred( MethodName.RemoveChild, effect );
				effect.CallDeferred( MethodName.QueueFree );
				StatusEffects.Remove( effectName );
			};
			CallDeferred( MethodName.AddChild, effect );
		}

		public virtual void PickupWeapon( in WeaponEntity weapon ) {
		}

		public virtual void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
			channel.Stream = stream;
			channel.Play();
		}

		public virtual void Damage( in Entity source, float nAmount ) {
			EmitSignalDamaged( source, this, nAmount );
			Health -= nAmount;

			if ( Health <= 0.0f ) {
				EmitSignalDie( source, this );
			}
		}

		public float GetHealth() => Health;
		public NodePath GetHash() => GetPath();
		public virtual StringName GetObjectName() => "Entity";

		public Faction GetFaction() => Faction;
		public int GetFactionImportance() => FactionImportance;

		public virtual void DetermineRelationStatus( Object other ) {
			if ( !RelationCache.TryGetValue( new RenownValue( other ), out RenownValue value ) ) {
				return;
			}
			float score = value.Value;
			int renownScore = other.GetRenownScore();

			// TODO: write some way of using renown to determine if the entity knows all this stuff about the other one

			if ( Faction.GetRelationStatus( other ) >= RelationStatus.Hates ) {
				score -= Faction.GetRelationScore( other );
			}

			/*
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
			*/

			value.Value = score;
		}
		public bool HasRelation( Object other ) => RelationCache.Contains( new RenownValue( other ) );
		public float GetRelationScore( Object other ) => RelationCache.TryGetValue( new RenownValue( other ), out RenownValue score ) ? score.Value : 0.0f;
		public RelationStatus GetRelationStatus( Object other ) {
			float score = GetRelationScore( other );
			
			if ( score < -100.0f ) {
				return RelationStatus.KendrickAndDrake;
			}
			if ( score < -50.0f ) {
				return RelationStatus.Hates;
			}
			if ( score < 0.0f ) {
				return RelationStatus.Dislikes;
			}
			if ( score > 25.0f ) {
				return RelationStatus.Friends;
			}
			if ( score > 100.0f ) {
				return RelationStatus.GoodFriends;
			}
			return RelationStatus.Neutral;
		}

		public override void _Ready() {
			base._Ready();

			if ( LevelData.Instance != null ) {
				LevelData.Instance.PlayerRespawn += ClearStatusEffects;
			}

			TraitCache = new HashSet<Trait>( Traits.Count );
			for ( int i = 0; i < Traits.Count; i++ ) {
				TraitCache.Add( Trait.Create( Traits[i] ) );
			}

			if ( Relations != null ) {
				RelationCache = new HashSet<RenownValue>( Relations != null ? Relations.Count : 0 );
				foreach ( var relation in Relations ) {
					if ( relation.Key is Faction faction && faction != null ) {
						RelationCache.Add( new RenownValue( faction, relation.Value ) );
					} else if ( relation.Key is Entity entity && entity != null ) {
						RelationCache.Add( new RenownValue( entity, relation.Value ) );
					} else {
						Console.PrintError( string.Format( "Entity._Ready: relation key {0} isn't a renown object!", relation.Key != null ? relation.Key.GetPath() : "nil" ) );
					}
				}
			} else {
				RelationCache = new HashSet<RenownValue>();
			}

			if ( Debts != null ) {
				DebtCache = new HashSet<RenownValue>( Debts != null ? Debts.Count : 0 );
				foreach ( var debt in Debts ) {
					if ( debt.Key is Faction faction && faction != null ) {
						DebtCache.Add( new RenownValue( faction, debt.Value ) );
					} else if ( debt.Key is Entity entity && entity != null ) {
						DebtCache.Add( new RenownValue( entity, debt.Value ) );
					} else {
						Console.PrintError( string.Format( "Entity._Ready: debt key {0} isn't a renown object!", debt.Key != null ? debt.Key.GetPath() : "nil" ) );
					}
				}
			} else {
				DebtCache = new HashSet<RenownValue>();
			}
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 20 ) != 0 ) {
				return;
			}

			base._Process( delta );

			/*

			Lock.WaitOne();
			LightAmount = 0.0f;
			for ( int i = 0; i < LightSources.Count; i++ ) {
				float distance = GlobalPosition.DistanceTo( LightSources[i].GlobalPosition );
				LightAmount += ( LightSources[i].Energy * LightSources[i].TextureScale ) - distance;
			}
			Lock.ReleaseMutex();
			
			*/
		}
	};
};