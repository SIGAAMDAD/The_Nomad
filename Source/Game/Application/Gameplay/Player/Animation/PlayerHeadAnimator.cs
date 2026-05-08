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
using Nomad.Core.Numerics;
using Nomad.CVars;
using Nomad.CVars.Global;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Prefabs;
using Nomad.Input.Interfaces;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================
	
	PlayerHeadAnimator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class PlayerHeadAnimator : NomadBehaviour
	{
		private static readonly Godot.Vector2 HEAD_OFFSET_RIGHT = new Godot.Vector2( 5.0f, 5.0f );
		private static readonly Godot.Vector2 HEAD_OFFSET_LEFT = new Godot.Vector2( 5.0f, -5.0f );

		public Vector2 AngleToCursor { get; private set; }
		public float LookAngle { get; private set; }

		private EngineSprite2D _headSprite;
		private PlayerPrefab _prefab;

		private Vector2 _windowSize;

		private readonly IInputSnapshotService _snapshotService;

		/*
		===============
		PlayerHeadAnimator
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public PlayerHeadAnimator()
		{
			var eventFactory = GameEventRegistry.Instance;
			var cvarSystem = CVarSystem.Instance;

			_snapshotService = ServiceLocator.GetService<IInputSnapshotService>();

			var windowSize = cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION );
			var size = (WindowSize)windowSize.Value;
			_windowSize = new Vector2( size.Width, size.Height ) * 0.5f;
			windowSize.ValueChanged.Subscribe( OnWindowSizeChanged );
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnInit()
		{
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			_headSprite = _prefab.FindChild<EngineSprite2D>( "HeadSprite" );
		}

		/*
		===============
		OnUpdate
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void OnUpdate( float delta )
		{
			base.OnUpdate( delta );

			var position = _prefab.GetViewport().GetMousePosition().ToSystem();

			AngleToCursor = position - _windowSize;
			LookAngle = MathF.Atan2( AngleToCursor.Y, AngleToCursor.X );

			bool facingLeft = AngleToCursor.X < 0.0f;
			_headSprite.Rotation = AngleMath.ToDegrees( LookAngle );

			_headSprite.FlipV = facingLeft;
			_headSprite.Offset = facingLeft ? HEAD_OFFSET_LEFT : HEAD_OFFSET_RIGHT;
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnShutdown()
		{
			base.OnShutdown();

			var cvarSystem = CVarSystem.Instance;
			var windowSize = cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION );
			windowSize.ValueChanged.Unsubscribe( OnWindowSizeChanged );
		}

		/*
		===============
		OnWindowSizeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWindowSizeChanged( in CVarValueChangedEventArgs<WindowResolution> args )
		{
			var size = _prefab.GetViewportRect().Size * 0.5f;
			_windowSize = size.ToSystem();
		}
	};
};
