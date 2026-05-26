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

using System.Collections.Generic;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Interactables;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	TempCheckpointPrefab

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class TempCheckpointPrefab : CheckpointPrefab
	{
		public override void BuildInteractionOptions( List<InteractionMenuOption> options )
		{
			options.Add( new InteractionMenuOption( new InternString( "Rest for a While" ), EntityInteractionKind.Rest ) );
			options.Add( new InteractionMenuOption( new InternString( "Leave Backpack" ), EntityInteractionKind.TurnIn ) );
			options.Add( new InteractionMenuOption( new InternString( "Get Up" ), EntityInteractionKind.Close ) );
		}

		public void Create()
		{
			Visible = true;
			ProcessMode = ProcessModeEnum.Pausable;
		}

		public void Destroy()
		{
			Visible = false;
			ProcessMode = ProcessModeEnum.Disabled;
		}

		public override void _Ready()
		{
			base._Ready();

			ProcessMode = ProcessModeEnum.Disabled;
		}
	};
};
