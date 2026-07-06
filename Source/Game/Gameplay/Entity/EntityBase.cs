/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any type,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Interactables;

namespace Nomad.Game.Gameplay.Entity
{
	/*
	===================================================================================

	EntityBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class EntityBase : IEntityBase
	{
		public EntityId Id => _entityId;
		private readonly EntityId _entityId;

		public InternString DefinitionId => _definitionId;
		private readonly InternString _definitionId;

		public InternString DisplayName => _displayName;
		private InternString _displayName;

		public EntityType Type => _type;
		private readonly EntityType _type;

		public EntityLifecycleState LifecycleState => _lifecycleState;
		private EntityLifecycleState _lifecycleState = EntityLifecycleState.Created;

		public EntityFlags Flags => _flags;
		private EntityFlags _flags;

		public uint Revision => _revision;
		private uint _revision = 1;

		public IReadOnlyCollection<InternString> Tags => _tags;
		private readonly HashSet<InternString> _tags = new();

		public bool IsValid => _entityId.IsValid;

		public bool IsSpawned =>
			_lifecycleState == EntityLifecycleState.Spawned ||
			_lifecycleState == EntityLifecycleState.Active ||
			_lifecycleState == EntityLifecycleState.Inactive ||
			_lifecycleState == EntityLifecycleState.Hidden;

		public bool IsActive => _lifecycleState == EntityLifecycleState.Active;

		public bool IsHidden =>
			_lifecycleState == EntityLifecycleState.Hidden ||
			_flags.HasFlag( EntityFlags.Hidden );

		public bool IsDestroyed =>
			_lifecycleState == EntityLifecycleState.Destroyed ||
			_flags.HasFlag( EntityFlags.Destroyed );

		public bool IsDisposed => _isDisposed;
		private bool _isDisposed = false;

		public bool IsInteractable =>
			!_isDisposed &&
			!IsDestroyed &&
			_flags.HasFlag( EntityFlags.Interactable ) &&
			!_flags.HasFlag( EntityFlags.InteractionDisabled ) &&
			!_flags.HasFlag( EntityFlags.Locked );

		/*
		===============
		EntityBase
		===============
		*/
		protected EntityBase(
			EntityId entityId,
			InternString definitionId,
			InternString displayName,
			EntityType type,
			EntityFlags flags = EntityFlags.None
		)
		{
			if ( !entityId.IsValid ) {
				throw new ArgumentException( "Entity id must be valid.", nameof( entityId ) );
			}

			_entityId = entityId;
			_definitionId = definitionId;
			_displayName = displayName;
			_type = type;
			_flags = flags;
		}

		/*
		===============
		Spawn
		===============
		*/
		public virtual void Spawn()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			SetLifecycleState( EntityLifecycleState.Spawned );
			OnSpawned();
		}

		/*
		===============
		Activate
		===============
		*/
		public virtual void Activate()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			SetLifecycleState( EntityLifecycleState.Active );
			OnActivated();
		}

		/*
		===============
		Deactivate
		===============
		*/
		public virtual void Deactivate()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			SetLifecycleState( EntityLifecycleState.Inactive );
			OnDeactivated();
		}

		/*
		===============
		Hide
		===============
		*/
		public virtual void Hide()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			AddFlags( EntityFlags.Hidden );
			SetLifecycleState( EntityLifecycleState.Hidden );
			OnHidden();
		}

		/*
		===============
		Show
		===============
		*/
		public virtual void Show()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			RemoveFlags( EntityFlags.Hidden );

			if ( _lifecycleState == EntityLifecycleState.Hidden ) {
				SetLifecycleState( EntityLifecycleState.Active );
			}

			OnShown();
		}

		/*
		===============
		Despawn
		===============
		*/
		public virtual void Despawn()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			SetLifecycleState( EntityLifecycleState.Despawning );
			OnDespawning();

			SetLifecycleState( EntityLifecycleState.Inactive );
			OnDespawned();
		}

		/*
		===============
		Destroy
		===============
		*/
		public virtual void Destroy()
		{
			ThrowIfDisposed();

			if ( IsDestroyed ) {
				return;
			}

			AddFlags( EntityFlags.Destroyed );
			SetLifecycleState( EntityLifecycleState.Destroyed );

			OnDestroyed();
		}

		/*
		===============
		HasFlags
		===============
		*/
		public bool HasFlags( EntityFlags flags )
		{
			return (_flags & flags) == flags;
		}

		/*
		===============
		AddFlags
		===============
		*/
		public virtual void AddFlags( EntityFlags flags )
		{
			ThrowIfDisposed();

			EntityFlags oldFlags = _flags;
			_flags |= flags;

			if ( oldFlags != _flags ) {
				AdvanceRevision();
				OnFlagsChanged( oldFlags, _flags );
			}
		}

		/*
		===============
		RemoveFlags
		===============
		*/
		public virtual void RemoveFlags( EntityFlags flags )
		{
			ThrowIfDisposed();

			EntityFlags oldFlags = _flags;
			_flags &= ~flags;

			if ( oldFlags != _flags ) {
				AdvanceRevision();
				OnFlagsChanged( oldFlags, _flags );
			}
		}

		/*
		===============
		SetFlags
		===============
		*/
		public virtual void SetFlags( EntityFlags flags )
		{
			ThrowIfDisposed();

			EntityFlags oldFlags = _flags;
			_flags = flags;

			if ( oldFlags != _flags ) {
				AdvanceRevision();
				OnFlagsChanged( oldFlags, _flags );
			}
		}

		/*
		===============
		SetDisplayName
		===============
		*/
		protected void SetDisplayName( InternString displayName )
		{
			ThrowIfDisposed();

			if ( _displayName.Equals( displayName ) ) {
				return;
			}

			InternString oldName = _displayName;
			_displayName = displayName;

			AdvanceRevision();
			OnDisplayNameChanged( oldName, _displayName );
		}

		/*
		===============
		HasTag
		===============
		*/
		public bool HasTag( InternString tag )
		{
			return _tags.Contains( tag );
		}

		/*
		===============
		AddTag
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool AddTag( InternString tag )
		{
			ThrowIfDisposed();

			if ( tag == InternString.Empty ) {
				return false;
			}

			if ( !_tags.Add( tag ) ) {
				return false;
			}

			AdvanceRevision();
			OnTagAdded( tag );

			return true;
		}

		/*
		===============
		RemoveTag
		===============
		*/
		public virtual bool RemoveTag( InternString tag )
		{
			ThrowIfDisposed();

			if ( !_tags.Remove( tag ) ) {
				return false;
			}

			AdvanceRevision();
			OnTagRemoved( tag );

			return true;
		}

		/*
		===============
		ClearTags
		===============
		*/
		public virtual void ClearTags()
		{
			ThrowIfDisposed();

			if ( _tags.Count == 0 ) {
				return;
			}

			_tags.Clear();

			AdvanceRevision();
			OnTagsCleared();
		}

		/*
		===============
		CanInteract
		===============
		*/
		public virtual bool CanInteract( in EntityInteractionContext context )
		{
			if ( !IsInteractable ) {
				return false;
			}

			if ( context.TargetId.IsValid && context.TargetId != Id ) {
				return false;
			}

			return true;
		}

		/*
		===============
		Interact
		===============
		*/
		public virtual EntityInteractionResult Interact( in EntityInteractionContext context )
		{
			ThrowIfDisposed();

			if ( !CanInteract( in context ) ) {
				return EntityInteractionResult.Fail( "Entity cannot be interacted with." );
			}

			EntityInteractionResult result = OnInteract( in context );

			if ( result.Success ) {
				AdvanceRevision();
				OnInteracted( in context, in result );
			}

			return result;
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_isDisposed = true;

			_tags.Clear();
			_lifecycleState = EntityLifecycleState.Disposed;

			Dispose( true );

			GC.SuppressFinalize( this );
		}

		/*
		===============
		Dispose
		===============
		*/
		protected virtual void Dispose( bool disposing )
		{
		}

		/*
		===============
		SetLifecycleState
		===============
		*/
		protected void SetLifecycleState( EntityLifecycleState newState )
		{
			if ( _lifecycleState == newState ) {
				return;
			}

			EntityLifecycleState oldState = _lifecycleState;
			_lifecycleState = newState;

			AdvanceRevision();
			OnLifecycleStateChanged( oldState, newState );
		}

		/*
		===============
		AdvanceRevision
		===============
		*/
		protected void AdvanceRevision()
		{
			unchecked {
				_revision++;
			}

			if ( _revision == 0 ) {
				_revision = 1;
			}
		}

		protected virtual void OnSpawned()
		{
		}

		protected virtual void OnActivated()
		{
		}

		protected virtual void OnDeactivated()
		{
		}

		protected virtual void OnHidden()
		{
		}

		protected virtual void OnShown()
		{
		}

		protected virtual void OnDespawning()
		{
		}

		protected virtual void OnDespawned()
		{
		}

		protected virtual void OnDestroyed()
		{
		}

		protected virtual void OnLifecycleStateChanged(
			EntityLifecycleState oldState,
			EntityLifecycleState newState
		)
		{
		}

		protected virtual void OnFlagsChanged(
			EntityFlags oldFlags,
			EntityFlags newFlags
		)
		{
		}

		protected virtual void OnDisplayNameChanged(
			InternString oldName,
			InternString newName
		)
		{
		}

		protected virtual void OnTagAdded( InternString tag )
		{
		}

		protected virtual void OnTagRemoved( InternString tag )
		{
		}

		protected virtual void OnTagsCleared()
		{
		}

		/*
		===============
		OnInteract
		===============
		*/
		/// <summary>
		/// Handles the actual interaction behavior.
		///
		/// Base entities succeed by default if they are interactable. Subclasses should
		/// override this for pickup, talk, open, rest, read, loot, consume, equip, etc.
		/// </summary>
		protected virtual EntityInteractionResult OnInteract( in EntityInteractionContext context )
		{
			return EntityInteractionResult.SuccessResult();
		}

		protected virtual void OnInteracted(
			in EntityInteractionContext context,
			in EntityInteractionResult result
		)
		{
		}

		protected void ThrowIfDisposed()
		{
			if ( _isDisposed ) {
				throw new ObjectDisposedException( GetType().Name );
			}
		}
	};
};
