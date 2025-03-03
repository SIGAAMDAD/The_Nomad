using Godot;
using SaveSystem;

public partial class ArchiveSystem : Node {
	private static ArchiveSystem _Instance;
	public static ArchiveSystem Instance => _Instance;

	private const int SLOT_LIMIT = 3;

	private System.Collections.Generic.List<Slot> SlotList = new System.Collections.Generic.List<Slot>();
	private int CurrentSlot = 0;

	[Signal]
	public delegate void SaveGameBeginEventHandler();
	[Signal]
	public delegate void SaveGameEndEventHandler();

	public static void SaveGame() {
		Instance.EmitSignal( "SaveGameBegin" );
		Godot.Collections.Array<Node> nodes = Instance.GetTree().GetNodesInGroup( "Archive" );
		Instance.SlotList[ Instance.CurrentSlot ].Save( nodes );
		Instance.EmitSignal( "SaveGameEnd" );
	}
	public static bool LoadGame() {
		Godot.Collections.Array<Node> nodes = Instance.GetTree().GetNodesInGroup( "Archive" );
		return Instance.SlotList[ Instance.CurrentSlot ].Load( nodes );
	}

	public void SetSlot( int nSlot ) {
		CurrentSlot = nSlot;
	}
	public bool SlotExists( int nSlot ) {
		if ( SlotList.Count == 0 ) {
			LoadSlotMetadata();
		}
		return SlotList[ nSlot ].IsValid();
	}
	private void LoadSlotMetadata() {
		if ( SlotList.Count > 0 ) {
			return;
		}
		for ( int i = 0; i < SLOT_LIMIT; i++ ) {
			SlotList.Add( new Slot( i ) );
		}
	}

	public override void _EnterTree() {
		base._EnterTree();
		if ( _Instance != null ) {
			this.QueueFree();
		}
		_Instance = this;
	}
	public override void _Ready() {
		base._Ready();

		LoadSlotMetadata();
	}
};