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
using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Interactables;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Prefabs;
using Nomad.Save.Extensions;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	CheckpointService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CheckpointService : ICheckpointService
	{
		public IReadOnlyList<ICheckpointEntity> Checkpoints => GetAllCheckpoints();

		private static readonly CheckpointDefinition TemporaryCheckpointDefinition = new CheckpointDefinition {
			Id = new CheckpointDefinitionId( new InternString( "temporary_checkpoint" ) ),
			DisplayName = new InternString( "Temporary Checkpoint" ),
			IsTemporary = true
		};

		/// <summary>
		/// Represents all the meliora in the game.
		/// </summary>
		private readonly Dictionary<CheckpointInstanceId, CheckpointInstance> _permanent = new();

		/// <summary>
		/// Represents the temporary firelink checkpoint.
		/// </summary>
		private readonly Dictionary<PlayerId, CheckpointInstance> _temporary = new();
		private readonly Dictionary<PlayerId, CheckpointInstanceId> _current = new();

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		private readonly IGameEventRegistryService _eventFactory;

		private bool _isDisposed = false;

		/*
		===============
		CheckpointService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		public CheckpointService( IGameEventRegistryService eventFactory )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_loadBegin = eventFactory
				.GetEvent<LoadBeginEventArgs>(
					LoadBeginEventArgs.Name,
					LoadBeginEventArgs.NameSpace
				)
				.Subscribe( OnLoadBegin );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_permanent.Clear();
			_temporary.Clear();
			_loadBegin.Dispose();
			_saveBegin.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
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
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		public bool TryActivateCheckpoint( PlayerId playerId, CheckpointInstanceId checkpoint )
		{
			playerId.ThrowIfInvalid( nameof( TryActivateCheckpoint ) );

			if ( !_permanent.TryGetValue( checkpoint, out CheckpointInstance instance ) ) {
				return false;
			}

			return instance.TryActivateCheckpoint( playerId );
		}

		/*
		===============
		IsPermanentCheckpoint
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		public bool IsPermanentCheckpoint( CheckpointInstanceId checkpoint )
		{
			return checkpoint.IsValid && _permanent.ContainsKey( checkpoint );
		}

		/*
		===============
		TryRegisterPermanent
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		public bool TryRegisterPermanent( CheckpointDefinition definition, CheckpointInstanceId checkpoint )
		{
			return TryRegisterPermanent( definition, checkpoint, null, out _ );
		}

		/*
		===============
		TryRegisterPermanent
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definition"></param>
		/// <param name="checkpoint"></param>
		/// <param name="prefab"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		internal bool TryRegisterPermanent( CheckpointDefinition definition, CheckpointInstanceId checkpoint, CheckpointPrefab? prefab, out CheckpointInstance? instance )
		{
			instance = null;

			if ( definition == null || !checkpoint.IsValid || definition.IsTemporary ) {
				return false;
			}

			if ( _permanent.TryGetValue( checkpoint, out instance ) ) {
				return true;
			}

			instance = new CheckpointInstance( definition, checkpoint, prefab, _eventFactory );
			_permanent[checkpoint] = instance;

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

			if ( !_current.TryGetValue( playerId, out CheckpointInstanceId checkpoint ) ) {
				return false;
			}

			if ( !TryGetCheckpoint( playerId, checkpoint, out CheckpointInstance instance ) ) {
				_current.Remove( playerId );
				return false;
			}

			if ( !instance.TryLeave( playerId ) ) {
				return false;
			}

			_current.Remove( playerId );

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
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		public bool TryRest( PlayerId playerId, CheckpointInstanceId checkpoint )
		{
			playerId.ThrowIfInvalid( nameof( TryRest ) );

			if ( _current.ContainsKey( playerId ) ) {
				return false;
			}

			if ( !TryGetCheckpoint( playerId, checkpoint, out CheckpointInstance instance ) ) {
				return false;
			}

			if ( !instance.TryRest( playerId ) ) {
				return false;
			}

			_current[playerId] = checkpoint;

			return true;
		}

		/*
		===============
		GetActivatedCheckpoints
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public List<ICheckpointEntity> GetActivatedCheckpoints()
		{
			List<ICheckpointEntity> checkpoints = new List<ICheckpointEntity>();

			foreach ( CheckpointInstance checkpoint in _permanent.Values ) {
				if ( checkpoint.Status != CheckpointStatus.Inactive ) {
					checkpoints.Add( checkpoint );
				}
			}

			foreach ( CheckpointInstance checkpoint in _temporary.Values ) {
				if ( checkpoint.Status != CheckpointStatus.Inactive ) {
					checkpoints.Add( checkpoint );
				}
			}

			return checkpoints;
		}

		/*
		===============
		GetTemporaryCheckpoints
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public List<ICheckpointEntity> GetTemporaryCheckpoints()
		{
			List<ICheckpointEntity> checkpoints = new List<ICheckpointEntity>( _temporary.Count );

			foreach ( CheckpointInstance checkpoint in _temporary.Values ) {
				checkpoints.Add( checkpoint );
			}

			return checkpoints;
		}

		/*
		===============
		TryCreateTemporary
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		public bool TryCreateTemporary( PlayerId playerId, out CheckpointInstanceId checkpoint )
		{
			playerId.ThrowIfInvalid( nameof( TryCreateTemporary ) );

			if ( _temporary.TryGetValue( playerId, out CheckpointInstance existing ) ) {
				checkpoint = existing.CheckpointId;
				return true;
			}

			checkpoint = new CheckpointInstanceId( Guid.NewGuid() );
			_temporary[playerId] = new CheckpointInstance( TemporaryCheckpointDefinition, checkpoint, _eventFactory );

			return true;
		}

		/*
		===============
		GetAllCheckpoints
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private List<ICheckpointEntity> GetAllCheckpoints()
		{
			List<ICheckpointEntity> checkpoints = new List<ICheckpointEntity>( _permanent.Count + _temporary.Count );

			foreach ( CheckpointInstance checkpoint in _permanent.Values ) {
				checkpoints.Add( checkpoint );
			}

			foreach ( CheckpointInstance checkpoint in _temporary.Values ) {
				checkpoints.Add( checkpoint );
			}

			return checkpoints;
		}

		/*
		===============
		TryGetCheckpoint
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="checkpoint"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		private bool TryGetCheckpoint( PlayerId playerId, CheckpointInstanceId checkpoint, out CheckpointInstance instance )
		{
			if ( !checkpoint.IsValid ) {
				instance = null;
				return false;
			}

			if ( _permanent.TryGetValue( checkpoint, out instance ) ) {
				return true;
			}

			if ( _temporary.TryGetValue( playerId, out instance ) && instance.CheckpointId.Equals( checkpoint ) ) {
				return true;
			}

			instance = null;
			return false;
		}

		/*
		===============
		OnSaveBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( this ) {
				using var writer = args.Writer.AddSection( nameof( CheckpointService ) );

				foreach ( var checkpoint in _permanent ) {
					writer.WriteBool( $"{checkpoint.Value.DefinitionId}:Activated", checkpoint.Value.Status != CheckpointStatus.Inactive );
				}
			}
		}

		/*
		===============
		OnLoadBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="Exception"></exception>
		private void OnLoadBegin( in LoadBeginEventArgs args )
		{
			lock ( this ) {
				using var reader = args.Reader.FindSection( nameof( CheckpointService ) );

				if ( reader == null ) {
					throw new Exception();
				}

				foreach ( var checkpoint in _permanent ) {
					bool activated = reader.ReadBool( $"{checkpoint.Value.DefinitionId}:Activated" );
				}
			}
		}
	};
};
