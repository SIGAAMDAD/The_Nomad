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

using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Util;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Game.Application.Gameplay.Entity;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class PlayerBase : EntityBase, IPlayerBase
	{
		public PlayerId PlayerId => new PlayerId( Id );

		//
		// Runtime
		//

		private readonly PlayerRuntime _runtime;
		internal PlayerRuntime Runtime => _runtime;
		internal PlayerPrefab Prefab { get; }

		public IGameEvent<PlayerDieEventArgs> Die => _die;
		private readonly IGameEvent<PlayerDieEventArgs> _die;

		/*
		===============
		PlayerBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="prefab"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		public PlayerBase( PlayerId playerId, PlayerPrefab prefab, int localPlayerIndex, IGameEventRegistryService eventFactory, ILoggerService logger )
			: base( new EntityId( playerId ), InternString.Empty, InternString.Empty, EntityType.Player, EntityFlags.None )
		{
			Prefab = prefab;

			_die = eventFactory.GetEvent<PlayerDieEventArgs>(
				PlayerDieEventArgs.Name,
				PlayerDieEventArgs.NameSpace
			);

			_runtime = PlayerBootstrapper.Bootstrap( playerId, prefab, localPlayerIndex, eventFactory, logger );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_die?.Dispose();
		}

		/*
		===============
		ApplySpawnProfile
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="spawnApplicator"></param>
		/// <param name="profile"></param>
		/// <param name="context"></param>
		public void ApplySpawnProfile( IPlayerSpawnApplicator spawnApplicator, PlayerSpawnProfileDefinition profile, in PlayerSpawnContext context )
		{
			spawnApplicator.Apply(
				this,
				profile,
				_runtime.DerivedStatService,
				_runtime.ResourceService,
				_runtime.FlagService,
				in context
			);
		}
	};
};
