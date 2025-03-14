using Godot;

/*
namespace Renown.World {
	public partial class ResourceProducer : InteractionItem {
		public enum Type : uint {
			Metal,
			Wood,
			Gunpowder,
			Drugflower,
			
			Count
		};
		
		[Export]
		private Settlement Location = null;
		[Export]
		private Resource Type = null;
		[Export]
		private uint MaxStorage = 0;
		[Export]
		private TradeRoute Route = null;
		
		private float ItemGeneration = 0.0f;
		private uint Storage = 0;
		private bool LocationIsStable = false;
		
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
		
		private void OnResourceReplenishTimerTimeout() {
			if ( !LocationIsStable ) {
				return;
			}
			if ( Storage < MaxStorage ) {
				ItemGeneration += ReplenishRate * (float)GetProcessDeltaTime();
				if ( ItemGeneration >= 1.0f ) {
					ItemGeneration = 0.0f;
					Storage++;
				}
			}
		}
		
		public override void _Ready() {
			ReplenishTimer = GetNode<Timer>( "ReplenishTimer" );
			ReplenishTimer.Connect( "timeout", Callable.From( OnResourceReplenishTimerTimeout ) );
			
			InteractArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DEntered ) );
			InteractArea.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DExited ) );
		}
	};
};
*/