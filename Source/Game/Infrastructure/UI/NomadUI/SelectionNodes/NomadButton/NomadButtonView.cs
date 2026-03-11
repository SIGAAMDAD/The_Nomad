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

using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Godot;
using Nomad.Core.Util;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.NomadButton {
	/*
	===================================================================================
	
	NomadButtonView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// <param name="owner"></param>
	/// <param name="animationConfig"></param>

	public class NomadButtonView : INomadButtonView {
		public InternString ButtonId => _buttonId;
		private readonly InternString _buttonId;

		public NomadButtonNode Owner => _owner;
		private readonly NomadButtonNode _owner;

		private readonly NomadButtonAnimation _animationConfig;

		public NomadButtonView( NomadButtonNode owner, NomadButtonAnimation animationConfig ) {
			_owner = owner;
			_buttonId = new InternString( owner.Name );
			_animationConfig = animationConfig;
		}

		/*
		===============
		SetText
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		public void SetText( string text ) {
			_owner.Text = text;
		}

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
		HoverPositionAnimation
		===============
		*/
		private void HoverPositionAnimation() {
			if ( !_animationConfig.AnimatePosition ) {
				return;
			}
			Tweening(
				"position",
				_owner.IsFocused ? _animationConfig.PositionValue : Vector2.Zero,
				_animationConfig.Duration
			);
		}

		/*
		===============
		HoverScaleAnimation
		===============
		*/
		private void HoverScaleAnimation() {
			if ( !_animationConfig.AnimateScale ) {
				return;
			}
			Tweening(
				"scale",
				_owner.IsFocused ? new Vector2( _animationConfig.ScaleIntensity, _animationConfig.ScaleIntensity ) : Vector2.One,
				_animationConfig.Duration
			);
		}

		/*
		===============
		Tweening
		===============
		*/
		private async void Tweening( NodePath property, Variant finalValue, float duration ) {
			Tween tween = _owner.CreateTween().SetTrans( _animationConfig.TransitionType );
			tween.TweenProperty( _owner, property, finalValue, duration );
			await _owner.ToSignal( tween, Tween.SignalName.Finished );
			tween.Kill();
		}
	};
};
