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

using Game.Application.Audio;
using Game.Application.UI;
using Godot;
using Nomad.Core.Events;
using Nomad.EngineUtils.UserInterface;

namespace Game.Infrastructure.UI.Nodes.Button {
	/*
	===================================================================================
	
	NomadButton
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class NomadButton : EngineButton {
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

		public bool IsFocused => _isFocused;
		private bool _isFocused = false;

		private ISubscriptionHandle _focused;
		private ISubscriptionHandle _unfocused;

		/*
		===============
		AnimateHover
		===============
		*/
		/// <summary>
		/// Activates button animations.
		/// </summary>
		public void AnimateHover() {
			HoverPositionAnimation();
			HoverScaleAnimation();
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			AddComponent<UIAudioFeedback>(comp => {
				comp.Button = this;
				comp.ClickSound = UIConstants.BUTTON_CLICKED_EVENT;
				comp.FocusedSound = UIConstants.BUTTON_FOCUSED_EVENT;
			});

			_focused = Focused.Subscribe( OnFocused );
			_unfocused = Unfocused.Subscribe( OnUnfocused );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown() {
			_focused?.Dispose();
			_unfocused?.Dispose();
		}

		/*
		===============
		HoverPositionAnimation
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void HoverPositionAnimation() {
			if ( !AnimatePosition ) {
				return;
			}
			Tweening(
				"position",
				_isFocused ? PositionValue : Vector2.Zero,
				Duration
			);
		}

		/*
		===============
		HoverScaleAnimation
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void HoverScaleAnimation() {
			if ( !AnimateScale ) {
				return;
			}
			Tweening(
				"scale",
				_isFocused ? new Vector2( ScaleIntensity, ScaleIntensity ) : Vector2.One,
				Duration
			);
		}

		/*
		===============
		Tweening
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="finalValue"></param>
		/// <param name="duration"></param>
		private async void Tweening( NodePath property, Variant finalValue, float duration ) {
			Tween tween = CreateTween().SetTrans( TransitionType );
			tween.TweenProperty( this, property, finalValue, duration );
			await ToSignal( tween, Tween.SignalName.Finished );
			tween.Kill();
		}

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// Focus callback for a <see cref="NomadButton"/>.
		/// </summary>
		private void OnFocused( in EmptyEventArgs args ) {
			_isFocused = true;
			AnimateHover();
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
			AnimateHover();
		}
	};
};
