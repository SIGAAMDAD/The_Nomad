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
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Infrastructure.Mods
{
	internal sealed class ModuleBehaviorRegistry<TBehavior>
		where TBehavior : class
	{
		private readonly Dictionary<string, Func<TBehavior>> _factories = new(
			StringComparer.Ordinal
		);

		public void Register( string id, Func<TBehavior> factory )
		{
			ArgumentGuard.ThrowIfNull( factory, nameof( factory ) );
			if ( string.IsNullOrWhiteSpace( id ) ) {
				throw new ArgumentException( "Behavior id cannot be null or whitespace.", nameof( id ) );
			}

			if ( !_factories.TryAdd( id, factory ) ) {
				throw new InvalidOperationException(
					$"A module behavior with id '{id}' has already been registered."
				);
			}
		}

		public TBehavior Create( string id )
		{
			if ( !_factories.TryGetValue( id, out Func<TBehavior>? factory ) ) {
				throw new KeyNotFoundException(
					$"No module behavior is registered with id '{id}'."
				);
			}

			return factory.Invoke();
		}

		public bool TryCreate( string id, out TBehavior? behavior )
		{
			if ( !_factories.TryGetValue( id, out Func<TBehavior>? factory ) ) {
				behavior = null;
				return false;
			}

			behavior = factory.Invoke();
			return true;
		}
	};
};
