using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using Godot;
using Steamworks;

namespace Renown.World {
	public class GlobalEconomy {
		private enum OrderType {
			Buy,
			Sell
		};
		private struct MarketOrder {
			public OrderType Type;
			public int MarketID;
			public int AgentID;
			public Resource ItemID;
			public int Amount;

			public MarketOrder( int MarketId, int AgentId, OrderType nType, int nAmount, Resource ItemId ) {
				Type = nType;
				MarketID = MarketId;
				AgentID = AgentId;
				ItemID = ItemId;
				Amount = nAmount;
			}
		};
		private class Commodity {
			public Resource ItemId;
			public float BasePrice;
			public float Max;
			public float Min;
			public float Price;
			public int Buys;
			public int Sells;

			public Commodity( Resource item ) {
				Min = 0.1f;
				Max = float.MaxValue;
				ItemId = item;

				Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)item.Get( "properties" );
				Godot.Variant basePrice = Godot.Variant.From( 0.0f );
				properties.TryGetValue( "value", out basePrice );
				BasePrice = (float)basePrice.AsDouble();
				Price = BasePrice;

				Buys = 0;
				Sells = 0;
			}
		};

		private static float Inflation = 1.0f;
		private static Dictionary<string, Commodity> CommodityData = new Dictionary<string, Commodity>();
		private static Dictionary<int, Marketplace> MarketCache = new Dictionary<int, Marketplace>();
		private static List<MarketOrder> Orders = new List<MarketOrder>();

		private static void UpdatePrices() {
			foreach ( var item in CommodityData ) {
				float variance = ( item.Value.Buys / item.Value.Sells ) * (float)Math.Clamp( ( item.Value.Buys / item.Value.Sells ) * ( 1 + Inflation ), item.Value.Min, item.Value.Max );
				variance = 1.0f + variance;

				item.Value.Price = item.Value.BasePrice * variance;
			}
		}
		public static void Init() {
			Godot.Collections.Array<Resource> items = (Godot.Collections.Array<Resource>)ResourceCache.ItemDatabase.Get( "items" );
			for ( int i = 0; i < items.Count; i++ ) {
				CommodityData.Add( (string)items[i].Get( "id" ), new Commodity( items[i]) );
			}

			WorldTimeManager.Instance.DayTimeStart += UpdatePrices;
		}

		private static int SubmitBuyOrder( int nAmount, Resource itemId ) {
			if ( CommodityData.TryGetValue( (string)itemId.Get( "id" ), out Commodity data ) ) {
				int buys = data.Buys;
				buys += nAmount;
				data.Buys = nAmount;
				
				int newSells = data.Sells - nAmount;
				if ( newSells >= 0 ) {
					return data.Sells = newSells;
				} else {
					data.Sells -= data.Sells - newSells;
					return data.Sells;
				}
			}
			return -1;
		}
		private static int SubmitSellOrder( int nAmount, Resource itemId ) {
			if ( CommodityData.TryGetValue( (string)itemId.Get( "id" ), out Commodity data ) ) {
				int buys = data.Buys;
				buys += nAmount;
				data.Buys = nAmount;

				int newBuys = data.Buys - nAmount;
				if ( newBuys >= 0 ) {
					return data.Buys = newBuys;
				} else {
					data.Buys -= data.Buys - newBuys;
					return data.Buys;
				}
			}
			return -1;
		}

		public static float GetPriceOfItem( Resource item ) {
			if ( CommodityData.TryGetValue( (string)item.Get( "id" ), out Commodity data ) ) {
				return data.Price;
			}
			return 0.0f;
		}

		public static int AddBuyOrder( Resource item, Marketplace market, Entity buyer, int nAmount ) {
			return SubmitBuyOrder( nAmount, item );
		}
		public static int AddSellOrder( Resource item, Marketplace market, Entity buyer, int nAmount ) {
			return SubmitSellOrder( nAmount, item );
		}

		public static void AddMarketplace( Marketplace market ) {
			MarketCache.Add( market.GetHashCode(), market );
		}
	};
};