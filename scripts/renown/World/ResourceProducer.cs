using Godot;

namespace Renown.World {
	public partial class ResourceProducer : InteractionItem {
		[Export]
		private Settlement Location = null;
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
		
		private Timer ReplenishTimer;
		
		[Signal]
		public delegate void AgentEnterAreaEventHandler();
		[Signal]
		public delegate void AgentExitAreaEventHandler();
		
		private void OnAreaBodyShape2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is not CharacterBody2D ) {
				return;
			}
			body.Call( "BeginInteraction", this );
		}
		private void OnAreaBodyShape2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is not CharacterBody2D ) {
				return;
			}
			body.Call( "EndInteraction", this );
		}

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
		
		private void OnResourceReplenishTimerTimeout() {
			if ( !LocationIsStable ) {
				return;
			}
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

			ReplenishTimer = GetNode<Timer>( "ReplenishTimer" );
			ReplenishTimer.SetProcess( false );
			ReplenishTimer.SetProcessInternal( false );
			ReplenishTimer.Connect( "timeout", Callable.From( OnResourceReplenishTimerTimeout ) );
			
			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DEntered ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DExited ) );
		}
	};
};