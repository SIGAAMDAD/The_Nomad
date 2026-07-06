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
using System.Runtime.CompilerServices;
using Nomad.Core.Numerics;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Gameplay.Runtime.Story.QuestTree
{
    internal sealed class QuestTreeNodeIndex
    {
        private int[] _globalToNode;

        public QuestTreeNodeIndex( int initialCapacity )
        {
            if ( initialCapacity < 1 ) {
                initialCapacity = 1;
            }

            _globalToNode = new int[initialCapacity];
        }

        public void Register( QuestDefinitionId questId, int nodeIndex )
        {
            if ( !questId.IsValid ) {
                return;
            }

            ulong raw = questId.Value;
            EnsureCapacity( raw );
            _globalToNode[(int)raw] = nodeIndex + 1;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryResolve( QuestDefinitionId questId, out int nodeIndex )
        {
            if ( !questId.IsValid ) {
                nodeIndex = -1;
                return false;
            }

            ulong raw = questId.Value;
            if ( raw >= (ulong)_globalToNode.Length ) {
                nodeIndex = -1;
                return false;
            }

            int encoded = _globalToNode[(int)raw];
            if ( encoded == 0 ) {
                nodeIndex = -1;
                return false;
            }

            nodeIndex = encoded - 1;
            return true;
        }

        private void EnsureCapacity( ulong raw )
        {
            if ( raw < (ulong)_globalToNode.Length ) {
                return;
            }

            if ( raw > int.MaxValue - 1 ) {
                throw new InvalidOperationException( "InternString id exceeded quest tree node index capacity." );
            }

            int required = (int)raw + 1;
            int capacity = MemoryMath.NextPowerOfTwo( required );
            Array.Resize( ref _globalToNode, capacity );
        }
    };
};
