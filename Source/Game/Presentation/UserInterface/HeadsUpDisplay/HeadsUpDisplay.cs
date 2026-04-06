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

using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.UI;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay {
	/*
	===================================================================================
	
	HeadsUpDisplay
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class HeadsUpDisplay : EnginePresentationLayer {
		private ISubscriptionGroup _eventGroup;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			base.OnInit();

			var eventFactory = GameEventRegistry.Instance;
			_eventGroup.Add(
				eventFactory.GetEvent<PlayerStatChangedEventArgs>( EventNames.PLAYER_STAT_CHANGED, EventNames.NAMESPACE ),
				OnStatChanged
			);
		}

		private void OnStatChanged( in PlayerStatChangedEventArgs args ) {
			switch ( args.StatId ) {
				case StatType.Health:
					break;
			}
		}
	};
};