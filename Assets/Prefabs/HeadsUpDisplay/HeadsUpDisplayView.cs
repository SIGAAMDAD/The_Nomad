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

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	HeadsUpDisplayView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class HeadsUpDisplayView : Control
	{
		public event Action<int> InteractionOptionPressed;

		private Label _interactionPromptKey = null;
		private Label _interactionPromptText = null;
		private Control _interactionPrompt = null;
		private Control _interactionMenu = null;
		private VBoxContainer _interactionMenuOptions = null;
		private readonly List<Button> _interactionButtons = new List<Button>( 6 );

		public override void _Ready()
		{
			base._Ready();

			CreateInteractionOverlay();
			HideInteractionPrompt();
			HideInteractionMenu();
		}

		public void ShowInteractionPrompt( string prompt )
		{
			_interactionPromptText.Text = prompt;
			_interactionPrompt.Visible = true;
		}

		public void HideInteractionPrompt()
		{
			if ( _interactionPrompt != null ) {
				_interactionPrompt.Visible = false;
			}
		}

		public void ShowInteractionMenu( IReadOnlyList<InteractionMenuOption> options )
		{
			EnsureButtonCount( options.Count );

			for ( int i = 0; i < _interactionButtons.Count; i++ ) {
				Button button = _interactionButtons[i];
				bool visible = i < options.Count;

				button.Visible = visible;
				button.Disabled = !visible;

				if ( visible ) {
					button.Text = options[i].Prompt.ToString();
				}
			}

			_interactionMenu.Visible = options.Count > 0;
		}

		public void HideInteractionMenu()
		{
			if ( _interactionMenu != null ) {
				_interactionMenu.Visible = false;
			}
		}

		private void CreateInteractionOverlay()
		{
			_interactionPrompt = new HBoxContainer {
				Name = "InteractionPrompt",
				MouseFilter = MouseFilterEnum.Ignore,
				AnchorsPreset = (int)LayoutPreset.CenterBottom
			};
			_interactionPrompt.SetAnchorsPreset( LayoutPreset.CenterBottom );
			_interactionPrompt.OffsetLeft = -80.0f;
			_interactionPrompt.OffsetTop = -96.0f;
			_interactionPrompt.OffsetRight = 80.0f;
			_interactionPrompt.OffsetBottom = -56.0f;
			AddChild( _interactionPrompt );

			_interactionPromptKey = new Label {
				Text = "E",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				CustomMinimumSize = new Vector2( 28.0f, 28.0f )
			};
			_interactionPrompt.AddChild( _interactionPromptKey );

			_interactionPromptText = new Label {
				Text = "Interact",
				VerticalAlignment = VerticalAlignment.Center
			};
			_interactionPrompt.AddChild( _interactionPromptText );

			_interactionMenu = new PanelContainer {
				Name = "InteractionMenu",
				MouseFilter = MouseFilterEnum.Stop
			};
			_interactionMenu.SetAnchorsPreset( LayoutPreset.Center );
			_interactionMenu.OffsetLeft = -96.0f;
			_interactionMenu.OffsetTop = -80.0f;
			_interactionMenu.OffsetRight = 96.0f;
			_interactionMenu.OffsetBottom = 80.0f;
			AddChild( _interactionMenu );

			_interactionMenuOptions = new VBoxContainer {
				Name = "Options"
			};
			_interactionMenu.AddChild( _interactionMenuOptions );
		}

		private void EnsureButtonCount( int count )
		{
			while ( _interactionButtons.Count < count ) {
				int optionIndex = _interactionButtons.Count;
				Button button = new Button {
					CustomMinimumSize = new Vector2( 160.0f, 32.0f ),
					FocusMode = FocusModeEnum.All
				};

				button.Pressed += () => InteractionOptionPressed?.Invoke( optionIndex );

				_interactionButtons.Add( button );
				_interactionMenuOptions.AddChild( button );
			}
		}
	};
};
