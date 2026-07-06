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
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.UI;
using Nomad.EngineUtils;

namespace Nomad.Game.Presentation.Audio
{
	/*
	===================================================================================
	
	UITabContainerAudioFeedback

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class UITabContainerAudioFeedback : NomadBehaviour
	{
		public ITabContainer TabContainer { get; set; }
		public string ClickSound { get; set; }
		public string FocusedSound { get; set; }

		private readonly IAudioEmitter _emitter;

		/*
		===============
		UITabContainerAudioFeedback
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public UITabContainerAudioFeedback()
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
			TabContainer?.TabChanged.Subscribe( OnTabChanged );
			TabContainer?.TabFocused.Subscribe( OnTabFocused );
		}

		/*
		===============
		OnTabFocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnTabFocused( in int args )
		{
			_emitter.PlaySound( FocusedSound );
		}

		/*
		===============
		OnTabChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnTabChanged( in int args )
		{
			_emitter.PlaySound( ClickSound );
		}
	};
};
