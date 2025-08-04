using Godot;

public partial class LightManager : Node {
	private MultiMeshInstance2D MeshManager;
	private Texture2D LightTexture;
	private CanvasItemMaterial LightMaterial;

	private void CreateLight( PointLight2D owner ) {
		Sprite2D sprite = new Sprite2D();
		sprite.Texture = LightTexture;
		sprite.Scale = new Vector2(
			owner.TextureScale,
			owner.TextureScale
		);
		sprite.Modulate = new Color {
			R = owner.Energy,
			G = owner.Energy,
			B = owner.Energy
		};
		sprite.Material = LightMaterial;

		sprite.ProcessMode = ProcessModeEnum.Pausable;
		owner.SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );

		owner.SetDeferred( PointLight2D.PropertyName.Enabled, false );
		owner.CallDeferred( MethodName.AddChild, sprite );
	}

	public void ApplyChanges() {
		void nodeIterator( Godot.Collections.Array<Node> children ) {
			for ( int i = 0; i < children.Count; i++ ) {
				nodeIterator( children[ i ].GetChildren() );
				if ( children[ i ] is PointLight2D light && light != null ) {
					if ( SettingsData.GetLightingQuality() > LightingQuality.VeryLow ) {
						switch ( SettingsData.GetShadowQuality() ) {
						case ShadowQuality.Off:
							light.SetDeferred( Light2D.PropertyName.ShadowEnabled, false );
							break;
						case ShadowQuality.Low:
						case ShadowQuality.Medium:
						case ShadowQuality.High:
						case ShadowQuality.Ultra:
							light.SetDeferred( Light2D.PropertyName.ShadowEnabled, true );
							light.SetDeferred( Light2D.PropertyName.ShadowFilter, (long)SettingsData.GetShadowFilterEnum() );
							light.SetDeferred( Light2D.PropertyName.ShadowFilterSmooth, SettingsData.GetShadowFilterSmooth() );
							break;
						};
					}
					switch ( SettingsData.GetLightingQuality() ) {
					case LightingQuality.VeryLow:
						CreateLight( light );
						break;
					case LightingQuality.Low:
					case LightingQuality.High:
						break;
					};
				}
			}
		}
		nodeIterator( GetChildren() );
	}

	public override void _Ready() {
		base._Ready();

		LightTexture = ResourceCache.GetTexture( "res://textures/point_light.dds" );

		LightMaterial = new CanvasItemMaterial();
		LightMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
		LightMaterial.LightMode = CanvasItemMaterial.LightModeEnum.LightOnly;

		SettingsData.Instance.SettingsChanged += ApplyChanges;

		ApplyChanges();
	}
};