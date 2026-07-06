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

using Nomad.Game.Presentation.Audio;
using Godot;
using Nomad.Game.Sdk.Audio;

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
		private ButtonAnimator _animator;

		private readonly float _duration = 0.25f;
		private readonly Vector2 _scaleIntensity = new Vector2( 1.10f, 1.10f );

		private UIButtonAudioFeedback _feedback;

		private bool _isFocused = false;

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
			_animator.Animate(
				_isFocused ? _scaleIntensity : Vector2.One,
				_duration
			);
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
			HoverScaleAnimation();
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
			HoverScaleAnimation();
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

			_animator = new ButtonAnimator( this );

			_feedback = new UIButtonAudioFeedback() {
				Button = this,
				ClickSound = AudioEventIdConstants.GetEvent( AudioEventId.UIButtonPressed ).Path,
				FocusedSound = AudioEventIdConstants.GetEvent( AudioEventId.UIButtonFocused ).Path
			};
			_feedback.OnInit();

			FocusEntered += OnFocused;
			FocusExited += OnUnfocused;
			MouseEntered += OnFocused;
			MouseExited += OnUnfocused;
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree();

			_animator.Dispose();
		}
	};
};
