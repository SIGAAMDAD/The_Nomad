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

using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Core.ServiceRegistry.Services;
using Nomad.Events.Globals;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Prefabs {
	/*
	===================================================================================
	
	PlayerAggregate
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class PlayerAggregate : EngineCharacter2D {
		private PlayerJumpKit _jumpKit;
		private PlayerCamera2D _camera2D;

		private readonly IServiceScope _scope = ServiceRegistry.Instance.AddScoped<IServiceScope, ServiceScope>();

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			var eventFactory = GameEventRegistry.Instance;

			_scope.CreateInstance<IDashModuleRepository, DashModuleRepository>();
			_scope.CreateInstance<IDashModuleFactory, DashModuleFactory>();

			_jumpKit = AddComponent<PlayerJumpKit>();
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown() {
			_scope?.Dispose();
		}
	};
};