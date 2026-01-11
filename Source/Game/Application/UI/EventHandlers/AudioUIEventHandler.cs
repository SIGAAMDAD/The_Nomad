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

namespace Game.Application.UI.EventHandlers {
	/*
	===================================================================================
	
	AudioUIEventHandler
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class AudioUIEventHandler : IUIEventHandler {
		private readonly IAudioEmitter _emitter;

		public AudioUIEventHandler( IEmitterFactory emitterFactory, IGameEventRegistryService eventRegistry ) {
			_emitter = emitterFactory.CreateEmitter( "SoundCategory:UI" );
			
			UIEventHelper.SubscribeToUIEvent<ButtonClickedEventArgs>( eventRegistry, this, UIConstants.BUTTON_CLICKED_EVENT, OnButtonClicked );
			UIEventHelper.SubscribeToUIEvent<ButtonFocusedEventArgs>( eventRegistry, this, UIConstants.BUTTON_FOCUSED_EVENT, OnButtonFocused );
			UIEventHelper.SubscribeToUIEvent<OptionListValueSetEventArgs>( eventRegistry, this, UIConstants.OPTION_LIST_VALUE_SET_EVENT, OnListValueSet );
			UIEventHelper.SubscribeToUIEvent<OptionListFocusedEventArgs>( eventRegistry, this, UIConstants.OPTION_LIST_FOCUSED_EVENT, OnListFocused );
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