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
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events.Globals;

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
		public NomadButtonController( NomadButtonView view ) {
			_view = view;

			view.Owner.Focused.Subscribe( OnFocused );
			view.Owner.Unfocused.Subscribe( OnUnfocused );
			view.Owner.Clicked.Subscribe( OnPressed );
		}
		
		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// Focus callback for a <see cref="NomadButtonNode"/>.
		/// </summary>
		private void OnFocused( in EmptyEventArgs args ) {
			_isFocused = true;
			_view.AnimateHover();

			GameEventRegistry.GetEvent<ButtonFocusedEventArgs>( UIConstants.BUTTON_FOCUSED_EVENT, UIConstants.NAMESPACE ).Publish( new ButtonFocusedEventArgs( _view.ButtonId ) );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnUnfocused( in EmptyEventArgs args ) {
			_isFocused = false;
			_view.AnimateHover();

			GameEventRegistry.GetEvent<ButtonUnfocusedEventArgs>( UIConstants.BUTTON_UNFOCUSED_EVENT, UIConstants.NAMESPACE ).Publish( new ButtonUnfocusedEventArgs( _view.ButtonId ) );
		}
		
		/*
		===============
		OnPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnPressed( in EmptyEventArgs args ) {
			GameEventRegistry.GetEvent<ButtonClickedEventArgs>( UIConstants.BUTTON_CLICKED_EVENT, UIConstants.NAMESPACE ).Publish( new ButtonClickedEventArgs( _view.ButtonId ) );
		}
	};
};
