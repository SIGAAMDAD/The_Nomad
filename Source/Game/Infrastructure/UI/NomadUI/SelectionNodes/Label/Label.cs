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

using Godot;
using Nomad.Core.Events;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes {
	/*
	===================================================================================

	Label
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class Label : Godot.Label {
		private static readonly StringName @NormalThemeStyleBoxName = "normal";

		public bool IsFocused => _isFocused;
		private bool _isFocused = false;

		/*
		===============
		OnFocused
		===============
		*/
		public void OnFocused() {
			DisableMouseFocus();
			_isFocused = true;

			//UIAudioManager.OnButtonFocused( this );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		public void OnUnfocused() {
			_isFocused = false;

			//UIAudioManager.OnButtonUnfocused( this );
		}

		/*
		===============
		DisableMouseFocus
		===============
		*/
		public void DisableMouseFocus() {
			Control focusOwner = GetViewport().GuiGetHoveredControl();
			if ( focusOwner != null && focusOwner is Label label ) {
				label.OnUnfocused();
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var eventBus = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGodotEventBusService>();
			eventBus.ConnectSignal( this, Label.SignalName.FocusEntered, this, Callable.From( OnFocused ) );
			eventBus.ConnectSignal( this, Label.SignalName.MouseEntered, this, Callable.From( OnFocused ) );
			eventBus.ConnectSignal( this, Label.SignalName.FocusExited, this, Callable.From( OnUnfocused ) );
			eventBus.ConnectSignal( this, Label.SignalName.MouseExited, this, Callable.From( OnUnfocused ) );
		}
	};
};