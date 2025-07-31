using System.Collections.Generic;
using PlayerSystem.Runes;
using PlayerSystem.Perks;
using PlayerSystem.Totems;
using System.Runtime.CompilerServices;

namespace PlayerSystem {
	public class Inventory {
		private static readonly int MaximumQuickAccessSlots = 4;
		private static readonly int MaximumRuneSlots = 5;
		private static readonly float MaximumInventoryWeight = 500.0f;

		private ConsumableStack[] QuickAccess = new ConsumableStack[ MaximumQuickAccessSlots ];

		private float CurrentInventoryWeight = 0.0f;
		private HashSet<Rune> Runes = new HashSet<Rune>();
		private HashSet<Perk> Perks = new HashSet<Perk>();

		private Totem TotemSlot = null;
		private Perk PerkSlot = null;
		private Rune[] RuneSlots = new Rune[ MaximumRuneSlots ];

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsEncumbered() => CurrentInventoryWeight > MaximumInventoryWeight * 0.75f;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void AddRune( Rune rune ) => Runes.Add( rune );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool HasRune( Rune rune ) => Runes.Contains( rune );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void AddPerk( Perk perk ) => Perks.Add( perk );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool HasPerk( Perk perk ) => Perks.Contains( perk );

		public void EquipRune( int nSlot, Rune rune ) {
			if ( nSlot > RuneSlots.Length || nSlot < 0 ) {
				return;
			}
			RuneSlots[ nSlot ] = rune;
		}
		public void EquipTotem( Totem totem ) => TotemSlot = totem;
		public void EquipPerk( Perk perk ) => PerkSlot = perk;
	};
};