/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nomad.Core.Events;
using Nomad.Events.Extensions;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding.Events;

namespace Nomad.Game.Infrastructure.Mods
{
	internal sealed class ModEventRegistry : IModEventRegistry, IDisposable
	{
		private readonly IGameEventRegistryService _eventFactory;
		private readonly SubscriptionScope _subscriptions;
		private readonly ModSecurityPolicy _policy;
		private readonly ModuleManifest _identity;
		private readonly IModDiagnostics _diagnostics;

		private readonly HashSet<string> _localEvents = new( StringComparer.Ordinal );
		private readonly HashSet<string> _allowedGameEvents = new( StringComparer.Ordinal );

		private int _publishesThisFrame = 0;
		private bool _isDisposed = false;

		public ModEventRegistry( IGameEventRegistryService eventFactory, ModSecurityPolicy policy, IEnumerable<string> allowedGameEventKeys )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_policy = policy ?? throw new ArgumentNullException( nameof( policy ) );

			foreach ( var key in allowedGameEventKeys ) {
				_allowedGameEvents.Add( key );
			}
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_subscriptions.Dispose();

			// Only clear the mod-owned event namespace.
			_eventFactory.ClearEventsInNamespace( GetLocalEventNamespace() );

			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		public ISubscriptionHandle On<TArgs>(
			ModEventId<TArgs> eventId,
			EventCallback<TArgs> callback
		)
			where TArgs : struct
		{
			ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull( callback );

			string eventName = NormalizeLocalEventName( eventId.Name );
			string nameSpace = GetLocalEventNamespace();

			TrackLocalEvent( eventName );
			EnsureSubscriptionQuota();

			IGameEvent<TArgs> gameEvent = _eventFactory.GetEvent<TArgs>(
				eventName,
				nameSpace
			);

			EventCallback<TArgs> guarded = GuardCallback(
				eventName,
				nameSpace,
				callback
			);

			ISubscriptionHandle handle = gameEvent.Subscribe( guarded );
			return _subscriptions.Add( handle );
		}

		public ISubscriptionHandle OnGame<TArgs>(
			GameEventId<TArgs> eventId,
			EventCallback<TArgs> callback
		)
			where TArgs : struct
		{
			ThrowIfDisposed();
			ArgumentNullException.ThrowIfNull( callback );

			string key = GetGameEventKey( eventId.NameSpace, eventId.Name );

			if ( !_allowedGameEvents.Contains( key ) ) {
				throw new ModPolicyException(
					$"Mod '{_identity.Id}' is not allowed to subscribe to game event '{key}'."
				);
			}

			EnsureSubscriptionQuota();

			IGameEvent<TArgs> gameEvent = _eventFactory.GetEvent<TArgs>(
				eventId.Name,
				eventId.NameSpace
			);

			EventCallback<TArgs> guarded = GuardCallback(
				eventId.Name,
				eventId.NameSpace,
				callback
			);

			ISubscriptionHandle handle = gameEvent.Subscribe( guarded );
			return _subscriptions.Add( handle );
		}

		public void Publish<TArgs>(
			ModEventId<TArgs> eventId,
			in TArgs args
		)
			where TArgs : struct
		{
			ThrowIfDisposed();

			if ( !_policy.AllowLocalEventPublishing ) {
				throw new ModPolicyException(
					$"Mod '{_identity.Id}' is not allowed to publish local events."
				);
			}

			EnsurePublishQuota();

			string eventName = NormalizeLocalEventName( eventId.Name );
			string nameSpace = GetLocalEventNamespace();

			TrackLocalEvent( eventName );

			_eventFactory
				.GetEvent<TArgs>( eventName, nameSpace )
				.Publish( in args );
		}

		public Task PublishAsync<TArgs>(
			ModEventId<TArgs> eventId,
			TArgs args,
			CancellationToken ct = default
		)
			where TArgs : struct
		{
			ThrowIfDisposed();

			if ( !_policy.AllowLocalEventPublishing ) {
				throw new ModPolicyException(
					$"Mod '{_identity.Id}' is not allowed to publish local events."
				);
			}

			EnsurePublishQuota();

			string eventName = NormalizeLocalEventName( eventId.Name );
			string nameSpace = GetLocalEventNamespace();

			TrackLocalEvent( eventName );

			return _eventFactory
				.GetEvent<TArgs>( eventName, nameSpace )
				.PublishAsync( args, ct );
		}

		public void ResetFrameQuota()
		{
			_publishesThisFrame = 0;
		}

		private EventCallback<TArgs> GuardCallback<TArgs>(
			string eventName,
			string nameSpace,
			EventCallback<TArgs> callback
		)
			where TArgs : struct
		{
			return ( in TArgs args ) => {
				if ( _diagnostics.ShouldDisable( _identity ) ) {
					return;
				}

				try {
					callback( in args );
				} catch ( Exception ex ) {
					_diagnostics.ReportException(
						_identity,
						ex,
						$"Event callback '{nameSpace}.{eventName}'"
					);

					if ( _diagnostics.ShouldDisable( _identity ) ) {
						_diagnostics.Disable(
							_identity,
							$"Mod '{_identity.Id}' exceeded event callback failure limit."
						);

						_subscriptions.Dispose();
					}
				}
			};
		}

		private void TrackLocalEvent( string eventName )
		{
			if ( _localEvents.Contains( eventName ) ) {
				return;
			}

			if ( _localEvents.Count >= _policy.MaxLocalEvents ) {
				throw new ModPolicyException(
					$"Mod '{_identity.Id}' exceeded max local event count {_policy.MaxLocalEvents}."
				);
			}

			_localEvents.Add( eventName );
		}

		private void EnsureSubscriptionQuota()
		{
			if ( _subscriptions.Count >= _policy.MaxEventSubscriptions ) {
				throw new ModPolicyException(
					$"Mod '{_identity.Id}' exceeded max subscription count {_policy.MaxEventSubscriptions}."
				);
			}
		}

		private void EnsurePublishQuota()
		{
			_publishesThisFrame++;

			if ( _publishesThisFrame > _policy.MaxPublishesPerFrame ) {
				throw new ModPolicyException(
					$"Mod '{_identity.Id}' exceeded max event publishes per frame {_policy.MaxPublishesPerFrame}."
				);
			}
		}

		private string GetLocalEventNamespace()
		{
			return $"Mods.{_identity.Id}.Events";
		}

		private static string GetGameEventKey( string nameSpace, string name )
		{
			return $"{nameSpace}:{name}";
		}

		private static string NormalizeLocalEventName( string name )
		{
			if ( string.IsNullOrWhiteSpace( name ) ) {
				throw new ArgumentException( "Event name cannot be null or whitespace.", nameof( name ) );
			}

			if (
				name.Contains( "..", StringComparison.Ordinal ) ||
				name.Contains( ':', StringComparison.Ordinal ) ||
				name.Contains( '/', StringComparison.Ordinal ) ||
				name.Contains( '\\', StringComparison.Ordinal )
			) {
				throw new ModPolicyException(
					$"Invalid mod event name '{name}'."
				);
			}

			return name.Trim();
		}

		private void ThrowIfDisposed()
		{
			if ( _isDisposed ) {
				throw new ObjectDisposedException( nameof( ModEventRegistry ) );
			}
		}
	}
}
