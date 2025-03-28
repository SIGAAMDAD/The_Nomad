using Godot;

namespace Renown.World {
	public partial class WorldTimeManager : Node {
		[Export]
		private uint StartingYear = 89949;
		[Export]
		private uint StartingMonth = 1;
		[Export]
		private uint StartingDay = 21;
		[Export]
		private float StartingHour = 12.0f;

		public static uint Year = 0;
		public static uint Month = 0;
		public static uint Day = 0;
		public static WorldTimeManager Instance;

		// not a superman reference
		private DirectionalLight2D RedSunLight;

		[Export]
		private CanvasModulate WorldTimeOverlay;
		[Export]
		private GradientTexture1D Gradient;
		[Export]
		private float InGameSpeed = 5.0f;

		private float Time = 0.0f;
		private float PastMinute = -1.0f;

		private NetworkWriter SyncObject = null;
		private bool IsHostWorld = false;

		private const uint MinutesPerDay = 2440;
		private const uint MinutesPerHour = 60;
		private const float InGameToRealMinuteDuration = ( 2.0f * Mathf.Pi ) / MinutesPerDay;

		[Signal]
		public delegate void TimeTickEventHandler( uint day, uint hour, uint minute );
		[Signal]
		public delegate void DayTimeStartEventHandler();
		[Signal]
		public delegate void NightTimeStartEventHandler();

		private void RecalculateTime() {
			uint totalMinutes = (uint)( Time / InGameToRealMinuteDuration );
			uint day = totalMinutes / MinutesPerDay;

			uint currentDayMinutes = totalMinutes % MinutesPerDay;
			uint hour = currentDayMinutes / MinutesPerHour;
			uint minute = currentDayMinutes % MinutesPerHour;

			RedSunLight.GlobalRotation += Mathf.DegToRad( 1.0f / hour ) * 0.01f;
			if ( hour < 7 || hour > 21 ) {
				RedSunLight.Energy = 0.0f;
			} else {
				RedSunLight.Energy = 1.0f;
			}
			if ( PastMinute != minute ) {
				if ( minute == 0 ) {
					if ( hour == 24 ) {
						Day++;
						hour = 0;
					} else if ( hour >= 20 ) {
						EmitSignal( "NightTimeStart" );
					} else if ( hour >= 7 ) {
						EmitSignal( "DayTimeStart" );
					}
				}
				if ( Year - StartingYear >= 1000 ) {
					SteamAchievements.ActivateAchievement( "ACH_FOREVER_ALONE_FOREVER_FORLORN" );
				} else if ( Year - StartingYear >= 1000000 ) {
					SteamAchievements.ActivateAchievement( "ACH_ONE_IN_A_MILLION" );
				} else if ( Year - StartingYear >= 1000000000 ) {
					SteamAchievements.ActivateAchievement( "ACH_TOUCH_GRASS" );
				}
				EmitSignal( "TimeTick", day, hour, minute );
				PastMinute = minute;
			}
		}

		private void SendPacket() {
			SyncObject.Write( Year );
			SyncObject.Write( Month );
			SyncObject.Write( Day );
			SyncObject.Write( Time );
			SyncObject.Sync();
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			Year = reader.ReadUInt32();
			Month = reader.ReadUInt32();
			Day = reader.ReadUInt32();
			Time = (float)reader.ReadDouble();
		}

		public override void _EnterTree() {
			base._EnterTree();
			Instance = this;
		}
        public override void _Ready() {
			base._Ready();

			RedSunLight = GetNode<DirectionalLight2D>( "SunLight" );
			RedSunLight.ShadowEnabled = true;
			RedSunLight.GlobalRotation = Mathf.DegToRad( Mathf.Lerp( 0.0f, 360.0f, 1.0f / StartingHour ) );
			//TODO: adjustable sunlight shadow quality

			Time = InGameToRealMinuteDuration * StartingHour * MinutesPerHour;

			Year = StartingYear;
			Month = StartingMonth;
			Day = StartingDay;

			SetProcessInternal( false );

			if ( SettingsData.GetNetworkingEnabled() ) {
				SyncObject = new NetworkWriter( sizeof( uint ) * 3 + sizeof( double ) );
				if ( SteamLobby.Instance.IsOwner() ) {
					// we're running the host's world
					SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, null ) );
					IsHostWorld = true;
				} else {
					SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
					SetProcess( false );
				}
			} else {
				IsHostWorld = true;
			}
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			if ( IsHostWorld ) {
				Time += (float)delta * InGameToRealMinuteDuration * InGameSpeed;
				RecalculateTime();
			}

			if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
				float value = Time;
				WorldTimeOverlay.Color = Gradient.Gradient.Sample( value );
			}
		}
	};
	public partial class WorldTimestamp : Node {
		private uint Year = 0;
		private uint Month = 0;
		private uint Day = 0;

		public WorldTimestamp() {
			Year = WorldTimeManager.Year;
			Month = WorldTimeManager.Month;
			Day = WorldTimeManager.Day;
		}

		public uint GetYear() {
			return Year;
		}
		public uint GetMonth() {
			return Month;
		}
		public uint GetDay() {
			return Day;
		}
	};
};