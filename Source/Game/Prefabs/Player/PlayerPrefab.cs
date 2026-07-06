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

using Godot;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.Stats;
using Nomad.Scene.GameObjects;
using System.Collections.Generic;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	PlayerPrefab

	===================================================================================
	*/
	/// <summary>
	/// Full-3D player root. Older gameplay systems still consume SDK Vector2 values,
	/// but those values now represent the X/Z ground plane while Godot Y is vertical.
	/// </summary>

	public partial class PlayerPrefab : EngineCharacter3D
	{
		[Export]
		private Camera3D _camera;

		public PlayerId PeerId { get; set; }
		public PlayerInitializationDefinition Definition { get; init; }

		public PlayerPrefab()
		{
			Definition = new PlayerInitializationDefinition {
				Stats = new PlayerStatBlockDefinition {
					BaseStats = new Dictionary<BaseStatType, float> {
						[BaseStatType.BaseHealth] = 100.0f,
						[BaseStatType.BaseRage] = 100.0f,
						[BaseStatType.BaseMovementSpeed] = 200.0f,
						[BaseStatType.BaseSanity] = 90.0f,
						[BaseStatType.EncumbranceThreshold] = 100.0f,
						[BaseStatType.BaseDashSpeed] = 520.0f
					}
				},
				Resources = new PlayerSpawnResourceProfile {
					HealthFillPercent = 1.0f,
					RageFillPercent = 1.0f,
					SanityFillPercent = 1.0f
				}
			};
		}

		protected override void OnInit()
		{
			base.OnInit();

			var terrainInstances = GetTree().GetNodesInGroup( "TerrainInstance" );
			terrainInstances[0].Call( "set_camera", _camera );
		}
	};
};
