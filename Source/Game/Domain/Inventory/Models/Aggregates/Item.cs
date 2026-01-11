using Game.Domain.Inventory.Models.ValueObjects;
using Nomad.Core.Abstractions;
using System;

namespace Game.Domain.Inventory.Models.Aggregates {
	/*
	===================================================================================
	
	Item
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class Item : IAggregateRoot<ItemId> {
		public ItemId Id => throw new NotImplementedException();

		public DateTime CreatedAt => throw new NotImplementedException();

		public DateTime? ModifiedAt => throw new NotImplementedException();

		public int Version => throw new NotImplementedException();

		public bool Equals( IEntity<ItemId>? other ) {
			throw new NotImplementedException();
		}
	};
};