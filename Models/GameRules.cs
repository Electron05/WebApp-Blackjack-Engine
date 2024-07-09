namespace WebApplication1.Models{
	public class GameRules {
		//Rules of the game, like can the player double after split, can the player surrender, does dealer hit on soft 17, etc.
		public bool canDoubleAfterSplit { get; set; }
		public bool canSurrender { get; set; }
		public bool doesDealerHitOnSoft17 { get; set; }
		public int numberOfAllowedHands { get; set; }
		public int numberOfDecksInShoe { get; set; }
		public int deckPenetration { get; set; } // A number of decks that are dealt before the shoe is reshuffled
		public float blackjackPayout { get; set; } // Usually 1.5, but sometimes 1.2


		public GameRules(bool canDoubleAfterSplit, bool canSurrender, bool doesDealerHitOnSoft17, int numberOfAllowedHands, int numberOfDecksInShoe, int deckPenetration, float blackjackPayout) {
			this.canDoubleAfterSplit = canDoubleAfterSplit;
			this.canSurrender = canSurrender;
			this.doesDealerHitOnSoft17 = doesDealerHitOnSoft17;
			this.numberOfAllowedHands = numberOfAllowedHands;
			this.numberOfDecksInShoe = numberOfDecksInShoe;
			this.deckPenetration = deckPenetration;
			this.blackjackPayout = blackjackPayout;
		}
		

	}
}