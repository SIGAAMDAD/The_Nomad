#if TOOLS
using Godot;

[Tool]
public partial class Plugin : EditorPlugin {
	public override void _EnterTree() {
		base._EnterTree();

		AddUITypes();
	}

	public override void _ExitTree() {
		base._ExitTree();
	}

	private void AddSceneTypes() {
	}

	private void AddUITypes() {
		AddCustomType( "NomadPanel", "Control", GD.Load<CSharpScript>( "res://addons/NomadFramework/NomadPanel.cs" ), null );
		AddCustomType( "NomadVerticalContainer", "VBoxContainer", GD.Load<CSharpScript>( "res://addons/NomadFramework/NomadVerticalContainer.cs" ), null );
		AddCustomType( "NomadHortizontalContainer", "HBoxContainer", GD.Load<CSharpScript>( "res://addons/NomadFramework/NomadHorizontalContainer.cs" ), null );
		AddCustomType( "NomadPresentationLayer", "CanvasLayer", GD.Load<CSharpScript>( "res://addons/NomadFramework/NomadPresentationLayer.cs" ), null );
		AddCustomType( "NomadButton", "Button", GD.Load<CSharpScript>( "res://addons/NomadFramework/NomadButton.cs" ), null );
		AddCustomType( "NomadText", "Label", GD.Load<CSharpScript>( "res://addons/NomadFramework/NomadText.cs" ), null );
	}
}
#endif