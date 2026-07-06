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

namespace Nomad.Game.Sdk.Story.QuestTree
{
    public static class QuestNodeTypeExtensions
    {
        public static QuestNodeTypeInfo GetInfo(this QuestNodeType type)
        {
            return new QuestNodeTypeInfo(type, type.GetTraversalFlags());
        }

        public static QuestNodeTraversalFlags GetTraversalFlags(this QuestNodeType type)
        {
            switch (type)
            {
                case QuestNodeType.Entry:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.StartsTree;

                case QuestNodeType.Objective:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.AdvancesQuestProgress
                        | QuestNodeTraversalFlags.RequiresQuestCompletionToExit;

                case QuestNodeType.Choice:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.Branches
                        | QuestNodeTraversalFlags.RequiresQuestCompletionToExit;

                case QuestNodeType.Condition:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.Gates;

                case QuestNodeType.Wait:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.Waits
                        | QuestNodeTraversalFlags.RequiresQuestCompletionToExit;

                case QuestNodeType.Event:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.EmitsEvent;

                case QuestNodeType.Join:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.Joins;

                case QuestNodeType.Outcome:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.EndsTree
                        | QuestNodeTraversalFlags.RequiresQuestCompletionToExit;

                case QuestNodeType.Failure:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.EndsTree
                        | QuestNodeTraversalFlags.RequiresQuestFailureToExit;

                case QuestNodeType.Cleanup:
                    return QuestNodeTraversalFlags.CanEnter
                        | QuestNodeTraversalFlags.CanExit
                        | QuestNodeTraversalFlags.Cleanup
                        | QuestNodeTraversalFlags.HiddenByDefault;

                default:
                    return QuestNodeTraversalFlags.None;
            }
        }

        public static bool CanEnter(this QuestNodeType type)
        {
            return (type.GetTraversalFlags() & QuestNodeTraversalFlags.CanEnter) != 0;
        }

        public static bool CanExit(this QuestNodeType type)
        {
            return (type.GetTraversalFlags() & QuestNodeTraversalFlags.CanExit) != 0;
        }

        public static bool IsTerminal(this QuestNodeType type)
        {
            return (type.GetTraversalFlags() & QuestNodeTraversalFlags.EndsTree) != 0;
        }
    }
}
