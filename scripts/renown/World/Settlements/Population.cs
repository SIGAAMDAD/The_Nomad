using System;
using System.Collections.Generic;
using Godot;
using Renown.World.Buildings;
using Steamworks;

namespace Renown.World.Settlements {
	public class Population {
		public Settlement Owner;

		public List<Thinker> People = new List<Thinker>();

		public int Current;
		public int LastDayCurrent;
		public int LastYear;
		public int SchoolAge;
		public int AcademyAge;
		public int WorkingAge;
		public int[] AtAge = new int[100];

		public bool YearlyUpdateRequested;
		public int YearlyBirths;
		public int YearlyDeaths;
		public int LostRemoval;
		public int LostHomeless;
		public int LastChange;
		public int TotalAllYears;
		public int TotalYears;
		public int AveragePerYear;
		public int HighestEver;
		public int TotalCapacity;
		
		public int PeopleInHuts;
		public int PeopleInResidences;
		public int PeopleInManors;
		public int PercentagePlebs;

		private static readonly int[] BirthsPerAgeDecennium= [ 0, 3, 16, 9, 2, 0, 0, 0, 0, 0 ];
		private static readonly int[,] DeathsPerHealthPerAgeDecennium = {
			{ 20, 10, 5, 10, 20, 30, 50, 85, 100, 100 },
			{ 15, 8, 4, 8, 16, 25, 45, 70, 90, 100 },
			{ 10, 6, 2, 6, 12, 20, 30, 55, 80, 90 },
			{ 5, 4, 0, 4, 8, 15, 25, 40, 65, 80 },
			{ 3, 2, 0, 2, 6, 12, 20, 30, 50, 70 },
			{ 2, 0, 0, 0, 4, 8, 15, 25, 40, 60 },
			{ 1, 0, 0, 0, 2, 6, 12, 20, 30, 50 },
			{ 0, 0, 0, 0, 0, 4, 8, 15, 20, 40 },
			{ 0, 0, 0, 0, 0, 2, 6, 10, 15, 30 },
			{ 0, 0, 0, 0, 0, 0, 4, 5, 10, 20 },
			{ 0, 0, 0, 0, 0, 0, 0, 2, 5, 10 }
		};

		private void ResetAges() {
			const int block = 64; // cacheline align
			int index = 0;
			int length = Math.Min( block, AtAge.Length );

			while ( index < length ) {
				AtAge[ index++ ] = 0;
			}

			length = AtAge.Length;
			while ( index > length ) {
				Buffer.BlockCopy( AtAge, 0, AtAge, index, Math.Min( block, length - index ) );
				index += block;
			}
		}
		public void Recalculate() {
			ResetAges();

			System.Threading.Tasks.Parallel.ForEach( People, ( thinker ) => { System.Threading.Interlocked.Increment( ref AtAge[ thinker.GetAge() ] ); } );

			Current = People.Count;
			HighestEver = Math.Max( Current, HighestEver );
		}
		public void YearlyRecalculate() {
			YearlyUpdateRequested = false;
			LastYear = Current;
			Recalculate();

			LostRemoval = 0;
			TotalAllYears += Current;
			TotalYears++;
			AveragePerYear = TotalAllYears / TotalYears;
		}
		public void YearlyUpdate() {
			YearlyCalculateBirths();
			YearlyRecalculate();
		}
		public void YearlyCalculateBirths() {
			YearlyBirths = 0;
			for ( int decennium = 9; decennium >= 0; decennium-- ) {
				int people = GetPeopleInAgeDecennium( decennium );
				int births = Util.CalcAdjustWithPercentage( people, BirthsPerAgeDecennium[ decennium ] );
				int added = AddToHouses( births );
				for ( int i = 0; i < added; i++ ) {
					ThinkerFactory.QueueThinker( Owner, 0 );
				}
				AtAge[ 0 ] += added;
				YearlyBirths += added;
			}
		}

		public int AddToHouses( int numPeople ) {
			int added = 0;
			Owner.ForEachBuildings( ( building ) => {
				if ( building is BuildingHouse house && house != null ) {
					if ( house.Population > 0 ) {
						int maxPeople = house.MaxPeople;
						if ( house.Population < maxPeople ) {
							added++;
							house.Population++;
						}
					}
				}
			} );
			return added;
		}
		public int RemoveFromHouses( int numPeople ) {
			int removed = 0;
			for ( int i = 1; i < Owner.GetBuildingCount() && removed < numPeople; i++ ) {
				if ( Owner.GetBuilding( i ) is BuildingHouse house && house != null && house.GetState() == BuildingState.Stable ) {
					if ( house.Population > 0 ) {
						removed++;
						house.Population--;
					}
				}
			}
			return removed;
		}

		public void EvictOvercrowded() {
			Owner.ForEachBuildings( ( building ) => {
				if ( building is BuildingHouse house && house != null ) {
					int populationRoom = house.PopulationRoom();
					if ( populationRoom >= 0 ) {
						return;
					}

					int numEvictions = -populationRoom;
					if ( numEvictions < house.Population ) {
						house.Population -= numEvictions;
					}
				}
			} );
		}
		public void CalculateWorkingPeople() {
			int numPeasants = 0;
			int numNobles = 0;

			Owner.ForEachBuildings( ( building ) => {
				if ( building is BuildingHouse house && house != null ) {
					if ( house.Population <= 0 ) {
						return;
					}

					if ( house.GetLevel() >= HouseLevel.Manor ) {
						System.Threading.Interlocked.Add( ref numNobles, house.Population );
					} else {
						System.Threading.Interlocked.Add( ref numPeasants, house.Population );
					}
				}
			} );
		}
		public int PeopleOfWorkingAge() {
			return GetPeopleInAgeDecennium( 2 ) + GetPeopleInAgeDecennium( 3 ) + GetPeopleInAgeDecennium( 4 ) + GetPeopleInAgeDecennium( 5 );
		}
		public void CalculateEducationalAge() {
			SchoolAge = GetPeopleAgedBetween( 0, 14 );
			AcademyAge = GetPeopleAgedBetween( 14, 21 );
		}
		public int GetPeopleAgedBetween( int min, int max ) {
			int pop = 0;
			for ( int i = min; i < max; i++ ) {
				if ( People[i].GetAge() >= min && People[i].GetAge() <= max ) {
					pop++;
				}
			}
			return pop;
		}
		public int PercentInWorkforce() {
			if ( Current == 0 ) {
				return 0;
			}
			return Util.CalcPercentage( Owner.GetLabor().WorkersAvailable, Current );
		}
		public int GetPeopleInAgeDecennium( int decennium ) {
			int pop = 0;
			int check = decennium * 10;
			for ( int i = 0; i < People.Count; i++ ) {
				if ( People[i].GetAge() > check && People[i].GetAge() < check + 10 ) {
					pop++;
				}
			}
			return pop;
		}
		public void UpdateDay() {
			if ( LastDayCurrent != Current ) {
				Owner.EmitSignal( "PopulationChanged", Current );
			}
			LastDayCurrent = Current;
		}
		public int PopulationAtAge( int age ) {
			int count = 0;
			for ( int i = 0; i < People.Count; i++ ) {
				if ( People[i].GetAge() == age ) {
					count++;
				}
			}
			return count;
		}
		public int AverageAge() {
			Recalculate();
			if ( Current == 0 ) {
				return 0;
			}

			int ageSum = 0;
			for ( int i = 0; i < 100; i++ ) {
				ageSum += ( AtAge[i] * i );
			}
			return ageSum / Current;
		}
	};
};