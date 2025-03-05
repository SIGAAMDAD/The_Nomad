using Godot;

namespace Renown {
	public partial class Month : Resource {
		[Export]
		private string Name;
		[Export]
		private uint DayCount;
		
		public Month( string name, uint nDayCount ) {
			Name = name;
			DayCount = nDayCount;
		}
		
		public string GetMonthName() {
			return Name;
		}
		public uint GetDayCount() {
			return DayCount;
		}
	};
	
	public partial class WorldTime : Node {
		[Export]
		public uint Year = 89998;
		[Export]
		public int Month = 1;
		[Export]
		public uint Day = 34;
		[Export]
		private Godot.Collections.Array<Month> Months;

		private static WorldTime _Instance;
		public static WorldTime Instance => _Instance;
		
		private Timer DayTimer;
		private Timer NightTimer;
		private uint DaysInYear = 0;
		
		[Signal]
		public delegate void DayTimeStartEventHandler();
		[Signal]
		public delegate void NightTimeStartEventHandler();
		[Signal]
		public delegate void MonthStartEventHandler( Month month );
		
		private void OnDayTimerFinished() {
			NightTimer.Start();
			
			EmitSignal( "NightTimeStart" );
		}
		private void OnNightTimerFinished() {
			Day++;
			
			if ( Day >= Months[ Month ].GetDayCount() ) {
				Day = 0;
				Month++;
				if ( Month == Months.Count ) {
					Year++;
					Month = 0;
				}
				
				EmitSignal( "MonthStart", Months[ Month ] );
			}
			
			DayTimer.Start();
			
			EmitSignal( "DayTimeStart" );
		}
		
		public override void _Ready() {
			base._Ready();
			
			DaysInYear = 0;
			for ( int i = 0; i < Months.Count; i++ ) {
				DaysInYear += Months[i].GetDayCount();
			}
			
			DayTimer = GetNode<Timer>( "DayTimer" );
			DayTimer.Connect( "timeout", Callable.From( OnDayTimerFinished ) );
			
			NightTimer = GetNode<Timer>( "NightTimer" );
			NightTimer.Connect( "timeout", Callable.From( OnNightTimerFinished ) );
			
			DayTimer.Start();
			NightTimer.Start();
		}
		public override void _Process( double delta ) {
		}
	};
	
	public class WorldTimestamp {
		private uint SavedYear = 0;
		private int SavedMonth = 0;
		private uint SavedDay = 0;
		
		public WorldTimestamp() {
			SavedYear = WorldTime.Instance.Year;
			SavedMonth = WorldTime.Instance.Month;
			SavedDay = WorldTime.Instance.Day;
		}
		
		uint GetYear() {
			return SavedYear;
		}
		int GetMonth() {
			return SavedMonth;
		}
		uint GetDay() {
			return SavedDay;
		}
	};
};