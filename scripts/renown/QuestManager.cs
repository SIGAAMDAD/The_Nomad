using Godot;
using System.Collections.Generic;

namespace Renown {
	public partial class QuestManager : Node {
		private static Dictionary<string, Quest> ActiveQuests;
		private static Dictionary<string, Quest> FailedQuests;

		public static void Init() {
			ActiveQuests = new Dictionary<string, Quest>();
			FailedQuests = new Dictionary<string, Quest>();
		}
	};
};