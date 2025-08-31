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

using Godot;
using Multiplayer;
using Steam;

namespace Renown.World {
	/*
	===================================================================================
	
	WorldTimeManager
	
	===================================================================================
	*/
	
	public partial class WorldTimeManager : Node {
		public static uint Year { get; private set; } = 0;
		public static uint Month { get; private set; } = 0;
		public static uint Day { get; private set; } = 0;
		public static uint Hour { get; private set; } = 0;

		private static uint NetworkYear = 0;
		private static uint NetworkMonth = 0;
		private static uint NetworkDay = 0;
		private static uint NetworkHour = 0;
		private static float NetworkTime = 0.0f;

		private float Time = 0.0f;
		private float PastMinute = -1.0f;

		private NetworkSyncObject SyncObject = null;
		private bool IsHostWorld = false;

		private const uint MinutesPerDay = 2440;
		private const uint MinutesPerHour = 60;
		private const float InGameToRealMinuteDuration = ( 2.0f * Mathf.Pi ) / MinutesPerDay;

		private int TotalDaysInYear = 0;

		public static WorldTimeManager Instance;

		// not a superman reference
		public DirectionalLight2D RedSunLight { get; private set; }

		[Export]
		private CanvasModulate WorldTimeOverlay;
		[Export]
		private GradientTexture1D Gradient;
		[Export]
		private float InGameSpeed = 1.0f;

		[Export]
		private uint StartingYear = 89949;
		[Export]
		private uint StartingMonth = 1;
		[Export]
		private uint StartingDay = 21;
		[Export]
		private float StartingHour = 12.0f;
		[Export]
		private Month[] Months;

		[Signal]
		public delegate void TimeTickEventHandler( uint day, uint hour, uint minute );
		[Signal]
		public delegate void DayTimeStartEventHandler();
		[Signal]
		public delegate void NightTimeStartEventHandler();
		[Signal]
		public delegate void NewMonthEventHandler();
		[Signal]
		public delegate void NewYearEventHandler();

		public static WorldTimestamp Now() {
			return new WorldTimestamp();
		}

		private void RecalculateTime() {
			uint totalMinutes = (uint)( Time / InGameToRealMinuteDuration );
			//			uint day = totalMinutes / MinutesPerDay;

			uint currentDayMinutes = totalMinutes % MinutesPerDay;
			Hour = currentDayMinutes / MinutesPerHour;
			uint minute = currentDayMinutes % MinutesPerHour;

			RedSunLight.GlobalRotation += Mathf.DegToRad( 1.0f / Hour ) * 0.001f;
			if ( Hour < 7 || Hour > 21 ) {
				RedSunLight.Energy = 0.0f;
			} else {
				RedSunLight.Energy = 1.0f;
			}
			if ( PastMinute != minute ) {
				if ( Hour >= 24 ) {
					Day++;
					if ( Day >= Months[ Month ].GetDayCount() ) {
						Day = 0;
						Month++;
						EmitSignalNewMonth();
						if ( Month >= Months.Length ) {
							Month = 0;
							Year++;
							EmitSignalNewYear();
						}
					}
					Hour = 0;
				} else if ( Hour >= 20 ) {
					EmitSignalNightTimeStart();
				} else if ( Hour >= 7 ) {
					EmitSignalDayTimeStart();
				}
				EmitSignalTimeTick( Day, Hour, minute );
				PastMinute = minute;
			}
		}

		private void SendPacket() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );

			byte changedBits = 0;
			if ( Year != NetworkYear ) {
				changedBits |= 0b00000001;
				NetworkYear = Year;
			}
			if ( Month != NetworkMonth ) {
				changedBits |= 0b00000010;
				NetworkMonth = Month;
			}
			if ( Day != NetworkDay ) {
				changedBits |= 0b00000100;
				NetworkDay = Day;
			}
			if ( Hour != NetworkHour ) {
				changedBits |= 0b00001000;
				NetworkHour = Hour;
			}
			if ( Time != NetworkTime ) {
				changedBits |= 0b00010000;
				NetworkTime = Time;
			}

			SyncObject.Write( changedBits );
			if ( ( changedBits & 0b00000001 ) != 0 ) {
				SyncObject.Write( Year );
			}
			if ( ( changedBits & 0b00000010 ) != 0 ) {
				SyncObject.Write( (ushort)Month );
			}
			if ( ( changedBits & 0b00000100 ) != 0 ) {
				SyncObject.Write( (ushort)Day );
			}
			if ( ( changedBits & 0b00001000 ) != 0 ) {
				SyncObject.Write( (ushort)Hour );
			}
			if ( ( changedBits & 0b00010000 ) != 0 ) {
				SyncObject.Write( Time );
			}
			SyncObject.Sync();
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			SyncObject.BeginRead( reader );

			byte changedBits = SyncObject.ReadByte();
			if ( ( changedBits & 0b00000001 ) != 0 ) {
				Year = SyncObject.ReadUInt32();
			}
			if ( ( changedBits & 0b00000010 ) != 0 ) {
				Month = SyncObject.ReadUInt16();
			}
			if ( ( changedBits & 0b00000100 ) != 0 ) {
				Day = SyncObject.ReadUInt16();
			}
			if ( ( changedBits & 0b00001000 ) != 0 ) {
				Hour = SyncObject.ReadUInt16();
				WorldTimeOverlay.Color = Gradient.Gradient.Sample( Mathf.Lerp( 0.0f, Gradient.Width, 1.0f / Hour ) );
			}
			if ( ( changedBits & 0b00010000 ) != 0 ) {
				Time = SyncObject.ReadFloat();
			}
		}

		public static float GetGameSpeed() => Instance.InGameSpeed;

		public override void _EnterTree() {
			base._EnterTree();
			Instance = this;
		}
		public override void _ExitTree() {
			base._ExitTree();

			for ( int i = 0; i < Months.Length; i++ ) {
				Months[ i ] = null;
			}

			Instance = null;
		}
		public override void _Ready() {
			base._Ready();

			RedSunLight = GetNode<DirectionalLight2D>( "SunLight" );
			RedSunLight.GlobalRotation = Mathf.DegToRad( Mathf.Lerp( 0.0f, 360.0f, 1.0f / StartingHour ) );
			Time = InGameToRealMinuteDuration * StartingHour * MinutesPerHour;

			Year = StartingYear;
			Month = StartingMonth;
			Day = StartingDay;
			Hour = (uint)StartingHour;

			for ( int i = 0; i < Months.Length; i++ ) {
				TotalDaysInYear += Months[ i ].GetDayCount();
			}

			NewYear += () => {
				if ( Year - StartingYear >= 1000 ) {
					SteamAchievements.ActivateAchievement( "ACH_FOREVER_ALONE_FOREVER_FORLORN" );
				} else if ( Year - StartingYear >= 1000000 ) {
					SteamAchievements.ActivateAchievement( "ACH_ONE_IN_A_MILLION" );
				} else if ( Year - StartingYear >= 1000000000 ) {
					SteamAchievements.ActivateAchievement( "ACH_TOUCH_GRASS" );
				}
			};

			Console.PrintLine( string.Format( "Starting time is [{0}, {1}:{2}]", Year, Month, Day ) );

			SetProcessInternal( false );
			SetPhysicsProcess( false );
			SetPhysicsProcessInternal( false );

			/*
			if ( SettingsData.GetNetworkingEnabled() ) {
				SyncObject = new NetworkSyncObject( sizeof( uint ) * 3 + sizeof( float ) );
				if ( SteamLobby.Instance.IsOwner() ) {
					// we're running the host's world
					//				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, null ) );
					IsHostWorld = true;
				} else {
					//				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
					SetProcess( false );
				}
			} else {
				IsHostWorld = true;
			}
			*/
			IsHostWorld = true;
			SetProcess( true );

			/*
			Console.AddCommand(
				commandName: "set_time_scale",
				function: Callable.From(
					( string arg ) => {
						GD.Print( "Setting world time scale to " + arg );
						InGameSpeed = (float)Convert.ToDouble( arg );
					}
				),
				arguments: [ "" ],
				required: 1
			);
			*/
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			if ( IsHostWorld && ( Engine.GetProcessFrames() % 30 ) != 0 ) {
				Time += (float)delta * InGameToRealMinuteDuration * InGameSpeed;
				RecalculateTime();
			}

			if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
				WorldTimeOverlay.Color = Gradient.Gradient.Sample( Mathf.Lerp( 0.0f, Gradient.Width, 1.0f / Hour ) );
			}
		}
	};
	public readonly struct WorldTimestamp {
		private readonly uint SavedYear = 0;
		private readonly uint SavedMonth = 0;
		private readonly uint SavedDay = 0;
		
		public WorldTimestamp() {
			SavedYear = WorldTimeManager.Year;
			SavedMonth = WorldTimeManager.Month;
			SavedDay = WorldTimeManager.Day;
		}
		public WorldTimestamp( uint Year, uint Month, uint Day ) {
			SavedYear = Year;
			SavedMonth = Month;
			SavedDay = Day;
		}
		public WorldTimestamp( WorldTimestamp other ) {
			SavedYear = other.SavedYear;
			SavedMonth = other.SavedMonth;
			SavedDay = other.SavedDay;
		}

		public bool LaterThan( WorldTimestamp other ) => SavedYear > other.SavedYear && SavedMonth > other.SavedMonth && SavedDay > other.SavedDay;
		public bool EarlierThan( WorldTimestamp other ) => SavedYear < other.SavedYear && SavedMonth < other.SavedMonth && SavedDay < other.SavedDay;

		public bool LaterThanOrSame( WorldTimestamp other ) => SavedYear >= other.SavedYear && SavedMonth >= other.SavedMonth && SavedDay >= other.SavedDay;
		public bool EarlierThanOrSame( WorldTimestamp other ) => SavedYear <= other.SavedYear && SavedMonth <= other.SavedMonth && SavedDay <= other.SavedDay;

		public static bool operator >( WorldTimestamp a, WorldTimestamp b ) => a.LaterThan( b );
		public static bool operator <( WorldTimestamp a, WorldTimestamp b ) => a.EarlierThan( b );

		public static bool operator >=( WorldTimestamp a, WorldTimestamp b ) => a.LaterThanOrSame( b );
		public static bool operator <=( WorldTimestamp a, WorldTimestamp b ) => a.EarlierThanOrSame( b );

		public uint GetYear() => SavedYear;
		public uint GetMonth() => SavedMonth;
		public uint GetDay() => SavedDay;
	};
};