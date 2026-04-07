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
using System.Collections.Generic;
using Nomad.Game.Domain.Data.Player;

namespace Nomad.Game.Domain.Interfaces.Player {
	/// <summary>
	/// 
	/// </summary>
	public interface IPlayerFlagService : IDisposable {
		IReadOnlyList<string> CurrentFlags { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		bool GetFlags( PlayerFlags flags );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="flags"></param>
		void AddFlags( PlayerFlags flags );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="flags"></param>
		void RemoveFlags( PlayerFlags flags );
	};
};