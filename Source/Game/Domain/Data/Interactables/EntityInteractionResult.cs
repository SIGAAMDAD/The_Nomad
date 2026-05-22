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

namespace Nomad.Game.Domain.Data.Interactables
{
	/*
	===================================================================================

	EntityInteractionResult

	===================================================================================
	*/
	/// <summary>
	/// Result of an entity interaction attempt.
	/// </summary>

	public readonly struct EntityInteractionResult
	{
		public static readonly EntityInteractionResult Succeeded = new EntityInteractionResult(
			true,
			string.Empty,
			EntityInteractionResultKind.Success
		);

		public static readonly EntityInteractionResult Failed = new EntityInteractionResult(
			false,
			"Interaction failed.",
			EntityInteractionResultKind.Failed
		);

		public bool Success { get; }

		public string Message { get; }

		public EntityInteractionResultKind Kind { get; }

		public bool ShouldConsumeInput => Success;

		public EntityInteractionResult(
			bool success,
			string message,
			EntityInteractionResultKind kind
		)
		{
			Success = success;
			Message = message;
			Kind = kind;
		}

		public static EntityInteractionResult SuccessResult()
		{
			return Succeeded;
		}

		public static EntityInteractionResult SuccessResult( string message )
		{
			return new EntityInteractionResult(
				true,
				message,
				EntityInteractionResultKind.Success
			);
		}

		public static EntityInteractionResult Fail( string reason )
		{
			return new EntityInteractionResult(
				false,
				reason,
				EntityInteractionResultKind.Failed
			);
		}

		public static EntityInteractionResult OutOfRange()
		{
			return new EntityInteractionResult(
				false,
				"Target is out of range.",
				EntityInteractionResultKind.OutOfRange
			);
		}

		public static EntityInteractionResult Locked()
		{
			return new EntityInteractionResult(
				false,
				"Target is locked.",
				EntityInteractionResultKind.Locked
			);
		}

		public static EntityInteractionResult InvalidTarget()
		{
			return new EntityInteractionResult(
				false,
				"Invalid interaction target.",
				EntityInteractionResultKind.InvalidTarget
			);
		}
	};
};
