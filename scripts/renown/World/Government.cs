using System.Formats.Asn1;
using System.Reflection.Metadata;
using Godot;

namespace Renown.World {
	public partial class Government : Faction {
		[Export]
		private Godot.Collections.Array<Settlement> TerritoryList;

		private uint LastCollectedMonth = 0;

		//
		// renown events
		//
		[Signal]
		public delegate void TerritoryCapturedEventHandler( Faction oldOwner, Faction newOwner, Settlement territory );

		public override void Save() {
			base.Save();

			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() + "_Government" ) ) {
				writer.SaveUInt( nameof( LastCollectedMonth ), LastCollectedMonth );

				writer.SaveInt( "TerritoryCount", TerritoryList.Count );
				for ( int i = 0; i < TerritoryList.Count; i++ ) {
					writer.SaveString( string.Format( "Territory{0}Hash", i ), TerritoryList[i].GetPath() );
				}
			}
		}
		public override void Load() {
			base.Load();

			using ( var reader = ArchiveSystem.GetSection( GetPath() + "_Government" ) ) {
				LastCollectedMonth = reader.LoadUInt( nameof( LastCollectedMonth ) );
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

		private void OnManageMoney() {
			for ( int i = 0; i < MemberList.Count; i++ ) {
				float wage = ( MemberList[i] as Thinker ).GetOccupation().GetWage();
				DecreaseMoney( wage );
				MemberList[i].IncreaseMoney( wage );
			}

			float collectedTotal = 0.0f;
			for ( int i = 0; i < TerritoryList.Count; i++ ) {
				collectedTotal += TerritoryList[i].CollectTaxes();
			}
			IncreaseMoney( collectedTotal );
		}

		public override void _Ready() {
			base._Ready();

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}

			LastCollectedMonth = WorldTimeManager.Month;
			WorldTimeManager.Instance.NewMonth += OnManageMoney;
		}
	};
};
