using Godot;
using Renown.World.Settlements;

namespace Renown.World.Buildings {
	public enum BuildingType : uint {
		Mansion,
		House,
		Blacksmith,
		School,
		Factory,
		WeaponSmith,
		Shop,
		Farm,
		TaxOffice,
		Mining,

		Count
	};
	public enum BuildingState : uint {
		BuildingInProgress,
		Rubble,
		Stable,

		Count
	};
	public partial class Building : Node2D {
		private BuildingType Type;
		private BuildingState State;
		private LaborCategoryType LaborType;

		public int MaxLaborers;

		public BuildingType GetBuildingType() => Type;
		public BuildingState GetState() => State;

		public LaborCategoryType GetLaborCategory() => LaborType;
		public void SetLaborCategory( LaborCategoryType type ) => LaborType = type;
		
		public bool IsHouse() => Type == BuildingType.House;
		public bool IsMansion() => Type == BuildingType.Mansion;
		public bool IsBlacksmith() => Type == BuildingType.Blacksmith;
		public bool IsSchool() => Type == BuildingType.School;
		public bool IsFactory() => Type == BuildingType.Factory;
		public bool IsWeaponSmith() => Type == BuildingType.WeaponSmith;
		public bool IsShop() => Type == BuildingType.Shop;
		public bool IsFarm() => Type == BuildingType.Farm;
	};
};