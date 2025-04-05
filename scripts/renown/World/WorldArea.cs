using Godot;

namespace Renown.World {
	public partial class WorldArea : Area2D {
		public static DataCache<WorldArea> Cache;

		[Export]
		protected StringName AreaName;

		public StringName GetAreaName() => AreaName;

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		private void OnProcessAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			CharacterBody2D data = body as CharacterBody2D;
			if ( data == null ) {
				return;
			}
			data.CallDeferred( "SetLocation", this );
		}
		private void OnProcessAreaBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			CharacterBody2D data = body as CharacterBody2D;
			if ( data == null ) {
				return;
			}
//			data.CallDeferred( "SetLocation", null );
		}

		public override void _Ready() {	
			base._Ready();

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnProcessAreaBodyShape2DEntered ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnProcessAreaBodyShape2DExited ) );

			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = 0;
			ProcessThreadMessages = ProcessThreadMessagesEnum.Messages;

			if ( SettingsData.GetNetworkingEnabled() ) {
			}
			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Locations" ) ) {
				AddToGroup( "Locations" );
			}
		}
    };
};