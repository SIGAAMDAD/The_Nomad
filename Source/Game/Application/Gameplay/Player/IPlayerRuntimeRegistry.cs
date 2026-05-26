using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Application.Gameplay.Player
{
	internal interface IPlayerRuntimeRegistry
	{
		bool TryGetInventory( PlayerId playerId, out IPlayerInventoryCoordinator? inventory );
		bool TryGetState( PlayerId playerId, out IPlayerStateReader? reader, out IPlayerStateWriter? writer );
	}
}
