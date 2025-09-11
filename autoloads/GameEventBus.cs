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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Util;

/*
===================================================================================

GameEventBus

===================================================================================
*/
/// <summary>
/// 
/// </summary>

public partial class GameEventBus : Node {
	private readonly struct ConnectionInfo {
		public readonly GodotObject Source;
		public readonly StringName SignalName;
		public readonly Callable Callable;

		public ConnectionInfo( GodotObject source, StringName signalName, Callable callable ) {
			Source = source;
			SignalName = signalName;
			Callable = callable;
		}
	};
	private readonly struct SubscriptionInfo {
		public readonly Type EventType;
		public readonly Delegate EventHandler;

		public SubscriptionInfo( Type eventType, Delegate eventHandler ) {
			EventType = eventType;
			EventHandler = eventHandler;
		}
	};

	private static ObjectPool<ConnectionInfo> ConnectionPool = new ObjectPool<ConnectionInfo>();
	private static ObjectPool<SubscriptionInfo> SubscriptionPool = new ObjectPool<SubscriptionInfo>();

	private static readonly ConcurrentDictionary<GodotObject, List<ConnectionInfo>> Connections = new ConcurrentDictionary<GodotObject, List<ConnectionInfo>>();
	private static readonly ConcurrentDictionary<object, List<SubscriptionInfo>> Subscriptions = new ConcurrentDictionary<object, List<SubscriptionInfo>>();
	private static readonly ConcurrentDictionary<string, Callable> CachedCallables = new ConcurrentDictionary<string, Callable>();
	private static readonly ConcurrentDictionary<Type, Delegate> SubscriptionCache = new ConcurrentDictionary<Type, Delegate>();

	//
	// Global events
	//

	public static event Action<Renown.World.WorldArea> PlayerEnteredArea;
	public static event Action<Renown.World.WorldArea> PlayerExitedArea;
	public static event Action<Renown.World.WorldArea, int> PlayerRenownScoreChanged;
	public static event Action<Renown.World.WorldArea, Renown.Trait, float> PlayerTraitScoreChanged;

	/*
	===============
	EmitSignalPlayerEnteredArea
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void EmitSignalPlayerEnteredArea( Renown.World.WorldArea area ) {
		ArgumentNullException.ThrowIfNull( area );
		PlayerEnteredArea.Invoke( area );
	}

	/*
	===============
	EmitSignalPlayerExitedArea
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void EmitSignalPlayerExitedArea( Renown.World.WorldArea area ) {
		ArgumentNullException.ThrowIfNull( area );
		PlayerExitedArea.Invoke( area );
	}

	/*
	===============
	EmitSignalPlayerRenownScoreChanged
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void EmitSignalPlayerRenownScoreChanged( Renown.World.WorldArea area, int amount ) {
		ArgumentNullException.ThrowIfNull( area );
		PlayerRenownScoreChanged.Invoke( area, amount );
	}

	/*
	===============
	EmitSignalPlayerTraitScoreChanged
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void EmitSignalPlayerTraitScoreChanged( Renown.World.WorldArea area, Renown.Trait trait, float amount ) {
		ArgumentNullException.ThrowIfNull( area );
		PlayerTraitScoreChanged.Invoke( area, trait, amount );
	}

	/*
	===============
	ConnectSignal
	===============
	*/
	public static void ConnectSignal( GodotObject source, StringName signalName, GodotObject target, Action method ) {
		if ( !source.HasSignal( signalName ) ) {
			throw new InvalidOperationException( $"GodotObject {source.GetType().FullName} doesn't have signal {signalName}" );
		}

		string callableKey = $"{target.GetInstanceId()}:{method.Method.Name}";
		if ( !CachedCallables.TryGetValue( callableKey, out Callable callable ) ) {
			callable = Callable.From( method );
			CachedCallables[ callableKey ] = callable;
		}

		source.Connect( signalName, callable );
		if ( !Connections.TryGetValue( target, out List<ConnectionInfo> connectionList ) ) {
			connectionList = new List<ConnectionInfo>();
			Connections[ target ] = connectionList;
		}

		connectionList.Add( new ConnectionInfo(
			source: source,
			signalName: signalName,
			callable: callable
		) );

		if ( target is Node targetNode ) {
			targetNode.Connect( Node.SignalName.TreeExiting, Callable.From( () => DisconnectAllForObject( target ) ) );
		}
	}

	/*
	===============
	ConnectSignal
	===============
	*/
	public static void ConnectSignal( GodotObject source, StringName signalName, GodotObject target, Callable? method ) {
		if ( !method.HasValue ) {
			throw new ArgumentNullException( nameof( method ) );
		}
		if ( !source.HasSignal( signalName ) ) {
			throw new InvalidOperationException( $"GodotObject {source.GetType().FullName} doesn't have signal {signalName}" );
		}

		string callableKey = $"{target.GetInstanceId()}:{method.Value.Delegate.Method.Name}";
		if ( !CachedCallables.TryGetValue( callableKey, out Callable callable ) ) {
			CachedCallables[ callableKey ] = method.Value;
		}

		source.Connect( signalName, method.Value );
		if ( !Connections.TryGetValue( target, out List<ConnectionInfo> connectionList ) ) {
			connectionList = new List<ConnectionInfo>();
			Connections[ target ] = connectionList;
		}

		connectionList.Add( new ConnectionInfo(
			source: source,
			signalName: signalName,
			callable: method.Value
		) );

		Console.PrintDebug(
			$"Connected signal {signalName} from GodotObject {source.GetType().FullName} to GodotObject {target.GetType().FullName}"
		);

		if ( target is Node targetNode ) {
			targetNode.Connect( Node.SignalName.TreeExiting, Callable.From( () => DisconnectAllForObject( target ) ) );
		}
	}

	/*
	===============
	Subscribe
	===============
	*/
	public static void Subscribe<T>( object subscriber, Delegate handler ) {
		Type eventType = typeof( T );

		if ( SubscriptionCache.TryGetValue( eventType, out Delegate callback ) ) {
			SubscriptionCache[ eventType ] = Delegate.Combine( callback, handler );
		} else {
			SubscriptionCache.TryAdd( eventType, handler );
		}

		if ( !Subscriptions.TryGetValue( subscriber, out List<SubscriptionInfo> connectionList ) ) {
			connectionList = new List<SubscriptionInfo>();
			Subscriptions.TryAdd( subscriber, connectionList );
		}

		connectionList.Add( new SubscriptionInfo(
			eventType: eventType,
			eventHandler: handler
		) );

		if ( subscriber is Node node ) {
			node.Connect( Node.SignalName.TreeExiting, Callable.From( () => Unsubscribe<T>( subscriber, handler ) ) );
		}
	}

	/*
	===============
	Unsubscribe
	===============
	*/
	public static void Unsubscribe<T>( object subscriber, Delegate handler ) {
		Type eventType = typeof( T );

		if ( SubscriptionCache.TryGetValue( eventType, out Delegate callback ) ) {
			Delegate newCallback = Delegate.Remove( callback, handler );
			if ( newCallback == null ) {
				SubscriptionCache.TryRemove( new KeyValuePair<Type, Delegate>( eventType, callback ) );
			} else {
				SubscriptionCache[ eventType ] = newCallback;
			}
		}

		if ( Subscriptions.TryGetValue( subscriber, out List<SubscriptionInfo> connectionList ) ) {
			connectionList.RemoveAll( conn => conn.EventType == eventType && conn.EventHandler.Equals( handler ) );

			// if we've got no dangling connections, remove the object from the event cache
			if ( connectionList.Count == 0 ) {
				Subscriptions.TryRemove( new KeyValuePair<object, List<SubscriptionInfo>>( subscriber, connectionList ) );
			}
		}
	}

	/*
	===============
	ReleaseDanglingDelegates
	===============
	*/
	public static void ReleaseDanglingDelegates( Delegate @event ) {
		if ( @event == null ) {
			return;
		}
		Delegate[] invocations = @event.GetInvocationList();
		for ( int i = 0; i < invocations.Length; i++ ) {
			Delegate.Remove( @event, invocations[ i ] );
		}
	}

	/*
	===============
	DisposeAllConnections
	===============
	*/
	public static void DisposeAllConnections() {
		foreach ( var pair in Connections ) {
			DisconnectAllForObject( pair.Key );
		}

		ReleaseDanglingDelegates( PlayerEnteredArea );
		ReleaseDanglingDelegates( PlayerExitedArea );
		ReleaseDanglingDelegates( PlayerRenownScoreChanged );
		ReleaseDanglingDelegates( PlayerTraitScoreChanged );

		Connections.Clear();
		SubscriptionCache.Clear();
		CachedCallables.Clear();
	}

	/*
	===============
	DisconnectAllForObject
	===============
	*/
	public static void DisconnectAllForObject( object obj ) {
		if ( Connections.TryGetValue( obj as GodotObject, out List<ConnectionInfo> connections ) ) {
			for ( int i = 0; i < connections.Count; i++ ) {
				if ( connections[ i ].Source != null && IsInstanceValid( connections[ i ].Source ) ) {
					Console.PrintDebug(
						string.Format( "Disconnected signal {0} from GodotObject {1} to GodotObject {2}"
							, connections[ i ].SignalName, connections[ i ].Source.GetType().FullName,
							obj.GetType().FullName )
					);
					connections[ i ].Source.Disconnect( connections[ i ].SignalName, connections[ i ].Callable );
				}
			}
			Connections.TryRemove( new KeyValuePair<GodotObject, List<ConnectionInfo>>( obj as GodotObject, connections ) );
		}
		if ( Subscriptions.TryGetValue( obj, out List<SubscriptionInfo>? subscriptions ) ) {
			for ( int i = 0; i < subscriptions.Count; i++ ) {
				if ( subscriptions[ i ].EventType != null && subscriptions[ i ].EventHandler != null ) {
					if ( SubscriptionCache.TryGetValue( subscriptions[ i ].EventType, out Delegate? callback ) ) {
						Delegate newDelegate = Delegate.Remove( callback, subscriptions[ i ].EventHandler );
						if ( newDelegate == null ) {
							SubscriptionCache.TryRemove( new KeyValuePair<Type, Delegate>( subscriptions[ i ].EventType, subscriptions[ i ].EventHandler ) );
						} else {
							SubscriptionCache[ subscriptions[ i ].EventType ] = newDelegate;
						}
					}
				}
			}
		}
	}

	/*
	===============
	_EnterTree
	===============
	*/
	public override void _EnterTree() {
		base._EnterTree();

		ProcessMode = ProcessModeEnum.Always;
	}

	/*
	===============
	_ExitTree
	===============
	*/
	public override void _ExitTree() {
		base._ExitTree();
	}
};