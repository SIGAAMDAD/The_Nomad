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

using System;

namespace Nomad.Game.Sdk.Renown.Contract
{
    /// <summary>
    /// Extra parameters that can be added to a contract's terms of service.
    /// </summary>
    [Flags]
    public enum ContractFlags : ulong
    {
        None = 0,

        // Stealth / detection
        Ghost = 1UL << 0,              // No detection, no confirmed presence.
        Silent = 1UL << 1,             // No alarms, no loud weapons, no obvious disturbance.
        NoWitnesses = 1UL << 2,        // Nobody alive may report what happened.
        NoBodiesFound = 1UL << 3,      // Bodies must be hidden, dissolved, burned, moved, etc.
        NoEvidence = 1UL << 4,         // No shell casings, documents, broken locks, identifiable traces.

        // Lethality
        Extermination = 1UL << 5,      // Kill every hostile in the contract area.
        NonLethal = 1UL << 6,          // No killing the target/hostiles.
        TargetOnly = 1UL << 7,         // Only the named target may be harmed.
        NoCollateral = 1UL << 8,       // Civilians, bystanders, animals, property untouched.

        // Combat constraints
        NoFirearms = 1UL << 9,
        NoExplosives = 1UL << 10,
        NoMelee = 1UL << 11,
        NoMana = 1UL << 12,
        NoBulletTime = 1UL << 13,
        NoArmor = 1UL << 14,
        NoCompanions = 1UL << 15,

        // Method / presentation
        AccidentOnly = 1UL << 16,      // Must look accidental.
        PublicMessage = 1UL << 17,     // Must be visible/intimidating.
        MakeItUgly = 1UL << 18,        // Cruel client wants fear, gore, humiliation.
        CleanWork = 1UL << 19,         // Professional execution, minimal mess.
        FrameSomeone = 1UL << 20,      // Plant evidence implicating another faction/person.

        // Target handling
        BringAlive = 1UL << 21,
        BringDead = 1UL << 22,
        BringProof = 1UL << 23,
        BringHead = 1UL << 24,
        BringInsignia = 1UL << 25,
        ExtractUnharmed = 1UL << 26,
        InterrogateFirst = 1UL << 27,

        // Route / timing
        TimeSensitive = 1UL << 28,
        NightOnly = 1UL << 29,
        DaylightOnly = 1UL << 30,
        NoFastTravel = 1UL << 31,
        NoResting = 1UL << 32,
        DeepTerritory = 1UL << 33,

        // World simulation / faction pressure
        FactionInterferenceLikely = 1UL << 34,
        BountyCompetition = 1UL << 35,
        PoliticalTarget = 1UL << 36,
        ReligiousTarget = 1UL << 37,
        FamilyMatter = 1UL << 38,
        IllegalEvenForGuild = 1UL << 39,

        // Information
        PoorIntel = 1UL << 40,
        FalseIntel = 1UL << 41,
        UnknownTarget = 1UL << 42,
        MultipleTargets = 1UL << 43,
        MovingTarget = 1UL << 44,

        // Environmental
        HostileWeather = 1UL << 45,
        LowVisibility = 1UL << 46,
        HazardousArea = 1UL << 47,
        RestrictedZone = 1UL << 48,

        // Moral / trait hooks
        MercifulOpportunity = 1UL << 49,
        CruelClient = 1UL << 50,
        HonorableDuelRequested = 1UL << 51,
        WarCrimeRisk = 1UL << 52
    }
}
