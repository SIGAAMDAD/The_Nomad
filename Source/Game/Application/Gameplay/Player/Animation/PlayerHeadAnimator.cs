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
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Prefabs;
using Nomad.Input.Events;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	/*
	===================================================================================
	
	PlayerHeadAnimator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerHeadAnimator : NomadBehaviour {
		private Sprite2D _headSprite;
		private PlayerPrefab _prefab;

		public PlayerHeadAnimator() {
			var eventFactory = GameEventRegistry.Instance;
			
			eventFactory.GetEvent<AxisActionEventArgs>( $"Look:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnLookAngleChanged );
		}

		public override void OnInit() {
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			_headSprite = _prefab.GetNode<Sprite2D>( "HeadSprite" );
		}

		private void OnLookAngleChanged( in AxisActionEventArgs args ) {
			_headSprite.Rotation = _prefab.GetLocalMousePosition().Angle();
		}
	};
};