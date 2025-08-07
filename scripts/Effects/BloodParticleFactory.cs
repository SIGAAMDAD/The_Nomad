using Godot;
using System;

public partial class BloodParticleFactory : Node {
	private static BloodParticleFactory Instance;
	private const int BloodInstanceMax = 256;
	private const int ParticleBufferSize = 16;
	private const float ReleaseDelay = 60.0f;
	private const int CleanupThreshold = 24;

	private Timer ReleaseTimer;
	private MultiMeshInstance2D MeshManager;
	private NetworkSyncObject SyncObject;
	private readonly Transform2D[] TransformBuffer = new Transform2D[ ParticleBufferSize ];

	public override void _Ready() {
		base._Ready();

		Instance = this;

		ReleaseTimer = new Timer();
		ReleaseTimer.WaitTime = ReleaseDelay;
		ReleaseTimer.OneShot = true;
		ReleaseTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReleaseTimerTimeout ) );
		AddChild( ReleaseTimer );

		MeshManager = new MultiMeshInstance2D {
			Multimesh = new MultiMesh {
				Mesh = new QuadMesh { Size = new Vector2( 8f, -8f ) },
				InstanceCount = BloodInstanceMax,
				VisibleInstanceCount = 0
			},
			Texture = ResourceLoader.Load<Texture2D>( "res://textures/blood1.dds" ),
			ZIndex = 5
		};
		AddChild( MeshManager );

		LevelData.Instance.PlayerRespawn += ResetParticles;

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			SyncObject = new NetworkSyncObject( 1 + sizeof( float ) * 2 * ParticleBufferSize );
		}
	}

	private void CreateBloodSplatter( Vector2 from, Vector2 to ) {
		var multimesh = MeshManager.Multimesh;
		int currentCount = multimesh.VisibleInstanceCount;

		// Reset if we can't fit new batch
		if ( currentCount + ParticleBufferSize > BloodInstanceMax ) {
			multimesh.VisibleInstanceCount = 0;
			currentCount = 0;
		}

		Vector2 direction = ( from - to ).Normalized();
		Vector2 basePosition = to;

		for ( int i = 0; i < ParticleBufferSize; i++ ) {
			Vector2 offset = new Vector2(
				RNJesus.FloatRange( -60f, 60f ),
				RNJesus.FloatRange( -120f, 120f )
			) * direction;

			int targetIndex = currentCount + i;
			Transform2D transform = new Transform2D( 0f, basePosition + offset );

			multimesh.SetInstanceTransform2D( targetIndex, transform );
			TransformBuffer[ i ] = transform;
		}

		multimesh.VisibleInstanceCount = currentCount + ParticleBufferSize;
		ReleaseTimer.Start();

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			NetworkSync();
		}
	}

	private void NetworkSync() {
		if ( SyncObject == null ) {
			return;
		}

		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );

		for ( int i = 0; i < ParticleBufferSize;  i++ ) {
			SyncObject.Write( TransformBuffer[ i ].Origin.X );
			SyncObject.Write( TransformBuffer[ i ].Origin.Y );
		}

		SyncObject.Sync();
	}

	private void ReceivePacket( System.IO.BinaryReader packet ) {
		SyncObject.BeginRead( packet );
		ReleaseTimer.Start();

		MultiMesh multimesh = MeshManager.Multimesh;
		int currentCount = multimesh.VisibleInstanceCount;
		int count = Mathf.Min( SyncObject.ReadByte(), ParticleBufferSize );

		// Reset if needed before adding new particles
		if ( currentCount + count > BloodInstanceMax ) {
			multimesh.VisibleInstanceCount = 0;
			currentCount = 0;
		}

		for ( int i = 0; i < count; i++ ) {
			Vector2 position = new Vector2(
				SyncObject.ReadFloat(),
				SyncObject.ReadFloat()
			);

			int targetIndex = currentCount + i;
			multimesh.SetInstanceTransform2D( targetIndex, new Transform2D( 0f, position ) );
		}

		multimesh.VisibleInstanceCount = currentCount + count;
	}

	private void OnReleaseTimerTimeout() {
		int newCount = Math.Max( MeshManager.Multimesh.VisibleInstanceCount - CleanupThreshold, 0 );
		MeshManager.Multimesh.VisibleInstanceCount = newCount;

		if ( newCount > 0 ) {
			ReleaseTimer.Start();
		}
	}

	private void ResetParticles() {
		MeshManager.Multimesh.VisibleInstanceCount = 0;
		ReleaseTimer.Stop();
	}

	public static void Create( Vector2 from, Vector2 to ) => Instance.CreateBloodSplatter( from, to );
	public static void CreateDeferred( Vector2 from, Vector2 to ) => Instance.CallDeferred( MethodName.CreateBloodSplatter, from, to );
};