using System;
using System.Security.Cryptography;
using System.Text;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;

namespace Nomad.Game.Prefabs
{
	internal partial class CheckpointPrefab : InteractableRoot
	{
		[Export(PropertyHint.LocaleId, hintString: "The checkpoint's id")]
		private StringName _id;

		[Export(PropertyHint.LocaleId, hintString: "The checkpoint's display name")]
		private StringName _displayName;

		private CheckpointInstanceId _checkpointId = CheckpointInstanceId.Invalid;
		private IGameEvent<CheckpointActivationRequestedEventArgs>? _activationRequested;
		private IGameEvent<CheckpointRestRequestedEventArgs>? _restRequested;
		private IGameEvent<CheckpointLeaveRequestedEventArgs>? _leaveRequested;

		public override void _Ready()
		{
			base._Ready();
			_checkpointId = CreateCheckpointId();
			EnsureEvents();
			TryRegisterPermanent();
		}

		public bool RequestActivate( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestActivate ) );

			if ( !TryRegisterPermanent() ) {
				return false;
			}

			EnsureEvents();
			_activationRequested!.Publish( new CheckpointActivationRequestedEventArgs( playerId, _checkpointId ) );
			return true;
		}

		public bool RequestRest( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestRest ) );

			if ( !TryRegisterPermanent() ) {
				return false;
			}

			EnsureEvents();
			_restRequested!.Publish( new CheckpointRestRequestedEventArgs( playerId, _checkpointId ) );
			return true;
		}

		public bool RequestLeave( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestLeave ) );

			EnsureEvents();
			_leaveRequested!.Publish( new CheckpointLeaveRequestedEventArgs( playerId ) );
			return true;
		}

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

		private bool TryRegisterPermanent()
		{
			if ( _checkpointId.IsValid && ServiceLocator.Instance.TryGetService<ICheckpointService>( out ICheckpointService? service ) ) {
				return service.TryRegisterPermanent( CreateDefinition(), _checkpointId );
			}

			return false;
		}

		private void EnsureEvents()
		{
			if ( _activationRequested != null && _restRequested != null && _leaveRequested != null ) {
				return;
			}

			var eventFactory = ServiceLocator.GetService<IGameEventRegistryService>();

			_activationRequested = eventFactory.GetEvent<CheckpointActivationRequestedEventArgs>(
				CheckpointActivationRequestedEventArgs.Name,
				CheckpointActivationRequestedEventArgs.NameSpace
			);

			_restRequested = eventFactory.GetEvent<CheckpointRestRequestedEventArgs>(
				CheckpointRestRequestedEventArgs.Name,
				CheckpointRestRequestedEventArgs.NameSpace
			);

			_leaveRequested = eventFactory.GetEvent<CheckpointLeaveRequestedEventArgs>(
				CheckpointLeaveRequestedEventArgs.Name,
				CheckpointLeaveRequestedEventArgs.NameSpace
			);
		}

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

		private CheckpointInstanceId CreateCheckpointId()
		{
			string id = GetExportedId();

			if ( Guid.TryParse( id, out Guid guid ) ) {
				return new CheckpointInstanceId( guid );
			}

			byte[] bytes = MD5.HashData( Encoding.UTF8.GetBytes( id ) );
			return new CheckpointInstanceId( new Guid( bytes ) );
		}

		private string GetExportedId()
		{
			string exportedId = _id.ToString();
			return string.IsNullOrWhiteSpace( exportedId ) ? Name.ToString() : exportedId;
		}
	};
};
