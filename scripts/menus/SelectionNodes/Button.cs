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
using System;

namespace Menus.SelectionNodes {
	public partial class Button : Godot.Button, ISelectionNode {
		private static readonly StringName @HoverThemeStyleBoxName = "hover";
		private static readonly NodePath @ModulateColorThemePropertyName = "modulate_color";

		private static readonly Color BUTTON_FADE_IN_MODULATE = new Color { R = 2.0f, G = 2.004f, B = 0.0f, A = 1.0f };
		private static readonly Color BUTTON_FADE_OUT_MODULATE = new Color { R = 0.8f, G = 0.5f, B = 0.0f, A = 1.0f };
		private static readonly Color DEFAULT_COLOR = new Color { R = 1.0f, G = 1.0f, B = 1.0f, A = 1.0f };

		[Export( PropertyHint.Range, "0,10,0.001,or_greater" )]
		public float Duration = 1.0f;

		[Export]
		public bool AnimateScale = true;
		[Export]
		public bool AnimatePosition = false;
		[Export]
		public Tween.TransitionType TransitionType;

		[ExportGroup( "Scale Properties", "scale_" )]
		[Export]
		public float ScaleIntensity = 1.10f;

		[ExportGroup( "Position Properties", "position_" )]
		[Export]
		public Vector2 PositionValue = new Vector2( 0.0f, -4.0f );

		private Tween Tween;
		private Vector2 ButtonStartPos = Vector2.Zero;

		public event Action Pressed;
		public bool IsFocused => _isFocused;
		public StyleBoxTexture FocusedStyleBox => StyleBox;

		private bool _isFocused = false;

		private Tween StrobeTween;
		private Tween AnimationTween;
		private StyleBoxTexture StyleBox;

		/*
		===============
		DisableMouseFocus
		===============
		*/
		private void DisableMouseFocus() {
			Control focusNode = GetViewport().GuiGetHoveredControl();
			if ( focusNode != null && focusNode is Button button ) {
				button.OnUnfocused();
			}
		}

		/*
		===============
		StartStrobe
		===============
		*/
		private void StartStrobe() {
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
		private void StopStrobe() {
			if ( StrobeTween == null ) {
				return;
			}
			StrobeTween.Stop();
		}

		/*
		===============
		OnFocused
		===============
		*/
		private void OnFocused() {
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
		private void OnUnfocused() {
			StopStrobe();
			_isFocused = false;

			UIAudioManager.OnButtonUnfocused( this );
		}

		/*
		===============
		HoverScaleAnimation
		===============
		*/
		private void HoverScaleAnimation() {
			if ( !AnimateScale ) {
				return;
			}
			Tweening(
				this,
				"scale",
				_isFocused ? new Vector2( ScaleIntensity, ScaleIntensity ) : Vector2.One,
				Duration
			);
		}

		/*
		===============
		HoverPositionAnimation
		===============
		*/
		private void HoverPositionAnimation() {
			if ( !AnimatePosition ) {
				return;
			}
			Tweening(
				this,
				"position",
				_isFocused ? ButtonStartPos + PositionValue : ButtonStartPos,
				Duration
			);
		}

		/*
		===============
		Tweening
		===============
		*/
		private async void Tweening( GodotObject obj, NodePath property, Variant finalValue, float duration ) {
			Tween = CreateTween().SetParallel( true ).SetTrans( TransitionType );
			Tween.TweenProperty( obj, property, finalValue, duration );
			await ToSignal( Tween, Tween.SignalName.Finished );
			Tween.Kill();
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			GameEventBus.ConnectSignal( this, Button.SignalName.FocusEntered, this, Callable.From( OnFocused ) );
			GameEventBus.ConnectSignal( this, Button.SignalName.MouseEntered, this, Callable.From( OnFocused ) );
			GameEventBus.ConnectSignal( this, Button.SignalName.FocusExited, this, Callable.From( OnUnfocused ) );
			GameEventBus.ConnectSignal( this, Button.SignalName.MouseExited, this, Callable.From( OnUnfocused ) );
			GameEventBus.ConnectSignal( this, Button.SignalName.Pressed, this, Callable.From( () => Pressed?.Invoke() ) );

			StyleBox = (StyleBoxTexture)GetThemeStylebox( HoverThemeStyleBoxName );
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );

			HoverScaleAnimation();
			HoverPositionAnimation();
		}

		/*
		===============
		_ExitTree
		===============
		*/
		public override void _ExitTree() {
			base._ExitTree();

			GameEventBus.ReleaseDanglingDelegates( Pressed );
		}
	};
};