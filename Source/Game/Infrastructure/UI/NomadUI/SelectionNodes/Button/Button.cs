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
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using System.Runtime.CompilerServices;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes {
	/*
	===================================================================================
	
	Button
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class Button : global::Godot.Button, ISelectionNode {
		private static readonly StringName @HoverThemeStyleBoxName = "hover";
		private static readonly NodePath @ModulateColorThemePropertyName = "modulate_color";

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

		/// <summary>
		/// s
		/// </summary>
		public bool IsFocused => _isFocused;
		private bool _isFocused = false;

		/// <summary>
		/// 
		/// </summary>
		public StyleBoxTexture FocusedStyleBox => StyleBox;

		private Tween AnimationTween;
		private StyleBoxTexture StyleBox;

		private Tween Tween;
		private Vector2 ButtonStartPos = Vector2.Zero;

		/// <summary>
		/// 
		/// </summary>
		public readonly UIEvent Activated;

		/*
		===============
		Button
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public Button() {
			Activated = new UIEvent( this, nameof( Activated ) );
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
			Control focusNode = GetViewport().GuiGetHoveredControl();
			if ( focusNode != null && focusNode is Button button ) {
				button.OnUnfocused();
			}
		}

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public virtual void OnFocused() {
			DisableMouseFocus();
			_isFocused = true;

			//UIAudioManager.OnButtonFocused();
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
		}

		/*
		===============
		HoverScaleAnimation
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
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
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			var eventBus = ServiceRegistry.Get<IGameEventBusService>();
			eventBus.ConnectSignal( this, Button.SignalName.FocusEntered, this, Callable.From( OnFocused ) );
			eventBus.ConnectSignal( this, Button.SignalName.MouseEntered, this, Callable.From( OnFocused ) );
			eventBus.ConnectSignal( this, Button.SignalName.FocusExited, this, Callable.From( OnUnfocused ) );
			eventBus.ConnectSignal( this, Button.SignalName.MouseExited, this, Callable.From( OnUnfocused ) );
			eventBus.ConnectSignal( this, Button.SignalName.Pressed, this, Callable.From( () => Activated.Publish( EmptyEventArgs.Args ) ) );

			StyleBox = (StyleBoxTexture)GetThemeStylebox( HoverThemeStyleBoxName );
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ) {
			base._Process( delta );

			HoverScaleAnimation();
			HoverPositionAnimation();
		}
	};
};