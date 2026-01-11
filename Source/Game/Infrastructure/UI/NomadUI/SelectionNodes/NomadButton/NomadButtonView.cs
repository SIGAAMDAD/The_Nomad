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

	public class NomadButtonView( NomadButtonNode owner, NomadButtonAnimation animationConfig ) : INomadButtonView {
		public InternString ButtonId => _buttonId;
		private readonly InternString _buttonId = new( owner.Name );

		public NomadButtonNode Owner => owner;

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
			owner.Text = text;
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
			if ( !animationConfig.AnimatePosition ) {
				return;
			}
			Tweening(
				"position",
				owner.IsFocused ? animationConfig.PositionValue : Vector2.Zero,
				animationConfig.Duration
			);
		}

		/*
		===============
		HoverScaleAnimation
		===============
		*/
		private void HoverScaleAnimation() {
			if ( !animationConfig.AnimateScale ) {
				return;
			}
			Tweening(
				"scale",
				owner.IsFocused ? new Vector2( animationConfig.ScaleIntensity, animationConfig.ScaleIntensity ) : Vector2.One,
				animationConfig.Duration
			);
		}

		/*
		===============
		Tweening
		===============
		*/
		private async void Tweening( NodePath property, Variant finalValue, float duration ) {
			Tween tween = owner.CreateTween().SetTrans( animationConfig.TransitionType );
			tween.TweenProperty( owner, property, finalValue, duration );
			await owner.ToSignal( tween, Tween.SignalName.Finished );
			tween.Kill();
		}
	};
};