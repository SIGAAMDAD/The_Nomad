/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Godot;

namespace Items {
	/// <summary>
	/// Extra effects that are applied to ammunition
	/// </summary>
	public enum ExtraAmmoEffects : byte {
		/// <summary>
		/// Sets the victim on fire, think Dragon's Breath but not just for shotguns, and its a warcrime to use it
		/// </summary>
		Incendiary = 0x0001,

		/// <summary>
		/// Burns the skin of the victim, considered a warcrime, and has a slight AOE (very small)
		/// </summary>
		IonicCharge = 0x0002,

		/// <summary>
		/// EXPLOOOSION! Blows up on impact
		/// </summary>
		Explosive = 0x0004,

		/// <summary>
		/// More stance damage, less real damage, opposite of <see cref="HollowPoint"/>
		/// </summary>
		ArmorPiercing = 0x0008,

		/// <summary>
		/// Less stance damage, more real damage, opposite of <see cref="ArmorPiercing"/>
		/// </summary>
		HollowPoint = 0x0010
	};

	/// <summary>
	/// Flags that can be applied to <see cref="AmmoType.Pellets"/> to make them do fancy "bullshit" things
	/// </summary>
	public enum ShotgunBullshit : byte {
		/// <summary>
		/// Small iron darts that does DOT (damage over time), not high initial/upfront damage
		/// </summary>
		Flechette,

		/// <summary>
		/// Heavy round lead balls, great for destroying targets a close range but also
		/// has insane recoil.
		/// </summary>
		Buckshot,

		/// <summary>
		/// Thousands of extremely small pellets that are more of a spray-and-pray than anything else.
		/// Don't use this if you're actually trying to kill something
		/// </summary>
		Birdshot,

		/// <summary>
		/// A bunch of scrap metal shoved into a shotgun shell, extremely volatile, dangerous, and fun.
		/// Caution is advised
		/// </summary>
		Shrapnel,

		/// <summary>
		/// A dense, spherical ball of lead that can punch through a lot of stuff.
		/// Insanely high damage, but its a single projectile instead of a spread of pellets
		/// </summary>
		Slug,

		/// <summary>
		/// The bog-standard shotgun pellets
		/// </summary>
		None
	};

	/*
	===================================================================================

	Ammo

	===================================================================================
	*/
	/// <summary>
	/// Stores relevant information about an ammunition type
	/// </summary>

	public readonly struct Ammo {
		/// <summary>
		/// The ammo's range
		/// </summary>
		public readonly float Range = 0.0f;

		/// <summary>
		/// The ammo's (base) damage
		/// </summary>
		public readonly float Damage = 0.0f;

		/// <summary>
		/// How fast the ammo travels per second
		/// </summary>
		public readonly float Velocity = 0.0f;

		/// <summary>
		/// The item_id of the ItemDefinition
		/// </summary>
		public readonly string? ItemId = null;

		/// <summary>
		/// The base type of the ammo, see <see cref="AmmoType"/> for more information
		/// </summary>
		public readonly AmmoType Type;

		/// <summary>
		/// The extra effects that are applied to the ammo, see <see cref="ExtraAmmoFlags"/> for more information
		/// </summary>
		public readonly ExtraAmmoEffects Effects;

		/// <summary>
		/// The damage falloff over range
		/// </summary>
		public readonly Curve DamageFalloff;

		/// <summary>
		/// The sound effect played when the ammo is picked up, should be unique for distinction purposes
		/// </summary>
		public readonly AudioStream PickupSfx;

		//
		// shotgun exclusive data
		//

		/// <summary>
		/// See <see cref="ShotgunBullshit"/> for more
		/// </summary>
		public readonly ShotgunBullshit ShotgunBullshit;

		/// <summary>
		/// The amount of pellets per shot
		/// </summary>
		public readonly int PelletCount = 0;

		private static readonly Dictionary<string, ExtraAmmoEffects> ExtraFlags = new Dictionary<string, ExtraAmmoEffects>{
			{ "incendiary", ExtraAmmoEffects.Incendiary },
			{ "ionic_charge", ExtraAmmoEffects.IonicCharge },
			{ "explosive", ExtraAmmoEffects.Explosive },
			{ "armor_piercing", ExtraAmmoEffects.ArmorPiercing },
			{ "hollow_point", ExtraAmmoEffects.HollowPoint }
		};

		/*
		===============
		Ammo
		===============
		*/
		/// <summary>
		/// Constructs an ammo object based on a provided ItemDefinition
		/// </summary>
		/// <param name="resource">The ItemDefinition of the ammo</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="resource"/> is null</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="resource"/> doesn't contain a properties dictionary, or if an ammo effect isn't valid</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if an ammo's effects array isn't a PackedStringArray type</exception>
		public Ammo( Resource? resource ) {
			ArgumentNullException.ThrowIfNull( resource );

			ItemId = resource.Get( "id" ).AsString();

			Godot.Collections.Dictionary properties = resource.Get( "properties" ).AsGodotDictionary()
				?? throw new InvalidOperationException( "ammo ItemDefinition resource doesn't contain a properties dictionary" );

			if ( properties.TryGetValue( "effects", out Variant value ) ) {
				if ( value.VariantType != Variant.Type.PackedStringArray ) {
					throw new ArgumentOutOfRangeException( "Error initializing ammo flags, effects isn't a PackedStringArray" );
				}
				string[] effects = value.AsStringArray();
				for ( int i = 0; i < effects.Length; i++ ) {
					if ( ExtraFlags.TryGetValue( effects[ i ], out ExtraAmmoEffects flags ) ) {
						Effects |= ExtraFlags[ effects[ i ] ];
					} else {
						throw new KeyNotFoundException( $"ammo flag {effects[ i ]} isn't a valid effect" );
					}
				}
			}

			LoadProperty( properties, "damage_falloff", out DamageFalloff );
			LoadProperty( properties, "pickup_sfx", out PickupSfx );
			LoadProperty( properties, "damage", out Damage );
			LoadProperty( properties, "range", out Range );
			LoadProperty( properties, "velocity", out Velocity );
			LoadProperty( properties, "type", out Type );

			if ( Type == AmmoType.Pellets ) {
				LoadProperty( properties, "shotgun_bullshit", out ShotgunBullshit );
				LoadProperty( properties, "pellet_count", out PelletCount );
			}
		}

		/*
		===============
		LoadProprety
		===============
		*/
		/// <summary>
		/// Loads a property from the <paramref name="properties"/> dictionary
		/// </summary>
		/// <typeparam name="T">The type of the value to load</typeparam>
		/// <param name="properties">The ItemDefinition properties dictionary</param>
		/// <param name="keyName">The name of the property to load</param>
		/// <param name="value">The variable to put the value into</param>
		/// <exception cref="InvalidOperationException">Thrown if the type in the variant isn't the same as the expected value's type</exception>
		/// <exception cref="KeyNotFoundException">Thrown if the property wasn't found</exception>
		private static void LoadProperty<[MustBeVariant] T>( in Godot.Collections.Dictionary properties, in string keyName, out T value ) {
			if ( properties.TryGetValue( keyName, out Variant variant ) ) {
				try {
					value = variant.As<T>();
				} catch ( InvalidCastException ) {
					throw new InvalidOperationException( $"ammo ItemDefinition \"{keyName}\" property must be a {typeof( T ).FullName}" );
				}
			} else {
				throw new KeyNotFoundException( $"ammo ItemDefinition property \"{keyName}\" wasn't found" );
			}
		}
	};
};