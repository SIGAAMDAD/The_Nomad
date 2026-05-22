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
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Domain.Interfaces.Inventory
{
	/// <summary>
	///
	/// </summary>
	public interface IItemInstanceRepository : IDisposable
	{
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TItemDefinition"></typeparam>
		/// <param name="itemId"></param>
		/// <returns></returns>
		IItemInstance<TItemDefinition>? Get<TItemDefinition>( ItemInstanceId itemId )
			where TItemDefinition : ItemDefinition;

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TItemDefinition"></typeparam>
		/// <param name="itemId"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		bool TryGet<TItemDefinition>( ItemInstanceId itemId, out IItemInstance<TItemDefinition>? instance )
			where TItemDefinition : ItemDefinition;
	};
};
