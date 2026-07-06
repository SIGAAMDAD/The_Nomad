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

using System.Runtime.CompilerServices;

namespace Nomad.Game.Gameplay.Story
{
	internal readonly struct ObjectiveId
	{
		public static readonly ObjectiveId Invalid = new ObjectiveId( -1 );

		public readonly int Value;

		public bool IsValid
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get
			{
				return Value >= 0;
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ObjectiveId( int value )
		{
			Value = value;
		}
	};
};
