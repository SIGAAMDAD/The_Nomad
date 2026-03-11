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

namespace Game.Application.UI {
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

		public const string MUGSHOT_FOCUSED_EVENT = NAMESPACE + ":MugshotFocusedEvent";

		public const string MENU_STATE_CHANGED_EVENT = NAMESPACE + ":MenuStateChanged";

		public const string MENU_TRANSITION_REQUESTED_EVENT = NAMESPACE + ":MenuTransitionRequested";
		public const string MENU_TRANSITION_COMPLETED_EVENT = NAMESPACE + ":MenuTransitionCompleted";
	};
};