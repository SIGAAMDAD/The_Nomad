using Godot;
using PlayerSystem;

public enum DoorState : byte {
	Locked,
	Unlocked,

	Count
};

public partial class Door : InteractionItem {
	[Export]
	private CollisionShape2D InteractBody;
	[Export]
	private Resource Key;
	[Export]
	private DoorState State;

	private bool UserHasKey( Player user ) {
		Godot.Collections.Array<Resource> stacks = (Godot.Collections.Array<Resource>)user.GetInventory().Get( "stacks" );
		for ( int i = 0; i < stacks.Count; i++ ) {
			if ( (string)stacks[i].Get( "item_id" ) == (string)Key.Get( "id" ) ) {
				return true;
			}
		}
		return false;
	}

	public DoorState GetState() => State;
	public bool UseDoor( Player user ) {
		switch ( State ) {
		case DoorState.Locked:
			if ( !UserHasKey( user ) ) {
				HeadsUpDisplay.StartThoughtBubble( "You don't have the required key" );
				return false;
			}
			State = DoorState.Unlocked;
			return true;
		case DoorState.Unlocked:
			return true;
		default:
			Console.PrintError( "Door.UseDoor: invalid door state!" );
			break;
		};
		return false;
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			player.BeginInteraction( this );
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			player.EndInteraction();
		}
	}
	public override InteractionType GetInteractionType() {
		return InteractionType.Door;
	}

	private void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() ) ) {
			writer.SaveByte( nameof( State ), (byte)State );
		}
	}
	private void Load() {
		using ( var reader = ArchiveSystem.GetSection( GetPath() ) ) {
			State = (DoorState)reader.LoadByte( nameof( State ) );
		}
	}

	public override void _Ready() {
		AddChild( InteractBody );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		if ( !IsInGroup( "Archive" ) ) {
			AddToGroup( "Archive" );
		}

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}
	}
};
