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

using Nomad.Events.Globals;
using Nomad.Game.Domain.Events.World;
using Nomad.UI;
using Godot;

namespace Nomad.Game.Prefabs {
	/*
	===================================================================================
	
	HeadsUpDisplayView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class HeadsUpDisplayView : EnginePanel {
		private Label _dateLabel;

		protected override void OnInit() {
			base.OnInit();

			_dateLabel = GetNode<Label>( "DateLabel" );

			GameEventRegistry
				.GetEvent<MinuteChangedEventArgs>( EventNames.MINUTE_CHANGED, EventNames.NAMESPACE )
				.Subscribe( OnMinuteChanged );
		}

		private void OnMinuteChanged( in MinuteChangedEventArgs args ) {
			_dateLabel.Text = $"{args.Time.Hour}:{args.Time.Minute} {args.Time.Month}, {args.Time.Day} {args.Time.Year}";
		}
	};
};