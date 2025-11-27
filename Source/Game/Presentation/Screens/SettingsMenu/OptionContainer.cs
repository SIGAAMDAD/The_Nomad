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

using Game.Application.Common.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes;
using Godot;
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using System.Collections.Generic;

namespace Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	OptionContainer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public abstract partial class OptionContainer : TabBar {
		[Export]
		protected RichTextLabel? DescriptionLabel;

		public void AddOption( OptionNode node ) {
		}

		/*
		===============
		LinkNodes
		===============
		*/
		/// <summary>
		/// Links inner option adjustment nodes to their appropriate ui elements.
		/// </summary>
		protected abstract void LinkNodes();

		/*
		===============
		OnVisibilityChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected abstract void OnVisibilityChanged();

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		/// <remarks>
		/// By making it sealed, we force the inheritors to only use the <see cref="LinkNodes"/> method.
		/// </remarks>
		public sealed override void _Ready() {
			base._Ready();

			LinkNodes();

			var eventBus = ServiceRegistry.Get<IGameEventBusService>();
			eventBus.ConnectSignal( this, SignalName.VisibilityChanged, this, OnVisibilityChanged );
		}
	};
};