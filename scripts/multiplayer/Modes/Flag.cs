using Godot;
using Renown;

namespace Multiplayer.Modes {
	public partial class Flag : InteractionItem {
		[Export]
		private Team Team;

		[Signal]
		public delegate void StolenEventHandler( Entity source );
		[Signal]
		public delegate void ReturnedEventHandler( Entity source );
		[Signal]
		public delegate void CapturedEventHandler( Entity source );

		private void OnFlagCaptured( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Entity entity && entity != null ) {
				if ( (Team)entity.GetMeta( "Team" ) != Team ) {
					entity.Die += ( source, target ) => {
						// flag drop

					};
					Reparent( entity );
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			// only the host runs the objective logic
			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnFlagCaptured ) );
		}
	};
};