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
using System.Diagnostics;
using Menus;

/*
===================================================================================

Poem

===================================================================================
*/

public partial class Poem : Control {
	private static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private static readonly Color HiddenColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );

	private bool Loading = false;

	private Label Author;
	private Label PressEnter;
	private Timer[] Timers;
	private Label[] Labels;
	private int CurrentTimer = 0;

	/*
	===============
	OnTimerTimeout
	===============
	*/
	private void OnTimerTimeout() {
		AdvanceTimer();
	}

	/*
	===============
	OnTransitionFinished
	===============
	*/
	private void OnTransitionFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnTransitionFinished ) );

		GameConfiguration.GameMode = GameMode.SinglePlayer;

		Console.PrintLine( "Loading game..." );

		World.LoadTime = Stopwatch.StartNew();

		Hide();
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://levels/world.tscn" );

		/*
		if ( SettingsData.GetNetworkingEnabled() ) {
			Console.PrintLine( "Networking enabled, creating co-op lobby..." );

			GameConfiguration.GameMode = GameMode.Online;

			SteamLobby.Instance.SetMaxMembers( 4 );
			string name = SteamManager.GetSteamName();
			if ( name[ name.Length - 1 ] == 's' ) {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
			} else {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
			}

			SteamLobby.Instance.CreateLobby();
		} else {
			GameConfiguration.GameMode = GameMode.SinglePlayer;
		}
		*/
	}

	/*
	===============
	AdvanceTimer
	===============
	*/
	private void AdvanceTimer() {
		if ( Loading ) {
			return;
		}
		if ( CurrentTimer >= Labels.Length ) {
			Loading = true;
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnTransitionFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
			return;
		}

		CurrentTimer++;
		if ( CurrentTimer < Timers.Length ) {
			Tween FadeTween = CreateTween();
			FadeTween.TweenProperty( Labels[ CurrentTimer ], "modulate", DefaultColor, 1.5f );

			Timers[ CurrentTimer ].Start();
		} else {
			Tween FadeTween = CreateTween();
			FadeTween.TweenProperty( Author, "modulate", DefaultColor, 1.5f );
			FadeTween.TweenProperty( PressEnter, "modulate", DefaultColor, 0.9f );
		}
	}

	/*
	===============
	_Ready

	godot initialization override
	===============
	*/
	public override void _Ready() {
		Theme = AccessibilityManager.DefaultTheme;

		base._Ready();

		Timers = [
			GetNode<Timer>( "VBoxContainer/Label/Timer1" ),
			GetNode<Timer>( "VBoxContainer/Label2/Timer2" ),
			GetNode<Timer>( "VBoxContainer/Label3/Timer3" ),
			GetNode<Timer>( "VBoxContainer/Label4/Timer4" ),
			GetNode<Timer>( "VBoxContainer/Label5/Timer5" )
		];
		Labels = [
			GetNode<Label>( "VBoxContainer/Label" ),
			GetNode<Label>( "VBoxContainer/Label2" ),
			GetNode<Label>( "VBoxContainer/Label3" ),
			GetNode<Label>( "VBoxContainer/Label4" ),
			GetNode<Label>( "VBoxContainer/Label5" )
		];

		for ( int i = 0; i < Timers.Length; i++ ) {
			Timers[ i ].Connect( Timer.SignalName.Timeout, Callable.From( OnTimerTimeout ) );
		}

		Tween FadeTween = CreateTween();
		FadeTween.TweenProperty( Labels[ CurrentTimer ], "modulate", DefaultColor, 1.5f );

		Author = GetNode<Label>( "VBoxContainer/AuthorName" );
		PressEnter = GetNode<Label>( "VBoxContainer/PressEnter" );
	}

	/*
	===============
	_UnhandledInput
	===============
	*/
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustPressed( "ui_advance" ) ) {
			CallDeferred( MethodName.AdvanceTimer );
		}
	}
};