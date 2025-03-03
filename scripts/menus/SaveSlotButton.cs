using Godot;
using System;

public partial class SaveSlotButton : Button {
	[Export]
	private int Slot = 0;

	public override void _Ready() {
		if ( ArchiveSystem.Instance.SlotExists( Slot ) ) {
			Text = "SLOT " + Slot.ToString();
		} else {
			Text = "UNUSED_SLOT";
		}
	}
};
