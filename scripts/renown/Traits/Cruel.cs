using System.Runtime.CompilerServices;

namespace Renown.Traits {
	public partial class Cruel : Trait {
		public Cruel() {
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override string GetTraitName() => "Cruel";
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override TraitType GetTraitType() => TraitType.Cruel;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static sbyte GetFearBias() => 30;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static sbyte GetTrustBias() => -10;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.Merciful ) {
				return true;
			}
			return false;
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.Merciful ) {
				return true;
			}
			return false;
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.Merciful ) {
				return false;
			}
			return true;
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.Merciful ) {
				return false;
			}
			return true;
		}
	};
};