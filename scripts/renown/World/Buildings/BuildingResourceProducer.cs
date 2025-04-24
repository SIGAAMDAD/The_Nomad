using System.Collections.Generic;
using Renown.Thinkers;
using Godot;

namespace Renown.World.Buildings {
	/// <summary>
	/// generates raw materials from thin air.
	/// merchants call the GetResources function, then sell the resources
	/// </summary>
	public partial class BuildingResourceProducer : Building {
		[Export]
		private ResourceType ProduceType;
		[Export]
		private uint ReplenishInterval = 1; // in hours
		[Export]
		private int MaxStorage = 5000;
		[Export]
		private float ReplenishRate = 0.5f;

		private List<Thinker> Workers = null;

		private uint LastReplenishTime = 0;

		private float ItemGeneration = 0.0f;
		private int Storage = 0;

		public int GetStorage() => Storage;
		public float GetPriority() => ReplenishRate;

		public ResourceType GetResourceType() => ProduceType;
		public int GetMaxStorage() => MaxStorage;

		public void IncreasePriority( float nAmount ) => ReplenishRate += nAmount;
		public void DecreasePriority( float nAmount ) => ReplenishRate -= nAmount;

		public int GetResources( int nAmount ) {
			if ( Storage - nAmount < 0 ) {
				int tmp = Storage;
				Storage = 0;
				return tmp;
			}
			Storage -= nAmount;
			return nAmount;
		}
		private void OnResourceReplenish( uint day, uint hour, uint minute ) {
			if ( State != BuildingState.Stable ) {
				return; // not a stable location
			}
			if ( hour - LastReplenishTime >= ReplenishInterval ) {
				// TODO: send enforcers/mercs to make them work harder
				int activeWorkers = 0;
				for ( int i = 0; i < Workers.Count; i++ ) {
					if ( Workers[i].GetState() == ThinkerState.Working ) {
						activeWorkers++;
					}
				}
				if ( Storage < MaxStorage ) {
					int addAmount = (int)( ReplenishRate * activeWorkers );
					Storage += (int)( ReplenishRate * activeWorkers );
					GD.Print( "Replenished resource producer: " + addAmount );
				}
				LastReplenishTime = hour;
			}
		}

		public void AddWorker( Thinker thinker ) {
			Workers.Add( thinker );
			thinker.Die += ( Entity source, Entity target ) => { Workers.Remove( target as Thinker ); };
			GD.Print( "...Added worker to production plant " + this );
		}

		public override void _Ready() {
			base._Ready();

			WorldTimeManager.Instance.TimeTick += OnResourceReplenish;

			Workers = new List<Thinker>();

			ProcessMode = ProcessModeEnum.Disabled;
		}
	};
};