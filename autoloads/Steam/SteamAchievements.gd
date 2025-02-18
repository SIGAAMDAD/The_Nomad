extends Node

# don't judge me...
enum AchievementID {
	R_U_Cheating = 0,
	Complete_Domination,
	Geneva_Suggestion,
	Pyromaniac,
	Massacre,
	Just_A_Minor_Inconvenience,
	Jack_The_Ripper,
	Explosion_Connoisseuir,
	God_of_War,
	Boom_Headshot,
	Respectful,
	A_Legend_is_Born,
	Building_the_Legend,
	Same_Shit_Different_Day,
	Shut_The_FUCK_Up,
	This_Is_SPARTA,
	Unearthed_Arcana,
	Awaken_The_Ancients,
	Its_High_Noon,
	Death_From_Above,
	Kombatant,
	Laughing_In_Deaths_Face,
	Right_Back_at_U,
	Bitch_Slap,
	Chefs_Special,
	Knuckle_Sandwich,
	Brother_In_Arms,
	BRU,
	GIT_PWNED,
	Silent_Death,
	Well_Done_Weeb,
	Its_Treason_Then,
	Heartless,
	Bushido,
	Maximus_The_Merciful,
	Cheer_Up_Love_The_Cavalrys_Here,
	Worse_Than_Death,
	Looks_Like_Meats_Back_On_Our_Menu_Boys,
	GYAT,
	To_The_Slaughter,
	One_Man_Army,
	You_Call_That_A_KNIFE,
	AMERICA_FUCK_YEA,
	Sussy,
	Live_To_Fight_Another_Day,
	Remember_Us,
	Zandutsu_That_Shit,
	MORE,
	Edgelord,
	That_Actually_WORKED,
	NANOMACHINES_SON,
	Cool_Guys_Dont_Look_At_Explosions,
	Double_Take,
	Triple_Threat,
	DAYUUM_I_Aint_Gonna_Sugarcoat_It,
	Back_From_The_Brink,
	Dance_DANCE_DANCE,
	Bang_Bang_I_Shot_Em_Down,
	BOP_IT,
	Just_A_Leap_of_Faith,
	Send_Them_To_Jesus,
	Rizzlord,
	AHHH_GAHHH_HAAAAAAA,
	Absolutely_Necessary_Precautions,
	Stop_Hitting_Yourself,
	Fuck_This_Shit_Im_Out,
	Smoke_Break,
	
	Count
};

class SteamAchievement:
	var _id:AchievementID = 0
	var _id_string:String = ""
	var _name:String = ""
	var _description:String = ""
	var _achieved:bool = false
	var _value:Variant = null
	
	func _init( id: AchievementID, name: String, description: String ) -> void:
		_id = id
		_id_string = "ACH_" + AchievementID.keys()[ id ].to_upper()
		_name = name
		_description = description
		
		if SteamManager._is_me:
			Steam.setAchievement( _id_string )
		
		print( "Added SteamAPI Achievement ", _id_string, "/\"", _name, "\"" )

var ACHIEVEMENT_TABLE:Dictionary = {
	AchievementID.R_U_Cheating: SteamAchievement.new( AchievementID.R_U_Cheating, "R U Cheating?", "Beat the game on Insane or higher" ),
	AchievementID.Complete_Domination: SteamAchievement.new( AchievementID.Complete_Domination, "Complete Domination", "End the War of the Wastes with Galakas still in your control" ),
	AchievementID.Geneva_Suggestion: SteamAchievement.new( AchievementID.Geneva_Suggestion, "Geneva Suggestion", "Commit your first warcrime" ),
	AchievementID.Pyromaniac: SteamAchievement.new( AchievementID.Pyromaniac, "Pyromaniac", "Light 10 enemies on fire at the same time" ),
	AchievementID.Massacre: SteamAchievement.new( AchievementID.Massacre, "Massacre", "Beat a level without sparing anyone except for non-combatants" ),
	AchievementID.Just_A_Minor_Inconvenience: SteamAchievement.new( AchievementID.Just_A_Minor_Inconvenience, "Just a Minor Inconvenience", "Beat the game without taking any damage" ),
	AchievementID.Jack_The_Ripper: SteamAchievement.new( AchievementID.Jack_The_Ripper, "Jack the Ripper", "Get a 10 kill streak of only executions" ),
	AchievementID.Explosion_Connoisseuir: SteamAchievement.new( AchievementID.Explosion_Connoisseuir, "Explosion Connoisseuir", "Kill 10 enemies all at once with a single explosion" ),
	AchievementID.God_of_War: SteamAchievement.new( AchievementID.God_of_War, "God of War", "End a skirmish with all allies left standing" ),
	AchievementID.Boom_Headshot: SteamAchievement.new( AchievementID.Boom_Headshot, "BOOM! Headshot", "Kill an enemy from 1500 tiles away with a headshot" ),
	AchievementID.Respectful: SteamAchievement.new( AchievementID.Respectful, "Respectful", "Let an enemy captain go after a skirmish" ),
	AchievementID.A_Legend_is_Born: SteamAchievement.new( AchievementID.A_Legend_is_Born, "A Legend is Born", "Complete the Golden Gate Massacre mission on hard or higher difficulty" ),
	AchievementID.Building_the_Legend: SteamAchievement.new( AchievementID.Building_the_Legend, "Building the Legend", "Earn 200 renown" ),
	AchievementID.Same_Shit_Different_Day: SteamAchievement.new( AchievementID.Same_Shit_Different_Day, "Same Shit, Different Day", "Complete a contract" ),
	AchievementID.Shut_The_FUCK_Up: SteamAchievement.new( AchievementID.Shut_The_FUCK_Up, "Shut The FUCK UP!", "Kill a negotiator" ),
	AchievementID.This_Is_SPARTA: SteamAchievement.new( AchievementID.This_Is_SPARTA, "This is SPARTA!", "Kill a negotiator by kicking them off a cliff" ),
	AchievementID.Unearthed_Arcana: SteamAchievement.new( AchievementID.Unearthed_Arcana, "Unearthed Arcana", "Collect all Aliáthol artifacts" ),
	AchievementID.Awaken_The_Ancients: SteamAchievement.new( AchievementID.Awaken_The_Ancients, "Awaken The Ancients", "Wake Graak Up" ),
	AchievementID.Its_High_Noon: SteamAchievement.new( AchievementID.Its_High_Noon, "It's High Noon", "Get at least 6 kills with a fan the hammer in one go" ),
	AchievementID.Death_From_Above: SteamAchievement.new( AchievementID.Death_From_Above, "Death From Above", "End a skirmish without entering melee combat" ),
	AchievementID.Kombatant: SteamAchievement.new( AchievementID.Kombatant, "Kombatant", "Execute an enemy" ),
	AchievementID.Laughing_In_Deaths_Face: SteamAchievement.new( AchievementID.Laughing_In_Deaths_Face, "Laughing in Death's Face", "Defeat Death" ),
	AchievementID.Right_Back_at_U: SteamAchievement.new( AchievementID.Right_Back_at_U, "Right Back At U", "Parry a bullet" ),
	AchievementID.Bitch_Slap: SteamAchievement.new( AchievementID.Bitch_Slap, "Bitch Slap", "Kill a Boss using only your fists" ),
	AchievementID.Chefs_Special: SteamAchievement.new( AchievementID.Chefs_Special, "Chef's Special", "Slice a bullet in half without bullet time" ),
	AchievementID.Knuckle_Sandwich: SteamAchievement.new( AchievementID.Knuckle_Sandwich, "Knuckle Sandwich", "Get a 3 parry streak" ),
	AchievementID.Brother_In_Arms: SteamAchievement.new( AchievementID.Brother_In_Arms, "Brother In Arms", "Hire a Mercenary" ),
	AchievementID.BRU: SteamAchievement.new( AchievementID.BRU, "BRU", "Commit 1,000 warcrimes" ),
	AchievementID.GIT_PWNED: SteamAchievement.new( AchievementID.GIT_PWNED, "GIT PWNED", "Defeat Gurso, The Hellhammer" ),
	AchievementID.Silent_Death: SteamAchievement.new( AchievementID.Silent_Death, "Silent Death", "Finish a level killing all combatants and without being seen" ),
	AchievementID.Well_Done_Weeb: SteamAchievement.new( AchievementID.Well_Done_Weeb, "Well Done, WEEB", "Complete 5 Contracts using only the Katana" ),
	AchievementID.Its_Treason_Then: SteamAchievement.new( AchievementID.Its_Treason_Then, "It's Treason Then?", "Convict an Ally of treason" ),
	AchievementID.Heartless: SteamAchievement.new( AchievementID.Heartless, "Heartless", "Earn the Cruel Renown Trait" ),
	AchievementID.Bushido: SteamAchievement.new( AchievementID.Bushido, "Bushido", "Earn the Honorous Renown Trait" ),
	AchievementID.Maximus_The_Merciful: SteamAchievement.new( AchievementID.Maximus_The_Merciful, "Maximus The Merciful", "Earn the Merciful Renown Trait" ),
	AchievementID.Cheer_Up_Love_The_Cavalrys_Here: SteamAchievement.new( AchievementID.Cheer_Up_Love_The_Cavalrys_Here, "Cheer Up Love, The Cavalry's Here!", "Earn the Reliable Renown Trait" ),
	AchievementID.Worse_Than_Death: SteamAchievement.new( AchievementID.Worse_Than_Death, "Worse Than Death", "Earn the War Criminal Renown Trait" ),
	AchievementID.Looks_Like_Meats_Back_On_Our_Menu_Boys: SteamAchievement.new( AchievementID.Looks_Like_Meats_Back_On_Our_Menu_Boys, "Looks Like Meat's Back On Our Menu Boys!", "Gib a combatant" ),
	AchievementID.GYAT: SteamAchievement.new( AchievementID.GYAT, "GYAT", "Gib a combatant whilst dashing backwards" ),
	AchievementID.To_The_Slaughter: SteamAchievement.new( AchievementID.To_The_Slaughter, "To The Slaughter", "Kill everyone in a skirmish, including allies" ),
	AchievementID.One_Man_Army: SteamAchievement.new( AchievementID.One_Man_Army, "One Man Army", "End a skirmish with only yourself in the warband" ),
	AchievementID.You_Call_That_A_KNIFE: SteamAchievement.new( AchievementID.You_Call_That_A_KNIFE, "You Call That A KNIFE?", "Kill a sword wielding enemy with a bladed weapon" ),
	AchievementID.AMERICA_FUCK_YEA: SteamAchievement.new( AchievementID.AMERICA_FUCK_YEA, "AMERICA, FUCK YEAH!", "Hold down the trigger of a fully automatic weapon for at least 10 consecutive seconds" ),
	AchievementID.Sussy: SteamAchievement.new( AchievementID.Sussy, "Sussy...", "Complete a Sabotague mission being seen only once" ),
	AchievementID.Live_To_Fight_Another_Day: SteamAchievement.new( AchievementID.Live_To_Fight_Another_Day, "Live to Fight Another Day", "Rescue an ally whilst under suppressive fire from the enemy" ),
	AchievementID.Remember_Us: SteamAchievement.new( AchievementID.Remember_Us, "Remember Us", "Complete a Hold The Line Mission" ),
	AchievementID.Zandutsu_That_Shit: SteamAchievement.new( AchievementID.Zandutsu_That_Shit, "Zandutsu That Shit", "Complete an Assassination Contract with no collateral" ),
	AchievementID.MORE: SteamAchievement.new( AchievementID.MORE, "MOAAR!", "Acquire the Fusion Cannon" ),
	AchievementID.Edgelord: SteamAchievement.new( AchievementID.Edgelord, "Edgelord", "Equip an all black outfit with the dual katana swords" ),
	AchievementID.That_Actually_WORKED: SteamAchievement.new( AchievementID.That_Actually_WORKED, "That Actually... WORKED?", "Complete a successful Rebellion" ),
	AchievementID.NANOMACHINES_SON: SteamAchievement.new( AchievementID.NANOMACHINES_SON, "NANOMACHINES, SON!", "Equip a biomechanical limb or get a gene splice" ),
	AchievementID.Cool_Guys_Dont_Look_At_Explosions: SteamAchievement.new( AchievementID.Cool_Guys_Dont_Look_At_Explosions, "Cool Guys Don't Look At Explosions", "Complete a Contract with only explosives" ),
	AchievementID.Double_Take: SteamAchievement.new( AchievementID.Double_Take, "Double Take", "Dual Wield any weapon" ),
	AchievementID.Triple_Threat: SteamAchievement.new( AchievementID.Triple_Threat, "Triple Threat", "Get A Triple Wallbang kill" ),
	AchievementID.DAYUUM_I_Aint_Gonna_Sugarcoat_It: SteamAchievement.new( AchievementID.DAYUUM_I_Aint_Gonna_Sugarcoat_It, "Dayuum! I ain't gonna sugarcoat it", "Counter parry a sword wielder" ),
	AchievementID.Back_From_The_Brink: SteamAchievement.new( AchievementID.Back_From_The_Brink, "Back From the Brink", "Complete a Hellbreak" ),
	AchievementID.Dance_DANCE_DANCE: SteamAchievement.new( AchievementID.Dance_DANCE_DANCE, "Dance, Dance! DANCE!", "Trigger a fear response on a burning enemy" ),
	AchievementID.Bang_Bang_I_Shot_Em_Down: SteamAchievement.new( AchievementID.Bang_Bang_I_Shot_Em_Down, "Bang, Bang, I Shot 'em Down", "Use an explosive barrel to kill at least 15 combatants" ),
	AchievementID.BOP_IT: SteamAchievement.new( AchievementID.BOP_IT, "BOP IT!", "Achievement a 10 kill streak of melee only" ),
	AchievementID.Just_A_Leap_of_Faith: SteamAchievement.new( AchievementID.Just_A_Leap_of_Faith, "Just A Leap of Faith", "Jump off Eagle's Peak" ),
	AchievementID.Send_Them_To_Jesus: SteamAchievement.new( AchievementID.Send_Them_To_Jesus, "Send Them To Jesus", "Get a double execution" ),
	AchievementID.Rizzlord: SteamAchievement.new( AchievementID.Rizzlord, "Rizzlord", "Convince a warband leader to surrender" ),
	AchievementID.AHHH_GAHHH_HAAAAAAA: SteamAchievement.new( AchievementID.AHHH_GAHHH_HAAAAAAA, "Ahhh, Gahhh, HAAAAAAA!", "Blow yourself up" ),
	AchievementID.Absolutely_Necessary_Precautions: SteamAchievement.new( AchievementID.Absolutely_Necessary_Precautions, "Absolutely Necessary Precautions", "Get the Maximum Collateral on a mission with explosives" ),
	AchievementID.Stop_Hitting_Yourself: SteamAchievement.new( AchievementID.Stop_Hitting_Yourself, "Stop hitting yourself", "Rip off a combatant's limb and kill them with it" ),
	AchievementID.Fuck_This_Shit_Im_Out: SteamAchievement.new( AchievementID.Fuck_This_Shit_Im_Out, "Fuck This Shit, IM OUT!", "Leave an ongoing battle that you started" ),
	AchievementID.Smoke_Break: SteamAchievement.new( AchievementID.Smoke_Break, "Smoke Break", "Trigger the idle animation" )
};

func _on_achievement_stored() -> void:
	pass

func _on_current_stats_recieved( gameId: int, result: int, userId: int ) -> void:
	print( "Got local player statistics & achievments" )
	
	if userId != SteamManager._steam_id:
		print( "Not this user, aborting." )
		return
	
	if gameId != SteamManager._steam_app_id:
		print( "Not this game, aborting." )
		return
	
	if result != Steam.RESULT_OK:
		print( "Steam couldn't fetch stats: %s" % result )
	
	print( "Fetched user statistics." )
	
	for achievement in ACHIEVEMENT_TABLE.values():
		var steamStatus := Steam.getAchievement( achievement._id_string )
		
		if !steamStatus[ "ret" ]:
			continue
		
		ACHIEVEMENT_TABLE[ achievement._id ]._achieved = steamStatus[ "achieved" ]

# Called when the node enters the scene tree for the first time.
func init() -> void:
	Steam.user_achievement_stored.connect( _on_achievement_stored )
	Steam.current_stats_received.connect( _on_current_stats_recieved )
	Steam.requestUserStats( SteamManager._steam_id )

func activate_achievement( id: AchievementID ) -> void:
	if !ACHIEVEMENT_TABLE.has( id ):
		push_error( "[STEAM] Achievement %s doesn't exist!"  % var_to_str( id ) )
		return
	
	var achievement := Steam.getAchievement( ACHIEVEMENT_TABLE[ id ]._id_string )
	if achievement[ "achieved" ]:
		print( "Achievement %s already completed." % ACHIEVEMENT_TABLE[ id ]._id_string )
		return
	
	Steam.setAchievement( ACHIEVEMENT_TABLE[ id ]._id_string )
	Steam.storeStats()
