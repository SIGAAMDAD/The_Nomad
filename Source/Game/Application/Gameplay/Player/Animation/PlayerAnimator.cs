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
using Nomad.Core.Numerics;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;
using Godot;
using Nomad.Core.Util;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================

	PlayerAnimator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class PlayerAnimator : NomadBehaviour
	{
		public Guid Id { get; set; }

		protected PlayerPrefab prefab;
		protected AnimatedSprite2D animator;

		private PlayerHeadAnimator _headAnimator;

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerAnimationStateChangedEventArgs" )]
		[EventPayload( "OldAnimationId", typeof( InternString ), Order = 1 )]
		[EventPayload( "NewAnimationId", typeof( InternString ), Order = 2 )]
		public abstract IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged { get; }

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

			prefab = Object.CastAs<PlayerPrefab>();
			_headAnimator = prefab.GetComponent<PlayerHeadAnimator>();

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerMovementChangedEventArgs>(
					$"{Id}:{PlayerMovementChangedEventArgs.Name}",
					PlayerMovementChangedEventArgs.NameSpace
				)
				.Subscribe( OnPlayerMovementChanged );
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

			Flip( _headAnimator.AngleToCursor.X < 0.0f );
		}

		/*
		===============
		Flip
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="flip"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void Flip( bool flip )
		{
			animator.FlipH = flip;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected abstract void OnPlayerMovementChanged( in PlayerMovementChangedEventArgs args );
	};
};
