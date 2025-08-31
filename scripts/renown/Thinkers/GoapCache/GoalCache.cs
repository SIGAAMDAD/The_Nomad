/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using MountainGoap;
using System.Collections.Generic;

namespace Renown.Thinkers.GoapCache {
	/// <summary>
	/// generic goals that aren't faction specific
	/// </summary>
	public enum GoalType : uint {
		FindCover,
		InvestigateDisturbance,
		SurveyForThreats,
		KillTarget,
		Survive,
		Follow,
		Fallback,
		Goto,
		RunAway,
		GotoCover,

		Count
	};

	/*
	===================================================================================
	
	GoapGoalCache
	
	===================================================================================
	*/
	/// <summary>
	/// Stores global GOAP goal Dictionary
	/// </summary>
	
	public static class GoapGoalCache {
		public static Dictionary<GoalType, BaseGoal> Cache;

		static GoapGoalCache() {
			Cache = new Dictionary<GoalType, BaseGoal> {
				{
					GoalType.Survive,
					new ExtremeGoal(
						name: "Survive",
						weight: 1.0f,
						desiredState: new Dictionary<string, bool>{
							{ "Health", true }
						}
						//desiredState: new Dictionary<string, ComparisonValuePair>{
						//{ "Health", new ComparisonValuePair { Value = 25.0f, Operator = ComparisonOperator.GreaterThan } }
						//}
					)
				},
				{
					GoalType.RunAway,
					new Goal(
						name: "RunAway",
						weight: 0.95f,
						desiredState: new Dictionary<string, object?>{
							{ "InDanger", false }
						}
					)
				},
				{
					GoalType.KillTarget,
					new Goal(
						name: "KillTarget",
						weight: 0.9f,
						desiredState: new Dictionary<string, object?>{
							{ "Target", null }
						}
					)
				},
				{
					GoalType.InvestigateDisturbance,
					new Goal(
						name: "InvestigateDisturbance",
						weight: 0.8f,
						desiredState: new Dictionary<string, object?>{
							{ "PlayerVisible", true }
						}
					)
				},
				{
					GoalType.SurveyForThreats,
					new Goal(
						name: "SurveyForThreats",
						weight: 0.8f,
						desiredState: new Dictionary<string, object?>{
							{ "IsAlerted", true }
						}
					)
				},
				{
					GoalType.FindCover,
					new Goal(
						name: "FindCover",
						weight: 0.9f,
						desiredState: new Dictionary<string, object?>{
							{ "FoundCover", true }
						}
					)
				},
				{
					GoalType.GotoCover,
					new Goal(
						name: "GotoCover",
						weight: 0.9f,
						desiredState: new Dictionary<string, object?>{
							{ "InCover", true }
						}
					)
				},
				{
					GoalType.Follow,
					new ComparativeGoal(
						name: "Follow",
						weight: 0.6f,
						desiredState: new Dictionary<string, ComparisonValuePair>{
							{ "TargetDistance", new ComparisonValuePair { Value = 200.0f, Operator = ComparisonOperator.LessThan } }
						}
					)
				}
			};
		}
	};
};