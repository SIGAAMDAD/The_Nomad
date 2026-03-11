/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Game.Domain.Events.UI;
using Game.Infrastructure.Audio;
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Events.Global;

namespace Game.Application.UI.EventHandlers {
	/*
	===================================================================================
	
	AudioUIEventHandler
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class AudioUIEventHandler {
		private readonly IAudioEmitter _emitter;

		public AudioUIEventHandler( IEmitterFactory emitterFactory, IGameEventRegistryService eventRegistry ) {
			_emitter = emitterFactory.CreateEmitter( "SoundCategory:UI" );

			var clickedEvent = GameEventRegistry.GetEvent<ButtonClickedEventArgs>( UIConstants.BUTTON_CLICKED_EVENT, UIConstants.NAMESPACE );
			clickedEvent.Subscribe( OnButtonClicked );
			
			var focusedEvent = GameEventRegistry.GetEvent<ButtonFocusedEventArgs>( UIConstants.BUTTON_FOCUSED_EVENT, UIConstants.NAMESPACE );
			focusedEvent.Subscribe( OnButtonFocused );

			var optionListFocused = GameEventRegistry.GetEvent<OptionListFocusedEventArgs>( UIConstants.OPTION_LIST_FOCUSED_EVENT, UIConstants.NAMESPACE );
			optionListFocused.Subscribe( OnListFocused );

			var optionListSet = GameEventRegistry.GetEvent<OptionListValueSetEventArgs>( UIConstants.OPTION_LIST_VALUE_SET_EVENT, UIConstants.NAMESPACE );
			optionListSet.Subscribe( OnListValueSet );
		}

		/*
		===============
		OnListValueSet
		===============
		*/
		private void OnListValueSet( in OptionListValueSetEventArgs args ) {
			_emitter.PlaySound( AudioConstants.BUTTON_PRESSED );
		}

		/*
		===============
		OnButtonFocused
		===============
		*/
		private void OnListFocused( in OptionListFocusedEventArgs args ) {
			_emitter.PlaySound( AudioConstants.BUTTON_FOCUSED );
		}

		/*
		===============
		OnButtonClicked
		===============
		*/
		private void OnButtonClicked( in ButtonClickedEventArgs args ) {
			_emitter.PlaySound( AudioConstants.BUTTON_PRESSED );
		}

		/*
		===============
		OnButtonFocused
		===============
		*/
		private void OnButtonFocused( in ButtonFocusedEventArgs args ) {
			_emitter.PlaySound( AudioConstants.BUTTON_FOCUSED );
		}
	};
};
