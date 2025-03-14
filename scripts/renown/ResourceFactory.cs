using Godot;
using GDExtension.Wrappers;

/*
namespace Renown {
	public partial class ResourceFactory : Node2D {
		[Export]
		public Settlement Location = null;
		[Export]
		public CraftStation CraftMethod = null;
		[Export]
		public uint Reserve = 0;
		
		public enum Type : uint {
			Bullets,
			Guns,
			Ale,
			Rations,
			Tobacco,
			Cocaine,
			
			Count
		};
		
		private Type ResourceType;
		private uint Stock = 0;
		
		// we might have multiple different resource requirements
		private Dictionary<ResourceProducer.Type, uint> ResourceCount = new Dictionary<ResourceProducer.Type, uint>();
		private bool HaveResources = false;
		
		private bool IsValidResource( ResourceProducer.Type Type ) {
			switch ( ResourceType ) {
			case Type.Bullets:
				return Type == ResourceProducer.Gunpowder || Type == ResourceProducer.Metal;
			default:
				break;
			};
			return false;
		}
		public void RecieveResource( ResourceProducer.Type Type, uint nAmount ) {
			if ( !IsValidResource( Type ) ) {
				return;
			}
			
			ResourceCount[ Type ] += nAmount;
			HaveResources = true;
		}
		
		public override void _Ready() {
		}
		public override void _Process( double delta ) {
			if ( !HaveResources ) {
				return;
			}
			
			
		}
		
		public uint CalcProductPrice() {
			uint price = 0;
			
			
		}
		public void BuyProduct( uint nAmount ) {
			Stock -= nAmount;
			
		}
	};
};
*/