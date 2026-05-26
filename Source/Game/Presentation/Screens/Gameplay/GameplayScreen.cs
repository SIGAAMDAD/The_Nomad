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

using Nomad.Core.Engine.Services;
using Nomad.Core.OnlineServices;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Globals;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Prefabs;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.Gameplay
{
	internal sealed partial class GameplayScreen : EnginePresentationLayer
	{
		private IHudRoot _hudRoot;

		protected override void OnInit()
		{
			base.OnInit();

			var eventFactory = GameEventRegistry.Instance;
			_hudRoot = new HudRoot(
				new PlayerId( new PeerId( Constants.LOCAL_GUID ) ),
				GetNode<HeadsUpDisplayView>( "HeadsUpDisplay" ),
				ServiceLocator.GetService<ILocalizationService>(),
				eventFactory
			);
		}

		protected override void OnUpdate( float delta )
		{
			base.OnUpdate( delta );

			_hudRoot.Render( delta );
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();

			_hudRoot.Dispose();
		}
	};
};
