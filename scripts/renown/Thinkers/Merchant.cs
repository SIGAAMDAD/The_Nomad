using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Merchant : Thinker {
		private enum State : int {
			FindingCargo,
			TransportingCargo,
			SellingCargo,

			Count
		};

		[Export]
		private Godot.Collections.Dictionary<ResourceType, int> StartingCargo = new Godot.Collections.Dictionary<ResourceType, int>();

		private Dictionary<ResourceType, int> Cargo;
		private TradeRoute CurrentRoute = null;
		private Settlement TradeLocation = null;
		private State Action;

		public override void _Ready() {
			base._Ready();

			Cargo = new Dictionary<ResourceType, int>( StartingCargo.Count );
			foreach ( var cargo in StartingCargo ) {
				Cargo.Add( cargo.Key, cargo.Value );
			}
			StartingCargo = null;

			Action = Cargo.Count == 0 ? State.FindingCargo : State.TransportingCargo;
		}

		private Settlement GetClosestTradeLocation() {
			Settlement location = null;
			float bestDistance = float.MaxValue;

			foreach ( var settlement in Settlement.Cache.Cache ) {
				float dist = GlobalPosition.DistanceTo( settlement.Value.GlobalPosition );
				if ( dist < bestDistance ) {
					location = settlement.Value;
				}
			}
			return location;
		}
		private void FindCargo() {
			if ( Location == null ) {
				TradeLocation = GetClosestTradeLocation();
				GotoPosition = TradeLocation.GlobalPosition;
			}
		}
		protected override void Think( float delta ) {
			switch ( Action ) {
			case State.FindingCargo:
				FindCargo();
				break;
			case State.TransportingCargo:
				break;
			};
		}
	};
};