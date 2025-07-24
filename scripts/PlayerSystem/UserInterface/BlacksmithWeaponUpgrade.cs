using Godot;

namespace PlayerSystem {
	public partial class BlacksmithWeaponUpgrade : VBoxContainer {
		private HBoxContainer Cloner;

		private void OnWeaponSelected( InputEvent @event ) {
			if ( !( @event is InputEventMouseButton mouseButton && mouseButton != null ) ) {
				return;
			} else if ( mouseButton.ButtonIndex != MouseButton.Left || !mouseButton.Pressed ) {
				return;
			}
		}
		private void OnFillData() {
			Godot.Collections.Dictionary<int, WeaponEntity> weapons = LevelData.Instance.ThisPlayer.GetWeaponsStack();

			foreach ( var weapon in weapons ) {
				HBoxContainer container = Cloner.Duplicate() as HBoxContainer;

				{
					TextureRect rect = container.GetChild( 0 ).GetChild( 0 ) as TextureRect;
					rect.Texture = weapon.Value.Icon;
				}
				{
					Label name = container.GetChild( 1 ) as Label;
					if ( weapon.Value.Level > 0 ) {
						name.Text = string.Format( "{0}+{1}", weapon.Value.Data.Get( "name" ).AsString(), weapon.Value.Level );
					} else {
						name.Text = weapon.Value.Data.Get( "name" ).AsString();
					}
				}
				container.GuiInput += OnWeaponSelected;

				AddChild( container );
			}
		}

		public override void _Ready() {
			base._Ready();

			Cloner = GetNode<HBoxContainer>( "Cloner" );

			Connect( SignalName.VisibilityChanged, Callable.From( OnFillData ) );
		}
	};
};