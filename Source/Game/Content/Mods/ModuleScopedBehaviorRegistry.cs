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
using Nomad.Core.Util;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModuleScopedBehaviorRegistry : INomadBehaviorRegistry
	{
		private readonly INomadBehaviorRegistry _registry;
		private readonly IModuleContext _context;

		public ModuleScopedBehaviorRegistry( INomadBehaviorRegistry registry, IModuleContext context )
		{
			_registry = registry ?? throw new ArgumentNullException( nameof( registry ) );
			_context = context ?? throw new ArgumentNullException( nameof( context ) );
		}

		public void Add<TBehavior, TDefinition>(
			string behaviorId,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			Register( new InternString( behaviorId ), factory );
		}

		public void Add<TBehavior, TDefinition>(
			InternString behaviorId,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			Register( behaviorId, factory );
		}

		public void Register<TBehavior, TDefinition>(
			InternString behaviorId,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			_registry.Register( behaviorId, _context, factory );
		}

		public void Register<TBehavior, TDefinition>(
			InternString behaviorId,
			IModuleContext ownerContext,
			NomadBehaviorFactory<TBehavior, TDefinition> factory
		)
			where TBehavior : class
		{
			_registry.Register( behaviorId, ownerContext, factory );
		}

		public bool Contains<TBehavior>( string behaviorId )
			where TBehavior : class
		{
			return _registry.Contains<TBehavior>( behaviorId );
		}

		public bool Contains<TBehavior>( InternString behaviorId )
			where TBehavior : class
		{
			return _registry.Contains<TBehavior>( behaviorId );
		}

		public TBehavior Create<TBehavior, TDefinition>(
			InternString behaviorId,
			TDefinition definition
		)
			where TBehavior : class
		{
			return _registry.Create<TBehavior, TDefinition>( behaviorId, definition );
		}

		public TBehavior Create<TBehavior, TDefinition>(
			InternString behaviorId,
			IModuleContext context,
			TDefinition definition
		)
			where TBehavior : class
		{
			return _registry.Create<TBehavior, TDefinition>( behaviorId, context, definition );
		}

		public bool TryCreate<TBehavior, TDefinition>(
			InternString behaviorId,
			TDefinition definition,
			out TBehavior behavior
		)
			where TBehavior : class
		{
			return _registry.TryCreate( behaviorId, definition, out behavior );
		}

		public bool TryCreate<TBehavior, TDefinition>(
			InternString behaviorId,
			IModuleContext context,
			TDefinition definition,
			out TBehavior behavior
		)
			where TBehavior : class
		{
			return _registry.TryCreate( behaviorId, context, definition, out behavior );
		}
	}
}
