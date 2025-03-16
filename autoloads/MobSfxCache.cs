using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class MobSfxCache : Node {
	public static List<AudioStream> TargetSpotted;
	public static List<AudioStream> HeavyDead;
	public static List<AudioStream> Ceasefire;
	public static List<AudioStream> Alert;
	public static List<AudioStream> Confusion;
	public static List<AudioStream> ManDown;
	public static List<AudioStream> Curse;
	public static List<AudioStream> TargetPinned;
	public static List<AudioStream> TargetRunning;
	public static List<AudioStream> OutOfTheWay;
	public static List<AudioStream> CheckItOut;
	public static AudioStream ManDown2;
	public static AudioStream ManDown3;
	public static AudioStream Deaf;
	public static AudioStream SquadWiped;
	public static List<AudioStream> NeedBackup;
	public static AudioStream Unstoppable;
	
	public static List<AudioStream> Help;
	public static List<AudioStream> RepeatPlease;
	
	public static void Cache() {
		GD.Print( "Loading mob sound effects..." );

		TargetSpotted = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21198.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21199.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21167.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21200.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21201.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21202.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21203.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21204.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21205.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21207.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21208.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21209.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21210.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21211.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21212.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21213.mp3" ),
		};
		TargetPinned = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21161.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21162.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21163.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21186.mp3" ),
		};
		TargetRunning = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21156.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21157.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21159.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21160.mp3" ),
		};
		Ceasefire = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_0.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_2.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_3.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/ceasefire_cmd_4.mp3" ),
		};
		OutOfTheWay = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21376.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21377.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21381.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_0.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/get_down_cmd_1.mp3" ),
		};
		Curse = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21009.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21010.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21011.mp3" )
		};
		Alert = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21028.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21029.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21030.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21033.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21034.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21026.mp3" )
		};
		CheckItOut = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21100.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21172.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21175.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/search_area_cmd_0.mp3" )
		};
		Confusion = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21164.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21165.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21168.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21169.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21170.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21171.mp3" ),
		};
		HeavyDead = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14859.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14860.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14861.mp3" )
		};
		ManDown = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21348.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21359.mp3" )
		};
		ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/man_down_2_callout_0.mp3" );
		ManDown2 = ResourceLoader.Load<AudioStream>( "res://sounds/barks/men_down_3_callout_0.mp3" );
		Deaf = ResourceLoader.Load<AudioStream>( "res://sounds/barks/deaf_callout.mp3" );
		SquadWiped = ResourceLoader.Load<AudioStream>( "res://sounds/barks/squad_wiped_callout_0.mp3" );

		NeedBackup = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21193.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21194.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21195.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21196.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21197.mp3" ),
		};
		Help = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21189.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/21190.mp3" )
		};
		RepeatPlease = new List<AudioStream>{
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14865.mp3" ),
			ResourceLoader.Load<AudioStream>( "res://sounds/barks/14866.mp3" )
		};
	}
};