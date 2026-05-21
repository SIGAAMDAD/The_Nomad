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
using System.Reflection;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Infrastructure
{
	internal abstract class DynamicTypeFactory<AttributeType, BehaviorInterface, BehaviorType>
		where AttributeType : Attribute
		where BehaviorType : BehaviorInterface
	{
		protected readonly Dictionary<string, Type> types = new();

		public DynamicTypeFactory()
		{
			foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() ) {
				RegisterFromAssembly( assembly );
			}
		}

		/*
		===============
		RegisterFromAssembly
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="assembly"></param>
		public void RegisterFromAssembly( Assembly assembly )
		{
			ArgumentGuard.ThrowIfNull( assembly );

			foreach ( var type in assembly.GetTypes() ) {
				if ( type.IsAbstract || type.IsInterface ) {
					continue;
				}
				if ( !typeof( BehaviorInterface ).IsAssignableFrom( type ) ) {
					continue;
				}

				var attr = type.GetCustomAttribute<AttributeType>();
				if ( attr == null ) {
					continue;
				}
//				types[attr.Id] = type;
			}
		}
	};
};
