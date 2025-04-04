using System.Diagnostics;
using Godot;

namespace Renown.World {
	public enum WeatherType : uint {
		Clear,
		Humid,
		Snowing,
		British,
		Raining,
		ThunderStorm,
		Windy,
		Blazing,
		
		Count
	};
	
	public partial class Biome : WorldArea {
		/// <summary>
		/// The current weather of the biome
		/// </summary>
		[Export]
		private WeatherType CurrentWeather = WeatherType.Clear;
		
		/// <summary>
		/// Chance of biome clear & sunny weather
		/// </summary>
		[Export]
		private float WeatherChanceClear = 0.0f;
		
		/// <summary>
		/// Chance of biome hot & humid weather
		/// </summary>
		[Export]
		private float WeatherChanceHumid = 0.0f;
		
		/// <summary>
		/// Chance of biome snow
		/// </summary>
		[Export]
		private float WeatherChanceSnowing = 0.0f;
		
		/// <summary>
		/// Chance of biome british weather
		/// </summary>
		[Export]
		private float WeatherChanceBritish = 0.0f;
		
		/// <summary>
		/// Chance of biome rain
		/// </summary>
		[Export]
		private float WeatherChanceRaining = 0.0f;
		
		/// <summary>
		/// Chance of biome thunderstorm
		/// </summary>
		[Export]
		private float WeatherChanceThunderStorm = 0.0f;
		
		/// <summary>
		/// Chance of biome wind
		/// </summary>
		[Export]
		private float WeatherChanceWindy = 0.0f;
		
		/// <summary>
		/// Chance of biome heatstroke weather
		/// </summary>
		[Export]
		private float WeatherChanceBlazing = 0.0f;
		
		/// <summary>
		/// How often weather changes are calculated in this biome
		/// </summmary>
		[Export]
		private float WeatherChangeInterval = 0.0f;
		
		private Timer WeatherChangeTimer;
		private Area2D RegionArea;
		private System.Collections.Generic.Dictionary<WeatherType, float> WeatherChances;
		
		[Signal]
		public delegate void AgentEnteredAreaEventHandler( CharacterBody2D agent );
		[Signal]
		public delegate void AgentExitedAreaEventHandler( CharacterBody2D agent );
		
		private void OnRegionBodyShape2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is not CharacterBody2D ) {
				return;
			}
			body.Call( "SetLocation", this );
			EmitSignal( "AgentEnteredArea", (CharacterBody2D)body );
		}
		private void OnRegionBodyShape2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is not CharacterBody2D ) {
				return;
			}
			EmitSignal( "AgentExitedArea", (CharacterBody2D)body );
		}
		private void OnWeatherChangeTimerTimeout() {
			float chance = 0.0f;
			WeatherType weather = WeatherType.Clear;
			
			// TODO: have the current season have an impact on the weathere
			for ( int i = 0; i < WeatherChances.Count; i++ ) {
				float other = WeatherChances[ weather ];
				if ( other > chance ) {
					chance = other;
					weather = (WeatherType)i;
				}
			}
			CurrentWeather = weather;
		}
		
		public override void _Ready() {
			WeatherChances = new System.Collections.Generic.Dictionary<WeatherType, float>{
				{ WeatherType.Clear, WeatherChanceClear },
				{ WeatherType.Humid, WeatherChanceHumid },
				{ WeatherType.Snowing, WeatherChanceSnowing },
				{ WeatherType.British, WeatherChanceBritish },
				{ WeatherType.Raining, WeatherChanceRaining },
				{ WeatherType.ThunderStorm, WeatherChanceThunderStorm },
				{ WeatherType.Windy, WeatherChanceWindy },
				{ WeatherType.Blazing, WeatherChanceBlazing },
			};
			
			WeatherChangeTimer = new Timer();
			WeatherChangeTimer.SetProcess( false );
			WeatherChangeTimer.SetProcessInternal( false );
			WeatherChangeTimer.WaitTime = WeatherChangeInterval;
			WeatherChangeTimer.Connect( "timeout", Callable.From( OnWeatherChangeTimerTimeout ) );
			AddChild( WeatherChangeTimer );
			
			RegionArea = GetNode<Area2D>( "BiomeArea" );
			RegionArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnRegionBodyShape2DEntered ) );
			RegionArea.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnRegionBodyShape2DExited ) );

			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = Constants.THREAD_GROUP_BIOMES;
			ProcessThreadMessages = ProcessThreadMessagesEnum.MessagesPhysics;
		}
	};
};
