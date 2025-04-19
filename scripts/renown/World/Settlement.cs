using System;
using Godot;
using Renown.Thinkers;

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
		[Export]
		private Godot.Collections.Array<FamilyTree> FamilyList;

		private int Population = 0;

		protected System.Threading.Thread ThinkThread = null;
		protected bool Quit = false;

		public Road[] GetTradeRoutes() => TradeRoutes;
		public int GetPopulation() => Population;
		public float GetBirthRate() => BirthRate;
		public Marketplace[] GetMarketplaces() => Markets;
		public Godot.Collections.Array<FamilyTree> GetFamilyTrees() => FamilyList;

		private System.Threading.Thread WorkThread = null;
		private System.Threading.ReaderWriterLock LockObject = new System.Threading.ReaderWriterLock();

		public void AddThinker( Thinker thinker ) {
			GetTree().CurrentScene.GetNode( "Thinkers" ).CallDeferred( "add_child", thinker );
		}

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

			WorkThread = new System.Threading.Thread( Think );
			WorkThread.Priority = Importance;
			WorkThread.Start();

			if ( !IsInGroup( "Settlements" ) ) {
				AddToGroup( "Settlements" );
			}
			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				OnGenerateThinkers();
			}
		}

		private void Think() {
			while ( !Quit ) {
				System.Threading.Thread.Sleep( ThreadSleep );
			}
		}
	};
};