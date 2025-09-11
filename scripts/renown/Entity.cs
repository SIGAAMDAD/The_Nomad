/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using Renown.World;
using ResourceCache;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Renown {
	/*
	===================================================================================
	
	Entity
	
	===================================================================================
	*/
	/// <summary>
	/// The base class from which all entities inherit from
	/// </summary>
	
	public partial class Entity : CharacterBody2D, Object {
		[ExportCategory( "Base Stats" )]

		/// <summary>
		/// The health of the entity
		/// </summary>
		[Export]
		public float Health { get; protected set; } = 0.0f;

		/// <summary>
		/// The renown of an entity, think of it sort as "street cred"
		/// </summary>
		[Export]
		public int RenownScore { get; protected set; } = 0;

		/// <summary>
		/// The WorldArea the entity is currently colliding with
		/// </summary>
		[Export]
		public WorldArea? Location { get; protected set; } = null;

		[ExportCategory( "Faction" )]

		/// <summary>
		/// The faction that the entity belongs to. Used for faction-specific behaviors with the AI
		/// </summary>
		[Export]
		public Faction? Faction { get; protected set; } = null;

		/// <summary>
		/// How much the entity's faction cares about them
		/// </summary>
		[Export]
		public int FactionImportance { get; protected set; } = 0;

		/// <summary>
		/// The current status effects being applied to the entity
		/// </summary>
		/// <seealso cref="AddStatusEffect"/>
		protected ConcurrentDictionary<string, StatusEffect> StatusEffects = new ConcurrentDictionary<string, StatusEffect>();

		[Signal]
		public delegate void DamagedEventHandler( Entity source, Entity target, float amount );
		[Signal]
		public delegate void DieEventHandler( Entity source, Entity target );

		/*
		===============
		GetHash
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public NodePath GetHash() {
			return GetPath();
		}

		/*
		===============
		GetObjectName
		===============
		*/
		/// <summary>
		/// Returns the entity's object name, for debugging purposes
		/// </summary>
		/// <returns>The debug name of a <see cref="Renown.Object"/></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual StringName GetObjectName() {
			return "Entity";
		}

		/*
		===============
		GetRenownScore
		===============
		*/
		/// <summary>
		/// Returns the renown score of an entity if the entity has a renown score, otherwise,
		/// 0
		/// </summary>
		/// <returns>The entity's renown score (if present), or 0</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual int GetRenownScore() {
			return 0;
		}

		/*
		===============
		GetMoney
		===============
		*/
		/// <summary>
		/// Returns the money value of an entity if the entity has a money value, otherwise,
		/// 0
		/// </summary>
		/// <returns>The entity's money (if present), or 0</returns>
		public virtual float GetMoney() {
			return 0.0f;
		}

		/*
		===============
		SetLocation
		===============
		*/
		public virtual void SetLocation( in WorldArea location ) {
			Location = location;
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		/// Override this function to archive and entity's state to disk
		/// </summary>
		public virtual void Save() {
		}

		/*
		===============
		Load
		===============
		*/
		/// <summary>
		/// Override this function to load an entity's state from disk
		/// </summary>
		public virtual void Load() {
		}

		/*
		 * narrative functions
		 */


		/*
		===============
		OnChallenged
		===============
		*/
		public virtual void OnChallenged() {
		}

		/*
		===============
		OnIntimidated
		===============
		*/
		public virtual void OnIntimidated() {
		}

		/*
		===============
		ClearStatusEffects
		===============
		*/
		/// <summary>
		/// Clears all StatusEffect objects currently being applied to the entity and
		/// clears the Dictionary
		/// </summary>
		public virtual void ClearStatusEffects() {
			foreach ( var effect in StatusEffects ) {
				effect.Value.Stop();
			}
			StatusEffects.Clear();
		}

		/*
		===============
		AddStatusEffects
		===============
		*/
		/// <summary>
		/// Adds a StatusEffect object to the entity
		/// </summary>
		/// <param name="effectName">The scene file name of the status effect</param>
		public virtual void AddStatusEffect( string effectName ) {
			if ( effectName == null || effectName.Length == 0 ) {
				Console.PrintError( $"Entity.AddStatusEffect: invalid effectName (null or empty)" );
				return;
			}
			if ( StatusEffects.TryGetValue( effectName, out StatusEffect? data ) ) {
				data.ResetTimer();
				return;
			}

			PackedScene? scene = SceneCache.GetScene( $"res://scenes/status_effects/{effectName}.tscn" );
			if ( scene == null ) {
				Console.PrintError(
					$"Entity.AddStatusEffect: invalid effect, ensure that all status effects are located in res://scenes/status_effects/ as scene files"
				);
				return;
			}

			StatusEffect effect = scene.Instantiate<StatusEffect>();
			effect.SetVictim( this );
			StatusEffects.TryAdd( effectName, effect );
			effect.Timeout += () => {
				CallDeferred( MethodName.RemoveChild, effect );
				effect.CallDeferred( MethodName.QueueFree );
				StatusEffects.TryRemove( new KeyValuePair<string, StatusEffect>( effectName, effect ) );
			};
			CallDeferred( MethodName.AddChild, effect );
		}

		/*
		===============
		PickupWeapon
		===============
		*/
		public virtual void PickupWeapon( WeaponEntity weapon ) {
		}

		/*
		===============
		PlaySound
		===============
		*/
		/// <summary>
		/// Plays a sound effect from the provided channel and stream local to the entity's position
		/// </summary>
		/// <param name="channel">The audio channel to play the audio from</param>
		/// <param name="stream">The audio file to play</param>
		public virtual void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
			if ( channel == null ) {
				Console.PrintError( "Entity.PlaySound: channel is null" );
				return;
			}

			// no need to check stream here since it'll just stay silent
			channel.Stream = stream;
			channel.Play();
		}

		/*
		===============
		Damage
		===============
		*/
		public virtual void Damage( in Entity source, float amount ) {
			EmitSignalDamaged( source, this, amount );
			Health -= amount;

			if ( Health <= 0.0f ) {
				EmitSignalDie( source, this );
			}
		}

		/*
		===============
		HasRelation
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual bool HasRelation( Object other ) {
			return false;
		}

		/*
		===============
		GetRelationScore
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual float GetRelationScore( Object other ) {
			return 0.0f;
		}

		/*
		===============
		GetRelationStatus
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual RelationStatus GetRelationStatus( Object other ) {
			return RelationStatus.Neutral;
		}

		/*
		===============
		DetermineRelationStatus
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual void DetermineRelationStatus( Object other ) {
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			if ( LevelData.Instance != null ) {
				LevelData.Instance.PlayerRespawn += ClearStatusEffects;
			}

			EntityManager.RegisterProcess( this );
			EntityManager.RegisterPhysicsProcess( this );
		}

		/*
		===============
		Update
		===============
		*/
		public virtual void Update( double delta ) {
		}

		/*
		===============
		PhysicsUpdate
		===============
		*/
		public virtual void PhysicsUpdate( double delta ) {
		}
	};
};