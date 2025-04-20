using Renown.World.Buildings;

namespace Renown.World.Settlements {
	public class Economy {
		public Settlement Owner;

		public float TreasuryValue;
		public int TaxPercentage;
		public int EstimatedTaxUncollected;
		public float EstimatedTaxIncome;
		public float EstimatedWages;
		public FinanceOverview LastYear;
		public FinanceOverview ThisYear;
		public float InterestSoFar;
		public int SalarySoFar;
		public float WagesSoFar;
		public int WageRatePaidThisYear;
		public int WageRatePaidLastYear;

		public void CalculateTotals() {
		}

		public bool IsOutOfMoney() {
			return TreasuryValue <= -5000;
		}
		public void UpdateEstimateTaxes() {
			Taxes taxes = Owner.GetTaxes();

			taxes.Monthly.CollectedCitizens = 0;
			taxes.Monthly.CollectedNobles = 0;

			Owner.ForEachBuildings( ( Building building ) => {
				if ( building is BuildingHouse house && house != null ) {
					if ( !house.TaxCoverage ) {
						return;
					}
					
					int houseTaxMultiplier = house.TaxMultiplier;

					if ( house.IsNobility() ) {
						taxes.Monthly.CollectedNobles += house.Population * houseTaxMultiplier;
					} else {
						taxes.Monthly.CollectedCitizens += house.Population * houseTaxMultiplier;
					}
				}
			} );
		}
		public void EstimateWages() {
			float monthlyWages = Owner.GetLabor().Wages * Owner.GetLabor().WorkersEmployed / 10.0f / 12.0f;
			ThisYear.Expenses.Wages = WagesSoFar;
			EstimatedWages = ( 12.0f - WorldTimeManager.Month ) * monthlyWages + WagesSoFar;
		}
		public void CollectMonthlyTaxes() {
			Console.PrintLine( string.Format( "Collecting montly taxes for {0}...", Owner.Name ) );

			Taxes taxes = Owner.GetTaxes();
			taxes.TaxedCitizens = 0;
			taxes.TaxedNobles = 0;
			taxes.UntaxedCitizens = 0;
			taxes.UntaxedNobles = 0;
			taxes.Monthly.UncollectedCitizens = 0;
			taxes.Monthly.CollectedCitizens = 0;
			taxes.Monthly.UncollectedNobles = 0;
			taxes.Monthly.CollectedNobles = 0;

			for ( HouseLevel level = HouseLevel.MudHut; level < HouseLevel.Count; level++ ) {
				
			}
			Owner.ForEachBuildings( ( building ) => {
				if ( building is BuildingHouse house && house != null ) {
					int tax = house.Population * house.TaxMultiplier;
					if ( house.TaxCoverage ) {
						if ( house.IsNobility() ) {
							taxes.TaxedNobles += house.Population;
							taxes.Monthly.CollectedNobles += tax;
						} else {
							taxes.TaxedCitizens += house.Population;
							taxes.Monthly.CollectedCitizens += tax;
						}

						house.TaxIncomeOrStorage += tax;
					} else {
						// Tax EVASION!
						if ( house.IsNobility() ) {
							taxes.UntaxedNobles += house.Population;
							taxes.Monthly.UncollectedNobles += tax;
						} else {
							taxes.UntaxedCitizens += house.Population;
							taxes.Monthly.UncollectedCitizens += tax;
						}
					}
				}
			} );

			// TODO: make a tax cut, write in a tax evasion renown event
			
			float taxCityDivider = 2;
			float collectedNobles = Util.CalcAdjustWithPercentage( taxes.Monthly.CollectedNobles / taxCityDivider, TaxPercentage );
			float collectedCitizens = Util.CalcAdjustWithPercentage( taxes.Monthly.CollectedCitizens / taxCityDivider, TaxPercentage );
			float collectedTotal = collectedNobles + collectedCitizens;

			taxes.Yearly.CollectedNobles += collectedNobles;
			taxes.Yearly.CollectedCitizens += collectedCitizens;
			taxes.Yearly.UncollectedNobles += Util.CalcAdjustWithPercentage( taxes.Monthly.UncollectedNobles / taxCityDivider, TaxPercentage );
			taxes.Yearly.UncollectedCitizens += Util.CalcAdjustWithPercentage( taxes.Monthly.UncollectedCitizens / taxCityDivider, TaxPercentage );

			TreasuryValue += collectedTotal;

			int totalPatricians = taxes.TaxedNobles + taxes.UntaxedNobles;
			int totalPlebs = taxes.TaxedCitizens + taxes.UntaxedCitizens;
			taxes.PercentageTaxedNobles = (sbyte)Util.CalcPercentage( taxes.TaxedNobles, totalPatricians );
			taxes.PercentageTaxedCitizens = (sbyte)Util.CalcPercentage( taxes.TaxedCitizens, totalPlebs );
			taxes.PercentageTaxedPeople = (sbyte)Util.CalcPercentage( taxes.TaxedNobles + taxes.TaxedCitizens, totalPatricians + totalPlebs );
		}
		public void PayMonthlyWages() {
			float wages = Owner.GetLabor().Wages * Owner.GetLabor().WorkersEmployed / 10.0f / 12.0f;
			TreasuryValue -= wages;
			WagesSoFar += wages;
			WageRatePaidThisYear += Owner.GetLabor().Wages;
		}
		public void PayMonthlyInterest() {
			if ( TreasuryValue < 0 ) {
				float interest = Util.CalcAdjustWithPercentage( -TreasuryValue, 10.0f ) / 12.0f;
				TreasuryValue -= interest;
				InterestSoFar += interest;
			}
		}
		public void PayMonthlySalary() {
			if ( IsOutOfMoney() ) {
				return;
			}
		}
		public void ResetTaxes() {
			Taxes taxes = Owner.GetTaxes();

			LastYear.Income.Taxes = taxes.Yearly.CollectedCitizens + taxes.Yearly.CollectedNobles;
			taxes.Yearly.CollectedCitizens = 0;
			taxes.Yearly.CollectedNobles = 0;
			taxes.Yearly.UncollectedCitizens = 0;
			taxes.Yearly.UncollectedNobles = 0;

			// reset tax income in building list
			for ( int i = 0; i < Owner.GetBuildingCount(); i++ ) {
				if ( Owner.GetBuilding( i ) is BuildingHouse house && house != null && house.GetState() == BuildingState.Stable ) {
					house.TaxIncomeOrStorage = 0;
				}
			}
		}

		public void AdvanceMonth() {
			CollectMonthlyTaxes();
			PayMonthlyWages();
			PayMonthlyInterest();
			PayMonthlySalary();
		}

		public void CopyAmountsToLastYear() {
		}
		public void AdvanceYear() {
			ResetTaxes();
		}
	};
};