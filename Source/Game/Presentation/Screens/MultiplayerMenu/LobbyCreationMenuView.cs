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
using System;

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu {
	/*
	===================================================================================
	
	LobbyCreationMenuView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class LobbyCreationMenuView : Control {
		public event Action Back;
		public event Action CreateLobby;
		public event Action Reset;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();
		}
	};
};