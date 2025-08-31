/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using System.Collections.Generic;
using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public enum SettlementType : uint {
		Village,
		City,

		Count,
	};
	public partial class Settlement : WorldArea {
		public static DataCache<Settlement> Cache = null;

		[Export]
		private SettlementType Type;
		[Export]
		private float Treasury = 0;
		[Export]
		private Road[] TradeRoutes;

		private int PlayerRenownScore = 0;

		/// <summary>
		/// the one npc that doesn't use a renown thinker except for premade npcs.
		/// there will only ever be one merc master per settlement
		/// </summary>
		[Export]
		private Thinker MercenaryMaster;

		private int Population = 0;
		private int MaxPopulation = 0;

		private int PlayerRenown = 0;

		private HashSet<Politician> Politicians;

		[Signal]
		public delegate void PopulationChangedEventHandler( int nCurrent );
		[Signal]
		public delegate void RequestedMoneyEventHandler( Settlement settlement, float nAmount );

		public SettlementType GetSettlementType() => Type;
		public int GetPopulation() => Population;
		public Thinker GetMercenaryMaster() => MercenaryMaster;

		public override void Save() {
			base.Save();
		}
		public override void Load() {
			base.Load();
		}

		public override void _Ready() {
			base._Ready();

			Politicians = new HashSet<Politician>();

			ProcessMode = ProcessModeEnum.Pausable;

			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
			if ( !ArchiveSystem.IsLoaded() ) {
			}
		}
	};
};