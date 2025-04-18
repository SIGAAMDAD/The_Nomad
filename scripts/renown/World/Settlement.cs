using System;
using Godot;

namespace Renown.World {
	public partial class Settlement : WorldArea {
		public static DataCache<Settlement> Cache = null;

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

		private int Population = 0;

		public Road[] GetTradeRoutes() => TradeRoutes;
		public int GetPopulation() => Population;
		public float GetBirthRate() => BirthRate;
		public Marketplace[] GetMarketplaces() => Markets;

		public override void Save() {
			base.Save();

			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() ) ) {
				writer.SaveInt( "Population", Population );
				writer.SaveFloat( "BirthRate", BirthRate );
			}
		}
		public override void Load() {
			base.Load();

			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			Population = reader.LoadInt( "Population" );
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
				return;
			}

			int addPopulation = (int)( Population * BirthRate );
			Console.PrintLine( "Generating" + addPopulation.ToString() + " thinkers for " + AreaName + "..." );
			for ( int i = 0; i < addPopulation; i++ ) {
				Thinker thinker = new Thinker();
				GetTree().Root.GetNode<Node>( "World/Thinkers" ).AddChild( thinker );
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( SettingsData.GetNetworkingEnabled() ) {

			}
			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				OnGenerateThinkers();
			}
		}
	};
};