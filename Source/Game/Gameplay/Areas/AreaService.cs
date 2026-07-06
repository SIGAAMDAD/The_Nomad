/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Core.Events;
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Sdk.Areas;
using Nomad.Game.Sdk.Events.World;
using Nomad.Save.Services;

namespace Nomad.Game.Gameplay.Areas
{
    /// <summary>
    /// Gameplay-facing mutation service for authored areas.
    /// </summary>
    internal class AreaService
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.World", PayloadName = "AreaPlayerStatusChangedEventArgs")]
        [EventPayload("Id", typeof(Guid), Order = 1)]
        [EventPayload("NewStatus", typeof(AreaPlayerStatus), Order = 2)]
        public IGameEvent<AreaPlayerStatusChangedEventArgs> PlayerStatusChanged => _playerStatusChanged;
        private readonly IGameEvent<AreaPlayerStatusChangedEventArgs> _playerStatusChanged = default;

        private readonly CompiledAreaDatabase _areaDatabase;
        private readonly AreaRuntimeState _runtimeState;

        public AreaService(
            IGameEventRegistryService eventFactory,
            CompiledAreaDatabase? areaDatabase = null)
        {
            _areaDatabase = areaDatabase ?? CompiledAreaDatabase.Empty;
            _runtimeState = new AreaRuntimeState(_areaDatabase);

            eventFactory
                .GetEvent<SaveBeginEventArgs>(SaveBeginEventArgs.Name, SaveBeginEventArgs.NameSpace)
                .Subscribe(OnSaveBegin);
        }

        public bool TryGetStatus(AreaDefinitionId areaId, out AreaPlayerStatus status)
        {
            if (!_areaDatabase.TryGetIndex(areaId, out ushort index)) {
                status = default;
                return false;
            }

            status = _runtimeState.GetStatus(index);
            return true;
        }

        public bool TrySetStatus(AreaDefinitionId areaId, AreaPlayerStatus status)
        {
            if (!_areaDatabase.TryGetIndex(areaId, out ushort index)) {
                return false;
            }

            AreaPlayerStatus previous = _runtimeState.GetStatus(index);

            if (previous == status) {
                return true;
            }

            _runtimeState.SetStatus(index, status);
            _playerStatusChanged?.Publish(new AreaPlayerStatusChangedEventArgs(Guid.Empty, status));
            return true;
        }

        public bool TryMarkDiscovered(AreaDefinitionId areaId)
        {
            return TryPromoteStatus(areaId, AreaPlayerStatus.Discovered);
        }

        public bool TryMarkVisited(AreaDefinitionId areaId)
        {
            return TryPromoteStatus(areaId, AreaPlayerStatus.Visited);
        }

        public bool TryMarkCompleted(AreaDefinitionId areaId)
        {
            return TrySetStatus(areaId, AreaPlayerStatus.Completed);
        }

        public bool TrySetProgress(
            AreaDefinitionId areaId,
            ushort landmarksCompleted,
            ushort activitiesCompleted,
            ushort collectiblesFound,
            ushort fastTravelPointsUnlocked)
        {
            if (!_areaDatabase.TryGetIndex(areaId, out ushort index)) {
                return false;
            }

            ref AreaProgressState progress = ref _runtimeState.GetProgress(index);
            progress.LandmarksCompleted = landmarksCompleted;
            progress.ActivitiesCompleted = activitiesCompleted;
            progress.CollectiblesFound = collectiblesFound;
            progress.FastTravelPointsUnlocked = fastTravelPointsUnlocked;

            ref readonly CompiledAreaDefinition area = ref _areaDatabase.GetByIndex(index);

            if (area.ProgressTotal > 0 && progress.CompletedTotal >= area.ProgressTotal) {
                _runtimeState.SetStatus(index, AreaPlayerStatus.Completed);
            }

            return true;
        }

        public bool TryGetCompletionRatio(AreaDefinitionId areaId, out float ratio)
        {
            if (!_areaDatabase.TryGetIndex(areaId, out ushort index)) {
                ratio = 0.0f;
                return false;
            }

            ref readonly CompiledAreaDefinition area = ref _areaDatabase.GetByIndex(index);

            if (area.ProgressTotal == 0) {
                ratio = _runtimeState.GetStatus(index) == AreaPlayerStatus.Completed ? 1.0f : 0.0f;
                return true;
            }

            AreaProgressState progress = _runtimeState.GetProgress(index);
            ratio = Math.Clamp((float)progress.CompletedTotal / area.ProgressTotal, 0.0f, 1.0f);
            return true;
        }

        private bool TryPromoteStatus(AreaDefinitionId areaId, AreaPlayerStatus minimumStatus)
        {
            if (!TryGetStatus(areaId, out AreaPlayerStatus current)) {
                return false;
            }

            if (current >= minimumStatus) {
                return true;
            }

            return TrySetStatus(areaId, minimumStatus);
        }

        private void OnSaveBegin(in SaveBeginEventArgs args)
        {
            lock (this) {
                var writer = args.Writer.AddSection("AreaData");
            }
        }
    }
}
