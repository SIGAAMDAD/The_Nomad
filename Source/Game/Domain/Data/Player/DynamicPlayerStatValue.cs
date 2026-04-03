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

using Nomad.Core.Util;

namespace Nomad.Game.Domain.Data.Player {
	/*
	===================================================================================
	
	DynamicPlayerStatValue
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public readonly struct DynamicPlayerStatValue {
		public InternString StatId => _statId;
		private readonly InternString _statId;

		public float Value => _value;
		private readonly float _value;

		private readonly StatLimits _limits;

		public DynamicPlayerStatValue( InternString statId, float value, StatLimits limits ) {
			_statId = statId;
			_value = value;
			_limits = limits;
		}
	};
};