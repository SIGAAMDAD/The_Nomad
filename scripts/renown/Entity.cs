using Godot;
using Renown.World;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// the base parent class which all living renown entities inherit from
/// implements most common renown events
/// </summary>
namespace Renown {
	public partial class Entity : CharacterBody2D, Object {
		[ExportCategory( "Base Stats" )]
		[Export]
		protected float Health;
		[Export]
		protected int RenownScore = 0;

		[Export]
		protected WorldArea Location;

		[ExportCategory( "Faction" )]
		[Export]
		protected Faction Faction;
		[Export]
		protected int FactionImportance;

		protected Dictionary<string, StatusEffect> StatusEffects = new Dictionary<string, StatusEffect>();

		[Signal]
		public delegate void DamagedEventHandler( Entity source, Entity target, float nAmount );
		[Signal]
		public delegate void DieEventHandler( Entity source, Entity target );

		public WorldArea GetLocation() => Location;
		public virtual void SetLocation( in WorldArea location ) => Location = location;

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		//
		// narrative functions
		//
		public virtual void OnChallenged() {
		}
		public virtual void OnIntimidated() {
			float crueltyScore = LevelData.Instance.ThisPlayer.GetTraitScore( TraitType.Cruel );
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

		public virtual void PickupWeapon( WeaponEntity weapon ) {
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

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual bool HasRelation( Object other ) => false;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual float GetRelationScore( Object other ) => 0.0f;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual RelationStatus GetRelationStatus( Object other ) => RelationStatus.Neutral;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual void DetermineRelationStatus( Object other ) { }
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual int GetRenownScore() => RenownScore;

		public float GetHealth() => Health;
		public NodePath GetHash() => GetPath();
		public virtual StringName GetObjectName() => "Entity";

		public Faction GetFaction() => Faction;
		public int GetFactionImportance() => FactionImportance;

		public override void _Ready() {
			base._Ready();

			if ( LevelData.Instance != null ) {
				LevelData.Instance.PlayerRespawn += ClearStatusEffects;
			}
		}
	};
};