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

public partial class DinoRun : CanvasLayer {
	private Timer CactusTimer;
	private Area2D Dino;
	private Label ScoreLabel;
	private Sprite2D Ground;

	private Vector2 Velocity = Vector2.Zero;
	private float Gravity = 30.0f;
	private float JumpForce = -450.0f;
	private uint Score = 0;
	private bool Active = false;

	private PackedScene Cactus;

	private void OnVisibilityChanged() {
	}
	private void OnDinoAreaEntered( Rid bodyRid, Node2D body, int localShapeIndex, int bodyShapeIndex ) {
		if ( body.IsInGroup( "DinoCactus" ) ) {
			Active = false;
			ScoreLabel.Text = "GAME OVER (SCORE: " + Score.ToString() + ")";
			CactusTimer.Stop();
		}
	}
	private void OnCactusTimerTimeout() {
		if ( !Active ) {
			return;
		}

		CactusTimer.WaitTime = GD.RandRange( 1.0f, 2.5f );
		CactusTimer.Start();

		Area2D cacti = Cactus.Instantiate<Area2D>();
		cacti.Position = new Vector2( 1200.0f, Ground.Position.Y - 30.0f );
		AddChild( cacti );
	}

	public override void _Ready() {
		base._Ready();

		Dino = GetNode<Area2D>( "Dino" );
		Dino.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnDinoAreaEntered ) );

		Ground = GetNode<Sprite2D>( "Ground" );

		ScoreLabel = GetNode<Label>( "TextLabel" );

		CactusTimer = new Timer();
		CactusTimer.WaitTime = GD.RandRange( 1.0f, 2.5f );
		CactusTimer.OneShot = true;
		CactusTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnCactusTimerTimeout ) );
		AddChild( CactusTimer );

		Cactus = ResourceLoader.Load<PackedScene>( "res://scenes/menus/dino_cactus.tscn" );

		Score = 0;
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( !Active ) {
			CactusTimer.Start();
			Active = true;
		}

		if ( Input.IsActionJustPressed( "dino_jump" ) ) {
			Velocity.Y = JumpForce;
		}
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( !Active ) {
			return;
		}

		Score += System.Convert.ToUInt32( delta );
		ScoreLabel.Text = "SCORE: " + Score.ToString();
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( !Active ) {
			return;
		}

		Velocity.Y += Gravity;
		Dino.Position += Velocity * (float)delta;

		if ( Dino.Position.Y > Ground.Position.Y - 30.0f ) {
			Dino.Position = new Vector2( Ground.Position.X, Ground.Position.Y - 30.0f );
			Velocity.Y = 0.0f;
		}
	}
};