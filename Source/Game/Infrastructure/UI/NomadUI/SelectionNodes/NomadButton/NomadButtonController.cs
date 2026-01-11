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

using Game.Application.UI;
using Game.Domain.Events.UI;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events.Private;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.NomadButton {
	/*
	===================================================================================
	
	NomadButtonController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class NomadButtonController : INomadButtonController {
		public InternString ButtonId => _view.ButtonId;

		public bool IsFocused => _isFocused;
		private bool _isFocused = false;

		private readonly NomadButtonView _view;

		/*
		===============
		NomadButtonController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		public NomadButtonController( IGodotEventBusService eventBus, NomadButtonView view ) {
			_view = view;

			eventBus.ConnectSignal( view.Owner, NomadButtonNode.SignalName.FocusEntered, view.Owner, Callable.From( OnFocused ) );
			eventBus.ConnectSignal( view.Owner, NomadButtonNode.SignalName.FocusExited, view.Owner, Callable.From( OnUnfocused ) );
			eventBus.ConnectSignal( view.Owner, NomadButtonNode.SignalName.MouseEntered, view.Owner, Callable.From( OnFocused ) );
			eventBus.ConnectSignal( view.Owner, NomadButtonNode.SignalName.MouseExited, view.Owner, Callable.From( OnUnfocused ) );
			eventBus.ConnectSignal( view.Owner, NomadButtonNode.SignalName.Pressed, view.Owner, Callable.From( OnPressed ) );
		}

		/*
		===============
		DisableMouseFocus
		===============
		*/
		/// <summary>
		/// Disables the focus of another UI element pinned by the mouse.
		/// </summary>
		public void DisableMouseFocus() {
			Control focusNode = _view.Owner.GetViewport().GuiGetHoveredControl();
			if ( focusNode != null && focusNode is NomadButtonNode button ) {
				button.Controller.OnUnfocused();
			}
		}
		
		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// Focus callback for a <see cref="NomadButtonNode"/>.
		/// </summary>
		public void OnFocused() {
			DisableMouseFocus();
			_isFocused = true;
			_view.AnimateHover();

			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.BUTTON_FOCUSED_EVENT, new ButtonFocusedEventArgs( _view.ButtonId ) );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void OnUnfocused() {
			_isFocused = false;
			_view.AnimateHover();
			
			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.BUTTON_UNFOCUSED_EVENT, new ButtonUnfocusedEventArgs( _view.ButtonId ) );
		}
		
		/*
		===============
		OnPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnPressed() {
			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.BUTTON_CLICKED_EVENT, new ButtonClickedEventArgs( _view.ButtonId ) );
		}
	};
};