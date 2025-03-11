using Godot;

namespace Renown.World {
	public partial class Government : Node2D {
		public class Debt {
			public Government Loaner;
			public Government Debter;
			public float Amount = 0.0f;
			
			public Debt( Government loaner, Government debter, float nAmount ) {
				Loader = loaner;
				Debter = debter;
				Amount = nAmount;
			}
		};
		
		/// <summary>
		/// Taxation rate increases when the government needs more money,
		/// but when tax increases, debts grow higher, and then crime goes up.
		/// no one escapes the wrath of the IRS
		/// </summary>
		[Export]
		private float DefaultTaxationRate = 1.0f;
		/// <summary>
		/// Reserves will be used when hiring mercenaries, and paying government
		/// workers
		/// </summary>
		[Export]
		private float FederalReserves = 0.0f;
		[Export]
		private Godot.Collections.Array<Thinkers.Thinker> Members;
		[Export]
		private Godot.Collections.Array<Thinkers.Thinker> Contractors;
		
		private System.Collections.Generic.List<Debt> Debts;
		
		private float InflationRate = 0.0f;
		private float TaxationRate = DefaultTaxationRate;
		private float CorruptionScore = 0.0f;
		
		public float GetDebt() {
			return FederalReserves >= 0.0f ? 0.0f : -FederalReserves;
		}
		public float GetReserves() {
			return FederalReserves;
		}
		public Godot.Collections.Array<Thinkers.Thinker> GetMembers() {
			return Members;
		}
		public float GetCorruption() {
			return CorruptionScore;
		}
		public float GetSalesTax() {
			return InflationRate + TaxationRate;
		}
		
		public void Think() {
			float delta = (float)GetProcessDeltaTime();
		}
		public bool CanGrantLoan( float nAmount ) {
			return FederalReserves - nAmount > 0.0f;
		}
		
		public float GetFromReserves( float nAmount ) {
			FederalReserves -= nAmount;
			
			if ( FederalReserves < 0.0f ) {
				TaxationRate += nAmount / 100.0f;
				
				// start printing money
				InflationRate += nAmount / 100.0f;
			} else {
				TaxationRate = DefaultTaxationRate;
				InflationRate -= nAmount / 100.0f;
				if ( InflationRate < 0.0f ) {
					InflationRate = 0.0f;
				}
			}
			
			return nAmount;
		}
		public void CollectTaxes( System.Collections.Generic.List<Thinkers.Thinker> citizens ) {
			for ( int i = 0; i < citizens.Count; i++ ) {
				float money = citizens[i].GetMoney();
				
				if ( money - TaxationRate < 0.0f ) {
					
				}
			}
		}
		public void PayWorkers() {
			for ( int i = 0; i < Contractors.Count; i++ ) {
				float pay = Contractors[i];
			}
		}
		
		public override void _Ready() {
			base._Ready();
			
			WorldTime.
		}
	};
};
