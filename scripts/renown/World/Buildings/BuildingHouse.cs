using System;
using System.Collections.Generic;

namespace Renown.World.Buildings {
	public enum HouseLevel : uint {
		MudHut,
		Cottage,
		Residence,
		Manor,
		Estate,

		Count,
	};
	public partial class BuildingHouse : Building {
		public static readonly int MaxHouseMembers = 16;

		private HouseLevel Level;

		public List<Thinker> Members = new List<Thinker>();
		
		public int MaxPeople;
		public int Population;
		public int TaxIncomeOrStorage;
		public byte WaterSupply;
		public byte HouseHappiness;
		public bool CriminalActive;
		public bool TaxCoverage;

		public bool WaterRequired;
		public bool FoodRequired;
		public bool BeerRequired;

		public int CrimeRisk;

		public int TaxMultiplier;

		public int PopulationRoom() {
			return Math.Max( MaxPeople - Population, 0 ) - Population;
		}

		public HouseLevel GetLevel() => Level;
		public bool IsNobility() => Level >= HouseLevel.Manor;
		public bool AddMember( Thinker thinker ) {
			if ( Members.Count >= MaxPeople ) {
				return false;
			}
			Members.Add( thinker );
			return true;
		}

		public override void _Ready() {
			base._Ready();

			switch ( Level ) {
			case HouseLevel.MudHut:
				MaxPeople = MaxHouseMembers;
				break;
			case HouseLevel.Cottage:
				MaxPeople = 12;
				break;
			case HouseLevel.Residence:
				MaxPeople = 8;
				break;
			case HouseLevel.Manor:
			case HouseLevel.Estate:
				MaxPeople = 4;
				break;
			};
		}
	};
};