using Nomad.Core.Abstractions;
using Nomad.Core.Util;

namespace Game.Domain.Inventory.Models.ValueObjects {
	public readonly record struct ItemId(
		InternString Id
	) : IValueObject<ItemId>;
};