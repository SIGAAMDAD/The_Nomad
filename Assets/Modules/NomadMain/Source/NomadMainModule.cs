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

using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Mods;
using Nomad.Modules.NomadMain.Items;

namespace Nomad.Modules.NomadMain
{
    public sealed class NomadMainModule : NomadModule
    {
        public override string Id => "gdr.nomadmain";

        public override void OnPreLoad( IModuleContext context )
        {
            context.Logger.Debug( "NomadMain preload started." );
        }

        public override void OnInitialized( IModuleContext context )
        {
            context.Logger.Info(
                $"NomadMain initialized against mod API {context.RuntimeApiVersion}."
            );

            context.Behaviors.AddConsumable<FirelinkChargeBehavior>(
                "gdr.nomadmain.firelink_charge",
                ( module, definition ) => new FirelinkChargeBehavior( definition, module.Events )
            );
        }

        public override void OnShutdown( IModuleContext context )
        {
            context.Logger.Info( "NomadMain shutting down." );
        }
    }
}
