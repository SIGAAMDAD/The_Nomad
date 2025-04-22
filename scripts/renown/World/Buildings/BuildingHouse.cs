using Godot;
using Renown.Thinkers;

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
		public static readonly int MaxHouseMembers = 30;

		private Family Family = null;
		
		[Export]
		private HouseLevel Level;
		public int MaxPeople;
		[Export]
		public int TaxIncomeOrStorage;
		public byte WaterSupply;
		public byte HouseHappiness;
		public bool CriminalActive;

		[Export]
		public bool TaxCoverage;

		public bool WaterRequired;
		public bool FoodRequired;
		public bool BeerRequired;

		[Export]
		public int CrimeRisk;
		[Export]
		public int TaxMultiplier;

		public bool HasOwner() => Family != null;
		public HouseLevel GetLevel() => Level;
		public bool IsNobility() => Family.GetSocietyRank() >= SocietyRank.Upper;

		public void SetOwner( Family family ) => Family = family;
		public Family GetFamily() => Family;

		public override void _Ready() {
			base._Ready();

			ProcessMode = ProcessModeEnum.Disabled;

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