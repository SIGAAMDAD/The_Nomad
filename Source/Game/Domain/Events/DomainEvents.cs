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

using Nomad.Core.Events;
using Nomad.Core.Util;
using NomadCore.Domain.Models.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Domain.Events {
	/*
	===================================================================================
	
	DomainEvents
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class DomainEvents {
		/*
		===============
		GetEvent
		===============
		*/
		public static IGameEvent<TArgs> GetEvent<TArgs>( IGameEventRegistryService eventRegistry, string name )
			where TArgs : struct
		{
			return eventRegistry.GetEvent<TArgs>( name );
		}

		/*
		===============
		SubscribeToEvent
		===============
		*/
		public static void SubscribeToEvent<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, EventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetEvent<TArgs>( eventRegistry, name );
			uiEvent.Subscribe( subscriber, callback );
		}

		/*
		===============
		SubscribeToEventAsync
		===============
		*/
		public static void SubscribeToEventAsync<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, AsyncEventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetEvent<TArgs>( eventRegistry, name );
			uiEvent.SubscribeAsync( subscriber, callback );
		}

		/*
		===============
		UnsubscribeFromEvent
		===============
		*/
		public static void UnsubscribeFromEvent<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, EventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetEvent<TArgs>( eventRegistry, name );
			uiEvent.Unsubscribe( subscriber, callback );
		}

		/*
		===============
		UnsubscribeFromEventAsync
		===============
		*/
		public static void UnsubscribeFromEventAsync<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, AsyncEventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetEvent<TArgs>( eventRegistry, name );
			uiEvent.UnsubscribeAsync( subscriber, callback );
		}

		/*
		===============
		PublishEvent
		===============
		*/
		public static void PublishEvent<TArgs>( IGameEventRegistryService eventRegistry, string name, TArgs args )
			where TArgs : struct
		{
			var uiEvent = GetEvent<TArgs>( eventRegistry, name );
			uiEvent.Publish( args );
		}

		/*
		===============
		PublishEventAsync
		===============
		*/
		public static async Task PublishEventAsync<TArgs>( IGameEventRegistryService eventRegistry, string name, TArgs args, CancellationToken ct = default )
			where TArgs : struct
		{
			var uiEvent = GetEvent<TArgs>( eventRegistry, name );
			await uiEvent.PublishAsync( args, ct );
		}
	};
};