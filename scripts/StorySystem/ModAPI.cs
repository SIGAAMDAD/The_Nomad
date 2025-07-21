using System.Collections.Generic;
using StorySystem;

public static class ModAPI {
	private static Dictionary<string, StoryModule> Modules = new Dictionary<string, StoryModule>();

	public static void RegisterModule( string modId, StoryModule module ) {
		if ( !Modules.ContainsKey( modId ) ) {
			Modules.Add( modId, module );
			module.OnRegistered();
		}
	}
};