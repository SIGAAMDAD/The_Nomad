using Godot;

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

	public DoorState GetState() => State;
	public bool UseDoor( Player user ) {
		switch ( State ) {
		case DoorState.Locked: {
			Godot.Collections.Array<Resource> stacks = (Godot.Collections.Array<Resource>)user.GetInventory().Get( "stacks" );
			break; }
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
