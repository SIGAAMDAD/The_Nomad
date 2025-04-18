using Godot;
using System.Collections.Generic;
using Renown.World;
using System.Linq;
using System.Reflection;

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
//		[Export]
//		private Godot.Collections.Array<ResourceType> TradeTypes;
		
		private Dictionary<ResourceType, int> Inventory = null;
		private Road CurrentRoad = null;
		
		private MerchantState State = MerchantState.Idle;
		private Marketplace CurrentMarket = null;
		
		private uint LastHourTraded = 0;
		private int NumDaysTrading = 0;
		
		// TODO: write one-on-one trading

		public override void SetLocation( WorldArea location ) {
			base.SetLocation( location );

			if ( Location is Settlement settlement && settlement != null ) {
				// find a suitable marketplace
				Marketplace[] markets = settlement.GetMarketplaces();
				float bestDistance = float.MaxValue;

				for ( int i = 0; i < markets.Length; i++ ) {
					float distance = markets[i].GlobalPosition.DistanceTo( GlobalPosition );
					if ( distance < bestDistance ) {
						CurrentMarket = markets[i];
						bestDistance = distance;
					}
				}

				SetNavigationTarget( CurrentMarket.GlobalPosition );
			}
		}

		private void OnTimeTick( uint day, uint hour, uint minute ) {
			if ( hour != LastHourTraded ) {
				LastHourTraded = hour;
			}
		}
		private void OnDayStart() {
			if ( State == MerchantState.MarketTrading ) {
				NumDaysTrading++;
				if ( NumDaysTrading >= 7 ) {
					Settlement to = Settlement.Cache.FindNearest( GlobalPosition );
					CurrentRoad = RoadNetwork.GetRoad( to, Location );
					if ( CurrentRoad == null ) {
						// blocked/restricted for some reason
						return;
					}
					State = MerchantState.Travelling;
					SetNavigationTarget( CurrentRoad.FindClosestEntryPoint( Location, GlobalPosition ).GlobalPosition );
				}
			}
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
				break;
			case MerchantState.Travelling:
				break;
			case MerchantState.RoadTrading:
			case MerchantState.MarketTrading:
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
		

		private void TradeThink() {
		}
	};
};
