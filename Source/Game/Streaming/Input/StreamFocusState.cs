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
using Nomad.Game.Sdk.Events.Player.Movement;

namespace Nomad.Game.Streaming
{
	internal sealed class StreamFocusState
	{
		public Vector3 Origin;
		public Vector2 Velocity;
		public Vector2 MoveInput;
		public Vector3 CameraForward;

		public bool HasMovementState;
		public bool HasCameraState;

		public bool HasUsableState => HasMovementState || HasCameraState;

		public void Apply( in PlayerLocomotionCueEventArgs args )
		{
			Origin = args.Origin.ToGodot();
			Velocity = args.NewVelocity.ToGodot();
			MoveInput = args.MoveInput.ToGodot();
			HasMovementState = true;
		}

		public void Apply( in PlayerCameraStatusChangedEventArgs args )
		{
			Origin = args.Origin.ToGodot();
			CameraForward = args.Forward.ToGodot();
			HasCameraState = true;
		}

		public void ApplyDirect( Vector3 origin, Vector2 velocity, Vector2 moveInput, Vector3 cameraForward )
		{
			Origin = origin;
			Velocity = velocity;
			MoveInput = moveInput;
			CameraForward = cameraForward;
			HasMovementState = true;
			HasCameraState = cameraForward.LengthSquared() > 0.0001f;
		}
	};
};
