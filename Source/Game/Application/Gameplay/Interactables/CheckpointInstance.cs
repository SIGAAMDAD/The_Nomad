/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Interactables;
using Nomad.Game.Sdk.Events.Interactables;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	CheckpointInstance

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CheckpointInstance : InteractableAggregate, ICheckpointEntity
	{
		public CheckpointStatus Status => _status;
		private CheckpointStatus _status = CheckpointStatus.Inactive;

		public CheckpointInstanceId CheckpointId => _instanceId;

		public PlayerId CurrentPlayerId => _currentPlayerId;
		private PlayerId _currentPlayerId = PlayerId.Invalid;

		public bool IsTemporary => _definition.IsTemporary;
		public bool CanRest => _status == CheckpointStatus.Activated || ( IsTemporary && _status == CheckpointStatus.Inactive );

		public uint CheckpointRevision => _revision;
		private uint _revision = 0;

		private readonly CheckpointDefinition _definition;
		private readonly CheckpointInstanceId _instanceId;

		private readonly IGameEvent<CheckpointActivationRequestedEventArgs> _activationRequested = null;
		private readonly IGameEvent<CheckpointRestRequestedEventArgs> _restRequested = null;
		private readonly IGameEvent<CheckpointLeaveRequestedEventArgs> _leaveRequested = null;

		/*
		===============
		CheckpointInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <param name="instanceId"></param>
		/// <param name="prefab"></param>
		/// <param name="eventFactory"></param>
		public CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, CheckpointPrefab prefab, IGameEventRegistryService eventFactory )
			: this( definition, instanceId, prefab, EntityFlags.Interactable | EntityFlags.Persistent, eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_activationRequested = eventFactory
				.GetEvent<CheckpointActivationRequestedEventArgs>(
					CheckpointActivationRequestedEventArgs.Name,
					CheckpointActivationRequestedEventArgs.NameSpace
				);

			_restRequested = eventFactory
				.GetEvent<CheckpointRestRequestedEventArgs>(
					CheckpointRestRequestedEventArgs.Name,
					CheckpointRestRequestedEventArgs.NameSpace
				);

			_leaveRequested = eventFactory
				.GetEvent<CheckpointLeaveRequestedEventArgs>(
					CheckpointLeaveRequestedEventArgs.Name,
					CheckpointLeaveRequestedEventArgs.NameSpace
				);
		}

		/*
		===============
		CheckpointInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <param name="instanceId"></param>
		/// <param name="eventFactory"></param>
		public CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, IGameEventRegistryService eventFactory )
			: this( definition, instanceId, null, EntityFlags.Interactable | EntityFlags.Transient, eventFactory )
		{
		}

		/*
		===============
		CheckpointInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <param name="instanceId"></param>
		/// <param name="prefab"></param>
		/// <param name="flags"></param>
		/// <param name="eventFactory"></param>
		private CheckpointInstance( CheckpointDefinition definition, CheckpointInstanceId instanceId, CheckpointPrefab? prefab, EntityFlags flags, IGameEventRegistryService eventFactory )
			: base(
				prefab,
				new EntityId( instanceId.Value ),
				RequireDefinition( definition ).Id.Value,
				RequireDefinition( definition ).DisplayName,
				flags,
				eventFactory
			)
		{
			_instanceId = instanceId;
			_definition = definition;
		}

		/*
		===============
		RequireDefinition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		private static CheckpointDefinition RequireDefinition( CheckpointDefinition definition )
		{
			return definition ?? throw new ArgumentNullException( nameof( definition ) );
		}

		/*
		===============
		RequestActivate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public bool RequestActivate( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestActivate ) );

			_activationRequested.Publish(
				new CheckpointActivationRequestedEventArgs(
					playerId,
					_instanceId
				)
			);

			return true;
		}

		/*
		===============
		RequestRest
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public bool RequestRest( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestRest ) );

			_restRequested.Publish(
				new CheckpointRestRequestedEventArgs(
					playerId,
					_instanceId
				)
			);

			return true;
		}

		/*
		===============
		RequestLeave
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public bool RequestLeave( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestLeave ) );

			_leaveRequested.Publish(
				new CheckpointLeaveRequestedEventArgs(
					playerId
				)
			);

			return true;
		}

		/*
		===============
		RequestInteraction
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="kind"></param>
		/// <returns></returns>
		public EntityInteractionResult RequestInteraction( PlayerId playerId, EntityInteractionKind kind )
		{
			bool requested = kind switch {
				EntityInteractionKind.Activate => RequestActivate( playerId ),
				EntityInteractionKind.Rest => RequestRest( playerId ),
				EntityInteractionKind.Close => RequestLeave( playerId ),
				_ => false
			};

			return requested ? EntityInteractionResult.SuccessResult() : EntityInteractionResult.InvalidTarget();
		}

		/*
		===============
		SetStatus
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="status"></param>
		private void SetStatus( CheckpointStatus status )
		{
			if ( _status == status ) {
				return;
			}

			_status = status;
			_revision++;
		}

		/*
		===============
		TryActivateCheckpoint
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public bool TryActivateCheckpoint( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( TryActivateCheckpoint ) );

			if ( IsTemporary ) {
				return false;
			}

			if ( _status != CheckpointStatus.Inactive ) {
				return true;
			}

			SetStatus( CheckpointStatus.Activated );

			return true;
		}

		/*
		===============
		TryRest
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public bool TryRest( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( TryRest ) );

			if ( _currentPlayerId.IsValid && _currentPlayerId != playerId ) {
				return false;
			}

			if ( !CanRest ) {
				return false;
			}

			if ( _status == CheckpointStatus.Inactive ) {
				SetStatus( CheckpointStatus.Activated );
			}

			_currentPlayerId = playerId;
			SetStatus( CheckpointStatus.Current );

			return true;
		}

		/*
		===============
		TryLeave
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public bool TryLeave( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( TryLeave ) );

			if ( _status != CheckpointStatus.Current ) {
				return false;
			}

			if ( _currentPlayerId.IsValid && _currentPlayerId != playerId ) {
				return false;
			}

			_currentPlayerId = PlayerId.Invalid;
			SetStatus( CheckpointStatus.Activated );

			return true;
		}

		/*
		===============
		Interact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override EntityInteractionResult Interact( in EntityInteractionContext context )
		{
			return RequestInteraction( new PlayerId( context.ActorId ), context.Kind );
		}
	};
};
