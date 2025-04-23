using Godot;
using Steamworks;

namespace Renown.World {
	public partial class Government : Faction {
		private struct FinancialReport {
			public float IncomeTaxTotal;
			public float WagesPaidTotal;
			public float TotalLost;
			public float TotalIncrease;
			public float LastTotalLost;
			public float LastTotalIncrease;
		};

		[Export]
		private Godot.Collections.Array<Settlement> TerritoryList;

		private uint LastCollectedTime = 0;
		private FinancialReport EconomyStats = new FinancialReport();

		//
		// renown events
		//
		[Signal]
		public delegate void TerritoryCapturedEventHandler( Faction oldOwner, Faction newOwner, Settlement territory );

		public override void Save() {
			base.Save();

			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() + "_Government" ) ) {
				writer.SaveUInt( nameof( LastCollectedTime ), LastCollectedTime );

				writer.SaveInt( "TerritoryCount", TerritoryList.Count );
				for ( int i = 0; i < TerritoryList.Count; i++ ) {
					writer.SaveString( string.Format( "Territory{0}Hash", i ), TerritoryList[i].GetPath() );
				}
			}
		}
		public override void Load() {
			base.Load();

			using ( var reader = ArchiveSystem.GetSection( GetPath() + "_Government" ) ) {
				LastCollectedTime = reader.LoadUInt( nameof( LastCollectedTime ) );
			}
		}

		private void OnSettlementRequestedFunds( Settlement settlement, float nAmount ) {

		}
		public void AddTerritory( Settlement settlement ) {
			TerritoryList.Add( settlement );
			settlement.RequestedMoney += OnSettlementRequestedFunds;
		}
		public void LoseTerritory( Settlement settlement ) {
			TerritoryList.Remove( settlement );
			settlement.RequestedMoney -= OnSettlementRequestedFunds;
		}
		
		public override void PayWorker( float nIncomeTax, float nAmount, Thinker thinker ) {
			base.PayWorker( nIncomeTax, nAmount, thinker );
			EconomyStats.IncomeTaxTotal += nIncomeTax;
			EconomyStats.WagesPaidTotal += nAmount;
		}

		private void OnManageMoney() {
			if ( WorldTimeManager.Day - LastCollectedTime < 7 ) {
				return;
			}
			LastCollectedTime = WorldTimeManager.Day;

			float collectedTotal = 0.0f;
			float avgRate = 0.0f;

			for ( int i = 0; i < TerritoryList.Count; i++ ) {
				collectedTotal += TerritoryList[i].CollectTaxes();
				avgRate += TerritoryList[i].GetTaxationRate();
			}

			IncreaseMoney( collectedTotal );
			avgRate /= TerritoryList.Count;

			GD.Print( "Government Financial Report:" );
			GD.Print( "\t[Taxes]" );
			GD.Print( "\t\tIncome Tax Collected: " + EconomyStats.IncomeTaxTotal );
			GD.Print( "\t\tTotal Collected: " + collectedTotal );
			GD.Print( "\t\tAverage Taxation Rate: " + avgRate );
			GD.Print( "\t\tTotal Citizens: " + MemberList.Count );
			GD.Print( "\t[Wages]" );
			GD.Print( "\t\tTotal Wages Paid: " + EconomyStats.WagesPaidTotal );
			GD.Print( "\t[Worth]" );
			GD.Print( "\t\tGained: " + EconomyStats.TotalIncrease );
			GD.Print( "\t\tLost: " + EconomyStats.TotalLost );
			GD.Print( "\t\tNet Increase: " + EconomyStats.TotalIncrease / EconomyStats.LastTotalIncrease );
			GD.Print( "\t\tNet Loss: " + EconomyStats.TotalLost / EconomyStats.LastTotalLost );

			float change = EconomyStats.TotalIncrease - EconomyStats.TotalLost;
			if ( change > 0.0f ) {
				GD.Print( "\t\tNet Change: +" + change );
			} else {
				GD.Print( "\t\tNet Change: " + change );
			}

			GD.Print( "\tCurrent Funds: " + Money );

			EconomyStats.LastTotalIncrease = EconomyStats.TotalIncrease;
			EconomyStats.LastTotalLost = EconomyStats.TotalLost;

			EconomyStats.TotalIncrease = 0.0f;
			EconomyStats.TotalLost = 0.0f;
			EconomyStats.IncomeTaxTotal = 0.0f;
			EconomyStats.WagesPaidTotal = 0.0f;
		}

		public override void DecreaseMoney( float nAmount ) {
			base.DecreaseMoney( nAmount );
			EconomyStats.TotalLost += nAmount;
		}
		public override void IncreaseMoney( float nAmount ) {
			base.IncreaseMoney( nAmount );
			EconomyStats.TotalIncrease += nAmount;
		}

		public override void _Ready() {
			base._Ready();

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}

			LastCollectedTime = WorldTimeManager.Day;
			WorldTimeManager.Instance.DayTimeStart += OnManageMoney;

			EconomyStats.TotalIncrease = 0.0f;
			EconomyStats.TotalLost = 0.0f;
			EconomyStats.LastTotalIncrease = 1.0f;
			EconomyStats.LastTotalLost = 1.0f;
			EconomyStats.IncomeTaxTotal = 0.0f;
			EconomyStats.WagesPaidTotal = 0.0f;
		}
	};
};
