using System;
using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown.Thinkers.Occupations {
	// TODO: write a merchant that can move from city to city
	public partial class Merchant : Occupation {
		private Dictionary<Resource, int> Inventory = null;
		private MarketplaceSlot Stall = null;

		public Merchant( Thinker worker, Faction company )
			: base( worker, company )
		{ }

		public void SetTradingSlot( MarketplaceSlot slot ) {
			Stall = slot;
		}

		public override void Save( SaveSystem.SaveSectionWriter writer, string key ) {
			base.Save( writer, key );

			writer.SaveInt( key + "InventorySize", Inventory.Count );
			int count = 0;
			foreach ( var type in Inventory ) {
				writer.SaveString( string.Format( "{0}Inventory{1}Type", key, count ), (string)type.Key.Get( "id" ) );
				writer.SaveInt( string.Format( "{0}Inventory{1}Count", key, count ), type.Value );
				count++;
			}
		}
		public override void Load( SaveSystem.SaveSectionReader reader, string key ) {
			base.Load( reader, key );

			int inventorySize = reader.LoadInt( key + "InventorySize" );
			Inventory = new Dictionary<Resource, int>( inventorySize );
			for ( int i = 0; i < inventorySize; i++ ) {
				Inventory.Add(
					(Resource)ResourceCache.ItemDatabase.Call( "get_item", reader.LoadString( string.Format( "{0}Inventory{1}Type", key, i ) ) ),
					reader.LoadInt( string.Format( "{0}Inventory{1}Count", key, i ) )
				);
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				Inventory = new Dictionary<Resource, int>();

				Random random = new Random();
				int inventorySize = random.Next( 36, 72 );
				Godot.Collections.Array<Resource> items = (Godot.Collections.Array<Resource>)ResourceCache.ItemDatabase.Get( "items" );

				for ( int i = 0; i < inventorySize; i++ ) {
					int item = random.Next( 0, items.Count - 1 );
					Inventory.Add( items[ item ], (int)( inventorySize / (float)( (Godot.Collections.Dictionary)items[ item ].Get( "properties" ) )[ "value" ] ) );
				}
			}
		}

		public override void Process() {
		}
	};
};