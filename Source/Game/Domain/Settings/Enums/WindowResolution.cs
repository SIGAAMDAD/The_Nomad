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

namespace Game.Domain.Settings.Enums {
	public enum WindowResolution : byte {
		Res_640x480,
		Res_800x600,
		Res_1024x768,
		Res_1280x720,
		Res_1280x768,
		Res_1280x800,
		Res_1280x1024,
		Res_1360x768,
		Res_1366x768,
		Res_1440x900,
		Res_1536x864,
		Res_1600x900,
		Res_1600x1200,
		Res_1680x1050,
		Res_1920x1080,
		Res_1920x1200,
		Res_2048x1152,
		Res_2048x1536,
		Res_2560x1080,
		Res_2560x1440,
		Res_2560x1600,
		Res_3440x1440,
		Res_3840x2160,
		Res_Native,

		Count,

		Default = Res_Native
	};
};