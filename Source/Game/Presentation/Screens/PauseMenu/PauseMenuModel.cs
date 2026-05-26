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
using Nomad.Audio.Interfaces;
using Nomad.Game.Sdk;

namespace Nomad.Game.Presentation.Screens.PauseMenu
{
	/*
	===================================================================================
	
	PauseMenuModel
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class PauseMenuModel
	{
		public bool IsPaused { get; private set; } = false;

		private readonly IAudioEmitter _emitter;

		/*
		===============
		PauseMenuModel
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="factory"></param>
		public PauseMenuModel( IEmitterFactory factory )
		{
			ArgumentNullException.ThrowIfNull( factory );
			_emitter = factory.CreateEmitter( "SoundCategory:UI" );
		}

		/*
		===============
		SetState
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="isPaused"></param>
		/// <returns><see langword="true"/> when the pause state changed.</returns>
		public bool SetPaused( bool isPaused )
		{
			if ( IsPaused == isPaused ) {
				return false;
			}

			if ( isPaused ) {
				_emitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.UIGamePaused ).Path );
			} else {
				_emitter.PlaySound( AudioEventIdConstants.GetEvent( AudioEventId.UIGameResumed ).Path );
			}

			IsPaused = isPaused;
			return true;
		}
	};
};
