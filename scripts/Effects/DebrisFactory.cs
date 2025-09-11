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
using ResourceCache;

/*
===================================================================================

DebrisFactory

===================================================================================
*/

public partial class DebrisFactory : Node {
	private static readonly int INSTANCE_COUNT = 4096;

	private Timer ReleaseTimer = null;
	private MultiMeshInstance2D MeshManager = null;

	private Vector2[] Speeds = null;
	private Transform2D[] Transforms = null;
	private Color[] Colors = null;

	private static DebrisFactory Instance = null;

	/*
	===============
	Create
	===============
	*/
	public static void Create( in Vector2 position ) {
		const int numSmokeClouds = 8;

		int instanceCount = Instance.MeshManager.Multimesh.VisibleInstanceCount;
		int startIndex = instanceCount;

		instanceCount += numSmokeClouds;
		if ( instanceCount > Instance.MeshManager.Multimesh.InstanceCount ) {
			instanceCount = numSmokeClouds;
			startIndex = 0;
		}

		if ( Instance.ReleaseTimer.IsStopped() ) {
			Instance.ReleaseTimer.Start();
		}

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			Instance.Speeds[ startIndex + i ] = new Vector2( RNJesus.FloatRange( -2.5f, 2.5f ), RNJesus.FloatRange( -0.75f, 0.75f ) );
			Instance.Transforms[ startIndex + i ] = new Transform2D( 0.0f, position );
			Instance.Colors[ startIndex + i ].A = 1.0f;
			Instance.MeshManager.Multimesh.VisibleInstanceCount++;
			Instance.MeshManager.Multimesh.SetInstanceTransform2D( startIndex + i, Instance.Transforms[ i ] );
		}
		Instance.MeshManager.Multimesh.InstanceCount = instanceCount;
	}

	/*
	===============
	OnReleaseTimerTimeout
	===============
	*/
	private void OnReleaseTimerTimeout() {
		int instanceCount = MeshManager.Multimesh.VisibleInstanceCount - 1;
		if ( instanceCount < 0 ) {
			instanceCount = 0;
		}
		MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
		if ( Instance.MeshManager.Multimesh.VisibleInstanceCount > 0 ) {
			ReleaseTimer.Start();
		}
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

		Instance = this;

		ReleaseTimer = new Timer() {
			Name = nameof( ReleaseTimer ),
			OneShot = true,
			WaitTime = 3.5f
		};
		GameEventBus.ConnectSignal( ReleaseTimer, Timer.SignalName.Timeout, this, OnReleaseTimerTimeout );
		AddChild( ReleaseTimer );

		MeshManager = new MultiMeshInstance2D() {
			Name = nameof( MeshManager ),
			Texture = TextureCache.GetTexture( "res://textures/env/dustcloud.png" ),
			ZIndex = 10,
			Multimesh = new MultiMesh() {
				InstanceCount = INSTANCE_COUNT,
				VisibleInstanceCount = 0,
				Mesh = new QuadMesh() {
					Size = new Vector2( 32.0f, -32.0f ) 
				}
			}
		};
		AddChild( MeshManager );

		Speeds = new Vector2[ MeshManager.Multimesh.InstanceCount ];
		Transforms = new Transform2D[ MeshManager.Multimesh.InstanceCount ];
		Colors = new Color[ MeshManager.Multimesh.InstanceCount ];

		for ( int i = 0; i < Colors.Length; i++ ) {
			Colors[ i ] = new Color( 1.0f, 0.25f, 0.0f, 1.0f );
		}
	}

	/*
	===============
	_Process
	===============
	*/
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		base._Process( delta );

		for ( int i = 0; i < Instance.MeshManager.Multimesh.VisibleInstanceCount; i++ ) {
			if ( Speeds[ i ].X > 0.0f ) {
				Speeds[ i ].X -= 0.0025f;
				if ( Speeds[ i ].X < 0.0f ) {
					Speeds[ i ].X = 0.0f;
				}
			} else if ( Speeds[ i ].X < 0.0f ) {
				Speeds[ i ].X += 0.0025f;
				if ( Speeds[ i ].X > 0.0f ) {
					Speeds[ i ].X = 0.0f;
				}
			}
			if ( Speeds[ i ].Y > 0.0f ) {
				Speeds[ i ].Y -= 0.0025f;
				if ( Speeds[ i ].Y < 0.0f ) {
					Speeds[ i ].Y = 0.0f;
				}
			} else if ( Speeds[ i ].Y < 0.0f ) {
				Speeds[ i ].Y += 0.0025f;
				if ( Speeds[ i ].Y > 0.0f ) {
					Speeds[ i ].Y = 0.0f;
				}
			}
			Transforms[ i ].Origin += Speeds[ i ];
			Colors[ i ].A -= 0.25f * (float)delta;

			MeshManager.Multimesh.SetInstanceColor( i, Colors[ i ] );
			MeshManager.Multimesh.SetInstanceTransform2D( i, Transforms[ i ] );
		}
	}
};