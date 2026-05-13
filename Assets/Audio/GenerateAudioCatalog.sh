#!/bin/sh
dotnet run --project FmodAudioCatalogGenerator.csproj Banks/GUIDs.txt ../../Source/Game/Domain/AudioEventIdContants.cs Nomad.Game.Domain.Audio AudioEventIdConstants
rm -r bin/
rm -r obj/
