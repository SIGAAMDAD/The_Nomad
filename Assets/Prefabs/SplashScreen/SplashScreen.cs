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
using Nomad.UI;
using System;

namespace Nomad.Game.Prefabs {
	/*
	===================================================================================

	SplashScreen

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class SplashScreen : EnginePanel {
		private VideoStreamPlayer _videoPlayer;
		private Control _logos;

		public event Action Finished;

		/*
		===============
		OnGodotAnimationFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnGodotAnimationFinished() {
			_videoPlayer.Visible = false;

			var timer = _logos.GetNode<Timer>( "Timer" );
			timer.Start();
			_logos.Visible = true;
		}

		/*
		===============
		OnTimerTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnTimerTimeout() {
			Visible = false;
			CallDeferred( Node.MethodName.QueueFree );
			Finished?.Invoke();
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			// godot splash video attribution
			_videoPlayer = GetNode<VideoStreamPlayer>( "GodotLogoAnimation" );
			_videoPlayer.Connect( VideoStreamPlayer.SignalName.Finished, Callable.From( OnGodotAnimationFinished ) );
			
			// logo attributions
			var timer = GetNode<Timer>( "ThirdPartyLogos/Timer" );
			timer.Connect( Timer.SignalName.Timeout, Callable.From( OnTimerTimeout ) );
			_logos = GetNode<Control>( "ThirdPartyLogos" );
		}
	};
};
