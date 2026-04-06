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

using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;
using Nomad.Game.Prefabs;
using Nomad.Input;
using Nomad.Input.Events;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Interactables {
	/*
	===================================================================================
	
	Interactable
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal partial class Interactable : Area2D, IInteractable {
		private static readonly InternString INTERACT_ACTION_NAME = new( "Interact" );

		public PlayerInteractionStatus PlayerStatus {
			get => _playerStatus;
			set {
				PlayerInteractionStatus oldValue = _playerStatus;
				_playerStatus = value;
				_statusChanged.Publish( new PlayerInteractionStatusChangedEventArgs( oldValue, value ) );
			}
		}
		private PlayerInteractionStatus _playerStatus = PlayerInteractionStatus.None;

		public IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged => _statusChanged;
		private IGameEvent<PlayerInteractionStatusChangedEventArgs> _statusChanged;

		private ISubscriptionHandle _playerInteractionAction;

		/*
		===============
		OnInteractionAction
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnInteractionAction( in ButtonActionEventArgs args ) {
			if ( _playerStatus != PlayerInteractionStatus.InRange || args.ActionId != INTERACT_ACTION_NAME ) {
				return;
			}
			if ( args.Phase == InputActionPhase.Performed ) {
				PlayerStatus = PlayerInteractionStatus.Interacting;
			}
		}

		/*
		===============
		OnBodyShapeExited
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyShapeExited( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex ) {
			if ( body is PlayerPrefab && ( _playerStatus == PlayerInteractionStatus.InRange || _playerStatus == PlayerInteractionStatus.Interacting ) ) {
				PlayerStatus = PlayerInteractionStatus.None;
			}
		}

		/*
		===============
		OnBodyShapeEntered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex ) {
			if ( body is PlayerPrefab ) {
				PlayerStatus = PlayerInteractionStatus.InRange;
			}
		}

		public override void _Ready() {
			base._Ready();

			_statusChanged = GameEventRegistry.GetEvent<PlayerInteractionStatusChangedEventArgs>( EventNames.PLAYER_INTERACTION_STATUS_CHANGED, EventNames.NAMESPACE );
			_playerInteractionAction = GameEventRegistry.GetEvent<ButtonActionEventArgs>( $"Interact:{Constants.Events.BUTTON_ACTION}", Constants.Events.NAMESPACE )
				.Subscribe( OnInteractionAction );

			BodyShapeEntered += OnBodyShapeEntered;
			BodyShapeExited += OnBodyShapeExited;
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _ExitTree() {
			base._ExitTree();
			
			_playerInteractionAction?.Dispose();
		}
	};
};