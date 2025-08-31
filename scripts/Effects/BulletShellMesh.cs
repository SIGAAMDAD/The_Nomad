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
using Renown;
using ResourceCache;
using System;
using System.Collections.Generic;
using Menus;

/*
===================================================================================

BulletShellMesh

===================================================================================
*/

public partial class BulletShellMesh : Node {
	private static readonly int BulletShellInstanceMax = 256;

	private Dictionary<string, MultiMeshInstance2D> Meshes = new Dictionary<string, MultiMeshInstance2D>( 64 );
	private readonly Dictionary<AmmoType, AudioStream[]>? BulletShellSounds = null;

	private static BulletShellMesh Instance = null;

	public BulletShellMesh() {
		BulletShellSounds = new Dictionary<AmmoType, AudioStream[]> {
			{ AmmoType.Light,
				[
					AudioCache.GetStream( "res://sounds/env/bullet_shell_0.ogg" ),
					AudioCache.GetStream( "res://sounds/env/bullet_shell_1.ogg" )
				]
			},
			{ AmmoType.Heavy,
				[
					AudioCache.GetStream( "res://sounds/env/bullet_shell_0.ogg" ),
					AudioCache.GetStream( "res://sounds/env/bullet_shell_1.ogg" )
				]
			},
			{ AmmoType.Pellets,
				[
					AudioCache.GetStream( "res://sounds/env/shotgun_shell_0.ogg" ),
					AudioCache.GetStream( "res://sounds/env/shotgun_shell_1.ogg" )
				]
			}
		};
	}

	/*
	===============
	SendUpdate
	===============
	*/
	private void SendUpdate( Godot.Vector2 position, string ammo ) {
		using var writer = new Multiplayer.NetworkWriter( 256, Steamworks.Constants.k_nSteamNetworkingSend_Reliable );

		writer.WriteUInt8( (byte)Steam.SteamLobby.MessageType.GameData );
		writer.WriteInt32( GetPath().GetHashCode() );
		writer.WriteString( ammo );
		writer.WriteVector2( position );
	}

	/*
	===============
	ReceivePacket
	===============
	*/
	private void ReceivePacket( System.IO.BinaryReader packet ) {
		using var reader = new Multiplayer.NetworkReader( packet );

		string ammo = reader.ReadString();

		if ( !Meshes.TryGetValue( ammo, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( ammo );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= BulletShellInstanceMax ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}

		instance.Multimesh.VisibleInstanceCount++;
		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, reader.ReadVector2() ) );
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

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			Steam.SteamLobby.Instance.AddNetworkNode( GetPath(), new Steam.SteamLobby.NetworkNode( this, null, ReceivePacket ) );
		}

		Instance = this;
	}

	/*
	===============
	AddMesh
	===============
	*/
	private MultiMeshInstance2D AddMesh( string ammo ) {
		Resource? ammoType =  ItemCache.GetItem( ammo )  ?? throw new ArgumentException( $"AmmoType {ammo} couldn't be found" );

		MultiMeshInstance2D meshInstance = new MultiMeshInstance2D();
		meshInstance.Multimesh = new MultiMesh();
		meshInstance.Multimesh.Mesh = new QuadMesh();
		( meshInstance.Multimesh.Mesh as QuadMesh ).Size = new Vector2( 10.0f, -4.0f );
		meshInstance.Texture = (Texture2D)ammoType.Get( "properties" ).AsGodotDictionary()[ "casing_icon" ];
		meshInstance.Multimesh.InstanceCount = BulletShellInstanceMax;
		meshInstance.Multimesh.VisibleInstanceCount = 0;
		meshInstance.ZIndex = 5;
		meshInstance.Show();
		AddChild( meshInstance );
		Meshes.Add( ammo, meshInstance );

		return meshInstance;
	}

	/*
	===============
	OnTimerTimeout
	===============
	*/
	private static void OnTimerTimeout( Timer timer, Entity from, string ammo ) {
		Resource? ammoType =  ItemCache.GetItem( ammo )  ?? throw new ArgumentException( $"AmmoType {ammo} couldn't be found" );
		if ( !Instance.BulletShellSounds.TryGetValue( (AmmoType)ItemCache.GetItem( ammo ).Get( "properties" ).AsGodotDictionary()[ "type" ].AsInt32(), out AudioStream[]? streams ) ) {
			throw new Exception( $"Ammo object {ammo} doesn't have a valid AmmoType" );	
		}

		AudioStreamPlayer2D player = new AudioStreamPlayer2D() {
			Stream = streams[ RNJesus.IntRange( 0, streams.Length - 1 ) ],
			VolumeDb = SettingsData.GetEffectsVolumeLinear(),
		};
		player.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( () => { from.CallDeferred( MethodName.RemoveChild, player ); player.CallDeferred( MethodName.QueueFree ); } ) );

		from.AddChild( player );
		player.Play();

		Instance.RemoveChild( timer );
		timer.QueueFree();
	}

	/*
	===============
	AddShellInternal
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="from"></param>
	/// <param name="ammoID"></param>
	private void AddShellInternal( Entity from, string ammoID ) {
		ArgumentNullException.ThrowIfNull( from );
		ArgumentException.ThrowIfNullOrEmpty( ammoID );

		if ( !Meshes.TryGetValue( ammoID, out MultiMeshInstance2D instance ) ) {
			instance = Instance.AddMesh( ammoID );
		}
		if ( instance.Multimesh.VisibleInstanceCount >= BulletShellInstanceMax ) {
			instance.Multimesh.VisibleInstanceCount = 0;
		}
		instance.Multimesh.VisibleInstanceCount++;

		instance.Multimesh.SetInstanceTransform2D( instance.Multimesh.VisibleInstanceCount, new Transform2D( 0.0f, from.GlobalPosition ) );
		SendUpdate( from.GlobalPosition, ammoID );

		Timer timer = new Timer();
		timer.WaitTime = 0.15f;
		timer.OneShot = true;
		timer.Connect( Timer.SignalName.Timeout, Callable.From( () => OnTimerTimeout( timer, from, ammoID ) ) );
		Instance.AddChild( timer );
		timer.Start();
	}

	/*
	===============
	AddShellDeferred
	===============
	*/
	/// <summary>
	/// Creates a bullet shell sprite at the entity's position and plays a sound of the brass dropping
	/// </summary>
	/// <param name="from">The entity</param>
	/// <param name="ammoID">The ItemDefinition item_id from an Ammo object</param>
	/// <seealso cref="AddShell"/>
	public static void AddShellDeferred( Entity from, string ammoID ) {
		Instance.CallDeferred( MethodName.AddShellInternal, from, ammoID );
	}

	/*
	===============
	AddShell
	===============
	*/
	/// <summary>
	/// Creates a bullet shell sprite at the entity's position and plays a sound of the brass dropping
	/// </summary>
	/// <param name="from">The entity</param>
	/// <param name="ammoID">The ItemDefinition item_id from an Ammo object</param>
	public static void AddShell( Entity from, string ammoID ) {
		Instance.AddShellInternal( from, ammoID );
	}
};