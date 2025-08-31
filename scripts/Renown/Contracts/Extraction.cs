/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Renown.Thinkers;
using Renown.World;

namespace Renown.Contracts {
	public readonly struct ExtractionData {
		public readonly Thinker Target;
		public readonly string Name;
		public readonly WorldTimestamp DueDate;
		public readonly ContractFlags Flags;
		public readonly ContractType Type;
		public readonly float BasePay;
		public readonly WorldArea Area;
		public readonly Object Contractor;
		public readonly Faction Guild;

		public ExtractionData( string name, WorldTimestamp duedate, ContractFlags flags, ContractType type,
			float basePay, WorldArea area, Object contractor, Faction guild, Thinker Target )
		{
			Name = name;
			DueDate = duedate;
			Flags = flags;
			Type = type;
			BasePay = basePay;
			Area = area;
			Contractor = contractor;
			Guild = guild;
			this.Target = Target;
		}
	};

	public partial class Extraction : Contract {
		private readonly ExtractionData Data;

		public Extraction( in ExtractionData data )
			: base(
				name: data.Name,
				duedate: data.DueDate,
				flags: data.Flags,
				type: data.Type,
				basePay: data.BasePay,
				area: data.Area,
				contractor: data.Contractor,
				guild: data.Guild,
				totalPay: null
			)
		{ }
	};
};