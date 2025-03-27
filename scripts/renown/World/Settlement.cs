using Godot;

namespace Renown.World {
	public partial class Settlement : WorldArea {
		public static DataCache<Settlement> Cache;

		[Export]
		private TradeRoute[] TradeRoutes;
		[Export]
		private ResourceProducer[] Producers;
		[Export]
		private ResourceFactory[] Factories;

		public override void Save() {
			base.Save();
		}
		public override void Load() {
			base.Load();
		}

		public override void _Ready() {
			base._Ready();

			if ( SettingsData.GetNetworkingEnabled() ) {

			}
			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
		}
	};
};