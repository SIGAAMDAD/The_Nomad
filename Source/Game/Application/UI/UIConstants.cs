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

namespace Nomad.Game.Application.UI {
	public static class UIConstants {
		public const string NAMESPACE = "UIEvents";

		public const string BUTTON_CLICKED_EVENT = NAMESPACE + ":ButtonClicked";
		public const string BUTTON_FOCUSED_EVENT = NAMESPACE + ":ButtonFocused";
		public const string BUTTON_UNFOCUSED_EVENT = NAMESPACE + ":ButtonUnfocused";
		
		public const string OPTION_CHECKBOX_TOGGLED_EVENT = NAMESPACE + ":OptionCheckboxToggled";
		public const string OPTION_SLIDER_TOGGLED_LEFT_EVENT = NAMESPACE + ":OptionSliderLeft";
		public const string OPTION_SLIDER_TOGGLED_RIGHT_EVENT = NAMESPACE + ":OptionSliderRight";
		public const string OPTION_SLIDER_VALUE_CHANGED_EVENT = NAMESPACE + ":OptionSliderValueChanged";
		public const string OPTION_LIST_VALUE_SET_EVENT = NAMESPACE + ":OptionListValueSet";
		public const string OPTION_LIST_FOCUSED_EVENT = NAMESPACE + ":OptionListFocused";

		public const string MENU_STATE_CHANGED_EVENT = NAMESPACE + ":MenuStateChanged";

		public const string MENU_TRANSITION_REQUESTED_EVENT = NAMESPACE + ":MenuTransitionRequested";
		public const string MENU_TRANSITION_COMPLETED_EVENT = NAMESPACE + ":MenuTransitionCompleted";

		public const string MAP_ADDED_TO_LOBBY_FILTER_EVENT = NAMESPACE + ":MapAddedToLobbyFilter";
		public const string MAP_REMOVED_FROM_LOBBY_FILTER_EVENT = NAMESPACE + ":MapRemovedFromLobbyFilter";
		public const string GAMEMODE_ADDED_TO_LOBBY_FILTER_EVENT = NAMESPACE + ":GameModeAddedToLobbyFilter";
		public const string GAMEMODE_REMOVED_FROM_LOBBY_FILTER_EVENT = NAMESPACE + ":GameModeRemovedFromLobbyFilter";
		public const string SHOW_FULL_LOBBIES_CHANGED_EVENT = NAMESPACE + ":ShowFullLobbiesFilterChanged";
	};
};