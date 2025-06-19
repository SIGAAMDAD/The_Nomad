using Godot;
using System;

public partial class BloodParticleFactory : Node {
	private Timer ReleaseTimer = null;
	private MultiMeshInstance2D MeshManager = null;

	private static BloodParticleFactory Instance = null;
	private static readonly int BloodInstanceMax = 256;

	private Transform2D[] TempBuffer = new Transform2D[ 16 ];
	private NetworkSyncObject SyncObject = new NetworkSyncObject( 1 + ( sizeof( float ) * 2 * 16 ) );

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
		SyncObject.BeginRead( packet );

		ReleaseTimer.Start();

		if ( MeshManager.Multimesh.VisibleInstanceCount >= BloodInstanceMax ) {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
		}

		int count = SyncObject.ReadByte();
		for ( int i = 0; i < count; i++ ) {
			MeshManager.Multimesh.VisibleInstanceCount++;
			MeshManager.Multimesh.SetInstanceTransform2D( MeshManager.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, SyncObject.ReadVector2() ) );
		}
	}
	private void NetworkSync( int offset ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );
		SyncObject.Write( (byte)TempBuffer.Length );
		for ( int i = 0; i < TempBuffer.Length; i++ ) {
			SyncObject.Write( TempBuffer[ i ].Origin );
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

		for ( int i = 0; i < bloodAmount; i++ ) {
			Godot.Vector2 position = to;
			position.Y += RNJesus.FloatRange( -120.25f, 120.25f );
			position.X += RNJesus.FloatRange( -150.25f, 150.25f );
			MeshManager.Multimesh.VisibleInstanceCount++;

			TempBuffer[ i ] = new Transform2D( 0.0f, position );
			MeshManager.Multimesh.SetInstanceTransform2D( MeshManager.Multimesh.VisibleInstanceCount, TempBuffer[ i ] );
		}

		NetworkSync( start );
	}
	public static void Create( Vector2 from, Vector2 to ) {
		Instance.CreateBloodSplatter( from, to );
	}
	public static void CreateDeferred( Vector2 from, Vector2 to ) {
		Instance.CallDeferred( "CreateBloodSplatter", from, to );
	}
};