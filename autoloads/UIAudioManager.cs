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
using Menus;

/*
===================================================================================

UIAudioManager

===================================================================================
*/

public partial class UIAudioManager : Node {
	private static AudioStreamPlayer UISfxStream;
	private static AudioStreamPlayer UIMusicStream;

	private static AudioStream ButtonPressed;
	private static AudioStream ButtonFocused;
	private static AudioStream Activate;
	private static AudioStream LoopingTheme;
	private static AudioStream IntroTheme;

	/*
	===============
	OnSettingsChanged
	===============
	*/
	private void OnSettingsChanged() {
		UISfxStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
	}

	/*
	===============
	OnMenuIntroThemeFinished
	===============
	*/
	private static void OnMenuIntroThemeFinished() {
		UIMusicStream.Stream = LoopingTheme;
		UIMusicStream.Set( "parameters/looping", true );
		UIMusicStream.Disconnect( "finished", Callable.From( OnMenuIntroThemeFinished ) );
		UIMusicStream.Play();
	}

	/*
	===============
	OnButtonFocused
	===============
	*/
	public static void OnButtonFocused() {
		UISfxStream.Stream = ButtonFocused;
		UISfxStream.Play();
	}

	/*
	===============
	OnButtonPressed
	===============
	*/
	public static void OnButtonPressed() {
		UISfxStream.Stream = ButtonPressed;
		UISfxStream.Play();
	}

	/*
	===============
	OnActivate
	===============
	*/
	public static void OnActivate() {
		UISfxStream.Stream = Activate;
		UISfxStream.Play();
	}

	/*
	===============
	PlayCustomSound
	===============
	*/
	public static void PlayCustomSound( AudioStream stream ) {
		UISfxStream.Stream = stream;
		UISfxStream.Play();
	}

	/*
	===============
	PlayTheme
	===============
	*/
	public static void PlayTheme() {
		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
		UIMusicStream.Stream = IntroTheme;
		if ( !UIMusicStream.IsConnected( "finished", Callable.From( OnMenuIntroThemeFinished ) ) ) {
			UIMusicStream.Connect( "finished", Callable.From( OnMenuIntroThemeFinished ) );
		}
		UIMusicStream.Play();
	}

	/*
	===============
	OnAudioFadeFinished
	===============
	*/
	private static void OnAudioFadeFinished() {
		UIMusicStream.Stop();
	}

	/*
	===============
	FadeMusic
	===============
	*/
	public static void FadeMusic() {
		Node Root = (Node)Engine.GetMainLoop().Get( "root" );

		Tween AudioFade = Root.GetTree().CreateTween();
		AudioFade.TweenProperty( UIMusicStream, "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );
	}

	/*
	===============
	_Ready
	===============
	*/
	public override void _Ready() {
		base._Ready();

		ButtonPressed = ResourceLoader.Load<AudioStream>( "res://sounds/ui/button_pressed.ogg" );
		ButtonFocused = ResourceLoader.Load<AudioStream>( "res://sounds/ui/button_focused.ogg" );
		Activate = ResourceLoader.Load<AudioStream>( "res://sounds/ui/begin_game.ogg" );

		IntroTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_intro.ogg" );
		LoopingTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" );

		UISfxStream = new AudioStreamPlayer();
		UISfxStream.Name = "UISfxStream";
		UISfxStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( UISfxStream );

		UIMusicStream = new AudioStreamPlayer();
		UIMusicStream.Name = "UIMusicStream";
		AddChild( UIMusicStream );

		SettingsData.Instance.SettingsChanged += OnSettingsChanged;
	}
};