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
using Renown.World.Buildings;

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
		private Government Government;
		[Export]
		private Marketplace[] Markets;
		[Export]
		private Road[] TradeRoutes;
		[Export]
		private float BirthRate = 0.0f;
		[Export]
		private float TaxationRate = 30.75f;

		private int PlayerRenownScore = 0;

		/// <summary>
		/// the statistics regarding the socioeconomic makeup of the settlement.
		/// will change over time the economy evolves
		/// </summary>
		[Export]
		private Godot.Collections.Dictionary<SocietyRank, float> SocietyScale;

		/// <summary>
		/// the one npc that doesn't use a renown thinker except for premade npcs.
		/// there will only ever be one merc master per settlement
		/// </summary>
		[Export]
		private Thinker MercenaryMaster;

		private int Population = 0;
		private int MaxPopulation = 0;

		private HashSet<Politician> Politicians;

		[Signal]
		public delegate void PopulationChangedEventHandler( int nCurrent );
		[Signal]
		public delegate void RequestedMoneyEventHandler( Settlement settlement, float nAmount );

		public SettlementType GetSettlementType() => Type;
		public float GetTaxationRate() => TaxationRate;
		public Road[] GetTradeRoutes() => TradeRoutes;
		public float GetBirthRate() => BirthRate;
		public Marketplace[] GetMarketplaces() => Markets;
		public int GetPopulation() => Population;
		public Government GetGovernment() => Government;
		public Thinker GetMercenaryMaster() => MercenaryMaster;

		public float GetSocietyRankMaxPercentage( SocietyRank rank ) {
			return SocietyScale[ rank ];
		}
		public int GetNumberOfSocietyRank( SocietyRank rank ) {
			return (int)( Population / SocietyScale[ rank ] );
		}

		public override void Save() {
			base.Save();

			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() ) ) {
				writer.SaveFloat( nameof( BirthRate ), BirthRate );
				if ( Government != null ) {
					writer.SaveString( nameof( Government ), Government.GetPath() );
				}

				//
				// population
				//

				writer.SaveInt( nameof( Population ), Population );
				writer.SaveFloat( nameof( BirthRate ), BirthRate );

				//
				// save economy state
				//
			}
		}
		public override void Load() {
			base.Load();

			using ( var reader = ArchiveSystem.GetSection( GetPath() ) ) {
				// save file compatibility
				if ( reader == null ) {
					return;
				}

				BirthRate = reader.LoadFloat( "BirthRate" );
			}
		}

		public void OnGenerateThinkers() {
			Godot.Collections.Array<Node> thinkers = GetTree().GetNodesInGroup( "Thinkers" );

			Population = 0;
			for ( int i = 0; i < thinkers.Count; i++ ) {
				Thinker thinker = thinkers[i] as Thinker;
				if ( thinker.GetLocation() == this ) {
					Population++;
				}
			}
			if ( Population >= MaxPopulation ) {
				Console.PrintLine( "Maximum population already reached for settlement " + Name );
				return;
			}

			int addPopulation = MaxPopulation;
			Console.PrintLine( "Generating " + addPopulation.ToString() + " thinkers for " + AreaName + "..." );
			for ( int i = 0; i < addPopulation; i++ ) {
				ThinkerFactory.QueueThinker( this );
			}
		}

		private void DecreaseMoney( float nAmount ) {
			Treasury -= nAmount;

			if ( Treasury < 0.0f ) {
				EmitSignalRequestedMoney( this, nAmount );
			}
		}

		public float CollectTaxes() {
			float totalCollected = 0.0f;

			totalCollected += Population * TaxationRate;

			return totalCollected;
		}

		public override void _Ready() {
			base._Ready();

			Politicians = new HashSet<Politician>();

			ProcessMode = ProcessModeEnum.Pausable;

			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				CallDeferred( "OnGenerateThinkers" );
			}
		}
	};
};