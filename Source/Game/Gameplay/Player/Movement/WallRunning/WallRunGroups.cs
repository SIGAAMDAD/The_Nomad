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
using System.Runtime.CompilerServices;

namespace Nomad.Game.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunGroups

	===================================================================================
	*/

	internal static class WallRunGroups
	{
		public static readonly StringName Runnable = "wall_run";
		public static readonly StringName Blocked = "no_wall_run";
		public static readonly StringName Horizontal = "wall_run_horizontal";
		public static readonly StringName Up = "wall_run_up";
		public static readonly StringName Down = "wall_run_down";
		public static readonly StringName NoHorizontal = "wall_run_no_horizontal";
		public static readonly StringName NoUp = "wall_run_no_up";
		public static readonly StringName NoDown = "wall_run_no_down";

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool HasSpecificModeGroup( Node node )
		{
			return node.IsInGroup( Horizontal ) || node.IsInGroup( Up ) || node.IsInGroup( Down );
		}
	};
};
