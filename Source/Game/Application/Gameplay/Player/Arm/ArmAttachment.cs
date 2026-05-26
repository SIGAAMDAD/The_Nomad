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

using Nomad.Core.Events;
using Nomad.Input;
using Nomad.Input.ValueObjects;
using Nomad.Game.Application.Gameplay.Player;

namespace Nomad.Game.Application.Gameplay.Player
{
	internal class ArmAttachment
	{
		public ArmAttachment( IGameEventRegistryService eventFactory )
		{
			eventFactory
				.GetEvent<ButtonActionEventArgs>( $"UseArmAttachment:{ButtonActionEventArgs.Name}", ButtonActionEventArgs.NameSpace )
				.Subscribe( OnUseArmAttachmentTriggered );
		}

		private void OnUseArmAttachmentTriggered( in ButtonActionEventArgs args )
		{
			if ( args.Phase != InputActionPhase.Started ) {
				return;
			}
		}
	};
};
