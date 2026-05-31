#!/bin/sh
dotnet run --project FmodAudioCatalogGenerator.csproj Banks/GUIDs.txt ../../Source/Game/Sdk/AudioEventIdConstants.cs Nomad.Game.Sdk.Audio AudioEventIdConstants
rm -r bin/
rm -r obj/
