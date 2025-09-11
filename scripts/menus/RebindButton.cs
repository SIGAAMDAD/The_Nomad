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

#if GUIDE_CS
using Godot;
using System.Linq;

namespace Menus {
	/*
	===================================================================================
	
	RebindButton
	
	===================================================================================
	*/
	/// <summary>
	/// Handles rebinding controls with GUIDE
	/// </summary>

	public partial class RebindButton : Control {
		private enum DeviceType {
			Keyboard = 1,
			Mouse = 2,
			Joy = 4
		};

		[Export]
		public StringName? ActionLabel { get; private set; } = "";
		[Export]
		public StringName? ActionName { get; private set; } = "";
		[Export]
		public StringName? DisplayCategory { get; private set; } = "";
		[Export]
		public Node? Action { get; private set; } = null;

		private Label? BindName;
		private RichTextLabel? Label;
		private Node? InputDetector;

		private void OnInputBindingDetected( Resource input, Resource item ) {
			GameEventBus.DisconnectAllForObject( InputDetector );

			if ( input == null ) {
				return;
			}

			Godot.Collections.Array<GodotObject> collisions = SettingsData.Remapper.Call( "get_input_collisions", item, input ).AsGodotArray<GodotObject>();
			if ( collisions.Any( ( it ) => { return !it.Get( "is_remappable" ).AsBool(); } ) ) {
				return;
			}

			for ( int i = 0; i < collisions.Count; i++ ) {
				SettingsData.Remapper.Call( "set_bound_input", collisions[ i ], Variant.From<GodotObject>( null ) );
			}

			SettingsData.Remapper.Call( "set_bound_input", item, input );
		}
		private void HandleRebind( Resource item ) {
			InputDetector.Call( "detect",
				item.Get( "value_type" ),
				new Godot.Collections.Array<int>() { (int)DeviceType.Keyboard, (int)DeviceType.Mouse, (int)DeviceType.Joy }
			);

			Label.ParseBbcode( "Press Any Key..." );

			GameEventBus.ConnectSignal( InputDetector, "input_detected", this, Callable.From<Resource>( ( input ) => OnInputBindingDetected( input, item ) ) );
		}
		private void Rebind( InputEvent @event, Resource item ) {
			if ( @event is InputEventMouseButton button && button != null && button.ButtonIndex == MouseButton.Left ) {
				HandleRebind( item );
			}
		}

		public override void _Ready() {
			base._Ready();

			BindName = GetNode<Label>( "HBoxContainer/RebindNameLabel" );
			Label = GetNode<RichTextLabel>( "HBoxContainer/RebindButtonLabel" );

			InputDetector = GetNode( "%GUIDEInputDetector" );
		}
	};
};
#endif