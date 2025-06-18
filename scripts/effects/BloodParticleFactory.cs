using Godot;
using System;
using System.Diagnostics;

public partial class BloodParticleFactory : Node {
	private Timer ReleaseTimer = null;
	private MultiMeshInstance2D MeshManager = null;

	private static BloodParticleFactory Instance = null;
	private static readonly int BloodInstanceMax = 256;

	private NetworkWriter SyncObject = new NetworkWriter( 2048 );

	private void OnReleaseTimerTimeout() {
		int instanceCount = MeshManager.Multimesh.VisibleInstanceCount - 24;
		if ( instanceCount < 0 ) {
			instanceCount = 0;
		}
		MeshManager.Multimesh.VisibleInstanceCount = instanceCount;
		if ( Instance.MeshManager.Multimesh.VisibleInstanceCount > 0 ) {
			ReleaseTimer.Start();
		}
	}

	private void ReceivePacket( System.IO.BinaryReader packet ) {
		ReleaseTimer.Start();

		if ( MeshManager.Multimesh.VisibleInstanceCount >= BloodInstanceMax ) {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
		}

		int count = packet.ReadByte();
		for ( int i = 0; i < count; i++ ) {
			Godot.Vector2 position = new Godot.Vector2(
				(float)packet.ReadHalf(),
				(float)packet.ReadHalf()
			);
			MeshManager.Multimesh.SetInstanceTransform2D( MeshManager.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, position ) );
		}
	}
	private void NetworkSync( int offset, int count, Span<Transform2D> positions ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );
		SyncObject.Write( (byte)count );
		for ( int i = 0; i < count; i++ ) {
			SyncObject.Write( positions[ i ].Origin );
		}
		SyncObject.Sync();
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;

		ReleaseTimer = new Timer();
		ReleaseTimer.WaitTime = 32.0f;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.Connect( "timeout", Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );

		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Multimesh.Mesh = new QuadMesh();
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/blood1.png" );
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 8.0f, -8.0f );
		MeshManager.ZIndex = 10;
		AddChild( MeshManager );

		// cache a shitload
		MeshManager.Multimesh.InstanceCount = BloodInstanceMax;
		MeshManager.Multimesh.VisibleInstanceCount = 0;

		// clean up on respawn
		LevelData.Instance.PlayerRespawn += () => {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
			ReleaseTimer.Stop();
		};

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
		}
	}

	private void CreateBloodSplatter( Vector2 from, Vector2 to ) {
		int bloodAmount = 16;

		if ( MeshManager.Multimesh.VisibleInstanceCount >= BloodInstanceMax ) {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
		}

		ReleaseTimer.Start();

		int start = MeshManager.Multimesh.VisibleInstanceCount;

		Span<Transform2D> positions = stackalloc Transform2D[ bloodAmount ];
		for ( int i = 0; i < bloodAmount; i++ ) {
			Godot.Vector2 position = to;
			position.Y += RNJesus.FloatRange( -120.25f, 120.25f );
			position.X += RNJesus.FloatRange( -150.25f, 150.25f );
			MeshManager.Multimesh.VisibleInstanceCount++;

			positions[ i ] = new Transform2D( 0.0f, position );
			MeshManager.Multimesh.SetInstanceTransform2D( MeshManager.Multimesh.VisibleInstanceCount, positions[ i ] );
		}

		NetworkSync( start, bloodAmount, positions );
	}
	public static void Create( Vector2 from, Vector2 to ) {
		Instance.CreateBloodSplatter( from, to );
	}
	public static void CreateDeferred( Vector2 from, Vector2 to ) {
		Instance.CallDeferred( "CreateBloodSplatter", from, to );
	}
};