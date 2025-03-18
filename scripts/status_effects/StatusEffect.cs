using Godot;

public enum StatusEffectType : uint {
	Burning,
	Poisoned,
	Intoxicated,
	High,

	Count
};

public partial class StatusEffect : Node2D {
	public virtual StatusEffectType GetEffectType() {
		return StatusEffectType.Count;
	}
};