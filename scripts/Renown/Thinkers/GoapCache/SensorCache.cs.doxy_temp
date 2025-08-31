using MountainGoap;
using System.Collections.Generic;

namespace Renown.Thinkers.GoapCache {
	/// <summary>
	/// generic goals that aren't faction specific
	/// </summary>
	public enum SensorType : uint {
		SightSensor,

		Count
	};

	public static class GoapSensorCache {
		public static Dictionary<SensorType, Sensor> Cache;

		static GoapSensorCache() {
			Cache = new Dictionary<SensorType, Sensor> {
				{
					SensorType.SightSensor,
					new Sensor(
						runCallback: ( agent ) => ( (Thinker)agent.State[ "Owner" ] ).Call( "CheckSight" ),
						name: "SightSensor"
					)
				}
			};
		}
	};
};