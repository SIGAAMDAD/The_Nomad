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
using System.Collections.Generic;
using System.Diagnostics;

/*
===================================================================================

GameEventBus

===================================================================================
*/
/// <summary>
/// 
/// </summary>

public partial class GameEventBus : Node {
	private struct ConnectionInfo {
		public GodotObject Source;
		public StringName SignalName;
		public Callable Callable;
		public Type EventType;
		public Delegate EventHandler;
	};

	private static readonly Dictionary<object, List<ConnectionInfo>> Connections = new Dictionary<object, List<ConnectionInfo>>();
	private static readonly Dictionary<string, Callable> CachedCallables = new Dictionary<string, Callable>();
	private static readonly Dictionary<Type, Delegate> EventHandlers = new Dictionary<Type, Delegate>();

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

		connectionList.Add( new ConnectionInfo {
			Source = source,
			SignalName = signalName,
			Callable = callable
		} );

		if ( target is Node targetNode ) {
			targetNode.TreeExiting += () => DisconnectAllForObject( target );
		}
	}

	/*
	===============
	ConnectSignal
	===============
	*/
	public static void ConnectSignal( GodotObject source, StringName signalName, GodotObject target, Callable method ) {
		if ( !source.HasSignal( signalName ) ) {
			throw new InvalidOperationException( $"GodotObject {source.GetType().FullName} doesn't have signal {signalName}" );
		}

		string callableKey = $"{target.GetInstanceId()}:{method.Delegate.Method.Name}";
		if ( !CachedCallables.TryGetValue( callableKey, out method ) ) {
			CachedCallables[ callableKey ] = method;
		}

		source.Connect( signalName, method );
		if ( !Connections.TryGetValue( target, out List<ConnectionInfo> connectionList ) ) {
			connectionList = new List<ConnectionInfo>();
			Connections[ target ] = connectionList;
		}

		connectionList.Add( new ConnectionInfo {
			Source = source,
			SignalName = signalName,
			Callable = method
		} );

		if ( target is Node targetNode ) {
			targetNode.TreeExiting += () => DisconnectAllForObject( target );
		}
	}

	/*
	===============
	Subscribe
	===============
	*/
	public static void Subscribe<T>( object subscriber, Action handler ) {
		Type eventType = typeof( T );

		if ( EventHandlers.TryGetValue( eventType, out Delegate callback ) ) {
			EventHandlers[ eventType ] = Delegate.Combine( callback, handler );
		} else {
			EventHandlers.Add( eventType, handler );
		}

		if ( !Connections.TryGetValue( subscriber, out List<ConnectionInfo> connectionList ) ) {
			connectionList = new List<ConnectionInfo>();
			Connections.Add( subscriber, connectionList );
		}

		connectionList.Add( new ConnectionInfo {
			EventType = eventType,
			EventHandler = handler
		} );

		if ( subscriber is Node node ) {
			node.Connect( Node.SignalName.TreeExiting, Callable.From( () => Unsubscribe<T>( subscriber, handler ) ) );
		}
	}

	/*
	===============
	Unsubscribe
	===============
	*/
	public static void Unsubscribe<T>( object subscriber, Action handler ) {
		Type eventType = typeof( T );

		if ( EventHandlers.TryGetValue( eventType, out Delegate callback ) ) {
			Delegate newCallback = Delegate.Remove( callback, handler );
			if ( newCallback == null ) {
				EventHandlers.Remove( eventType );
			} else {
				EventHandlers[ eventType ] = newCallback;
			}
		}

		if ( Connections.TryGetValue( subscriber, out List<ConnectionInfo> connectionList ) ) {
			connectionList.RemoveAll( conn => conn.EventType == eventType && conn.EventHandler.Equals( handler ) );

			// if we've got no dangling connections, remove the object from the event cache
			if ( connectionList.Count == 0 ) {
				Connections.Remove( subscriber );
			}
		}
	}

	/*
	===============
	EmitSignal
	===============
	*/
	public static void EmitSignal<T>( T eventData ) {
		Type eventType = typeof( T );

		if ( EventHandlers.TryGetValue( eventType, out Delegate handler ) ) {
			try {
				( handler as Action<T> )?.Invoke( eventData );
			} catch ( Exception e ) {
				Console.PrintError( $"Error in event handler for {eventType.FullName}: {e.Message}" );
			}
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

		Connections.Clear();
		EventHandlers.Clear();
		CachedCallables.Clear();
	}

	/*
	===============
	DisconnectAllForObject
	===============
	*/
	public static void DisconnectAllForObject( object obj ) {
		if ( Connections.TryGetValue( obj, out List<ConnectionInfo> connections ) ) {
			for ( int i = 0; i < connections.Count; i++ ) {
				if ( connections[ i ].Source != null && IsInstanceValid( connections[ i ].Source ) ) {
					connections[ i ].Source.Disconnect( connections[ i ].SignalName, connections[ i ].Callable );
				}

				// remove subscriptions
				if ( connections[ i ].EventType != null && connections[ i ].EventHandler != null ) {
					if ( EventHandlers.TryGetValue( connections[ i ].EventType, out Delegate callback ) ) {
						Delegate newDelegate = Delegate.Remove( callback, connections[ i ].EventHandler );
						if ( newDelegate == null ) {
							EventHandlers.Remove( connections[ i ].EventType );
						} else {
							EventHandlers[ connections[ i ].EventType ] = newDelegate;
						}
					}
				}
			}
			Connections.Remove( obj );
		}
		if ( obj is Node node ) {
			node.TreeExiting -= () => DisconnectAllForObject( obj );
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