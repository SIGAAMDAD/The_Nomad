using Godot;

namespace Renown.World {
	public partial class Government : Faction {
		[Export]
		private Godot.Collections.Array<Thinker> Members;
		[Export]
		private Godot.Collections.Array<Settlement> TerritoryList;

		private uint LastCollectedMonth = 0;

		//
		// renown events
		//
		[Signal]
		public delegate void TerritoryCapturedEventHandler( Faction oldOwner, Faction newOwner, Settlement territory );

		private void OnCheckTaxes() {
			if ( LastCollectedMonth != WorldTimeManager.Month ) {
				for ( int i = 0; i < TerritoryList.Count; i++ ) {
					TerritoryList[i].CollectTaxes();
				}
				LastCollectedMonth = WorldTimeManager.Month;
			}
		}

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
		}

		public override void _Ready() {
			base._Ready();

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}

			LastCollectedMonth = WorldTimeManager.Month;
			WorldTimeManager.Instance.DayTimeStart += OnCheckTaxes;
		}
	};
};
