/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using System.Runtime.CompilerServices;

namespace PlayerSystem {
	/*
	===================================================================================
	
	BlacksmithWeaponUpgrade
	
	===================================================================================
	*/
	
	public partial class BlacksmithWeaponUpgrade : VBoxContainer {
		private HBoxContainer Cloner;

		/*
		===============
		OnWeaponSelected
		===============
		*/
		private static void OnWeaponSelected( InputEvent @event ) {
			if ( !( @event is InputEventMouseButton mouseButton && mouseButton != null ) ) {
				return;
			} else if ( mouseButton.ButtonIndex != MouseButton.Left || !mouseButton.Pressed ) {
				return;
			}
		}

		/*
		===============
		OnFillData
		===============
		*/
		private void OnFillData() {
			Godot.Collections.Dictionary<int, WeaponEntity>? weapons = LevelData.Instance.ThisPlayer.Inventory.WeaponsStack;

			foreach ( var weapon in weapons ) {
				HBoxContainer container = Cloner.Duplicate() as HBoxContainer;

				{
					TextureRect rect = container.GetChild( 0 ).GetChild( 0 ) as TextureRect;
					rect.Texture = weapon.Value.Icon;
				}
				{
					Label name = container.GetChild( 1 ) as Label;
					if ( weapon.Value.Level > 0 ) {
						name.Text = $"{weapon.Value.Data.Get( "name" ).AsString()}+{weapon.Value.Level}";
					} else {
						name.Text = weapon.Value.Data.Get( "name" ).AsString();
					}
				}
				container.GuiInput += OnWeaponSelected;

				AddChild( container );
			}
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

			Cloner = GetNode<HBoxContainer>( "Cloner" );

			Connect( SignalName.VisibilityChanged, Callable.From( OnFillData ) );
		}
	};
};