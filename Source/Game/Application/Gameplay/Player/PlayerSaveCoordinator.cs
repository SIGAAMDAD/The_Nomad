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
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Application.Gameplay.Player.State;
using Nomad.Game.Sdk.Player.Stats;
using Nomad.Game.Prefabs;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerSaveCoordinator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerSaveCoordinator
	{
		private readonly IPlayerDerivedStatService _derivedStatService;
		private readonly IPlayerResourceService _resourceService;
		private readonly PlayerPrefab _prefab;
		private readonly PlayerStateCoordinator _stateReader;

		private readonly object _lock = new();

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		/*
		===============
		PlayerSaveCoordinator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="derivedStatService"></param>
		/// <param name="resourceService"></param>
		/// <param name="stateReader"></param>
		/// <param name="prefab"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlayerSaveCoordinator( IPlayerDerivedStatService derivedStatService, IPlayerResourceService resourceService, PlayerStateCoordinator stateReader, PlayerPrefab prefab, IGameEventRegistryService eventFactory )
		{
			_derivedStatService = derivedStatService ?? throw new ArgumentNullException( nameof( derivedStatService ) );
			_resourceService = resourceService ?? throw new ArgumentNullException( nameof( resourceService ) );
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_stateReader = stateReader ?? throw new ArgumentNullException( nameof( stateReader ) );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_loadBegin = eventFactory
				.GetEvent<LoadBeginEventArgs>(
					LoadBeginEventArgs.Name,
					LoadBeginEventArgs.NameSpace
				)
				.Subscribe( OnLoadBegin );
		}

		/*
		===============
		OnSaveBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( _lock ) {
				using var section = args.Writer.AddSection( "Player" );

				section.AddField( nameof( DerivedStatType.EffectiveHealthMax ), _derivedStatService.GetValue( DerivedStatType.EffectiveHealthMax ) );
				section.AddField( nameof( DerivedStatType.EffectiveRageMax ), _derivedStatService.GetValue( DerivedStatType.EffectiveRageMax ) );
				section.AddField( nameof( DerivedStatType.EffectiveSanityMax ), _derivedStatService.GetValue( DerivedStatType.EffectiveSanityMax ) );

				var position = _prefab.GlobalPosition.ToSystem();
				section.AddField( "Position.X", position.X );
				section.AddField( "Position.Y", position.Y );
				section.AddField( "Position.Z", position.Z );

				section.AddField( "State", (byte)_stateReader.Current );

				section.AddField( nameof( PlayerResourceType.Health ), _resourceService.GetValue( PlayerResourceType.Health ) );
				section.AddField( nameof( PlayerResourceType.Rage ), _resourceService.GetValue( PlayerResourceType.Rage ) );
				section.AddField( nameof( PlayerResourceType.Sanity ), _resourceService.GetValue( PlayerResourceType.Sanity ) );

				var jumpKit = _prefab.GetComponent<PlayerJumpKit>();
				var module = jumpKit.Module;

				section.AddField( $"DashModule", module.Name );
			}
		}

		/*
		===============
		OnLoadBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnLoadBegin( in LoadBeginEventArgs args )
		{
			lock ( _lock ) {
			}
		}
	};
};
