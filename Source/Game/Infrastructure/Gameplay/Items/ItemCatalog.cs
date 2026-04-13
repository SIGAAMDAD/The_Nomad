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

using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items {
	/*
	===================================================================================
	
	ItemCatalog
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class ItemCatalog {
		/*
		===============
		LoadItemBase
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="json"></param>
		/// <param name="definition"></param>
		public static ItemDefinition LoadItemBase( JsonElement json, ItemDefinition definition ) {
			return definition with {
				Weight = JsonLoader.TryGet( json, nameof( definition.Weight ), out float weight ) ? weight : 0.0f,
				BaseCost = JsonLoader.TryGet( json, nameof( definition.BaseCost ), out float baseCost ) ? baseCost : 0.0f,
				Name = JsonLoader.TryGet( json, nameof( definition.Name ), out string name ) ? name : $"Item#{definition.GetHashCode()}",
			};
		}
	};
};
