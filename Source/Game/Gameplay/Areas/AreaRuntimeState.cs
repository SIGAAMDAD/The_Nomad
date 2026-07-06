/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Gameplay.Areas
{
    internal sealed class AreaRuntimeState
    {
        private readonly AreaPlayerStatus[] _statuses;
        private readonly AreaProgressState[] _progress;

        public int Count => _statuses.Length;

        public AreaRuntimeState(CompiledAreaDatabase areaDatabase)
        {
            int count = areaDatabase?.Count ?? 0;
            _statuses = new AreaPlayerStatus[count];
            _progress = new AreaProgressState[count];
        }

        public AreaPlayerStatus GetStatus(ushort areaIndex)
        {
            return _statuses[areaIndex];
        }

        public void SetStatus(ushort areaIndex, AreaPlayerStatus status)
        {
            _statuses[areaIndex] = status;
        }

        public ref AreaProgressState GetProgress(ushort areaIndex)
        {
            return ref _progress[areaIndex];
        }

        public ReadOnlySpan<AreaPlayerStatus> Statuses => _statuses;
        public ReadOnlySpan<AreaProgressState> Progress => _progress;
    }
}
