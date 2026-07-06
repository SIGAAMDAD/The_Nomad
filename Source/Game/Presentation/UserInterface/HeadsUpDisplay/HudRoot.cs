/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Godot;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Configuration;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Prefabs;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.DashKitHeatBar;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HealthBar;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.InteractionMenu;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.RageBar;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HotSlotContainer;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.Crosshair;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Core.CVars;
using Nomad.CVars;
using Nomad.Game.Gameplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay
{
	/*
	===================================================================================

	HudRoot

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class HudRoot : IHudRoot
	{
		public HUDPreset Preset => _preset;
		private HUDPreset _preset;

		private readonly ComponentGroup _combatGroup;
		private readonly ComponentGroup _neutralGroup;
		private readonly ComponentGroup _explorationGroup;

		private bool _isDisposed = false;

		/*
		===============
		HudRoot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="root"></param>
		/// <param name="eventFactory"></param>
		public HudRoot(
			PlayerId playerId,
			HeadsUpDisplayView root,
			IWeaponSlotService slotsService,
			IItemInstanceRepository itemRegistry,
			ILocalizationService localizationService,
			ICVarSystemService cvarSystem,
			IGameEventRegistryService eventFactory
		)
		{
			playerId.ThrowIfInvalid( nameof( HudRoot ) );

			root = root ?? throw new ArgumentNullException( nameof( root ) );
			slotsService = slotsService ?? throw new ArgumentNullException( nameof( slotsService ) );
			itemRegistry = itemRegistry ?? throw new ArgumentNullException( nameof( itemRegistry ) );
			localizationService = localizationService ?? throw new ArgumentNullException( nameof( localizationService ) );
			cvarSystem = cvarSystem ?? throw new ArgumentNullException( nameof( cvarSystem ) );
			eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_preset = cvarSystem.GetCVarOrThrow( GameplayCVarRegistry.HUDPreset ).Value;

			_neutralGroup = new ComponentGroup(
				cvarSystem.GetCVarOrThrow( GameplayCVarRegistry.NeutralHUDPreset ).Value,
				new List<IHudComponentPresenter>() {
					new HealthBarPresenter( playerId, root.GetNode<HealthBarView>( "NeutralHUD/StatBarContainer/HealthBar" ), eventFactory ),
					new RageBarPresenter( playerId, root.GetNode<RageBarView>( "NeutralHUD/StatBarContainer/RageBar" ), eventFactory ),
					new DashKitHeatBarPresenter( playerId, root.GetNode<DashStatusBarView>( "NeutralHUD/StatBarContainer/DashStatusBar" ), eventFactory ),
					new InteractionMenuPresenter( playerId, root.GetNode<InteractionMenuView>( "NeutralHUD/InteractionMenu" ), eventFactory, localizationService )
				}
			);

			_combatGroup = new ComponentGroup(
				cvarSystem.GetCVarOrThrow( GameplayCVarRegistry.CombatHUDPreset ).Value,
				new List<IHudComponentPresenter> {
					new HotSlotContainerPresenter( playerId, root.GetNode<HotSlotsContainerView>( "CombatHUD/HotSlotsContainer" ), slotsService, itemRegistry ),
					new CrosshairPresenter( playerId, ResolveCrosshairView( root ), eventFactory )
				}
			);
		}

		private static CrosshairView ResolveCrosshairView( HeadsUpDisplayView root )
		{
			CrosshairView existing = root.GetNodeOrNull<CrosshairView>( "CombatHUD/Crosshair" );
			if ( existing != null ) {
				return existing;
			}

			Control parent = root.GetNodeOrNull<Control>( "CombatHUD" )
				?? throw new InvalidOperationException( "HeadsUpDisplay requires a CombatHUD Control to host the Crosshair view." );

			CrosshairView view = CreateCrosshairView();
			parent.AddChild( view );

			return view;
		}

		private static CrosshairView CreateCrosshairView()
		{
			const string scenePath = "res://Source/Game/Presentation/UserInterface/HeadsUpDisplay/Components/Crosshair/CrosshairView.tscn";
			if ( ResourceLoader.Exists( scenePath ) ) {
				var scene = ResourceLoader.Load<PackedScene>( scenePath );
				if ( scene.Instantiate() is CrosshairView instancedView ) {
					return instancedView;
				}
			}

			var view = new CrosshairView {
				Name = "Crosshair",
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			view.SetAnchorsPreset( Control.LayoutPreset.FullRect );
			view.SetOffsetsPreset( Control.LayoutPreset.FullRect );

			return view;
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_neutralGroup.Dispose();
			_combatGroup.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		Render
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public void Render( float delta )
		{
			_neutralGroup.Render( delta );
			_combatGroup.Render( delta );
		}
	};
};
