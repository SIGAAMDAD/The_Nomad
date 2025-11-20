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

using EventSystem;
using Menus.SelectionNodes;
using System;
using Settings;

namespace Menus.Settings.Options {
	/*
	===================================================================================
	
	AccessibilityOptionManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class AccessibilityOptionManager : OptionContainer {
		private AccessibilityConfig? Temp;

		private OptionCheckbox? HapticEnabled;
		private OptionSlider? HapticStrength;
		private OptionList? AutoAimMode;
		private OptionCheckbox? DyslexiaMode;
		private OptionCheckbox? GhostlyGuide;
		private OptionList? ColorblindMode;
		private OptionCheckbox? DisableFlashes;
		private OptionCheckbox? TextToSpeech;
		private OptionList? HUDPreset;
		private OptionCheckbox? EnableTutorials;
		private OptionCheckbox? ShowBlood;

		/*
		===============
		SetConfig
		===============
		*/
		/// <summary>
		/// Initializes the accessibility configuration values.
		/// </summary>
		/// <param name="config"></param>
		public override void SetConfig( in ConfigHandler config ) {
			Temp = config as AccessibilityConfig;
		}

		/*
		===============
		OnHapticToggled
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.HapticEnabled"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnHapticToggled( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "HapticEnabled" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnHapticStrengthValueChanged
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.HapticStrength"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnHapticStrengthValueChanged( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "HapticStrength" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnDyslexiaModeToggled
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.DyslexiaMode"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnDyslexiaModeToggled( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "DyslexiaMode" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnTextToSpeechToggled
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.TextToSpeech"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnTextToSpeechToggled( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "TextToSpeech" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnColorblindModeValueChanged
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.ColorblindMode"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnColorblindModeValueChanged( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "ColorblindMode" ] = (ColorblindMode)valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnAutoAimModeValueChanged
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.AutoAimMode"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnAutoAimModeValueChanged( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "AutoAimMode" ] = (AutoAimMode)valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnShowBloodToggled
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AccessibilityConfig.ShowBlood"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnShowBloodToggled( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "ShowBlood" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		LinkNodes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void LinkNodes() {
			HapticEnabled = GetNode<OptionCheckbox>( "HapticFeedbackToggle" );
			HapticEnabled.ValueChanged.Subscribe( this, OnHapticToggled );

			HapticStrength = GetNode<OptionSlider>( "HapticStrengthSlider" );
			HapticStrength.ValueChanged.Subscribe( this, OnHapticStrengthValueChanged );

			DyslexiaMode = GetNode<OptionCheckbox>( "DyslexiaModeToggle" );
			DyslexiaMode.ValueChanged.Subscribe( this, OnDyslexiaModeToggled );

			TextToSpeech = GetNode<OptionCheckbox>( "TextToSpeechToggle" );
			TextToSpeech.ValueChanged.Subscribe( this, OnTextToSpeechToggled );

			ColorblindMode = GetNode<OptionList>( "ColorblindModeList" );
			ColorblindMode.ValueChanged.Subscribe( this, OnColorblindModeValueChanged );

			AutoAimMode = GetNode<OptionList>( "AutoAimModeList" );
			AutoAimMode.ValueChanged.Subscribe( this, OnAutoAimModeValueChanged );

			ShowBlood = GetNode<OptionCheckbox>( "ShowBloodToggle" );
			ShowBlood.ValueChanged.Subscribe( this, OnShowBloodToggled );
		}

		/*
		===============
		OnVisibilityChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnVisibilityChanged() {
		}
	};
};