/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Game.Application.Audio;
using Godot;
using Nomad.Core.Events;
using Nomad.UI;
using Nomad.Game.Infrastructure.Audio;

namespace Nomad.Game.Infrastructure.UI.Nodes.Button {
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
		public float Duration = 0.25f;

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
			AddComponent<UIButtonAudioFeedback>(comp => {
				comp.Button = this;
				comp.ClickSound = AudioConstants.BUTTON_PRESSED;
				comp.FocusedSound = AudioConstants.BUTTON_FOCUSED;
			});

			Focused.Subscribe( OnFocused );
			Unfocused.Subscribe( OnUnfocused );
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
