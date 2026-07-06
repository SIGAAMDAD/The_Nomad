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

using Nomad.Core.Util;
using Nomad.Game.Sdk.Story.QuestTree;

namespace Nomad.Game.Gameplay.Runtime.Story.QuestTree
{
	internal readonly struct CompiledQuestCondition
	{
		public readonly bool HasCondition;
		public readonly InternString Fact;
		public readonly QuestConditionOperator Operator;
		public readonly bool ExpectedValue;

		public CompiledQuestCondition(
			InternString fact,
			QuestConditionOperator op,
			bool expectedValue
		)
		{
			HasCondition = fact != InternString.Empty;
			Fact = fact;
			Operator = op;
			ExpectedValue = expectedValue;
		}
	};
};
