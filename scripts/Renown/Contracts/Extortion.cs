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

using Renown.Thinkers;
using Renown.World;

namespace Renown.Contracts {
	public partial class Extortion : Contract {
		public static readonly float BASE_COST = 100.0f;

		private readonly float ExtortionAmount = 0.0f;

		public Extortion( in ExtortionData data )
			: base(
				name: data.Name,
				duedate: data.DueDate,
				flags: data.Flags,
				type: Type.Extortion,
				basePay: BASE_COST,
				area: data.Area,
				client: data.Client,
				guild: data.Guild,
				totalPay: 0.0f,
				target: null
			)
		{
			ExtortionAmount = data.ExortionAmount;
			//			Data.Target.Die += OnTargetDie;
		}

		private void OnTargetDie( Entity source, Entity target ) {
		}
	};
};