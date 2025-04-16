using Godot;
using System.Collections.Generic;
using Renown.World;

namespace Renown.Thinkers {
	public enum MerchantState : uint {
		Idle,
		Travelling,
		MarketTrading,
		RoadTrading,
		
		Count
	};
	public partial class Merchant : Thinker {
		[Export]
		private Godot.Collections.Dictionary<ResourceType, int> StartingGoods = null;
		[Export]
		private ResourceType[] TradeTypes;
		
		private Dictionary<ResourceType, int> Inventory = null;
		private TradeRoute CurrentRoad = null;
		
		private MerchantState State = MerchantState.Idle;
		private List<Merchant> CompatibleTrades = null;
		private Marketplace CurrentMarket = null;
		
		private int LastHourTraded = 0;
		private int NumDaysTrading = 0;
		
		// TODO: write one-on-one trading
		
		private void OnTimeTick( uint day, uint hour, uint minute ) {
			if ( hour != LastHourTraded ) {
				LastHourTraded = hour;
				
				// refresh trade list
				FindCompatibleTrades();
				
				for ( int i = 0; i < CompatibleTrades.Count; i++ ) {
					TryTrade( CompatibleTrades[i] );
				}
			}
		}
		private void OnDayStart() {
			if ( State == MerchantState.MarketTrading ) {
				NumDaysTrading++;
				if ( NumDaysTrading >= Constants.MaxMerchantTradingDays ) {
					Settlement to = Settlement.Cache.FindNearestSettlement( GlobalPosition );
					CurrentRoad = RoadNetwork.GetRoad( to, Location );
					if ( CurrentRoad == null ) {
						// blocked/restricted for some reason
						return;
					}
					State = MerchantState.Travelling;
					SetNavigationTarget( CurrentRoad.FindClosestEntryPoint( GlobalPosition ) );
				}
			}
		}
		
		public bool IsCompatibleMerchant( Merchant merchant ) {
			for ( int i = 0; i < TradeTypes.Length; i++ ) {
				for ( int a = 0; a < merchant.TradeTypes.Length; a++ ) {
					if ( TradeTypes[i] == merchant.TradeTypes[a] ) {
						return true;
					}
				}
			}
			return false;
		}
		
		public override void Save() {
			base.Save();
			
			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() + "_MerchantData" ) ) {
				writer.SaveInt( "inventory_size", Inventory.Count );
				int index = 0;
				foreach ( var item in Inventory ) {
					writer.SaveUInt( "inventory_type_" + index.ToString(), (uint)item.Key );
					writer.SaveInt( "inventory_amount_" + index.ToString(), item.Value );
					index++;
				}
			}
		}
		public override void Load() {
			base.Load();
			
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( GetPath() + "_MerchantData" );
			
			// save file compatibility
			if ( reader == null ) {
				return;
			}
			
			int numItems = reader.LoadInt( "inventory_size" );
			Inventory.Clear();
			Inventory = new Dictionary<ResourceType, int>( numItems );
			for ( int i = 0; i < numItems; i++ ) {
				Inventory.Add(
					(ResourceType)reader.LoadUInt( "inventory_type_" + i.ToString() ),
					reader.LoadInt( "inventory_amount_" + i.ToString() )
				);
			}
		}
		
		public override void _Ready() {
			base._Ready();
			
			WorldTimeManager.Instance.DayTimeStart += OnDayStart;
			WorldTimeManager.Instance.TimeTick += OnTimeTick;
			
			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			} else {
				Inventory = new Dictionary<ResourceType, int>( StartingGoods.Count );
				foreach ( var resource in StartingGoods ) {
					Inventory.Add( resource.Key, resource.Value );
				}
			}
		}
		protected override void Think( float delta ) {
			switch ( State ) {
			case MerchantState.Idle:
				IdleThink();
				break;
			case MerchantState.Travelling:
				break;
			case MerchantState.Trading:
				TradeThink();
				break;
			};
		}
		
		/// <summary>
		/// a good 'ol fashioned trade.
		/// can lead to a successful and peaceful trade
		/// can lead to a heated argument then a fist-fight or a shootout
		/// can also lead to a grudge, then a bounty
		/// </summary>
		private void TryTrade( Merchant merchant ) {
			//
			// find a compatible trade type (there could be multiple)
			//
			
			bool[] types = new bool[ (int)ResourceType.Count ];
		}
		
		private void FindCompatibleTrades() {
			if ( Location is Settlement settlement && settlement != null ) {
				Marketplace market = settlement.GetMarketplace();
				
				List<Merchant> sellers = market.GetActiveMerchants();
				
				// find compatible trades
				CompatibleTrades.Clear();
				for ( int i = 0; i < sellers.Count; i++ ) {
					if ( IsCompatibleMerchant( sellers[i] ) ) {
						CompatibleTrades.Add( sellers[i] );
					}
				}
			}
		}
		private void TradeThink() {
			if ( Location is Settlement settlement && settlement != null ) {
				if ( CurrentMarket == null ) {
					MarketplaceSlot slot = settlement.GetMarketplace().GetFreeTradingSpace();
					if ( slot == null ) {
						// well... shit
					}
					
					SetNavigationTarget( slot );
					FindCompatibleTrades();
				}
			}
		}
	};
};
