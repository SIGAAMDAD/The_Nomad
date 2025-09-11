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
using ResourceCache;
using System;
using System.Reflection;

/*
===================================================================================

UIAudioManager

===================================================================================
*/

public partial class UIAudioManager : Node {
	private static readonly StringName @ParameterLoopingPropertyName = "parameters/looping";

	public static Callable OnButtonFocusedCallable;
	public static Callable OnButtonUnfocusedCallable;
	public static Callable OnButtonPressedCallable;

	private static Callable OnMenuIntroMusicFinishedCallable;
	private static Callable OnAudioFadeFinishedCallable;

	private static AudioStreamPlayer UIButtonPressedStream;
	private static AudioStreamPlayer UIButtonFocusedStream;
	private static AudioStreamPlayer UIMusicStream;

	private static AudioStream? ButtonPressed;
	private static AudioStream? ButtonFocused;
	private static AudioStream? Activate;
	private static AudioStream? LoopingTheme;
	private static AudioStream? IntroTheme;

	/*
	===============
	OnButtonFocused
	===============
	*/
	public static void OnButtonFocused( Control? button ) {
		ArgumentNullException.ThrowIfNull( button );

		UIButtonFocusedStream.Stream = ButtonFocused;
		UIButtonFocusedStream.Play();
	}

	/*
	===============
	OnButtonUnfocused
	===============
	*/
	public static void OnButtonUnfocused( Control? button ) {
		ArgumentNullException.ThrowIfNull( button );
	}

	/*
	===============
	OnButtonPressed
	===============
	*/
	public static void OnButtonPressed() {
		UIButtonPressedStream.Stream = ButtonPressed;
		UIButtonPressedStream.Play();
	}

	/*
	===============
	OnActivate
	===============
	*/
	public static void OnActivate() {
		UIButtonPressedStream.Stream = Activate;
		UIButtonPressedStream.Play();
	}

	/*
	===============
	PlayCustomSound
	===============
	*/
	public static void PlayCustomSound( AudioStream stream ) {
		UIButtonPressedStream.Stream = stream;
		UIButtonPressedStream.Play();
	}

	/*
	===============
	PlayTheme
	===============
	*/
	public static void PlayTheme() {
		Node root = (Node)Engine.GetMainLoop().Get( SceneTree.PropertyName.Root );

		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
		UIMusicStream.Stream = IntroTheme;
		GameEventBus.ConnectSignal( UIMusicStream, AudioStreamPlayer2D.SignalName.Finished, root.GetNode( "/root/UiAudioManager" ), OnMenuIntroMusicFinishedCallable );
		UIMusicStream.Play();
	}

	/*
	===============
	FadeMusic
	===============
	*/
	public static void FadeMusic() {
		Node root = (Node)Engine.GetMainLoop().Get( SceneTree.PropertyName.Root );

		Tween audioFade = root.GetTree().CreateTween();
		audioFade.TweenProperty( UIMusicStream, "volume_db", -20.0f, 1.5f );
		audioFade.Connect( AudioStreamPlayer2D.SignalName.Finished, OnAudioFadeFinishedCallable );
	}

	/*
	===============
	OnSettingsChanged
	===============
	*/
	private void OnSettingsChanged() {
		UIButtonFocusedStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		UIButtonPressedStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
	}

	/*
	===============
	OnMenuIntroThemeFinished
	===============
	*/
	private void OnMenuIntroThemeFinished() {
		UIMusicStream.Stream = LoopingTheme;
		UIMusicStream.Set( ParameterLoopingPropertyName, true );
		GameEventBus.DisconnectAllForObject( UIMusicStream );
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
	_Ready
	===============
	*/
	public override void _Ready() {
		base._Ready();

		OnAudioFadeFinishedCallable = Callable.From( OnAudioFadeFinished );
		OnMenuIntroMusicFinishedCallable = Callable.From( OnMenuIntroThemeFinished );
		OnButtonFocusedCallable = Callable.From<Control>( OnButtonFocused );
		OnButtonUnfocusedCallable = Callable.From<Control>( OnButtonUnfocused );
		OnButtonPressedCallable = Callable.From( OnButtonPressed );

		ButtonPressed = AudioCache.GetStream( "res://sounds/ui/button_pressed.ogg" );
		ButtonFocused = AudioCache.GetStream( "res://sounds/ui/button_focused.ogg" );
		Activate = AudioCache.GetStream( "res://sounds/ui/begin_game.ogg" );

		IntroTheme = AudioCache.GetStream( "res://music/ui/menu_intro.ogg" );
		LoopingTheme = AudioCache.GetStream( "res://music/ui/menu_loop2.ogg" );

		UIButtonFocusedStream = new AudioStreamPlayer() {
			Name = nameof( UIButtonFocusedStream ),
			VolumeDb = SettingsData.GetEffectsVolumeLinear()
		};
		AddChild( UIButtonFocusedStream );

		UIButtonPressedStream = new AudioStreamPlayer() {
			Name = nameof( UIButtonPressedStream ),
			VolumeDb = SettingsData.GetEffectsVolumeLinear()
		};
		AddChild( UIButtonPressedStream );

		UIMusicStream = new AudioStreamPlayer() {
			Name = nameof( UIMusicStream ),
			VolumeDb = SettingsData.GetMusicVolumeLinear()
		};
		AddChild( UIMusicStream );

		SetProcess( false );

		SettingsData.SettingsChanged += OnSettingsChanged;
	}

	/*
	===============
	_ExitTree
	===============
	*/
	public override void _ExitTree() {
		base._ExitTree();

		SettingsData.SettingsChanged -= OnSettingsChanged;
	}
};