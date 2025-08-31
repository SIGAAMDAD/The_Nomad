using Godot;
using Interactables.Cutscenes;

namespace Interactables {
	/*
	===================================================================================
	
	CutsceneTrigger
	
	===================================================================================
	*/

	public partial class CutsceneTrigger : InteractionItem {
		[Export]
		private Cutscene Cutscene;
		[Export]
		private bool OneShot = false;

		public override InteractionType InteractionType => InteractionType.CutsceneTrigger;

		private bool Triggered = false;

		/*
		===============
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				if ( OneShot && Triggered ) {
					return;
				}
				Triggered = true;
				Cutscene.Start();
				player.BlockInput( true );
				Cutscene.Finished += () => player.BlockInput( false );
			}
		}

		/*
		===============
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		}

		/*
		===============
		Load
		===============
		*/
		private void Load() {
			using var reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			Triggered = reader.LoadBoolean( nameof( Triggered ) );
		}

		/*
		===============
		Save
		===============
		*/
		public void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter );

			writer.SaveBool( nameof( Triggered ), Triggered );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

			if ( IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( ArchiveSystem.IsLoaded() ) {
				Load();
			}
		}
	};
};