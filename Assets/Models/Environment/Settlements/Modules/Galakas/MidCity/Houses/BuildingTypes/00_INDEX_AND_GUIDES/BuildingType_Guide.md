# Galakas MidCity Building Types Guide

This guide explains the organized `BuildingTypes` folder for the Galakas MidCity CSG pack.

The BuildingTypes scenes are complete assembly scenes that instance modules from `Foundations`, `LowerModules`, `UpperModules`, and `Roofs`. They are not meant to reconstruct those components inside the assembly scene.

The footprint token in each filename, such as `W12D30`, is the module footprint token used by the generated set. Keep matching footprints when swapping lower/upper/roof modules.

## Category folders

### `01_Perimeter_Wall_Edge`
Tall/narrow edge buildings for the wall-side districts and cramped strips near Galakas' massive curtain walls.

### `02_Shopfronts_Rowhouses`
General mixed-use shop-houses, tenant rows, and stacked dwellings for ordinary MidCity streets.

### `03_Markets_Commercial_Rows`
Market-facing blocks, arcade rows, merchant stalls, and broad commercial anchors.

### `04_Inns_Taverns_Lodging`
Alehouses, inns, lodges, and stable-adjacent buildings used as social/rest points.

### `05_Civic_Guild_Offices`
Guild offices, clerks, scribes, counting houses, and tax-administration buildings.

### `06_Workshops_Production`
Craft/production buildings: dyers, printers, ropewalks, tannery backs, forge rows, and mixed noble workshops.

### `07_Storage_Warehouse_Service`
Granaries, warehouses, yards, and back-of-house service volumes that support the commercial district.

### `08_Courtyard_Wealthy_Blocks`
Merchant-family, old-noble, courtyard, and semi-elite blocks for the cleaner inner-MidCity transition.

### `09_Backstreet_Alley_SideDoors`
Alley-facing and long-side-door variants meant to break up repeated short-side entrances.

### `10_Rooftop_Route_Blocks`
Roofline-focused building masses that create height variation and receive later traversal overlays.

### `11_Bridges_Overpasses_StreetConnectors`
Buildings that shape covered passages, bridge-like street forms, overpasses, or special street connectors.

## Building type explanations

### `01_Perimeter_Wall_Edge`

#### `GalakasMidCity_BT_NarrowWallRow_Long_W04D12_CSG.tscn`
- **Footprint:** `W04D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** near walls, cramped edge lots, wall-shadow streets.
- **Use:** Long narrow row for wall-adjacent housing/commercial strips. Good near the giant walls where lots should feel compressed.

#### `GalakasMidCity_BT_OuterWall_TallNarrow_W03D06_CSG.tscn`
- **Footprint:** `W03D06`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** near walls, cramped edge lots, wall-shadow streets.
- **Use:** Tiny/tall wall-edge building. Use for cramped vertical housing or shops pressed against the massive Galakas walls.

### `02_Shopfronts_Rowhouses`

#### `GalakasMidCity_BT_Apothecary_RowEnd_W04D08_CSG.tscn`
- **Footprint:** `W04D08`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Small row-end shop-house. Works as a cap for compact street rows, especially when you want a low footprint vendor building.

#### `GalakasMidCity_BT_Baker_ShopHouse_W05D10_CSG.tscn`
- **Footprint:** `W05D10`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Small bakery shop-house. Use to add ordinary daily-life commerce and break up weapon/mercenary-heavy streets.

#### `GalakasMidCity_BT_Bookseller_Stacked_W06D12_CSG.tscn`
- **Footprint:** `W06D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Tall stacked bookseller/record shop. Good near scribes, guild offices, schools, archives, or wealthier merchant streets.

#### `GalakasMidCity_BT_CanalHouse_ThinLong_W04D12_CSG.tscn`
- **Footprint:** `W04D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Thin long house suitable for water-channel, drainage, or narrow-lot streets. Good for old irregular parcels.

#### `GalakasMidCity_BT_Chandler_Backlot_W05D10_CSG.tscn`
- **Footprint:** `W05D10`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Candle/utility shop with backlot feeling. Good for domestic-production streets and smaller craft clusters.

#### `GalakasMidCity_BT_CobblerAndLoft_W05D16_CSG.tscn`
- **Footprint:** `W05D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Cobbler shop with living/storage loft. Good for modest commercial streets and rows of practical trades.

#### `GalakasMidCity_BT_Goldsmith_NarrowTall_W04D12_CSG.tscn`
- **Footprint:** `W04D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Narrow tall goldsmith shop. Good for rich but tight streets, secure commercial lanes, or near counting houses.

#### `GalakasMidCity_BT_IrregularTallRow_Blockout_W06D20_CSG.tscn`
- **Footprint:** `W06D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Irregular tall rowhouse mass. Use to avoid overly clean repetition and create the lived-in medieval skyline.

#### `GalakasMidCity_BT_LongDormerHouse_W07D18_CSG.tscn`
- **Footprint:** `W07D18`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Long house with dormer-oriented silhouette. Useful for varied rooflines on dense residential/commercial streets.

#### `GalakasMidCity_BT_Rookery_TallPacked_W06D16_CSG.tscn`
- **Footprint:** `W06D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Tall packed dwelling/rookery mass. Good for density, poorer vertical housing, and less polished backstreets.

#### `GalakasMidCity_BT_StackedTenant_Long_W06D20_CSG.tscn`
- **Footprint:** `W06D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Long stacked tenant building. Good for density and everyday housing above commercial/service floors.

#### `GalakasMidCity_BT_Tailor_RowMid_W04D08_CSG.tscn`
- **Footprint:** `W04D08`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Small row-middle tailor shop. Use to fill ordinary commercial rows with small trade identity.

#### `GalakasMidCity_BT_TallGalleryHouse_W06D12_CSG.tscn`
- **Footprint:** `W06D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** ordinary streets, mixed-use rows, filler blocks.
- **Use:** Tall house with gallery-like upper mass. Useful for vertical variation and richer mixed-use street frontage.

### `03_Markets_Commercial_Rows`

#### `GalakasMidCity_BT_ArcadedShop_Row_Long_W12D30_CSG.tscn`
- **Footprint:** `W12D30`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Very long arcade-facing commercial row. Best for main market spines, covered vendor streets, and high foot-traffic merchant corridors.

#### `GalakasMidCity_BT_ButchersRow_DeepMarket_W05D16_CSG.tscn`
- **Footprint:** `W05D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Deep market row for butchers/food sellers. Works near markets, alleys with stronger working-class texture, and service lanes.

#### `GalakasMidCity_BT_ClothMarket_GrandLong_W10D30_CSG.tscn`
- **Footprint:** `W10D30`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Large cloth-market hall/row. Use as a main commercial anchor for textile merchants and cleaner inner-MidCity commerce.

#### `GalakasMidCity_BT_CoveredMarket_Hall_W08D20_CSG.tscn`
- **Footprint:** `W08D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Covered market hall. Good as a medium commercial anchor with interior-market implications.

#### `GalakasMidCity_BT_GreatMarketSpine_W12D30_CSG.tscn`
- **Footprint:** `W12D30`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Major market-spine block. Best as a primary landmark/commercial backbone for a long MidCity street.

#### `GalakasMidCity_BT_LongArcadeReceiver_W18D10_CSG.tscn`
- **Footprint:** `W18D10`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Long arcade receiver/end segment. Use as a terminus or connector for an arcade street, market frontage, or roofline run.

#### `GalakasMidCity_BT_MercerCorner_Tall_W10D16_CSG.tscn`
- **Footprint:** `W10D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Tall corner mercer/textile merchant building. Good for visually anchoring intersections and commercial corners.

#### `GalakasMidCity_BT_TripleShopRow_Long_W18D10_CSG.tscn`
- **Footprint:** `W18D10`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Very wide three-shop row. Use for broad street frontage and repeated storefront rhythm.

#### `GalakasMidCity_BT_Watchmaker_CornerBroad_W16D16_CSG.tscn`
- **Footprint:** `W16D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** main market streets, plazas, commercial spines.
- **Use:** Broad corner watchmaker/specialty shop. Use as a cleaner commercial landmark at intersections.

### `04_Inns_Taverns_Lodging`

#### `GalakasMidCity_BT_Alehouse_Block_Long_W12D24_CSG.tscn`
- **Footprint:** `W12D24`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** gate roads, tavern clusters, market edges, contract hubs.
- **Use:** Large tavern/social block. Use it near market streets or contract hubs where MidCity should feel busy, loud, and commercially alive.

#### `GalakasMidCity_BT_BackstreetLodge_W06D12_CSG.tscn`
- **Footprint:** `W06D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** gate roads, tavern clusters, market edges, contract hubs.
- **Use:** Compact lodge/boarding house. Good for quieter lodging lanes, traveler housing, or lower-prestige inns away from the main market.

#### `GalakasMidCity_BT_InnAndStable_Deep_W08D16_CSG.tscn`
- **Footprint:** `W08D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** gate roads, tavern clusters, market edges, contract hubs.
- **Use:** Deep inn/stable building. Use near caravan routes, market edges, or main roads entering MidCity.

#### `GalakasMidCity_BT_PilgrimInn_LongCorner_W12D24_CSG.tscn`
- **Footprint:** `W12D24`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** gate roads, tavern clusters, market edges, contract hubs.
- **Use:** Long corner inn for travelers/pilgrims. Use near gates, shrines, plaza edges, or transition streets.

#### `GalakasMidCity_BT_SideStreet_Alehouse_Long_W12D24_CSG.tscn`
- **Footprint:** `W12D24`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** gate roads, tavern clusters, market edges, contract hubs.
- **Use:** Long alehouse for side streets rather than main plazas. Good for mercenary-adjacent social spaces away from polished frontage.

### `05_Civic_Guild_Offices`

#### `GalakasMidCity_BT_BackstreetGuildOffice_W06D16_CSG.tscn`
- **Footprint:** `W06D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** guild quarters, administrative streets, tax/records zones.
- **Use:** Secondary guild office tucked off the main road. Use for minor guild bureaucracy, contract records, or less prestigious administrative streets.

#### `GalakasMidCity_BT_CountingHouse_Wide_W08D12_CSG.tscn`
- **Footprint:** `W08D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** guild quarters, administrative streets, tax/records zones.
- **Use:** Wide money/accounting office. Use near guilds, tax buildings, gold/merchant routes, or administrative plazas.

#### `GalakasMidCity_BT_GuildClerk_LongOffice_W10D30_CSG.tscn`
- **Footprint:** `W10D30`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** guild quarters, administrative streets, tax/records zones.
- **Use:** Long bureaucratic office row. Use for guild paperwork, contract clerks, records, and official commercial administration.

#### `GalakasMidCity_BT_GuildScribe_Block_W06D12_CSG.tscn`
- **Footprint:** `W06D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** guild quarters, administrative streets, tax/records zones.
- **Use:** Compact scribe/guild-record block. Good near guild offices, minor plazas, or streets with literacy/administration functions.

#### `GalakasMidCity_BT_OldGuildhall_Long_W14D22_CSG.tscn`
- **Footprint:** `W14D22`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** guild quarters, administrative streets, tax/records zones.
- **Use:** Old long guildhall. Use as an older landmark or guild anchor with heavier medieval massing.

#### `GalakasMidCity_BT_TaxOffice_Block_W10D16_CSG.tscn`
- **Footprint:** `W10D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** guild quarters, administrative streets, tax/records zones.
- **Use:** Tax office block. Good for Galakas bureaucracy/extortion flavor near guild, market, or gate-route streets.

### `06_Workshops_Production`

#### `GalakasMidCity_BT_DyersHall_LongWorkshop_W09D24_CSG.tscn`
- **Footprint:** `W09D24`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Long dyeing workshop/hall. Use in production streets, especially near cloth markets but slightly away from prestige areas.

#### `GalakasMidCity_BT_LongSideDoor_PrinterCourt_W09D24_CSG.tscn`
- **Footprint:** `W09D24`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Printer/court workshop with long-side entry. Good for side courtyards, lanes, and places where the front should not face the main street.

#### `GalakasMidCity_BT_LongSideDoor_Ropewalk_W12D30_CSG.tscn`
- **Footprint:** `W12D30`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Very long ropewalk/production building with long-side access. Use for narrow industrial strips and elongated craft yards.

#### `GalakasMidCity_BT_NobleWorkshopHybrid_W10D24_CSG.tscn`
- **Footprint:** `W10D24`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Hybrid of wealthy frontage and productive workshop. Good for richer craft patrons or old-money trade houses.

#### `GalakasMidCity_BT_PrinterCourt_Block_W12D20_CSG.tscn`
- **Footprint:** `W12D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Printer court block. Good for literate/civic neighborhoods and semi-industrial courtyards.

#### `GalakasMidCity_BT_Ropewalk_ExtremeLong_W12D30_CSG.tscn`
- **Footprint:** `W12D30`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Extreme-long ropewalk/craft building. Use where the street wants a very elongated industrial/craft edge.

#### `GalakasMidCity_BT_SootyForge_Row_W08D16_CSG.tscn`
- **Footprint:** `W08D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Forge/craft row with sootier industrial implication. Use near service streets and production clusters.

#### `GalakasMidCity_BT_TanneryBacklane_Long_W06D20_CSG.tscn`
- **Footprint:** `W06D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** craft streets, service alleys, production yards.
- **Use:** Long tannery/backlane production building. Use in smellier, dirtier service lanes away from prestige streets.

### `07_Storage_Warehouse_Service`

#### `GalakasMidCity_BT_GranaryRow_Long_W10D24_CSG.tscn`
- **Footprint:** `W10D24`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** back-of-market lanes, supply yards, service blocks.
- **Use:** Long granary/storage row. Use behind market streets, near service yards, or in supply-side districts.

#### `GalakasMidCity_BT_MerchantBacklane_Longhouse_W12D30_CSG.tscn`
- **Footprint:** `W12D30`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** back-of-market lanes, supply yards, service blocks.
- **Use:** Long merchant backlane building. Use behind the showier market frontage for storage/residence/service logic.

#### `GalakasMidCity_BT_UnderpassShop_Long_W08D20_CSG.tscn`
- **Footprint:** `W08D20`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** back-of-market lanes, supply yards, service blocks.
- **Use:** Long shop mass for underpass/covered route contexts. Use where the street passes beside or beneath heavier building volumes.

#### `GalakasMidCity_BT_WarehouseOverbuild_Long_W08D28_CSG.tscn`
- **Footprint:** `W08D28`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** back-of-market lanes, supply yards, service blocks.
- **Use:** Warehouse with overbuilt upper mass. Good for storage streets that still need vertical/roofline interest.

#### `GalakasMidCity_BT_YardAccess_Warehouse_W08D28_CSG.tscn`
- **Footprint:** `W08D28`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** back-of-market lanes, supply yards, service blocks.
- **Use:** Warehouse with yard-access logic. Good for loading areas, service courts, and back-of-market supply spaces.

### `08_Courtyard_Wealthy_Blocks`

#### `GalakasMidCity_BT_Caravanserai_MidBlock_W12D20_CSG.tscn`
- **Footprint:** `W12D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** Large trade-yard block. Use as a merchant inn/storage compound where caravans, pack animals, and trade guards would gather.

#### `GalakasMidCity_BT_CourtyardGate_House_W08D12_CSG.tscn`
- **Footprint:** `W08D12`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** House front that implies a gated courtyard beyond. Useful for semi-private merchant homes and more controlled access layouts.

#### `GalakasMidCity_BT_CourtyardSideEntry_Block_W10D24_CSG.tscn`
- **Footprint:** `W10D24`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** Longer courtyard block with side-entry logic. Good for merchant compounds, denser blocks, and streets where side access matters.

#### `GalakasMidCity_BT_DeepMerchantHouse_W06D16_CSG.tscn`
- **Footprint:** `W06D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** Deep merchant house. Use for well-off traders who need both residence and storage without becoming noble-scale.

#### `GalakasMidCity_BT_MerchantFamily_Longhouse_W12D24_CSG.tscn`
- **Footprint:** `W12D24`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** Large merchant-family longhouse. Good for affluent but still commercially tied households.

#### `GalakasMidCity_BT_OldNoble_Townhouse_W10D16_CSG.tscn`
- **Footprint:** `W10D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** Old noble townhouse. Good for the transition toward cleaner/richer inner MidCity and Imperium-facing streets.

#### `GalakasMidCity_BT_WealthyRow_CourtyardFront_W14D22_CSG.tscn`
- **Footprint:** `W14D22`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** cleaner inner MidCity, wealthy merchant streets, courtyard blocks.
- **Use:** Large wealthy row with courtyard frontage. Use near cleaner inner MidCity and the approach toward ImperiumCity.

### `09_Backstreet_Alley_SideDoors`

#### `GalakasMidCity_BT_AlleyFacing_Apothecary_W04D12_CSG.tscn`
- **Footprint:** `W04D12`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Narrow apothecary with alley-facing access. Good for side streets, hidden vendors, minor quest entrances, or quieter back lanes.

#### `GalakasMidCity_BT_AlleyFacing_Tailor_W05D16_CSG.tscn`
- **Footprint:** `W05D16`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Longer tailor shop with an alley entry. Use beside clothes markets or in cramped commercial lanes where access should not always face the main road.

#### `GalakasMidCity_BT_BacklaneDoor_Tenement_W06D28_CSG.tscn`
- **Footprint:** `W06D28`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Long tenant block with backlane door emphasis. Useful for dense housing just behind richer commercial streets.

#### `GalakasMidCity_BT_LongSideShop_Row_LeftEntry_W06D20_CSG.tscn`
- **Footprint:** `W06D20`
- **Access/entry intent:** long-side left entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Long shop row with left-side entry. Use to make alleys/courtyard edges feel navigable without repeating short-front doors.

#### `GalakasMidCity_BT_LongSideShop_Row_RightEntry_W06D20_CSG.tscn`
- **Footprint:** `W06D20`
- **Access/entry intent:** long-side right entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Long shop row with right-side entry. Pair with left-entry variants to alternate door placement along long street blocks.

#### `GalakasMidCity_BT_SideDoor_ClothHall_W10D30_CSG.tscn`
- **Footprint:** `W10D30`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Large cloth hall with side-door logic. Good for textile-market side streets and service entries.

#### `GalakasMidCity_BT_SideDoor_CobblerRow_W07D18_CSG.tscn`
- **Footprint:** `W07D18`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Cobbler row with side-door logic. Use in alleys or rows where door placement should face the pedestrian side path.

#### `GalakasMidCity_BT_SideDoor_DeepMerchantHouse_W08D20_CSG.tscn`
- **Footprint:** `W08D20`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Deep merchant house with side access. Good for courtyard blocks and long buildings along narrow lanes.

#### `GalakasMidCity_BT_SidePassage_MarketBlock_W08D20_CSG.tscn`
- **Footprint:** `W08D20`
- **Access/entry intent:** side/alley-oriented entry.
- **Placement:** alleys, side streets, courtyard edges, back lanes.
- **Use:** Market block with side-passage/double-door logic. Use to imply through-access, service lanes, or courtyard entry.

### `10_Rooftop_Route_Blocks`

#### `GalakasMidCity_BT_DenseRoofChase_Block_W08D28_CSG.tscn`
- **Footprint:** `W08D28`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Long dense block intended to create rooftops with frequent elevation changes. Good for chase-route corridors before overlaying traversal helpers.

#### `GalakasMidCity_BT_FortifiedMerchantRoof_W12D20_CSG.tscn`
- **Footprint:** `W12D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Merchant block with a stronger/defensive roof silhouette. Useful near richer streets where merchants protect goods and roof access.

#### `GalakasMidCity_BT_LongUpperGallery_Run_W08D28_CSG.tscn`
- **Footprint:** `W08D28`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Long upper-gallery mass. Good for creating balcony-like upper volumes and roof-adjacent routes without baked traversal props.

#### `GalakasMidCity_BT_MerchantSpine_VeryLong_W06D28_CSG.tscn`
- **Footprint:** `W06D28`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Very long narrow merchant spine. Use as a roofline corridor and dense street-wall segment.

#### `GalakasMidCity_BT_RoofGarden_Blockout_W08D20_CSG.tscn`
- **Footprint:** `W08D20`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Blockout mass for a roof-garden/flat-roof pause. Use as a calmer rooftop landing or visual breathing space.

#### `GalakasMidCity_BT_RooftopBridgeReceiver_W08D20_CSG.tscn`
- **Footprint:** `W08D20`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Building mass intended to receive a later rooftop bridge overlay. Use on opposite sides of alleys or route gaps.

#### `GalakasMidCity_BT_RooftopRun_Tenement_W08D28_CSG.tscn`
- **Footprint:** `W08D28`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Tenement block with a long roof-run profile. Use for chase routes above dense housing.

#### `GalakasMidCity_BT_RooftopTraining_Row_W06D28_CSG.tscn`
- **Footprint:** `W06D28`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Long row suitable for tutorial/testing roof traversal. Keep it where predictable roof pacing is useful.

#### `GalakasMidCity_BT_SlimRooftopConnector_W04D12_CSG.tscn`
- **Footprint:** `W04D12`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Slim connector building for roofline continuity. Use between larger masses to keep rooftop routes from becoming too broken.

#### `GalakasMidCity_BT_SlopedRoof_ChaseHouse_W06D16_CSG.tscn`
- **Footprint:** `W06D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Chase-house with a sloped roof profile. Good for controlled rooftop movement tests without adding separate traversal geometry.

#### `GalakasMidCity_BT_WideRooflineRun_W16D16_CSG.tscn`
- **Footprint:** `W16D16`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** roofline corridors and traversal-adjacent masses.
- **Use:** Wide roofline-run block. Use as a large rooftop route receiver or flat-ish movement segment.

### `11_Bridges_Overpasses_StreetConnectors`

#### `GalakasMidCity_BT_BridgeHouse_Linear_W18D10_CSG.tscn`
- **Footprint:** `W18D10`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** special connectors, covered streets, bridge-house sequences.
- **Use:** Long bridge-house mass. Use as a special linear block above or beside a passage, especially where roads compress beneath buildings.

#### `GalakasMidCity_BT_CoveredAlley_Row_W10D24_CSG.tscn`
- **Footprint:** `W10D24`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** special connectors, covered streets, bridge-house sequences.
- **Use:** Row mass designed to shape a covered alley condition. Use to create compression, shadow, and late-medieval street layering.

#### `GalakasMidCity_BT_GateStreet_OverpassLike_W10D16_CSG.tscn`
- **Footprint:** `W10D16`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** special connectors, covered streets, bridge-house sequences.
- **Use:** Gate-street overpass-like building. Use where a street narrows under a mass or where buildings visually pinch the route.

#### `GalakasMidCity_BT_LongButtressedHall_W14D22_CSG.tscn`
- **Footprint:** `W14D22`
- **Access/entry intent:** main/front entry or generic frontage.
- **Placement:** special connectors, covered streets, bridge-house sequences.
- **Use:** Large buttressed hall. Use as an old civic/market/guild hall where the architecture should feel heavier and older.

#### `GalakasMidCity_BT_MarketBridge_EndCap_W08D12_CSG.tscn`
- **Footprint:** `W08D12`
- **Access/entry intent:** connector/passage-oriented.
- **Placement:** special connectors, covered streets, bridge-house sequences.
- **Use:** End-cap for a bridge/market transition. Use at the end of covered streets, raised connections, or bridge-house sequences.
