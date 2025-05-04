using System.Collections.Generic;
using Godot;

public partial class ModsMenu : Control {
	private Dictionary<StringName, ModMetadata> ModList;

	private List<string> GetModList( string directory ) {
		List<string> modList = new List<string>();

		return modList;
	}

	public override void _Ready() {
		base._Ready();
	}
};