/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;

namespace Nomad.Game.Sdk.Renown.Region
{
    /// <summary>
    /// Compatibility namespace for regional renown. New code should use Nomad.Game.Sdk.Renown.Regional.IRenownTrackerService.
    /// </summary>
    [Obsolete("Use Nomad.Game.Sdk.Renown.Regional.IRenownTrackerService.")]
    public interface IRenownTrackerService : Nomad.Game.Sdk.Renown.Regional.IRenownTrackerService
    {
    }
}
