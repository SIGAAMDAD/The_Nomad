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

namespace Menus.SelectionNodes {
	public sealed partial class AdvancedButton : Button {
		[Export]
		private VBoxContainer ToggleContainer;

		/*
		===============
		OnPressed
		===============
		*/
		private void OnPressed() {
			GetParent<Control>().Hide();
			ToggleContainer.Show();
		}

		/*
		===============
		OnVisibilityChanged
		===============
		*/
		private void OnVisibilityChanged() {
			ToggleContainer.Visible = !GetParent<Control>().Visible;
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			GameEventBus.ConnectSignal( GetParent(), Control.SignalName.VisibilityChanged, this, OnVisibilityChanged );

			Pressed += OnPressed;
		}
	};
};