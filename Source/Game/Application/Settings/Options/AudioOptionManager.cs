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
	
	AudioOptionManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class AudioOptionManager : OptionContainer {
		private AudioConfig? Temp;

		private OptionSlider? EffectsVolume;
		private OptionCheckbox? EffectsOn;
		private OptionSlider? MusicVolume;
		private OptionCheckbox? MusicOn;

		/*
		===============
		SetConfig
		===============
		*/
		/// <summary>
		/// Initializes the audio configuration values.
		/// </summary>
		/// <param name="config"></param>
		public override void SetConfig( in ConfigHandler config ) {
			Temp = config as AudioConfig;
		}

		/*
		===============
		OnEffectsVolumeValueChanged
		===============
		*/
		/// <summary>
		/// Sets <see cref="AudioConfig.EffectsVolume"/>
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnEffectsVolumeValueChanged( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "EffectsVolume" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnEffectsToggled
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AudioConfig.EffectsOn"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnEffectsToggled( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "EffectsOn" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnMusicVolumeValueChanged
		===============
		*/
		/// <summary>
		/// Sets <see cref="AudioConfig.MusicVolume"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnMusicVolumeValueChanged( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "MusicVolume" ] = valueChanged.Value;
			} else {
				throw new InvalidCastException( nameof( args ) );
			}
		}

		/*
		===============
		OnMusicToggled
		===============
		*/
		/// <summary>
		/// Toggles <see cref="AudioConfig.MusicOn"/>.
		/// </summary>
		/// <param name="eventBase">The event object.</param>
		/// <param name="args">The event's arguments, should be a <see cref="OptionNode.ValueChangedEventData"/>.</param>
		/// <exception cref="InvalidCastException">Thrown if <paramref name="args"/> isn't a <see cref="OptionNode.ValueChangedEventData"/>.</exception>
		private void OnMusicToggled( in IGameEvent eventBase, in IEventArgs args ) {
			if ( args is OptionNode.ValueChangedEventData valueChanged ) {
				ArgumentNullException.ThrowIfNull( Temp );
				Temp[ "MusicOn" ] = valueChanged.Value;
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
			EffectsOn = GetNode<OptionCheckbox>( "VBoxContainer/EffectsOnToggle" );
			EffectsOn.ValueChanged.Subscribe( this, OnEffectsToggled );

			EffectsVolume = GetNode<OptionSlider>( "VBoxContainer/EffectsVolumeSlider" );
			EffectsVolume.ValueChanged.Subscribe( this, OnEffectsVolumeValueChanged );

			MusicOn = GetNode<OptionCheckbox>( "VBoxContainer/MusicOnToggle" );
			MusicOn.ValueChanged.Subscribe( this, OnMusicToggled );

			MusicVolume = GetNode<OptionSlider>( "TabContainer/Audio/VBoxContainer/MusicVolumeSlider" );
			MusicVolume.ValueChanged.Subscribe( this, OnMusicVolumeValueChanged );
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