/*
===========================================================================
The Nomad Framework
Copyright (C) 2025 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.UI;
using Nomad.EngineUtils;
using Godot;

namespace Nomad.Game.Application.Audio
{
	/*
	===================================================================================

	UIButtonAudioFeedback

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public class UIButtonAudioFeedback : NomadBehaviour
	{
		public Button Button { get; set; }
		public string ClickSound { get; set; }
		public string FocusedSound { get; set; }

		private readonly IAudioEmitter _emitter;

		/*
		===============
		UIButtonAudioFeedback
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public UIButtonAudioFeedback()
		{
			_emitter = ServiceLocator.GetService<IEmitterFactory>().CreateEmitter( "SoundCategory:UI" );
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void OnInit()
		{
			Button.Pressed += OnClicked;
			Button.FocusEntered += OnFocused;
			Button.FocusExited += OnUnfocused;
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnUnfocused()
		{
		}

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnFocused()
		{
			_emitter.PlaySound( FocusedSound );
		}

		/*
		===============
		OnClicked
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnClicked()
		{
			_emitter.PlaySound( ClickSound );
		}
	};
};
