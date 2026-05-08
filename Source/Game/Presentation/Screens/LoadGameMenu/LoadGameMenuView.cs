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
using Nomad.UI;
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

	public partial class LoadGameMenuView : Control
	{
		public event Action<int> SlotSelected;
		public event Action Back;
		public event Action LoadSlot;

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
		/// <param name="metadata"></param>
		public void AddSlot( SaveFileMetadata metadata )
		{
			EngineButton button = new EngineButton() {
				Text = $"{metadata.SaveName} {metadata.LastAccessDay}:{metadata.LastAccessMonth}:{metadata.LastAccessYear}"
			};
			_slotList.AddChild( button );
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

			_slotList = GetNode<VBoxContainer>( "MarginContainer/ScrollContainer/SlotList" );

			var dataProvider = ServiceLocator.GetService<ISaveDataProvider>();
			_presenter = new LoadGameMenuPresenter( this, new LoadGameMenuModel( dataProvider ), dataProvider );
		}
	};
};
