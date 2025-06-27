/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using System.Diagnostics;
using Godot;

public partial class LoadingScreen : CanvasLayer {
	private Label TipLabel;
	private Control Background;
	private Range Spinner;
	private string PrevTip = "";
	private Timer ImageChange;
	private Stopwatch TimeLoading = new Stopwatch();

	private readonly string[] Tips = [
		"You can parry anything that's flashed green, including blades, bullets, etc.",
		//		"Dashing into an enemy will send them flying", // not true unless we're using RigidBody2D instead of CharacterBody2D
		"You can dodge almost anything by dashing, don't blow yourself up though...",
		"Just parry the bullet dude!",
		"Different enemies require different tactics. No more brainless shooting!",
		"You're a one ton hunk of muscle and metal, use that to your advantage",
		"The harder you are hit, the harder you hit back",
		"Death follows you everywhere you go",
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
		"Stop hiding behind cover like a little coward", // i would put "bitch" here, but I don't want issues
		"Don't blame the game for your skill issue",
		"Slamming into enemies will hurt them...",
		"Fear the fighter, not the weapon",
		"There's no such thing as \"fighting dirty\", there's only fighting",
		"Thanks for playing, truly, you playing this means a lot to me. Now please continue...",
	];

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
			if ( Tips[ tipIndex ] == PrevTip ) {
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

	private void OnFadeOutFinished() {
		ProcessMode = ProcessModeEnum.Disabled;
		TimeLoading.Stop();

		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnFadeOutFinished ) );
		Hide();

		ImageChange.Stop();
		Spinner.SetProcess( false );
	}
	public void FadeOut() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnFadeOutFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
	}
	public void FadeIn() {
		Spinner.SetProcess( true );
		TimeLoading.Restart();
		ProcessMode = ProcessModeEnum.Always;

		Show();

		ImageChange.Start();

		OnImageChangeTimeout();
	}

	public override void _Ready() {
		base._Ready();

		ImageChange = GetNode<Timer>( "ImageChange" );
		ImageChange.Connect( "timeout", Callable.From( OnImageChangeTimeout ) );

		Background =  ResourceLoader.Load<PackedScene>( "res://scenes/menus/menu_background.tscn" ).Instantiate<Control>();
		Background.ZIndex = 3;
		AddChild( Background );

		TipLabel = GetNode<Label>( "Tips/TipLabel" );
		TipLabel.Show();

		Spinner = GetNode<Range>( "Tips/Spinner" );
		Spinner.SetProcess( false );
		Spinner.SetProcessInternal( false );
		Spinner.Show();

		SetProcess( false );
		SetProcessInternal( false );
	}
};