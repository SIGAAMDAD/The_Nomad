using Godot;
using Renown.Thinkers;
using Renown.World.Buildings;
using Renown.World.Settlements;

namespace Renown.World {
	public partial class Settlement : WorldArea {
		public static DataCache<Settlement> Cache = null;

		private Taxes Taxes = new Taxes();
		private Economy Economy = new Economy();
		private Labor Labor = new Labor();
		private Population Population = new Population();

		[Export]
		private int Health = 0;
		[Export]
		private int StartingReserves = 0;
		[Export]
		private Government Government;
		[Export]
		private Building[] Buildings;
		[Export]
		private Marketplace[] Markets;
		[Export]
		private Road[] TradeRoutes;
		[Export]
		private ResourceProducer[] Producers;
		[Export]
		private ResourceFactory[] Factories;
		[Export]
		private float BirthRate = 0.0f;
		[Export]
		private int MaxPopulation = 0;
		[Export]
		private Godot.Collections.Array<FamilyTree> FamilyList;
		[Export]
		public StringName[] NameCache;

		protected System.Threading.Thread ThinkThread = null;
		protected bool Quit = false;

		[Signal]
		public delegate void PopulationChangedEventHandler( int nCurrent );

		public Road[] GetTradeRoutes() => TradeRoutes;
		public float GetBirthRate() => BirthRate;
		public Marketplace[] GetMarketplaces() => Markets;
		public Godot.Collections.Array<FamilyTree> GetFamilyTrees() => FamilyList;
		public Population GetPopulation() => Population;
		public Taxes GetTaxes() => Taxes;
		public Labor GetLabor() => Labor;
		public Government GetGovernment() => Government;
		public int GetHealth() => Health;
		public int GetBuildingCount() => Buildings.Length;
		public Building GetBuilding( int nIndex ) => Buildings[ nIndex ];

		public void ForEachBuildings( System.Action<Building> callback, bool bThreaded = true ) {
			if ( bThreaded ) {
				System.Threading.Tasks.Parallel.ForEach( Buildings, callback );
			} else {
				for ( int i = 0; i < Buildings.Length; i++ ) {
					callback( Buildings[i] );
				}
			}
		}
		public void ForEachBuildingsValid( System.Action<Building> callback, bool bThreaded = true ) {
			if ( bThreaded ) {
				System.Threading.Tasks.Parallel.ForEach( Buildings, ( Building building ) => {
					if ( building.GetState() == BuildingState.Stable ) {
						callback( building );
					}
				} );
			} else {
				for ( int i = 0; i < Buildings.Length; i++ ) {
					if ( Buildings[i].GetState() == BuildingState.Stable ) {
						callback( Buildings[i] );
					}
				}
			}
		}

		public void AddThinker( Thinker thinker ) {
			GetTree().CurrentScene.GetNode( "Thinkers" ).CallDeferred( "add_child", thinker );
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

				writer.SaveInt( nameof( Population.Current ), Population.Current );
				writer.SaveInt( nameof( Population.LastDayCurrent ), Population.LastDayCurrent );
				writer.SaveInt( nameof( Population.LastYear ), Population.LastYear );
				writer.SaveInt( nameof( Population.SchoolAge ), Population.SchoolAge );
				writer.SaveInt( nameof( Population.AcademyAge ), Population.AcademyAge );
				writer.SaveInt( nameof( Population.WorkingAge ), Population.WorkingAge );

				//
				// save economy state
				//

				writer.SaveFloat( nameof( Economy.TreasuryValue ), Economy.TreasuryValue );
				writer.SaveInt( nameof( Economy.TaxPercentage ), Economy.TaxPercentage );
				writer.SaveFloat( nameof( Economy.InterestSoFar ), Economy.InterestSoFar );
				writer.SaveFloat( nameof( Economy.SalarySoFar ), Economy.SalarySoFar );
				writer.SaveFloat( nameof( Economy.WagesSoFar ), Economy.WagesSoFar );
				writer.SaveFloat( nameof( Economy.WageRatePaidLastYear ), Economy.WageRatePaidLastYear );
				writer.SaveFloat( nameof( Economy.WageRatePaidThisYear ), Economy.WageRatePaidThisYear );

				writer.SaveFloat( "LastYear.Income.Taxes", Economy.LastYear.Income.Taxes );
				writer.SaveFloat( "LastYear.Income.Exports", Economy.LastYear.Income.Exports );
				writer.SaveUInt( "LastYear.Income.GoldExtracted", Economy.LastYear.Income.GoldExtracted );
				writer.SaveFloat( "LastYear.Income.Donated", Economy.LastYear.Income.Donated );
				writer.SaveFloat( "LastYear.Income.Total", Economy.LastYear.Income.Total );
				writer.SaveFloat( "LastYear.Expenses.Imports", Economy.LastYear.Expenses.Imports );
				writer.SaveFloat( "LastYear.Expenses.Wages", Economy.LastYear.Expenses.Wages );
				writer.SaveFloat( "LastYear.Expenses.Construction", Economy.LastYear.Expenses.Construction );
				writer.SaveFloat( "LastYear.Expenses.Interest", Economy.LastYear.Expenses.Interest );
				writer.SaveFloat( "LastYear.Expenses.Salary", Economy.LastYear.Expenses.Salary );
				writer.SaveFloat( "LastYear.Expenses.Stolen", Economy.LastYear.Expenses.Stolen );
				writer.SaveFloat( "LastYear.Expenses.Debt", Economy.LastYear.Expenses.Debt );
				writer.SaveFloat( "LastYear.Expenses.Total", Economy.LastYear.Expenses.Total );
				writer.SaveFloat( "LastYear.NetInOut", Economy.LastYear.NetInOut );
				writer.SaveFloat( "LastYear.Balance", Economy.LastYear.Balance );

				writer.SaveFloat( "ThisYear.Income.Taxes", Economy.ThisYear.Income.Taxes );
				writer.SaveFloat( "ThisYear.Income.Exports", Economy.ThisYear.Income.Exports );
				writer.SaveUInt( "ThisYear.Income.GoldExtracted", Economy.ThisYear.Income.GoldExtracted );
				writer.SaveFloat( "ThisYear.Income.Donated", Economy.ThisYear.Income.Donated );
				writer.SaveFloat( "ThisYear.Income.Total", Economy.ThisYear.Income.Total );
				writer.SaveFloat( "ThisYear.Expenses.Imports", Economy.ThisYear.Expenses.Imports );
				writer.SaveFloat( "ThisYear.Expenses.Wages", Economy.ThisYear.Expenses.Wages );
				writer.SaveFloat( "ThisYear.Expenses.Construction", Economy.ThisYear.Expenses.Construction );
				writer.SaveFloat( "ThisYear.Expenses.Interest", Economy.ThisYear.Expenses.Interest );
				writer.SaveFloat( "ThisYear.Expenses.Salary", Economy.ThisYear.Expenses.Salary );
				writer.SaveFloat( "ThisYear.Expenses.Stolen", Economy.ThisYear.Expenses.Stolen );
				writer.SaveFloat( "ThisYear.Expenses.Debt", Economy.ThisYear.Expenses.Debt );
				writer.SaveFloat( "ThisYear.Expenses.Total", Economy.ThisYear.Expenses.Total );
				writer.SaveFloat( "ThisYear.NetInOut", Economy.ThisYear.NetInOut );
				writer.SaveFloat( "ThisYear.Balance", Economy.ThisYear.Balance );
			}
		}
		public override void Load() {
			base.Load();

			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			BirthRate = reader.LoadFloat( "BirthRate" );
		}

		public void OnGenerateThinkers() {
			Godot.Collections.Array<Node> thinkers = GetTree().GetNodesInGroup( "Thinkers" );

			Population.Current = 0;
			for ( int i = 0; i < thinkers.Count; i++ ) {
				Thinker thinker = thinkers[i] as Thinker;
				if ( thinker.GetLocation() == this ) {
					Population.Current++;
				}
			}
			if ( Population.Current >= MaxPopulation ) {
				Console.PrintLine( "Maximum population already reached for settlement " + Name );
				return;
			}

			int addPopulation = MaxPopulation;
			Console.PrintLine( "Generating " + addPopulation.ToString() + " thinkers for " + AreaName + "..." );
			for ( int i = 0; i < addPopulation; i++ ) {
				ThinkerFactory.QueueThinker( this );
			}

			Labor.AllocateWorkersToCategories();
			Economy.TreasuryValue = StartingReserves;
		}

		public void CollectTaxes() {
			Economy.CollectMonthlyTaxes();
		}

		public override void _ExitTree() {
			base._ExitTree();

			Quit = true;
		}
		public override void _Ready() {
			base._Ready();

			ThinkThread = new System.Threading.Thread( Think );
			ThinkThread.Priority = Importance;
			ThinkThread.Start();

			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				OnGenerateThinkers();
			}
		}
		public override void _Process( double delta ) {
			base._Process( delta );
			
			/*
			ImGui.Begin( "[Settlement] " + Name );
			if ( ImGui.CollapsingHeader( "Economy" ) ) {
				ImGui.Text( "Taxes:" );
				ImGui.Indent();
				{
					ImGui.Text( "TaxesNobles: " + Taxes.TaxesNobles );
					ImGui.Text( "TaxesCitizens: " + Taxes.TaxedCitizens );
					ImGui.Text( "Monthly.CollectedNobles: " + Taxes.Monthly.CollectedNobles );
					ImGui.Text( "Monthly.CollectedCitizens: " + Taxes.Monthly.CollectedCitizens );
					ImGui.Text( "Monthly.UncollectedNobles: " + Taxes.Monthly.UncollectedNobles );
					ImGui.Text( "Monthly.UncollectedCitizens: " + Taxes.Monthly.UncollectedCitizens );
					ImGui.Text( "PercentageTaxedCitizens: " + Taxes.PercentageTaxedCitizens );
					ImGui.Text( "PercentageTaxedNobles: " + Taxes.PercentageTaxedNobles );
					ImGui.Text( "PercentageTaxedPeople: " + Taxes.PercentageTaxedPeople );
					ImGui.Text( "UntaxedNobles: " + Taxes.UntaxedNobles );
					ImGui.Text( "UntaxedCitizens: " + Taxes.UntaxedCitizens );
				}
				ImGui.Unindent();
			}
			if ( ImGui.CollapsingHeader( "Buildings" ) ) {
				for ( int i = 0; i < Buildings.Length; i++ ) {
					ImGui.Text( "Building[" + Buildings[i].GetHashCode() + "]:" );
					ImGui.Text( "\tType: " + Buildings[i].GetBuildingType().ToString() );
				}
			}
			ImGui.End();
			*/
		}

		private void Think() {
			while ( !Quit ) {
				System.Threading.Thread.Sleep( ThreadSleep );
			}
		}
	};
};