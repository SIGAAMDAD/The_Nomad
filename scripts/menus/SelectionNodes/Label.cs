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
	public partial class Label : Godot.Label, ISelectionNode {
		private static readonly StringName @NormalThemeStyleBoxName = "normal";
		private static readonly NodePath @ModulateColorThemePropertyName = "modulate_color";

		private static readonly Color BUTTON_FADE_IN_MODULATE = new Color { R = 2.0f, G = 2.004f, B = 0.0f, A = 1.0f };
		private static readonly Color BUTTON_FADE_OUT_MODULATE = new Color { R = 0.8f, G = 0.5f, B = 0.0f, A = 1.0f };
		private static readonly Color DEFAULT_COLOR = new Color { R = 1.0f, G = 1.0f, B = 1.0f, A = 1.0f };

		public bool IsFocused => _isFocused;
		public StyleBoxTexture FocusedStyleBox => StyleBox;

		private bool _isFocused = false;

		private Tween StrobeTween;
		private StyleBoxTexture StyleBox;

		/*
		===============
		OnFocused
		===============
		*/
		public void OnFocused() {
			GD.Print( "Label focused" );

			DisableMouseFocus();
			StartStrobe();
			_isFocused = true;

			UIAudioManager.OnButtonFocused( this );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		public void OnUnfocused() {
			GD.Print( "Label unfocused" );

			StopStrobe();
			_isFocused = false;

			UIAudioManager.OnButtonUnfocused( this );
		}

		/*
		===============
		DisableMouseFocus
		===============
		*/
		private void DisableMouseFocus() {
			Control focusOwner = GetViewport().GuiGetHoveredControl();
			if ( focusOwner != null && focusOwner is Label label ) {
				label.OnUnfocused();
			}
		}

		/*
		===============
		StartStrobe
		===============
		*/
		public void StartStrobe() {
			StrobeTween = CreateTween();
			StrobeTween.TweenProperty( StyleBox, ModulateColorThemePropertyName, BUTTON_FADE_IN_MODULATE, 0.75f );
			StrobeTween.TweenProperty( StyleBox, ModulateColorThemePropertyName, BUTTON_FADE_OUT_MODULATE, 0.75f );
			StrobeTween.SetLoops();
		}

		/*
		===============
		StopStrobe
		===============
		*/
		public void StopStrobe() {
			if ( StrobeTween == null ) {
				return;
			}
			StrobeTween.Stop();

			// reset the color so that we don't have it in a puke lookin' state
			StyleBox.ModulateColor = DEFAULT_COLOR;
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			GameEventBus.ConnectSignal( this, Label.SignalName.FocusEntered, this, Callable.From( OnFocused ) );
			GameEventBus.ConnectSignal( this, Label.SignalName.MouseEntered, this, Callable.From( OnFocused ) );
			GameEventBus.ConnectSignal( this, Label.SignalName.FocusExited, this, Callable.From( OnUnfocused ) );
			GameEventBus.ConnectSignal( this, Label.SignalName.MouseExited, this, Callable.From( OnUnfocused ) );

			StyleBox = (StyleBoxTexture)GetThemeStylebox( NormalThemeStyleBoxName );
		}
	};
};