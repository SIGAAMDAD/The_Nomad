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
using System.Threading;
using System.Threading.Tasks;

namespace Game.Application.UI {
	/*
	===================================================================================
	
	UIEventHelper
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class UIEventHelper {
		public static IGameEvent<TArgs> GetUIEvent<TArgs>( IGameEventRegistryService eventRegistry, string name )
			where TArgs : struct
		{
			return eventRegistry.GetEvent<TArgs>( name );
		}

		public static void SubscribeToUIEvent<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, EventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetUIEvent<TArgs>( eventRegistry, name );
			uiEvent.Subscribe( subscriber, callback );
		}

		public static void SubscribeToUIEventAsync<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, AsyncEventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetUIEvent<TArgs>( eventRegistry, name );
			uiEvent.SubscribeAsync( subscriber, callback );
		}

		public static void UnsubscribeFromUIEvent<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, EventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetUIEvent<TArgs>( eventRegistry, name );
			uiEvent.Unsubscribe( subscriber, callback );
		}

		public static void UnsubscribeFromUIEventAsync<TArgs>( IGameEventRegistryService eventRegistry, object subscriber, string name, AsyncEventCallback<TArgs> callback )
			where TArgs : struct
		{
			var uiEvent = GetUIEvent<TArgs>( eventRegistry, name );
			uiEvent.UnsubscribeAsync( subscriber, callback );
		}

		public static void PublishUIEvent<TArgs>( IGameEventRegistryService eventRegistry, string name, in TArgs args )
			where TArgs : struct
		{
			var uiEvent = GetUIEvent<TArgs>( eventRegistry, name );
			uiEvent.Publish( in args );
		}

		public static async Task PublishUIEventAsync<TArgs>( IGameEventRegistryService eventRegistry, string name, TArgs args, CancellationToken ct = default )
			where TArgs : struct
		{
			var uiEvent = GetUIEvent<TArgs>( eventRegistry, name );
			await uiEvent.PublishAsync( args, ct );
		}
	};
};