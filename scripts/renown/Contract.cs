using Godot;
using DialogueManagerRuntime;

namespace Renown {
	public enum ContractType {
		Kidnapping,
		Assassination,
		Extortion,
		Sabotauge,

		Count
	};
	public partial class Contract : Resource {
		[Export]
		private ContractType Type;
		[Export]
		private uint DueYear = 0;
		[Export]
		private uint DueMonth = 0;
		[Export]
		private uint DueDay = 0;

		private World.WorldTimestamp DueDate;

		public ContractType GetContractType() {
			return Type;
		}
    };
};