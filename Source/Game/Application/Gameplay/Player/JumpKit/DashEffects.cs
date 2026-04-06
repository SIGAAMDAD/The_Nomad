/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Audio.Interfaces;
using Nomad.Game.Domain.Data.Player;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit {
	/*
	===================================================================================

	DashEffects

	===================================================================================
	*/
	/// <summary>
	/// Owns presentation and engine side-effects only.
	/// </summary>
	
	internal sealed class DashEffects {
		private readonly IAudioEmitter? _emitter;
		private readonly EngineLight2D? _light;

		public DashEffects( IAudioEmitter? emitter, EngineLight2D? light ) {
			_emitter = emitter;
			_light = light;
		}

		/*
		===============
		OnDashStarted
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		public void OnDashStarted( in DashStartResult result ) {
			// TODO:
			// - enable dash light
			// - play dash SFX
			// - start particles / trails
			// - set player dashing flags
			//
			// You now have a single place for all presentation behavior.
			_emitter.PlaySound( string.Empty );
			_ = result;
		}

		/*
		===============
		OnDashEnded
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void OnDashEnded() {
			_emitter.PlaySound( string.Empty );
			// TODO:
			// - disable dash light
			// - stop particles / trails
			// - clear player dashing flags
		}

		/*
		===============
		OnBurnoutTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		public void OnBurnoutTriggered( in DashStartResult result ) {
			_emitter.PlaySound( string.Empty );
			// TODO:
			// - play burnout SFX
			// - spawn burnout effect
			// - flash UI indicator
			_ = result;
		}

		/*
		===============
		OnBurnoutTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		public void OnBurnoutTriggered( in DashUpdateResult result ) {
			_emitter.PlaySound( string.Empty );
			// Optional overload if you later trigger burnout from Update()
			_ = result;
		}

		/*
		===============
		OnDashRecharged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		public void OnDashRecharged( in DashUpdateResult result ) {
			_emitter.PlaySound( string.Empty );
			// TODO:
			// - play recharge / ready SFX
			// - restore light state if needed
			// - notify HUD that dash is ready again
			_ = result;
		}
	};
};