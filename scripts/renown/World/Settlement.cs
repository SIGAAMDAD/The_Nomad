using System;
using System.Collections.Generic;
using System.Data;
using Godot;
using Renown.Thinkers;
using Renown.Thinkers.Occupations;
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
		private BuildingResourceProducer[] Producers;
		[Export]
		private float BirthRate = 0.0f;

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
		private MercenaryMaster MercenaryMaster;

		private List<Family> FamilyList = new List<Family>();
		private List<Building> BuildingList = new List<Building>();

		private System.Threading.Thread ThinkThread = null;
		private bool Quit = false;

		[Export]
		private float TaxationRate = 30.75f;

		private int MaxPopulation = 0;
		private int Population = 0;
		private HashSet<Thinker> Citizens = null;

		[Signal]
		public delegate void PopulationChangedEventHandler( int nCurrent );
		[Signal]
		public delegate void RequestedMoneyEventHandler( Settlement settlement, float nAmount );

		public float GetTaxationRate() => TaxationRate;
		public Road[] GetTradeRoutes() => TradeRoutes;
		public float GetBirthRate() => BirthRate;
		public Marketplace[] GetMarketplaces() => Markets;
		public List<Family> GetFamilies() => FamilyList;
		public int GetPopulation() => Population;
		public Government GetGovernment() => Government;
		public List<Building> GetBuildings() => BuildingList;
		public MercenaryMaster GetMercenaryMaster() => MercenaryMaster;

		public int GetNumThinkerInOccupation( OccupationType job ) {
			int count = 0;
			foreach ( var thinker in Citizens ) {
				if ( thinker.GetOccupation().GetOccupationType() == job ) {
					count++;
				}
			}
			return count;
		}

		public OccupationType CalcJob( Random random, Thinker thinker, out Building workPlace ) {
			OccupationType job = OccupationType.None;
			Span<int> chances = stackalloc int[ (int)OccupationType.Count ];

			for ( OccupationType occupation = OccupationType.None; occupation < OccupationType.Count; occupation++ ) {
				chances[ (int)occupation ] = Constants.JobChances_SocioEconomicStatus[ occupation ][ thinker.GetSocietyRank() ];
			}

			if ( thinker.GetSocietyRank() == SocietyRank.Lower ) {
				if ( Type == SettlementType.City ) {
					// ...child labor
					if ( thinker.GetAge() < 14 && thinker.GetAge() > 5 && random.Next( 0, 100 ) >= 75 ) {
						job = OccupationType.Industry;
					}
				}
			}
			for ( int i = 0; i < chances.Length; i++ ) {
				int rand = random.Next( 0, 100 );
				if ( rand <= chances[i] ) {
					job = (OccupationType)i;
				}
			}

			workPlace = null;
			if ( job == OccupationType.Industry ) {
				for ( int i = 0; i < BuildingList.Count; i++ ) {
					if ( BuildingList[i] is BuildingResourceProducer producer && producer != null ) {
						producer.AddWorker( thinker );
						workPlace = producer;
					}
				}
			}

			return job;
		}
		public void AddFamily( Family family ) {
			FamilyList.Add( family );
		}

		public float GetSocietyRankMaxPercentage( SocietyRank rank ) {
			return SocietyScale[ rank ];
		}
		public float GetPercentageOfSocietyRank( SocietyRank rank ) {
			return Util.CalcPercentage( GetNumberOfSocietyRank( rank ), Citizens.Count );
		}
		public int GetNumberOfSocietyRank( SocietyRank min, SocietyRank max ) {
			int count = 0;
			for ( int i = 0; i < FamilyList.Count; i++ ) {
				if ( FamilyList[i].GetSocietyRank() >= min && FamilyList[i].GetSocietyRank() <= max ) {
					count += FamilyList[i].GetMemberCount();
				}
			}
			return count;
		}
		public int GetNumberOfSocietyRank( SocietyRank rank ) {
			int count = 0;
			for ( int i = 0; i < FamilyList.Count; i++ ) {
				if ( FamilyList[i].GetSocietyRank() == rank ) {
					count += FamilyList[i].GetMemberCount();
				}
			}
			return count;
		}

		public void AssignHouse( Family family ) {
			for ( int i = 0; i < BuildingList.Count; i++ ) {
				if ( BuildingList[i] is BuildingHouse house && house != null ) {
					if ( !house.HasOwner() ) {
						house.SetOwner( family );
						family.SetHome( house );
						return;
					}
				}
			}
		}

		public void ForEachBuildings( System.Action<Building> callback, bool bThreaded = true ) {
			if ( bThreaded ) {
				System.Threading.Tasks.Parallel.ForEach( BuildingList, callback );
			} else {
				for ( int i = 0; i < BuildingList.Count; i++ ) {
					callback( BuildingList[i] );
				}
			}
		}
		public void ForEachBuildingsValid( System.Action<Building> callback, bool bThreaded = true ) {
			if ( bThreaded ) {
				System.Threading.Tasks.Parallel.ForEach( BuildingList, ( Building building ) => {
					if ( building.GetState() == BuildingState.Stable ) {
						callback( building );
					}
				} );
			} else {
				for ( int i = 0; i < BuildingList.Count; i++ ) {
					if ( BuildingList[i].GetState() == BuildingState.Stable ) {
						callback( BuildingList[i] );
					}
				}
			}
		}

		public void AddThinker( Thinker thinker ) {
			CallDeferred( "AddToPopulation", thinker );
		}
		public void AddToPopulation( Thinker thinker ) {
			if ( Citizens.Contains( thinker ) ) {
				return;
			}
			Citizens.Add( thinker );
		}
		public void RemoveFromPopulation( Thinker thinker ) {
			if ( !Citizens.Contains( thinker ) ) {
				return;
			}
			Citizens.Add( thinker );
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

				//
				// save economy state
				//
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

		public override void _ExitTree() {
			base._ExitTree();

			Quit = true;
		}
		public override void _Ready() {
			base._Ready();

			Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Buildings" );
			MaxPopulation = 0;
			for ( int i = 0; i < nodes.Count; i++ ) {
				Building building = nodes[i] as Building;
				if ( building.GetLocation() == this ) {
					if ( building.GetBuildingType() == BuildingType.House ) {
						MaxPopulation += ( building as BuildingHouse ).MaxPeople;
					}
					BuildingList.Add( building );
				}
			}

			Citizens = new HashSet<Thinker>( MaxPopulation );

			ProcessMode = ProcessModeEnum.Disabled;

			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				OnGenerateThinkers();
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
			float totalUncollected = 0.0f;

			foreach ( var citizen in Citizens ) {
				float expected;
				float paid;

				citizen.PayTaxes( out expected, out paid );
				
				totalCollected += paid;
				totalUncollected += expected - paid;
			}

			return totalCollected;
		}

		private void Think() {
			while ( !Quit ) {
				System.Threading.Thread.Sleep( ThreadSleep );
			}
		}
	};
};