using Godot;
using Renown.World.Buildings;

namespace Renown.World {
	public partial class ResourceProducer : InteractionItem {
		[Export]
		private Settlement Location = null;
		[Export]
		private Building Building = null;
		[Export]
		private ResourceType Type;
		[Export]
		private uint MaxStorage = 0;
		[Export]
		private TradeRoute Route = null;
		[Export]
		private float ReplenishRate = 0.5f;
		[Export]
		private ResourceFactory Factory;
	
		private float ItemGeneration = 0.0f;
		private uint Storage = 0;
		private bool LocationIsStable = true;

		private uint LastReplenishTime = 0;
		
		[Signal]
		public delegate void AgentEnterAreaEventHandler();
		[Signal]
		public delegate void AgentExitAreaEventHandler();

		public void Save() {
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( GetPath() );

			writer.SaveUInt( "storage", Storage );
		}
		public void Load() {
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}
			Storage = reader.LoadUInt( "storage" );
		}
		
		private void OnResourceReplenish( uint day, uint hour, uint minute ) {
			if ( Storage < MaxStorage ) {
				ItemGeneration += ReplenishRate;
				if ( ItemGeneration >= 1.0f ) {
					ItemGeneration = 0.0f;
					Storage++;
				}
			}
		}
		
		public override void _Ready() {
			base._Ready();

			WorldTimeManager.Instance.TimeTick += OnResourceReplenish;
		}
	};
};