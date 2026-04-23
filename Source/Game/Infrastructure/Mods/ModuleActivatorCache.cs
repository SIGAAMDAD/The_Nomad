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

using System.Linq.Expressions;
using System.Reflection;
using System;
using System.Collections.Generic;
using Nomad.Game.Domain.Interfaces.Mods;

namespace Nomad.Game.Infrastructure.Mods {
	public static class ModuleActivatorCache {
		private static readonly Dictionary<Type, Func<IGameModule>> _cache = new();

		public static Func<IGameModule> GetOrCreate( Type type ) {
			if ( _cache.TryGetValue( type, out var ctor ) ) {
				return ctor;
			}

			ConstructorInfo? ci = type.GetConstructor( Type.EmptyTypes ) ??
				throw new InvalidOperationException( $"{type.FullName} must have a public parameterless constructor." );

			NewExpression newExpr = Expression.New( ci );
			UnaryExpression castExpr = Expression.Convert( newExpr, typeof( IGameModule ) );
			ctor = Expression.Lambda<Func<IGameModule>>( castExpr ).Compile();

			_cache[type] = ctor;
			return ctor;
		}
	};
};