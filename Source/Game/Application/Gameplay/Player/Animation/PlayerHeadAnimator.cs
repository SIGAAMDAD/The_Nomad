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
using Nomad.Core.CVars;
using Nomad.Core.Engine.Windowing;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.CVars;
using Nomad.CVars.Global;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Prefabs;
using Nomad.Input.Events;
using Nomad.Input.Interfaces;
using Nomad.Scene.GameObjects;
using Nomad.Core.Input;

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
		private EngineSprite2D _headSprite;
		private PlayerPrefab _prefab;

		private Vector2 _windowSize;
		private Vector2 _mousePosition;

		private IInputSnapshotService _snapshotService;

		public PlayerHeadAnimator() {
			var eventFactory = GameEventRegistry.Instance;
			var cvarSystem = CVarSystem.Instance;

			_snapshotService = ServiceLocator.GetService<IInputSnapshotService>();
			
			eventFactory.GetEvent<AxisActionEventArgs>( $"Look:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnLookAngleChanged );
			eventFactory.GetEvent<MousePositionChangedEventArgs>( Core.Constants.Events.Input.MOUSE_POSITION_CHANGED_EVENT, Core.Constants.Events.Input.NAMESPACE )
				.Subscribe( OnMousePositionChanged );
			
			var windowSize = cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION );
			var size = (WindowSize)windowSize.Value;
			_windowSize = new Vector2( size.Width, size.Height ) * 0.5f;
			windowSize.ValueChanged.Subscribe( OnWindowSizeChanged );
		}

		public override void OnInit() {
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			_headSprite = _prefab.FindChild<EngineSprite2D>( "HeadSprite" );
		}

		public override void OnUpdate( float delta ) {
			base.OnUpdate( delta );

			var toMouse = _mousePosition - _windowSize;
			float angle = MathF.Atan2( toMouse.Y, toMouse.X );
			_headSprite.Rotation = AngleMath.RadToDeg( angle );
		}

		public override void OnShutdown() {
			base.OnShutdown();

			var eventFactory = GameEventRegistry.Instance;
			var cvarSystem = CVarSystem.Instance;

			eventFactory.GetEvent<AxisActionEventArgs>( $"Look:{Input.Constants.Events.AXIS_ACTION}", Input.Constants.Events.NAMESPACE )
				.Subscribe( OnLookAngleChanged );
			
			var windowSize = cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION );
			windowSize.ValueChanged.Unsubscribe( OnWindowSizeChanged );
		}

		private void OnLookAngleChanged( in AxisActionEventArgs args ) {
		}

		private void OnWindowSizeChanged( in CVarValueChangedEventArgs<WindowResolution> args ) {
			var size = (WindowSize)args.NewValue;
			_windowSize = new Vector2( size.Width, size.Height ) * 0.5f;
		}

		private void OnMousePositionChanged( in MousePositionChangedEventArgs args ) {
			_mousePosition = new Vector2( args.PositionX, args.PositionY );
		}
	};
};