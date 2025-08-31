using Godot;
using Renown;
using System.Collections.Generic;

public partial class BulletShellMesh : Node {
	private Dictionary<Resource, MultiMeshInstance2D> Meshes = new Dictionary<Resource, MultiMeshInstance2D>( 64 );

	private static BulletShellMesh Instance = null;
	private static readonly int BulletShellInstanceMax = 256;

	private NetworkSyncObject SyncObject;

	private void SendUpdate( Godot.Vector2 position, Resource ammo ) {
		if ( SyncObject == null ) {
			return;
		}
		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );
		SyncObject.Write( (string)ammo.Get( "id" ) );
		SyncObject.Write( position );
		SyncObject.Sync();
	}
	private void ReceivePacket( System.IO.BinaryReader packet ) {
		SyncObject.BeginRead( packet );

		Resource ammo = (Resource)ResourceCache.ItemDatabase.Call( "get_item", SyncObject.ReadString() );

		if ( !Meshes.TryGetValue( ammo, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( ammo );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= BulletShellInstanceMax ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}

		instance.Multimesh.VisibleInstanceCount++;
		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, SyncObject.ReadVector2() ) );
	}

	public override void _Ready() {
		base._Ready();

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			SyncObject = new NetworkSyncObject( 256 );
		}

		Instance = this;
	}
	private MultiMeshInstance2D AddMesh( Resource ammo ) {
		MultiMeshInstance2D meshInstance = new MultiMeshInstance2D();
		meshInstance.Multimesh = new MultiMesh();
		meshInstance.Multimesh.Mesh = new QuadMesh();
		( meshInstance.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 10.0f, -4.0f );
		meshInstance.Texture = (Texture2D)( (Godot.Collections.Dictionary)ammo.Get( "properties" ) )[ "casing_icon" ];
		meshInstance.Multimesh.InstanceCount = BulletShellInstanceMax;
		meshInstance.Multimesh.VisibleInstanceCount = 0;
		meshInstance.ZIndex = 5;
		meshInstance.Show();
		AddChild( meshInstance );
		Meshes.Add( ammo, meshInstance );

		return meshInstance;
	}
	private static void OnTimerTimeout( Timer timer, Entity from, Resource ammo ) {
		AudioStream stream = (AmmoType)(uint)( (Godot.Collections.Dictionary)ammo.Get( "properties" ) )[ "type" ] switch {
			AmmoType.Light => ResourceCache.BulletShell[ RNJesus.IntRange( 0, ResourceCache.BulletShell.Length - 1 ) ],
			AmmoType.Heavy => ResourceCache.BulletShell[ RNJesus.IntRange( 0, ResourceCache.BulletShell.Length - 1 ) ],
			AmmoType.Pellets => ResourceCache.ShotgunShell[ RNJesus.IntRange( 0, ResourceCache.ShotgunShell.Length - 1 ) ],
			_ => null
		};

		AudioStreamPlayer2D player = new AudioStreamPlayer2D();
		player.Stream = stream;
		player.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		player.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( () => { from.CallDeferred( MethodName.RemoveChild, player ); player.CallDeferred( MethodName.QueueFree ); } ) );
		from.AddChild( player );
		player.Play();
		Instance.RemoveChild( timer );
		timer.QueueFree();
	}
	private void AddShellInternal( Entity from, Resource ammo ) {
		if ( !Meshes.TryGetValue( ammo, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( ammo );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= BulletShellInstanceMax ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}
		instance.Multimesh.VisibleInstanceCount++;

		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, from.GlobalPosition ) );
		SendUpdate( from.GlobalPosition, ammo );

		Timer timer = new Timer();
		timer.WaitTime = 0.15f;
		timer.OneShot = true;
		timer.Connect( Timer.SignalName.Timeout, Callable.From( () => OnTimerTimeout( timer, from, ammo ) ) );
		Instance.AddChild( timer );
		timer.Start();
	}
	public static void AddShellDeferred( Entity from, Resource ammo ) {
		Instance.CallDeferred( MethodName.AddShellInternal, from, ammo );
	}
	public static void AddShell( Entity from, Resource ammo ) {
		Instance.AddShellInternal( from, ammo );
	}
};