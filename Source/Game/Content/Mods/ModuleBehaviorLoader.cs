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
using System.Linq;
using System.Reflection;
using Nomad.Game.Sdk.Mods;
using Nomad.Modding;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModuleBehaviorLoader<TBehavior>
		where TBehavior : class
	{
		public IReadOnlyList<ModuleBehaviorDescriptor> LoadFromAssembly( Assembly assembly )
		{
			var types = new List<ModuleBehaviorDescriptor>();

			foreach ( Type type in GetLoadableTypes( assembly ) ) {
				if ( !IsConcreteBehaviorType( type ) ) {
					continue;
				}

				ModuleBehaviorAttribute? attribute = type.GetCustomAttribute<ModuleBehaviorAttribute>();
				if ( attribute == null ) {
					continue;
				}

				types.Add(
					new ModuleBehaviorDescriptor {
						Id = attribute.Id,
						Type = type,
						Assembly = assembly
					}
				);
			}

			return types;
		}

		public TBehavior Create( Type type )
		{
			object? instance = Activator.CreateInstance( type );

			if ( instance is not TBehavior behavior ) {
				throw new InvalidOperationException(
					$"Type '{type.FullName}' could not be instantiated as '{typeof( TBehavior ).Name }'"
				);
			}

			return behavior;
		}

		private static bool IsConcreteBehaviorType( Type type )
		{
			return typeof( TBehavior ).IsAssignableFrom( type )
				&& type.IsClass
				&& !type.IsAbstract
				&& type.GetConstructor( Type.EmptyTypes ) != null;
		}

		private IReadOnlyList<Type> GetLoadableTypes( Assembly assembly )
		{
			try {
				return assembly.GetTypes();
			}
			catch ( ReflectionTypeLoadException e ) {
				return e.Types
					.Where( type => type != null )
					.Cast<Type>()
					.ToArray();
			}
		}
	};
};
