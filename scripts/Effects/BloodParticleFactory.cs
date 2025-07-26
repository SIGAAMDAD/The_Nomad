using Godot;
using System;

public partial class BloodParticleFactory : Node {
	private static BloodParticleFactory Instance = null;
	private static readonly int BloodInstanceMax = 256;
	private static readonly float BloodIntensityFactor = 0.02f;

	private Timer ReleaseTimer = null;
	private MultiMeshInstance2D MeshManager = null;

	private Transform2D[] TempBuffer = new Transform2D[ 16 ];
	private NetworkSyncObject SyncObject;

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

			float x = SyncObject.ReadFloat();
			float y = SyncObject.ReadFloat();
			MeshManager.Multimesh.SetInstanceTransform2D( MeshManager.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, new Godot.Vector2( x, y ) ) );
		}
	}
	private void NetworkSync( int offset ) {
		if ( SyncObject == null ) {
			return;
		}

		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );
		SyncObject.Write( (byte)TempBuffer.Length );
		for ( int i = 0; i < TempBuffer.Length; i++ ) {
			SyncObject.Write( TempBuffer[ i ].Origin.X );
			SyncObject.Write( TempBuffer[ i ].Origin.Y );
		}
		SyncObject.Sync();
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;

		ReleaseTimer = new Timer();
		ReleaseTimer.WaitTime = 60.0f;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );

		MeshManager = new MultiMeshInstance2D();
		MeshManager.Multimesh = new MultiMesh();
		MeshManager.Multimesh.Mesh = new QuadMesh();
		MeshManager.Texture = ResourceCache.GetTexture( "res://textures/blood1.png" );
		( MeshManager.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 8.0f, -8.0f );
		MeshManager.ZIndex = 5;
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
			SyncObject = new NetworkSyncObject( 1 + ( sizeof( float ) * 2 * 16 ) );
		}
	}

	private void CreateBloodSplatter( Vector2 from, Vector2 to ) {
		if ( MeshManager.Multimesh.VisibleInstanceCount >= BloodInstanceMax ) {
			MeshManager.Multimesh.VisibleInstanceCount = 0;
		}

		ReleaseTimer.Start();

		int start = MeshManager.Multimesh.VisibleInstanceCount;

		Godot.Vector2 direction = ( from - to ).Normalized();

		float intensity = Mathf.Clamp( BloodIntensityFactor, 0.5f, 3.0f );
		int decalCount = (int)Mathf.Clamp( intensity * 5.0f, 3.0f, 15.0f );

		Godot.Vector2 surfaceNormal = direction.Rotated( Mathf.Pi ).Normalized();

		for ( int i = 0; i < decalCount; i++ ) {
			if ( MeshManager.Multimesh.VisibleInstanceCount >= BloodInstanceMax ) {
				MeshManager.Multimesh.VisibleInstanceCount = 0;
			}

			//			Godot.Vector2 position = to;
			//			position.Y += RNJesus.IntRange( -90, 90 );
			//			position.X += RNJesus.IntRange( -140, 140 );
			Godot.Vector2 position = to + ( new Godot.Vector2( RNJesus.FloatRange( -60.0f, 60.0f ), RNJesus.FloatRange( -120.0f, 120.0f ) ) * direction );

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
		Instance.CallDeferred( MethodName.CreateBloodSplatter, from, to );
	}
};