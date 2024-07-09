namespace WebApplication1.Models{
	public class BettingStrategy{
        public BettingStrategy(int baseBankroll, int basicBet, int overT1Bet, int overT2Bet, int overT3Bet, int overT4Bet, int overT5Bet, int overT6Bet)
        {
            this.baseBankroll = baseBankroll;
            this.basicBet = basicBet;
            this.overT1Bet = overT1Bet;
            this.overT2Bet = overT2Bet;
            this.overT3Bet = overT3Bet;
            this.overT4Bet = overT4Bet;
            this.overT5Bet = overT5Bet;
            this.overT6Bet = overT6Bet;
        }

        public int baseBankroll { get; set; }
		public int basicBet { get; set; }
		public int overT1Bet { get; set; }
		public int overT2Bet { get; set; }
		public int overT3Bet { get; set; }
		public int overT4Bet { get; set; }
		public int overT5Bet { get; set; }
		public int overT6Bet { get; set; }



	}
}