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
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Save.Services;
using Nomad.Save.ValueObjects;
using System;

namespace Nomad.Game.Presentation.Screens.LoadGameMenu
{
	/*
	===================================================================================

	LoadGameMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class LoadGameMenuView : Control
	{
		public event Action<int> SlotSelected;
		public event Action Back;
		public event Action LoadSlot;
		public event Action DeleteSlot;

		private Label _nameLabel;
		private Label _difficultyLabel;
		private Label _lastAccessedTime;

		private VBoxContainer _slotList;

		private LoadGameMenuPresenter _presenter;

		/*
		===============
		AddSlot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="slotName"></param>
		/// <param name="slotIndex"></param>
		public void AddSlot( string slotName, int slotIndex )
		{
			NomadButton button = new NomadButton() {
				Text = slotName
			};
			button.Pressed += () => SlotSelected?.Invoke( slotIndex );
			_slotList.AddChild( button );
		}

		public void SetSlotName( string name )
		{
			_nameLabel.Text = name;
		}

		public void SetSlotDifficulty( string difficulty )
		{
			_difficultyLabel.Text = difficulty;
		}

		public void SetLastAccessedTime( string time )
		{
			_lastAccessedTime.Text = time;
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

			_slotList = GetNode<VBoxContainer>( "MarginContainer/HBoxContainer/ScrollContainer/SlotList" );
			GetNode<Button>( "ButtonContainer/BackButton" ).Pressed += () => Back?.Invoke();
			GetNode<Button>( "ButtonContainer/DeleteSlotButton" ).Pressed += () => DeleteSlot?.Invoke();
			GetNode<Button>( "ButtonContainer/LoadSlotButton" ).Pressed += () => LoadSlot?.Invoke();

			_nameLabel = GetNode<Label>( "MarginContainer/HBoxContainer/InfoContainer/NameLabel" );
			_difficultyLabel = GetNode<Label>( "MarginContainer/HBoxContainer/InfoContainer/DifficultyLabel" );
			_lastAccessedTime = GetNode<Label>( "MarginContainer/HBoxContainer/InfoContainer/LastAccessedDate" );

			_presenter = ScreenPresenterFactory.CreateLoadGameMenuPresenter( this );
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree();

			_presenter.Dispose();
		}
	};
};
