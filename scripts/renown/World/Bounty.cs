using Godot;
using System.Collections.Generic;

namespace Renown.World {
	public partial class Bounty : Contract {
		public static HashSet<Bounty> BountyCache = null;
		
		private CharacterBody2D Target = null;
		private float Amount = 0.0f;
		
		[Signal]
		public delegate void CompletedEventHandler( Bounty bounty, CharacterBody2D completer );
		
		public void Save() {
		}
		public void Load() {
		}
		
		public float GetAmount() => Amount;
		public void Increase( float nAmount ) => Amount += nAmount;
		public void SetCompleted( CharacterBody2D completer ) => EmitSignal( "Completed", completer );
		
		public static void InitCache() {
			BountyCache = new HashSet<Bounty>();
			
			if ( ArchiveSystem.Instance.IsLoaded() ) {
				SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Bounties" );
				
				// save file compatibility
				if ( reader == null ) {
					return;
				}
				
				int bountyCount = reader.LoadInt( "count" );
			}
		}
		public static Bounty Create( Faction creator, CharacterBody2D target, float nAmount ) {
			Bounty contract = new Bounty();
			
			contract.Amount = nAmount;
			contract.Target = target;
			contract.Completed += creator.OnBountyCompleted;
			contract.AddToGroup( "Archive" );
			
			BountyCache.Add( contract );
			
			contract.GetTree().CurrentScene.GetNode( "Bounties" ).AddChild( contract );
			
			return contract;
		}
		public static void Remove( Bounty bounty ) {
			BountyCache.Remove( bounty );
		}
		
		public override void _Ready() {
			base._Ready();
			
			SetProcess( false );
			SetProcessInternal( false );
		}
	};
};
