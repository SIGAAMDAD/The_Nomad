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

namespace Nomad.Game.Prefabs
{
	internal sealed partial class SyncPoint : Node3D
	{
		[Export] public float SyncDuration { get; set; } = 5.0f;
		[Export] public float OrbitDegrees { get; set; } = 360.0f;
		[Export] public float OrbitRadius { get; set; } = 9.0f;
		[Export] public float OrbitHeight { get; set; } = 4.0f;
		[Export] public float MidOrbitExtraHeight { get; set; } = 2.0f;
		[Export] public float MidOrbitExtraDistance { get; set; } = 2.5f;
		[Export] public Vector3 FocusOffset { get; set; } = new Vector3( 0f, 1.8f, 0f );
	};
};
