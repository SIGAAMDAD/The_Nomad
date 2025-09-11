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
using Steam;
using ResourceCache;

/*
===================================================================================

BloodParticleFactory

===================================================================================
*/
/// <summary>
/// The singleton autoload responsible for managing blood splatter effects
/// </summary>
/// <remarks>
/// Is thread safe only if called into using <b>BloodParticleFactory.CreateDeferred</b>
/// </remarks>

public partial class BloodParticleFactory : Node {
	private static readonly int BLOOD_INSTANCE_MAX = 256;
	private static readonly int PARTICLE_BUFFER_SIZE = 16;
	private static readonly float RELEASE_DELAY = 60.0f;
	private static readonly int CLEANUP_THRESHOLD = 24;

	private Timer ReleaseTimer;
	private MultiMeshInstance2D MeshManager;
	private Multiplayer.NetworkSyncObject SyncObject;

	private readonly Transform2D[] TransformBuffer = new Transform2D[ PARTICLE_BUFFER_SIZE ];

	private static BloodParticleFactory Instance;

	/*
	===============
	Create
	===============
	*/
	/// <summary>
	/// Create a blood splatter
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public static void Create( Vector2 from, Vector2 to ) {
		Instance.CreateBloodSplatter( in from, in to );
	}

	/*
	===============
	CreateDeffered
	===============
	*/
	/// <summary>
	/// Create a blood splatter, but on a separate thread, this automatically queues up a Godot API deferred call
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <seealso cref="Create"/>
	public static void CreateDeferred( in Vector2 from, in Vector2 to ) {
		Instance.CallDeferred( MethodName.Create, from, to );
	}

	/*
	===============
	CreateBloodSplatter
	===============
	*/
	private void CreateBloodSplatter( in Vector2 from, in Vector2 to ) {
		MultiMesh multimesh = MeshManager.Multimesh;
		int currentCount = multimesh.VisibleInstanceCount;

		// Reset if we can't fit new batch
		if ( currentCount + PARTICLE_BUFFER_SIZE > BLOOD_INSTANCE_MAX ) {
			multimesh.VisibleInstanceCount = 0;
			currentCount = 0;
		}

		Vector2 direction = ( from - to ).Normalized();
		Vector2 basePosition = to;

		for ( int i = 0; i < PARTICLE_BUFFER_SIZE; i++ ) {
			Vector2 offset = new Vector2(
				RNJesus.FloatRange( -60f, 60f ),
				RNJesus.FloatRange( -120f, 120f )
			) * direction;

			int targetIndex = currentCount + i;
			Transform2D transform = new Transform2D( 0f, basePosition + offset );

			multimesh.SetInstanceTransform2D( targetIndex, transform );
			TransformBuffer[ i ] = transform;
		}

		multimesh.VisibleInstanceCount = currentCount + PARTICLE_BUFFER_SIZE;
		ReleaseTimer.Start();

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			NetworkSync();
		}
	}

	/*
	===============
	NetworkSync
	===============
	*/
	private void NetworkSync() {
		if ( SyncObject == null ) {
			return;
		}

		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );

		for ( int i = 0; i < PARTICLE_BUFFER_SIZE; i++ ) {
			SyncObject.Write( TransformBuffer[ i ].Origin.X );
			SyncObject.Write( TransformBuffer[ i ].Origin.Y );
		}

		SyncObject.Sync();
	}

	/*
	===============
	ReceivePacket
	===============
	*/
	private void ReceivePacket( System.IO.BinaryReader packet ) {
		SyncObject.BeginRead( packet );
		ReleaseTimer.Start();

		MultiMesh multimesh = MeshManager.Multimesh;
		int currentCount = multimesh.VisibleInstanceCount;
		int count = Mathf.Min( SyncObject.ReadByte(), PARTICLE_BUFFER_SIZE );

		// Reset if needed before adding new particles
		if ( currentCount + count > BLOOD_INSTANCE_MAX ) {
			multimesh.VisibleInstanceCount = 0;
			currentCount = 0;
		}

		for ( int i = 0; i < count; i++ ) {
			Vector2 position = new Vector2(
				SyncObject.ReadFloat(),
				SyncObject.ReadFloat()
			);

			int targetIndex = currentCount + i;
			multimesh.SetInstanceTransform2D( targetIndex, new Transform2D( 0.0f, position ) );
		}

		multimesh.VisibleInstanceCount = currentCount + count;
	}

	/*
	===============
	OnReleaseTimerTimeout
	===============
	*/
	private void OnReleaseTimerTimeout() {
		int newCount = Math.Max( MeshManager.Multimesh.VisibleInstanceCount - CLEANUP_THRESHOLD, 0 );
		MeshManager.Multimesh.VisibleInstanceCount = newCount;

		if ( newCount > 0 ) {
			ReleaseTimer.Start();
		}
	}

	/*
	===============
	ResetParticles
	===============
	*/
	private void ResetParticles() {
		MeshManager.Multimesh.VisibleInstanceCount = 0;
		ReleaseTimer.Stop();
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

		ReleaseTimer = new Timer();
		ReleaseTimer.WaitTime = RELEASE_DELAY;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );

		MeshManager = new MultiMeshInstance2D() {
			Texture = TextureCache.GetTexture( "res://textures/blood1.dds" ),
			ZIndex = 10,
			Multimesh = new MultiMesh() {
				InstanceCount = BLOOD_INSTANCE_MAX,
				VisibleInstanceCount = 0,
				Mesh = new QuadMesh() {
					Size = new Vector2( 8.0f, -8.0f )
				}
			}
		};
		AddChild( MeshManager );

		GameEventBus.Subscribe<LevelData.PlayerRespawnEventHandler>( this, ResetParticles );

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			SyncObject = new Multiplayer.NetworkSyncObject( 1 + sizeof( float ) * 2 * PARTICLE_BUFFER_SIZE );
		}
	}
};