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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Core.Numerics;
using Nomad.EngineUtils;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================

	PlayerAnimationController

	===================================================================================
	*/
	/// <summary>
	/// <para>Handles all higher level animation routing. Sprite directionality and state coordination.</para>
	/// </summary>

	internal sealed class PlayerAnimationController : NomadBehaviour
	{
		public PlayerPrefab Prefab { get; set; }
		public float LookAngle { get; set; }

		private Vector2 _windowSize = Vector2.Zero;

		private readonly IPlayerStateReader _stateReader;
		private readonly PlayerId _playerId = PlayerId.Invalid;

		public PlayerAnimationController( PlayerId playerId, IPlayerStateReader stateReader, IGameEventRegistryService eventFactory )
		{
			_playerId = playerId;
			_stateReader = stateReader ?? throw new ArgumentNullException( nameof( stateReader ) );

			_stateReader.StateChanged.Subscribe( OnStateChanged );
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public void Update( float delta )
		{
			var position = Prefab.GetViewport().GetMousePosition().ToSystem();

			var angleToCursor = position - _windowSize;
			LookAngle = MathF.Atan2( angleToCursor.Y, angleToCursor.X );

			bool facingLeft = angleToCursor.X < 0.0f;
			float rotation = AngleMath.ToDegrees( LookAngle );

			CheckArmStatus( facingLeft );
		}

		/*
		===============
		CheckArmStatus
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="facingLeft"></param>
		private void CheckArmStatus( bool facingLeft )
		{
		}

		/*
		===============
		OnStateChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnStateChanged( in PlayerStateChangedEventArgs args )
		{
		}
	};
};
