using Godot;

namespace Renown.World {
	public partial class Settlement : Node2D {
		[Export]
		private StringName AreaTitle;
		[Export]
		private TradeRoute[] TradeRoutes;
		[Export]
		private ResourceProducer[] Producers;
		
	};
};