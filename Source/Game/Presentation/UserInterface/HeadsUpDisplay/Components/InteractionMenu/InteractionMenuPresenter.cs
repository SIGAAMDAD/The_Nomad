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
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Interactables;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Prefabs;
using Nomad.Input;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.InteractionMenu
{
	/*
	===================================================================================

	InteractionMenuPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class InteractionMenuPresenter : HudComponentPresenter<IInteractionMenuView>
	{
		private readonly PlayerId _playerId;

		private readonly List<InteractionMenuOption> _options = new List<InteractionMenuOption>( 6 );

		private readonly IDisposable _interactAction;

		private readonly ILocalizationService _localizationService;

		private InteractableRoot? _focused;

		/*
		===============
		InteractionMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="view"></param>
		/// <param name="eventFactory"></param>
		/// <param name="localizationService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public InteractionMenuPresenter(
			PlayerId playerId,
			IInteractionMenuView view,
			IGameEventRegistryService eventFactory,
			ILocalizationService localizationService
		)
			: base( view )
		{
			_playerId = playerId;
			_localizationService = localizationService ?? throw new ArgumentNullException( nameof( localizationService ) );

			InteractableRoot.InteractionFocusEntered += OnInteractionFocusEntered;
			InteractableRoot.InteractionFocusExited += OnInteractionFocusExited;
			view.OptionPressed += OnInteractionOptionPressed;

			_interactAction = (eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) ))
				.GetEvent<ButtonActionEventArgs>(
					$"Interact:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnInteractActionTriggered );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			InteractableRoot.InteractionFocusEntered -= OnInteractionFocusEntered;
			InteractableRoot.InteractionFocusExited -= OnInteractionFocusExited;
			view.OptionPressed -= OnInteractionOptionPressed;
			_interactAction.Dispose();
		}

		/*
		===============
		Render
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public override void Render( float delta )
		{
		}

		/*
		===============
		OnInteractionFocusEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="interactable"></param>
		/// <param name="playerId"></param>
		private void OnInteractionFocusEntered( InteractableRoot interactable, PlayerId playerId )
		{
			if ( playerId != _playerId ) {
				return;
			}

			_focused = interactable;

			view.HideMenu();
			view.ShowPrompt(
				_localizationService.Translate( interactable.InteractionPrompt )
			);
		}

		/*
		===============
		OnInteractionFocusExited
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="interactable"></param>
		/// <param name="playerId"></param>
		private void OnInteractionFocusExited( InteractableRoot interactable, PlayerId playerId )
		{
			if ( playerId != _playerId || _focused != interactable ) {
				return;
			}

			_focused = null;
			_options.Clear();

			view.HidePrompt();
			view.HideMenu();
		}

		/*
		===============
		OnInteractActionTriggered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnInteractActionTriggered( in ButtonActionEventArgs args )
		{
			if ( args.Phase != InputActionPhase.Canceled ) {
				return;
			}

			OnInteractPressed();
			view.HidePrompt();
		}

		/*
		===============
		OnInteractPressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnInteractPressed()
		{
			if ( _focused == null ) {
				return;
			}

			var result = _focused.RequestInteraction( _playerId, _focused.PrimaryInteractionKind );
			if ( result.Equals( EntityInteractionResult.Failed ) ) {
				return;
			}

			BuildAndShowMenu();
		}

		/*
		===============
		BuildAndShowMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void BuildAndShowMenu()
		{
			if ( _focused == null ) {
				return;
			}

			_options.Clear();
			_focused.BuildInteractionOptions( _options );
			view.ShowMenu( _options );

			if ( _options.Count == 0 ) {
				view.HideMenu();
			}
		}

		/*
		===============
		OnInteractionOptionPressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="optionIndex"></param>
		private void OnInteractionOptionPressed( int optionIndex )
		{
			if ( _focused == null || optionIndex < 0 || optionIndex >= _options.Count ) {
				return;
			}

			InteractionMenuOption option = _options[optionIndex];
			_focused.RequestInteraction( _playerId, option.Kind );

			if ( option.Kind == EntityInteractionKind.Close ) {
				view.HideMenu();
			}
		}
	};
};
