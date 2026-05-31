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
using Nomad.Core.Util;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Sdk.Interactables;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	InteractionMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class InteractionMenuView : MarginContainer, IInteractionMenuView
	{
		[Export]
		private PackedScene _buttonPrefab;

		public event Action<int> OptionPressed;

		private readonly HudComponentView _impl;

		private VBoxContainer _optionsContainer = null;
		private TextureRect _optionsMenu = null;
		private readonly List<Button> _buttons = new List<Button>( 6 );

		private Label _prompt;

		public InteractionMenuView()
		{
			_impl = new HudComponentView( this );
		}

		public void SetColor( System.Numerics.Vector4 color )
		{
			_impl.SetColor( color );
		}

		public void ShowMenu( IReadOnlyList<InteractionMenuOption> options )
		{
			EnsureButtonCount( options.Count );

			for ( int i = 0; i < _buttons.Count; i++ ) {
				Button button = _buttons[i];
				bool visible = i < options.Count;

				button.Visible = visible;
				button.Disabled = !visible;

				if ( visible ) {
					button.Text = options[i].Prompt.ToString();
				}
			}

			_optionsMenu.Visible = options.Count > 0;
		}

		public void HideMenu()
		{
			_optionsMenu.Hide();
		}

		public void ShowPrompt( string prompt )
		{
			_prompt.Text = $"E {prompt}";
			_prompt.Show();
		}

		public void HidePrompt()
		{
			_prompt.Hide();
		}

		private void EnsureButtonCount( int count )
		{
			while ( _buttons.Count < count ) {
				int optionIndex = _buttons.Count;
				Button button = _buttonPrefab.Instantiate<Button>();
				button.FocusMode = FocusModeEnum.All;
				button.MouseFilter = MouseFilterEnum.Stop;

				button.Pressed += () => OptionPressed?.Invoke( optionIndex );

				_buttons.Add( button );
				_optionsContainer.AddChild( button );
			}
		}

		public override void _Ready()
		{
			base._Ready();

			_optionsMenu = GetNode<TextureRect>( "Background" );
			_optionsContainer = _optionsMenu.GetNode<VBoxContainer>( "OptionsContainer" );
			_prompt = GetNode<Label>( "InteractionPrompt" );
			HideMenu();
		}
	};
};
