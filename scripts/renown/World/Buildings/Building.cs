using Godot;
using Renown.Thinkers;

namespace Renown.World.Buildings {
	public enum BuildingType : uint {
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
		Stable,
		BuildingInProgress,
		Rubble,

		Count
	};
	public partial class Building : Node2D {
		[Export]
		protected BuildingType Type;
		[Export]
		protected BuildingState State;
		[Export]
		protected OccupationType LaborType;
		[Export]
		protected Settlement Location;

		public int MaxLaborers;

		public Settlement GetLocation() => Location;

		public BuildingType GetBuildingType() => Type;
		public BuildingState GetState() => State;

		public OccupationType GetLaborCategory() => LaborType;
		public void SetLaborCategory( OccupationType type ) => LaborType = type;
		
		public bool IsHouse() => Type == BuildingType.House;
		public bool IsBlacksmith() => Type == BuildingType.Blacksmith;
		public bool IsSchool() => Type == BuildingType.School;
		public bool IsFactory() => Type == BuildingType.Factory;
		public bool IsWeaponSmith() => Type == BuildingType.WeaponSmith;
		public bool IsShop() => Type == BuildingType.Shop;
		public bool IsFarm() => Type == BuildingType.Farm;
	};
};