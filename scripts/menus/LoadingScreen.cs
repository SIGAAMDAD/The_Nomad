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

using System.Diagnostics;
using Godot;
using ResourceCache;

namespace Menus {
	/*
	===================================================================================
	
	LoadingScreen
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class LoadingScreen : CanvasLayer {
		public PackedScene LoadedScene { get; private set; }

		private Label TipLabel;
		private Control Background;
		private Range Spinner;
		private int PrevTip = 0;
		private Timer ImageChange;
		private Stopwatch TimeLoading;

		private System.Threading.Thread LoadThread;

		private readonly string[] Tips = [
			"You can parry anything that's flashed green, including blades, bullets, etc.",
		//		"Dashing into an enemy will send them flying", // not true unless we're using RigidBody2D instead of CharacterBody2D
			"You can dodge almost anything by dashing, don't blow yourself up though...",
			"Just parry the bullet dude!",
			"Different enemies require different tactics. No more brainless shooting!",
			"You're a one ton hunk of muscle and metal, use that to your advantage",
			"The harder you are hit, the harder you hit back",
			"Death follows you everywhere you go... literally",
			"Don't be scared to experiment",
			"Bathe in the blood of your enemies for some health",
			"Remember: warcrimes don't exist anymore!",
			"A mission can count as a stealth mission if there aren't any witnesses",
			"There are tips here, y'know, read 'em",
			"Always keep in mind that STEALTH is optional",
			"ANYTHING and EVERYTHING is a weapon",
			"Parry that you filthy casual",
			"This game won't baby you, so stop acting like a child",
			"You can slice bullets in half, just make sure whatever's behind you can take the hit",
			"You are literally too angry to die",
			"Stop hiding behind cover like a little coward", // I would put "bitch" here, but I don't want issues
			"Don't blame the game for your skill issue",
			"Slamming into enemies will hurt them...",
			"Fear the fighter, not the weapon",
			"There's no such thing as \"fighting dirty\", there's only fighting",
			"Follow the glowing footsteps, trust me.",
			"Thanks for playing, truly, you playing this means a lot to me. Now please continue...",
		];

		[Signal]
		public delegate void FinishedLoadingEventHandler();

		/*
		===============
		OnImageChangeTimeout
		===============
		*/
		private void OnImageChangeTimeout() {
			if ( TimeLoading.Elapsed.Seconds >= 15 ) {
				// break the 4th wall if we've been sitting here
				if ( RNJesus.IntRange( 0, 100 ) >= 60 ) {
					TipLabel.Text = "It's lonely in this loading screen, ain't it?";
					return;
				}
			}
			int tipIndex = RNJesus.IntRange( 0, Tips.Length - 1 );
			if ( Tips[ tipIndex ] == TipLabel.Text ) {
				if ( tipIndex == Tips.Length - 1 ) {
					tipIndex = 0;
				} else {
					tipIndex++;
				}
				if ( tipIndex == PrevTip ) {
					if ( tipIndex == Tips.Length - 1 ) {
						tipIndex = 0;
					} else {
						tipIndex++;
					}
				}
			}
			TipLabel.Text = Tips[ tipIndex ];

			ImageChange.Start();
		}

		/*
		===============
		OnFadeOutFinished
		===============
		*/
		private void OnFadeOutFinished() {
			ProcessMode = ProcessModeEnum.Disabled;
			TimeLoading.Stop();

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnFadeOutFinished ) );
			Hide();

			ImageChange.Stop();

			LoadThread.Join( 1000 );
			GetTree().ChangeSceneToPacked( LoadedScene );
			EmitSignalFinishedLoading();
		}

		/*
		===============
		FadeOut
		===============
		*/
		public void FadeOut() {
			UIAudioManager.FadeMusic();

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnFadeOutFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		}

		/*
		===============
		FadeIn
		===============
		*/
		public void FadeIn( string sceneToLoad, System.Action callback = null ) {
			Spinner.SetProcess( true );
			TimeLoading.Restart();
			ProcessMode = ProcessModeEnum.Always;

			Show();

			ImageChange.Start();

			OnImageChangeTimeout();

			LoadThread = new System.Threading.Thread( () => {
				callback?.Invoke();
				LoadedScene = SceneCache.GetScene( sceneToLoad );
				CallDeferred( MethodName.FadeOut );
			} );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			TimeLoading = new Stopwatch();

			ImageChange = GetNode<Timer>( "ImageChange" );
			ImageChange.Connect( Timer.SignalName.Timeout, Callable.From( OnImageChangeTimeout ) );

			Background = SceneCache.GetScene( "res://scenes/menus/menu_background.tscn" ).Instantiate<Control>();
			Background.ZIndex = 3;
			AddChild( Background );

			TipLabel = GetNode<Label>( "Tips/TipLabel" );
			TipLabel.Show();

			Spinner = GetNode<Range>( "Tips/Spinner" );

			ProcessMode = ProcessModeEnum.Disabled;
		}
	};
};