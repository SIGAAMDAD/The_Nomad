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
using Nomad.Events.Extensions;
using Nomad.Events.Globals;
using Nomad.UI;
using System;

namespace Nomad.Game.Presentation.Screens.LoadingScreen {
	/*
	===================================================================================
	
	LoadingScreen
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class LoadingScreen : EnginePanel {
		[Export]
		private string[] _tipList;

		private int _currentTip = 0;

		private EngineText _tipLabel;

		private IGameEvent<EmptyEventArgs> _tipSwitch;
		private ISubscriptionHandle _tipSubscription;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_tipSwitch = GameEventRegistry.GetEvent<EmptyEventArgs>( nameof( _tipSwitch ), nameof( LoadingScreen ) ).PublishEvery( EmptyEventArgs.Args, 4500 );
			_tipSubscription = _tipSwitch.Subscribe( OnSwitchTip );

			_tipLabel = FindChild<EngineText>( "TipLabel" );
			OnSwitchTip( default );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown() {
			_tipSubscription?.Dispose();
			_tipSwitch?.Dispose();
		}

		/*
		===============
		OnSwitchTip
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSwitchTip( in EmptyEventArgs args ) {
			_currentTip = Random.Shared.Next( 0, _tipList.Length - 1 );
			// TODO: add something here to avoid showing the same tip twice.
			_tipLabel.Text = _tipList[ _currentTip ];
		}
	};
};