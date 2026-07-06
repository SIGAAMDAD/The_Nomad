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

namespace Nomad.Game.Gameplay.Npc.Planner
{
	internal readonly struct ActionContext
	{
		public Vector2 Position => _position;
		private readonly Vector2 _position;

		public int SlotA => _slotA;
		private readonly int _slotA;

		public int SlotB => _slotB;
		private readonly int _slotB;

		public object? TargetRef => _targetRef;
		private readonly object? _targetRef;
	};
};
