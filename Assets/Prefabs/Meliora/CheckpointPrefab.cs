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
using System.Security.Cryptography;
using System.Text;
using Godot;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.Game.Application.Gameplay.Interactables;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Interfaces.Interactables;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	CheckpointPrefab

	===================================================================================
	*/
	/// <summary>
	/// Represents a checkpoint scene, this is the default permanent checkpoint prefab, or as
	/// called in the game, a "Meliora".
	/// </summary>

	internal partial class CheckpointPrefab : InteractableRoot
	{
		[Export(PropertyHint.LocaleId, hintString: "The checkpoint's id")]
		private StringName _id;

		[Export(PropertyHint.LocaleId, hintString: "The checkpoint's display name")]
		private StringName _displayName;

		private CheckpointInstanceId _checkpointId = CheckpointInstanceId.Invalid;
		private CheckpointInstance? _instance = null;

		public override InternString InteractionPrompt => new InternString( "Interact" );
		public override EntityInteractionKind PrimaryInteractionKind => EntityInteractionKind.Activate;

		public override void BuildInteractionOptions( List<InteractionMenuOption> options )
		{
			options.Add( new InteractionMenuOption( new InternString( "Rest for a While" ), EntityInteractionKind.Rest ) );
			options.Add( new InteractionMenuOption( new InternString( "Take in the View" ), EntityInteractionKind.Activate ) );
			options.Add( new InteractionMenuOption( new InternString( "Open Storage" ), EntityInteractionKind.Open ) );
			options.Add( new InteractionMenuOption( new InternString( "Get Up" ), EntityInteractionKind.Close ) );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready()
		{
			base._Ready();

			_checkpointId = CreateCheckpointId();
			TryEnsureInstance();
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

			if ( !TryEnsureInstance() ) {
				return false;
			}

			return _instance!.RequestActivate( playerId );
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

			if ( !TryEnsureInstance() ) {
				return false;
			}

			return _instance!.RequestRest( playerId );
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

			return TryEnsureInstance() && _instance!.RequestLeave( playerId );
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
		public override EntityInteractionResult RequestInteraction( PlayerId playerId, EntityInteractionKind kind )
		{
			return TryEnsureInstance()
				? _instance!.RequestInteraction( playerId, kind )
				: EntityInteractionResult.InvalidTarget();
		}

		/*
		===============
		TryEnsureInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private bool TryEnsureInstance()
		{
			if ( _instance != null ) {
				return true;
			}

			if ( !_checkpointId.IsValid ) {
				_checkpointId = CreateCheckpointId();
			}

			if ( !_checkpointId.IsValid || !ServiceLocator.Instance.TryGetService( out ICheckpointService? service ) ) {
				return false;
			}

			return service is CheckpointService checkpointService
				&& checkpointService.TryRegisterPermanent( CreateDefinition(), _checkpointId, this, out _instance );
		}

		/*
		===============
		CreateDefinition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private CheckpointDefinition CreateDefinition()
		{
			string id = GetExportedId();
			string displayName = string.IsNullOrWhiteSpace( _displayName.ToString() ) ? id : _displayName.ToString();

			return new CheckpointDefinition {
				Id = new CheckpointDefinitionId( new InternString( id ) ),
				DisplayName = new InternString( displayName ),
				IsTemporary = false
			};
		}

		/*
		===============
		CreateCheckpointId
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private CheckpointInstanceId CreateCheckpointId()
		{
			string id = GetExportedId();

			if ( Guid.TryParse( id, out Guid guid ) ) {
				return new CheckpointInstanceId( guid );
			}

			byte[] bytes = MD5.HashData( Encoding.UTF8.GetBytes( id ) );
			return new CheckpointInstanceId( new Guid( bytes ) );
		}

		/*
		===============
		GetExportedId
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private string GetExportedId()
		{
			string exportedId = _id.ToString();
			return string.IsNullOrWhiteSpace( exportedId ) ? Name.ToString() : exportedId;
		}
	};
};
