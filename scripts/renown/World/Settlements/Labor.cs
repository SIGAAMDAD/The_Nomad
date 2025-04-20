using System;
using System.Collections.Generic;
using Godot;
using Renown.World.Buildings;
using Steamworks;

namespace Renown.World.Settlements {
	public enum LaborCategoryType : uint {
		FoodProduction,
		IndustryCommerce,
		Education,
		Military,
		Government,

		Count
	};
	public struct LaborCategory {
		public LaborCategoryType Type;
		public int WorkersNeeded;
		public int WorkersAllocated;
		public int Buildings;
		public int Priority;
		public int TotalHousesCovered;
	};

	public class Labor {
		public Settlement Owner;

		public int Wages;
		public int WorkersAvailable;
		public int WorkersEmployed;
		public int WorkersUnemployed;
		public int WorkersNeeded;
		public int UnemployedPercentage;
		public int UnemployedPercentageForGoverment;
		public Random Random = new Random();
		public LaborCategory[] Categories = new LaborCategory[ (int)LaborCategoryType.Count ];

		private static readonly Dictionary<BuildingType, LaborCategoryType> BuildingLaborType = new Dictionary<BuildingType, LaborCategoryType>{
			{ BuildingType.Farm, LaborCategoryType.FoodProduction },
			{ BuildingType.Blacksmith, LaborCategoryType.IndustryCommerce },
			{ BuildingType.WeaponSmith, LaborCategoryType.Military },
			{ BuildingType.TaxOffice, LaborCategoryType.Government },
			{ BuildingType.School, LaborCategoryType.Education }
		};
		private static readonly Dictionary<LaborCategoryType, int> DefaultPriority = new Dictionary<LaborCategoryType, int>{
			{ LaborCategoryType.Military, 4 },
			{ LaborCategoryType.Government, 4 },
			{ LaborCategoryType.IndustryCommerce, 3 },
			{ LaborCategoryType.FoodProduction, 3 },
			{ LaborCategoryType.Education, 1 },
		};

		private static LaborCategoryType CategoryForBuilding( Building building ) {
			if ( BuildingLaborType.TryGetValue( building.GetBuildingType(), out LaborCategoryType type ) ) {
				return type;
			}
			return LaborCategoryType.Count;
		}

		public int RaiseWages() {
			if ( Wages >= 45 ) {
				return 0;
			}
			Wages += 1 + ( Random.Next() & 3 );
			if ( Wages > 45 ) {
				Wages = 45;
			}
			return Wages;
		}
		public int LowerWages() {
			if ( Wages <= 5 ) {
				return 0;
			}
			Wages -= 1 + ( Random.Next() & 3 );
			return 1;
		}
		public void ChangeWages( int nAmount ) {
			Wages += nAmount;
			Wages = Math.Clamp( Wages, 0, 100 );
		}
		public int WorkersAllocated( LaborCategoryType category ) {
			return Categories[ (int)category ].WorkersAllocated;
		}
		public void CalculateWorkers( int numPlebs, int numPatricians ) {
			Population population = Owner.GetPopulation();

			population.PercentagePlebs = Util.CalcPercentage( numPlebs, numPlebs + numPatricians );
			population.WorkingAge = Util.CalcAdjustWithPercentage( population.PeopleOfWorkingAge(), 60 );
			WorkersAvailable = Util.CalcAdjustWithPercentage( population.WorkingAge, population.PercentagePlebs );
		}

		private static bool ShouldHaveWorkers( Building building, LaborCategoryType category, bool checkAccess ) {
			if ( category < 0 ) {
				return false;
			}

			if ( category == LaborCategoryType.FoodProduction || category == LaborCategoryType.IndustryCommerce ) {
				if ( building.GetState() != BuildingState.Stable ) {
					return false;
				}
			}

			return true;
		}
		public void CalculateWorkersNeededPerCategory() {
			for ( int category = 0; category < (int)LaborCategoryType.Count; category++ ) {
				Categories[ category ].Buildings = 0;
				Categories[ category ].TotalHousesCovered = 0;
				Categories[ category ].WorkersAllocated = 0;
				Categories[ category ].WorkersNeeded = 0;
			}

			Owner.ForEachBuildingsValid( ( building ) => {
				LaborCategoryType category = CategoryForBuilding( building );
				building.SetLaborCategory( category );

				if ( !ShouldHaveWorkers( building, category, false ) ) {
					return;
				}

				Categories[ (int)category ].WorkersNeeded += building.MaxLaborers;
				Categories[ (int)category ].Buildings++;
			} );
		}

		public void AllocateWorkersToCategories() {
			int workersNeeded = 0;
			for ( int i = 0; i < (int)LaborCategoryType.Count; i++ ) {
				Categories[i].WorkersAllocated = 0;
				workersNeeded += Categories[i].WorkersNeeded;
			}
			workersNeeded = 0;
			if ( workersNeeded <= WorkersAvailable ) {
				for ( int i = 0; i < (int)LaborCategoryType.Count; i++ ) {
					Categories[ i ].WorkersAllocated = Categories[ i ].WorkersNeeded;
				}
				WorkersEmployed = WorkersNeeded;
			}
			else {
				// not enough workers
				int available = WorkersAvailable;

				// distribute by government-defined priority
				for ( int p = 1; p <= (int)LaborCategoryType.Count && available > 0; p++ ) {
					for ( int c = 0; c < (int)LaborCategoryType.Count; c++ ) {
						if ( p == Categories[c].Priority ) {
							int toAllocate = Categories[c].WorkersNeeded;
							if ( toAllocate > available ) {
								toAllocate = available;
							}
							Categories[ c ].WorkersAllocated = toAllocate;
							available -= toAllocate;
							break;
						}
					}
				}

				// (sort of) round-robin distribution over unprioritized categories:
				int guard = 0;
				do {
					guard++;
					if ( guard >= WorkersAvailable ) {
						break;
					}

					for ( int p = 0; p < (int)LaborCategoryType.Count; p++ ) {
						int category = DefaultPriority[ (LaborCategoryType)p ];
						if ( Categories[ category ].Priority == 0 ) {
							int needed = Categories[ category ].WorkersNeeded - Categories[ category ].WorkersAllocated;
							if ( needed > 0 ) {
								int toAllocate = DefaultPriority[ (LaborCategoryType)p ];
								if ( toAllocate > available ) {
									toAllocate = available;
								}
								if ( toAllocate > needed ) {
									toAllocate = needed;
								}
								Categories[ category ].WorkersAllocated += toAllocate;
								available -= toAllocate;
								if ( available <= 0 ) {
									break;
								}
							}
						}
					}
				} while ( available > 0 );

				WorkersEmployed = WorkersAvailable;
				for ( int i = 0; i < (int)LaborCategoryType.Count; i++ ) {
					WorkersNeeded += Categories[i].WorkersNeeded - Categories[i].WorkersAllocated;
				}
			}
			WorkersUnemployed = WorkersAvailable - WorkersEmployed;
			UnemployedPercentage = Util.CalcPercentage( WorkersUnemployed, WorkersAvailable );
		}

		public void SetPriority( int category, int newPriority ) {
			int oldPriority = Categories[ category ].Priority;
			if ( oldPriority == newPriority ) {
				return;
			}

			int shift;
			int fromPrio;
			int toPrio;

			if ( oldPriority <= 0 && newPriority > 0 ) {
				// shift all bigger than "newPriority" by one down (+1)
				shift = 1;
				fromPrio = newPriority;
				toPrio = 9;
			} else if ( oldPriority > 0 && newPriority <= 0 ) {
				// shift all bigger than "oldPriority" by one up (-1)
				shift = -1;
				fromPrio = oldPriority;
				toPrio = 9;
			} else if ( newPriority < oldPriority ) {
				// shift all between new and old by one down (+1)
				shift = 1;
				fromPrio = newPriority;
				toPrio = oldPriority;
			} else {
				// shift all between old and new by one up (-1)
				shift = -1;
				fromPrio = oldPriority;
				toPrio = newPriority;
			}

			Categories[ category ].Priority = newPriority;
			for ( int i = 0; i < (int)LaborCategoryType.Count; i++ ) {
				if ( i == category ) {
					continue;
				}
				int currentPriority = Categories[i].Priority;
				if ( fromPrio <= currentPriority && currentPriority <= toPrio ) {
					Categories[i].Priority += shift;
				}
			}
			AllocateWorkersToCategories();
		}
	};
};