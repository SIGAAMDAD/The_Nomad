using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Interfaces.Player.Inventory;
using Nomad.Game.Domain.Interfaces.Player.State;

namespace Nomad.Game.Application.Gameplay.Player
{
	internal interface IPlayerRuntimeRegistry
	{
		bool TryGetInventory( PlayerId playerId, out IPlayerInventoryCoordinator? inventory );
		bool TryGetState( PlayerId playerId, out IPlayerStateReader? reader, out IPlayerStateWriter? writer );
	};
};
