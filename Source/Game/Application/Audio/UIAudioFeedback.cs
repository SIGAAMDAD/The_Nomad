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

namespace Game.Application.Audio {
	/*
	===================================================================================
	
	UIAudioFeedback
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class UIAudioFeedback : NomadBehaviour {
		public IButton Button { get; set; }
		public string ClickSound { get; set; }
		public string FocusedSound { get; set; }

		private readonly IAudioEmitter _emitter;

		/*
		===============
		UIAudioFeedback
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public UIAudioFeedback() {
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
		public override void OnInit() {
			Button?.Clicked.Subscribe( OnClicked );
			Button?.Focused.Subscribe( OnFocused );
			Button?.Unfocused.Subscribe( OnUnfocused );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnUnfocused( in EmptyEventArgs args ) {
		}

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnFocused( in EmptyEventArgs args ) {
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
		/// <param name="args"></param>
		private void OnClicked( in EmptyEventArgs args ) {
			_emitter.PlaySound( ClickSound );
		}
	};
};
