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

using EventSystem;
using Godot;
using ResourceCache;
using System;

namespace Menus {
	/*
	===================================================================================
	
	AudioManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class AudioManager {
		public readonly ref struct PathParams {
			public readonly string IntroMusicPath;
			public readonly string LoopMusicPath;
			public readonly string FocusedSfxPath;
			public readonly string PressedSfxPath;
			public readonly string ActivatedSfxPath;

			public PathParams( string introMusic, string? loopMusic, string? focusedSfx, string? pressedSfx, string? activatsedSfx ) {
				ArgumentException.ThrowIfNullOrEmpty( loopMusic );
				ArgumentException.ThrowIfNullOrEmpty( focusedSfx );
				ArgumentException.ThrowIfNullOrEmpty( pressedSfx );
				ArgumentException.ThrowIfNullOrEmpty( activatsedSfx );

				IntroMusicPath = introMusic;
				LoopMusicPath = loopMusic;
				FocusedSfxPath = focusedSfx;
				PressedSfxPath = pressedSfx;
				ActivatedSfxPath = activatsedSfx;
			}
		};

		private readonly AudioStreamPlayer StreamPlayer;
		private readonly AudioStreamPlayer MusicPlayer;

		private readonly AudioStream FocusedEffect;
		private readonly AudioStream PressedEffect;
		private readonly AudioStream ActivatedEffect;

		private readonly AudioStream MenuIntroMusic;
		private readonly AudioStream MenuLoopMusic;

		private static AudioManager Instance;

		/*
		===============
		AudioManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mainMenu"></param>
		public AudioManager( Control? mainMenu, in PathParams pathParams ) {
			ArgumentNullException.ThrowIfNull( mainMenu );

			StreamPlayer = new AudioStreamPlayer() {
				Name = nameof( StreamPlayer )
			};
			mainMenu.AddChild( StreamPlayer );

			MusicPlayer = new AudioStreamPlayer() {
				Name = nameof( MusicPlayer )
			};
			mainMenu.AddChild( MusicPlayer );

			FocusedEffect = AudioCache.GetStream( pathParams.FocusedSfxPath );
			PressedEffect = AudioCache.GetStream( pathParams.PressedSfxPath );
			ActivatedEffect = AudioCache.GetStream( "res://sounds/ui/begin_game.ogg" );
			MenuIntroMusic = AudioCache.GetStream( "res://sounds/music/menu_intro.ogg" );
			MenuLoopMusic = AudioCache.GetStream( "res://sounds/music/menu_loop.ogg" );

			Instance = this;
		}

		/*
		===============
		PlayFocusedSound
		===============
		*/
		public static void PlayFocusedSound() {
			ArgumentNullException.ThrowIfNull( Instance );

			Instance.StreamPlayer.Stream = Instance.FocusedEffect;
			Instance.StreamPlayer.Play();
		}

		/*
		===============
		PlayPressedSound
		===============
		*/
		public static void PlayPressedSound() {
			ArgumentNullException.ThrowIfNull( Instance );

			Instance.StreamPlayer.Stream = Instance.PressedEffect;
			Instance.StreamPlayer.Play();
		}

		/*
		===============
		PlayActivatedSound
		===============
		*/
		public static void PlayActivatedSound() {
			ArgumentNullException.ThrowIfNull( Instance );

			Instance.StreamPlayer.Stream = Instance.ActivatedEffect;
			Instance.StreamPlayer.Play();
		}

		/*
		===============
		PlayMenuMusic
		===============
		*/
		public static void PlayMenuMusic() {
			ArgumentNullException.ThrowIfNull( Instance );

			Instance.MusicPlayer.Stream = Instance.MenuIntroMusic;
			GameEventBus.ConnectSignal( Instance.MusicPlayer, AudioStreamPlayer.SignalName.Finished, Instance.MusicPlayer, OnMenuIntroMusicFinished );
			Instance.MusicPlayer.Play();
		}

		/*
		===============
		OnMenuIntroMusicFinished
		===============
		*/
		private static void OnMenuIntroMusicFinished() {
			Instance.MusicPlayer.Stream = Instance.MenuLoopMusic;
			GameEventBus.CleanupSubscriber( Instance.MusicPlayer );
			GameEventBus.ConnectSignal( Instance.MusicPlayer, AudioStreamPlayer.SignalName.Finished, Instance.MusicPlayer, () => Instance.MusicPlayer.Play() );
			Instance.MusicPlayer.Play();
		}
	};
};