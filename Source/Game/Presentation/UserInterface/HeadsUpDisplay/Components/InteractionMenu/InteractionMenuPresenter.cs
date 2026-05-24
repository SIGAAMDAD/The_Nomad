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
using Godot;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.InteractionMenu
{
	internal sealed class InteractionMenuPresenter : IDisposable
	{
		private readonly PlayerId _playerId;
		private readonly HeadsUpDisplayView _view;
		private readonly List<InteractionMenuOption> _options = new List<InteractionMenuOption>( 6 );

		private InteractableRoot? _focused;
		private bool _menuVisible = false;
		private bool _wasInteractPressed = false;
		private bool _isDisposed = false;

		public InteractionMenuPresenter( PlayerId playerId, HeadsUpDisplayView view )
		{
			_playerId = playerId;
			_view = view ?? throw new ArgumentNullException( nameof( view ) );

			InteractableRoot.InteractionFocusEntered += OnInteractionFocusEntered;
			InteractableRoot.InteractionFocusExited += OnInteractionFocusExited;
			_view.InteractionOptionPressed += OnInteractionOptionPressed;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			InteractableRoot.InteractionFocusEntered -= OnInteractionFocusEntered;
			InteractableRoot.InteractionFocusExited -= OnInteractionFocusExited;
			_view.InteractionOptionPressed -= OnInteractionOptionPressed;

			_isDisposed = true;
		}

		public void Render()
		{
			bool interactPressed = global::Godot.Input.IsKeyPressed( Key.E );

			if ( interactPressed && !_wasInteractPressed ) {
				OnInteractPressed();
			}

			_wasInteractPressed = interactPressed;
		}

		private void OnInteractionFocusEntered( InteractableRoot interactable, PlayerId playerId )
		{
			if ( playerId != _playerId ) {
				GD.Print( $"PlayerId({playerId.Id}) != PlayerId({_playerId.Id})" );
				return;
			}

			_focused = interactable;
			_menuVisible = false;
			GD.Print( "Showing interaction prompt..." );

			_view.HideInteractionMenu();
			_view.ShowInteractionPrompt( interactable.InteractionPrompt.ToString() );
		}

		private void OnInteractionFocusExited( InteractableRoot interactable, PlayerId playerId )
		{
			if ( playerId != _playerId || _focused != interactable ) {
				return;
			}

			_focused = null;
			_options.Clear();
			_menuVisible = false;

			_view.HideInteractionPrompt();
			_view.HideInteractionMenu();
		}

		private void OnInteractPressed()
		{
			if ( _focused == null ) {
				return;
			}

			if ( !_menuVisible ) {
				_focused.RequestInteraction( _playerId, _focused.PrimaryInteractionKind );
				BuildAndShowMenu();
				return;
			}

			_view.HideInteractionMenu();
			_menuVisible = false;
		}

		private void BuildAndShowMenu()
		{
			if ( _focused == null ) {
				return;
			}

			_options.Clear();
			_focused.BuildInteractionOptions( _options );
			_view.ShowInteractionMenu( _options );
			_menuVisible = _options.Count > 0;
		}

		private void OnInteractionOptionPressed( int optionIndex )
		{
			if ( _focused == null || optionIndex < 0 || optionIndex >= _options.Count ) {
				return;
			}

			InteractionMenuOption option = _options[optionIndex];
			_focused.RequestInteraction( _playerId, option.Kind );

			if ( option.Kind == EntityInteractionKind.Close ) {
				_view.HideInteractionMenu();
				_menuVisible = false;
			}
		}
	};
};
