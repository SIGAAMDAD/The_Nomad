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
using Nomad.Game.Domain.Data.Gameplay;

namespace Nomad.Game.Domain.Events.Gameplay {
	public readonly struct WorldBootstrapFailureEventArgs {
		public Guid RequestId { get; }
		public WorldBootstrapMode Mode { get; }
		public string WorldId { get; }
		public WorldBootstrapFailureReason Reason { get; }
		public string? Detail { get; }

		public WorldBootstrapFailureEventArgs( Guid requestId, WorldBootstrapMode mode, string worldId, WorldBootstrapFailureReason reason, string? detail ) {
			RequestId = requestId;
			Mode = mode;
			WorldId = worldId;
			Reason = reason;
			Detail = detail;
		}
	};
};