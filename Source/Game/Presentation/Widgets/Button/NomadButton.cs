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
using Nomad.Game.Sdk;

namespace Nomad.Game.Presentation.Widgets.NomadButton
{
	/*
	===================================================================================

	NomadButton

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal partial class NomadButton : Button
	{
		private readonly float _duration = 0.25f;
		private readonly bool _animateScale = true;
		private readonly bool _animatePosition = false;
		private readonly Tween.TransitionType _transitionType = Tween.TransitionType.Linear;
		private readonly float _scaleIntensity = 1.10f;
		private readonly Vector2 _positionValue = new Vector2( 0.0f, -4.0f );

		private UIButtonAudioFeedback _feedback;

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
		private void AnimateHover()
		{
			HoverPositionAnimation();
			HoverScaleAnimation();
		}

		/*
		===============
		HoverPositionAnimation
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void HoverPositionAnimation()
		{
			if ( !_animatePosition ) {
				return;
			}
			Tweening(
				"position",
				_isFocused ? _positionValue : Vector2.Zero,
				_duration
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
		private void HoverScaleAnimation()
		{
			if ( !_animateScale ) {
				return;
			}
			Tweening(
				"scale",
				_isFocused ? new Vector2( _scaleIntensity, _scaleIntensity ) : Vector2.One,
				_duration
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
		private async void Tweening( NodePath property, Variant finalValue, float duration )
		{
			Tween tween = CreateTween().SetTrans( _transitionType );
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
		private void OnFocused()
		{
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
		private void OnUnfocused()
		{
			_isFocused = false;
			AnimateHover();
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready()
		{
			base._Ready();

			_feedback = new UIButtonAudioFeedback() {
				Button = this,
				ClickSound = AudioEventIdConstants.GetEvent( AudioEventId.UIButtonPressed ).Path,
				FocusedSound = AudioEventIdConstants.GetEvent( AudioEventId.UIButtonFocused ).Path
			};
			_feedback.OnInit();

			FocusEntered += OnFocused;
			FocusExited += OnUnfocused;
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
		public override void _Process( double delta )
		{
			base._Process( delta );

			AnimateHover();
		}
	};
};
