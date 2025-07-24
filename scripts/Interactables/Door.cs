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
	[Export]
	private Sprite2D Closed;
	[Export]
	private Sprite2D Open;
	[Export]
	private Node Area;
	[Export]
	private Node2D Destination;

	public Node2D GetDestination() => Destination;
	public Node GetArea() => Area;

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
	public bool UseDoor( Player user, out string message ) {
		message = "";
		switch ( State ) {
		case DoorState.Locked:
			if ( !UserHasKey( user ) ) {
				message = "You don't have the required key";
				return false;
			}
			message = string.Format( "Used key {0}", Key.Get( "name" ) );
			State = DoorState.Unlocked;
			Closed?.Hide();
			Open?.Show();
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
			if ( reader == null ) {
				return;
			}
			State = (DoorState)reader.LoadByte( nameof( State ) );
			switch ( State ) {
			case DoorState.Locked:
				Open?.Hide();
				Closed?.Show();
				break;
			case DoorState.Unlocked:
				Open?.Show();
				Closed?.Hide();
				break;
			};
		}
	}

	public override void _Ready() {
		base._Ready();

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		if ( !IsInGroup( "Archive" ) ) {
			AddToGroup( "Archive" );
		}

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}
	}
};
