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

using Nomad.Game.Domain.Interfaces.Player;
using System;
using System.Collections.Concurrent;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	DashModuleFactory
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class DashModuleFactory : IDashModuleFactory {
		private readonly ConcurrentDictionary<Type, Func<IDashModule>> _factories = new();

		/*
		===============
		RegisterModuleFactory
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="moduleType"></param>
		/// <param name="factory"></param>
		public void RegisterModuleFactory( Type moduleType, Func<IDashModule> factory ) {
			_factories[ moduleType ] = factory;
		}

		/*
		===============
		CreateModule
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="moduleType"></param>
		/// <returns></returns>
		public IDashModule CreateModule( Type moduleType ) {
			return _factories[ moduleType ].Invoke();
		}
	};
};