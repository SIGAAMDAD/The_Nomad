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

public partial class TransitionScreen : CanvasLayer {
	private static readonly StringName @FadeToBlackAnimationName = "fade_to_black";
	private static readonly StringName @FadeToNormalAnimationName = "fade_to_normal";

	private ColorRect ColorRect;
	private AnimationPlayer AnimationPlayer;

	public static event Action TransitionFinished;

	private static TransitionScreen Instance;

	/*
	===============
	Transition
	===============
	*/
	public static void Transition() {
		Instance.ColorRect.Show();
		Instance.AnimationPlayer.Play( FadeToBlackAnimationName );
	}

	/*
	===============
	OnAnimationFinished
	===============
	*/
	private void OnAnimationFinished( StringName animationName ) {
		if ( animationName == FadeToBlackAnimationName ) {
			TransitionFinished.Invoke();
			AnimationPlayer.Play( FadeToNormalAnimationName );
		} else if ( animationName == FadeToNormalAnimationName ) {
			ColorRect.Hide();
		} else {
			Console.PrintError( $"TransitionScreen.OnAnimationFinished: animation {animationName} isn't valid!" );
		}
	}

	/*
	===============
	_Ready
	===============
	*/
	public override void _Ready() {
		base._Ready();

		ColorRect = GetNode<ColorRect>( "ColorRect" );

		AnimationPlayer = GetNode<AnimationPlayer>( "AnimationPlayer" );
		GameEventBus.ConnectSignal( AnimationPlayer, AnimationPlayer.SignalName.AnimationFinished, this, Callable.From<StringName>( OnAnimationFinished ) );

		Instance = this;
	}
};