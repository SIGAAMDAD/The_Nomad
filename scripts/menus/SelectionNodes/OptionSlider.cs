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
using System;
using System.Runtime.CompilerServices;

namespace Menus.SelectionNodes {
	/*
	===================================================================================
	
	OptionSlider
	
	===================================================================================
	*/

	public partial class OptionSlider : HBoxContainer {
		[Export]
		public StringName Title { get; private set; }
		[Export]
		public StringName Description { get; private set; }
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Min = 0.0f;
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Max = 100.0f;

		private string DescriptionCached;
		private Godot.Label ValueLabel;
		private HSlider Input;

		public event Action<float> ValueChanged;

		/*
		===============
		GetDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string GetDescription() {
			return Description.IsEmpty ? "" : DescriptionCached;
		}

		/*
		===============
		SetValue
		===============
		*/
		public void SetValue( float value ) {
			Input.Value = value;
		}

		/*
		===============
		OnValueChanged
		===============
		*/
		private void OnValueChanged( float value ) {
			ValueLabel.Text = value.ToString();
			ValueChanged?.Invoke( value );
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			GetNode<SelectionNodes.Label>( "Title" ).Text = TranslationServer.Translate( Title );
			DescriptionCached = TranslationServer.Translate( Description );

			Input = GetNode<HSlider>( "Input" );
			Input.MinValue = Min;
			Input.MaxValue = Max;
			GameEventBus.ConnectSignal( Input, HSlider.SignalName.ValueChanged, this, Callable.From<float>( OnValueChanged ) );

			ValueLabel = Input.GetNode<Godot.Label>( "Value" );
		}

		/*
		===============
		_ExitTree
		===============
		*/
		public override void _ExitTree() {
			base._ExitTree();

			GameEventBus.ReleaseDanglingDelegates( ValueChanged );
		}
	};
};