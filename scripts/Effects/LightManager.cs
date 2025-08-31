/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Menus;
using ResourceCache;

/*
===================================================================================

LightManager

===================================================================================
*/
/// <summary>
/// Manages the creation and configuration of 2D lights within the scene.
/// <c>LightManager</c> handles the instantiation of light sprites for low-quality settings,
/// applies lighting and shadow changes based on user preferences, and responds to settings updates.
/// It integrates with the Godot engine and listens for changes in lighting and shadow quality,
/// updating all managed <see cref="PointLight2D"/> instances accordingly.
/// </summary>
/// <remarks>
/// Attach this node to your scene to enable dynamic light management. The manager automatically
/// updates lights when settings change and supports both high and low quality lighting modes.
/// </remarks>

public partial class LightManager : Node {
	private Texture2D LightTexture;
	private CanvasItemMaterial LightMaterial;

	/*
	===============
	CreateLight
	===============
	*/
	/// <summary>
	/// Creates a visual representation of a light for the specified <see cref="PointLight2D"/> owner.
	/// This method instantiates a <see cref="Sprite2D"/> using the configured light texture and material,
	/// sets its scale and color modulation based on the owner's energy and texture scale,
	/// and attaches it as a child to the owner. The owner's process mode and enabled state are also updated
	/// to ensure correct rendering behavior.
	/// </summary>
	/// <remarks>
	/// This method is used when lighting quality is set to very low, providing a simplified light effect.
	/// </remarks>
	/// <param name="owner">
	/// The <see cref="PointLight2D"/> instance to which the light sprite will be attached.
	/// </param>
	private void CreateLight( PointLight2D owner ) {
		Sprite2D sprite = new Sprite2D() {
			Texture = LightTexture,
			Scale = new Vector2() {
				X = owner.TextureScale,
				Y = owner.TextureScale
			},
			Modulate = new Color {
				R = owner.Energy,
				G = owner.Energy,
				B = owner.Energy
			},
			Material = LightMaterial,
			ProcessMode = ProcessModeEnum.Pausable
		};
		
		owner.SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );

		owner.SetDeferred( PointLight2D.PropertyName.Enabled, false );
		owner.CallDeferred( MethodName.AddChild, sprite );
	}

	/*
	===============
	ApplyChanges
	===============
	*/
	/// <summary>
	/// Applies any pending changes to the managed lights.
	/// This method updates the state of all lights managed by the <c>LightManager</c> instance,
	/// ensuring that any modifications (such as intensity, color, position, or activation state)
	/// are reflected in the scene or rendering context.
	/// </summary>
	/// <remarks>
	/// Call this method after making changes to light properties to ensure those changes take effect.
	/// </remarks>
	public void ApplyChanges() {
		void nodeIterator( Godot.Collections.Array<Node> children ) {
			for ( int i = 0; i < children.Count; i++ ) {
				nodeIterator( children[ i ].GetChildren() );
				if ( children[ i ] is PointLight2D light && light != null ) {
					if ( SettingsData.LightingQuality > LightingQuality.VeryLow ) {
						switch ( SettingsData.ShadowQuality ) {
							case ShadowQuality.Off:
								light.SetDeferred( Light2D.PropertyName.ShadowEnabled, false );
								break;
							case ShadowQuality.Low:
							case ShadowQuality.Medium:
							case ShadowQuality.High:
							case ShadowQuality.Ultra:
								light.SetDeferred( Light2D.PropertyName.ShadowEnabled, true );
								light.SetDeferred( Light2D.PropertyName.ShadowFilter, (long)SettingsData.ShadowFilterType );
								light.SetDeferred( Light2D.PropertyName.ShadowFilterSmooth, SettingsData.ShadowFilterSmooth );
								break;
						}
					}
					switch ( SettingsData.LightingQuality ) {
						case LightingQuality.VeryLow:
							CreateLight( light );
							break;
						case LightingQuality.Low:
						case LightingQuality.High:
							break;
					}
				}
			}
		}
		nodeIterator( GetChildren() );
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

		LightTexture = TextureCache.GetTexture( "res://textures/point_light.dds" );

		LightMaterial = new CanvasItemMaterial() {
			BlendMode = CanvasItemMaterial.BlendModeEnum.Add,
			LightMode = CanvasItemMaterial.LightModeEnum.LightOnly
		};

		SettingsData.Instance.SettingsChanged += ApplyChanges;

		ApplyChanges();
	}

	/*
	===============
	_ExitTree
	===============
	*/
	public override void _ExitTree() {
		base._ExitTree();

		SettingsData.Instance.SettingsChanged -= ApplyChanges;
	}
};