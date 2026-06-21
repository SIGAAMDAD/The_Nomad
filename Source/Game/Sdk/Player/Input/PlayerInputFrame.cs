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

using System.Numerics;
using System.Runtime.CompilerServices;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Sdk.Player.Input
{
    /*
	===================================================================================

	PlayerInputFrame

	===================================================================================
	*/
    /// <summary>
    /// A compact, serializable snapshot of one player's gameplay input for a single
    /// network/game tick.
    /// </summary>

    public readonly struct PlayerInputFrame
    {
        public static readonly PlayerInputFrame Empty = new PlayerInputFrame(
            PlayerId.Invalid,
            0,
            0,
            Vector2.Zero,
            Vector2.Zero,
            0.0f,
            PlayerInputButtons.None,
            PlayerInputButtons.None
        );

        public readonly PlayerId PlayerId;
        public readonly uint Tick;
        public readonly ushort Sequence;
        public readonly Vector2 Move;
        public readonly Vector2 AimDirection;
        public readonly float AimAngle;
        public readonly PlayerInputButtons ButtonsDown;
        public readonly PlayerInputButtons ButtonsPressed;

        public bool IsMoving => Move.LengthSquared() > 0.0001f;
        public bool SlidePressed => IsPressed(PlayerInputButtons.Slide);
        public bool SlideDown => IsDown(PlayerInputButtons.Slide);
        public bool DashPressed => IsPressed(PlayerInputButtons.Dash);
        public bool DashDown => IsDown(PlayerInputButtons.Dash);
        public bool InteractPressed => IsPressed(PlayerInputButtons.Interact);
        public bool InteractDown => IsDown(PlayerInputButtons.Interact);
        public bool JumpPressed => IsPressed(PlayerInputButtons.Jump);
        public bool JumpDown => IsDown(PlayerInputButtons.Jump);
        public bool DropPressed => IsPressed(PlayerInputButtons.Drop);
        public bool DropDown => IsDown(PlayerInputButtons.Drop);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerInputFrame(
            PlayerId playerId,
            uint tick,
            ushort sequence,
            Vector2 move,
            Vector2 aimDirection,
            float aimAngle,
            PlayerInputButtons buttonsDown,
            PlayerInputButtons buttonsPressed
        )
        {
            PlayerId = playerId;
            Tick = tick;
            Sequence = sequence;
            Move = move;
            AimDirection = aimDirection;
            AimAngle = aimAngle;
            ButtonsDown = buttonsDown;
            ButtonsPressed = buttonsPressed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDown(PlayerInputButtons button)
        {
            return (ButtonsDown & button) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPressed(PlayerInputButtons button)
        {
            return (ButtonsPressed & button) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerInputFrame WithPeer(PlayerId playerId)
        {
            return new PlayerInputFrame(
                playerId,
                Tick,
                Sequence,
                Move,
                AimDirection,
                AimAngle,
                ButtonsDown,
                ButtonsPressed
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerInputFrame WithTick(uint tick)
        {
            return new PlayerInputFrame(
                PlayerId,
                tick,
                Sequence,
                Move,
                AimDirection,
                AimAngle,
                ButtonsDown,
                ButtonsPressed
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerInputFrame WithoutTransientButtons(uint tick)
        {
            return new PlayerInputFrame(
                PlayerId,
                tick,
                Sequence,
                Move,
                AimDirection,
                AimAngle,
                ButtonsDown,
                PlayerInputButtons.None
            );
        }
    }
}
