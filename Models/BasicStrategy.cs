namespace WebApplication1.Models{
    public class HardSumStrategy {
        public int PlayerHandValue { get; set; }
        public string DealerVisibleCard { get; set; }
        public string Action { get; set; }

        // Constructor to ensure PlayerHandValue and DealerVisibleCard are provided
        public HardSumStrategy(int playerHandValue, string dealerVisibleCard, string action) {
            PlayerHandValue = playerHandValue;
            DealerVisibleCard = dealerVisibleCard;
            Action = action;
        }
    }

    public class SoftSumStrategy {
        public int PlayerCardNextToAce { get; set; }
        public string DealerVisibleCard { get; set; }
        public string Action { get; set; }

        // Constructor to ensure PlayerCardNextToAce and DealerVisibleCard are provided
        public SoftSumStrategy(int playerCardNextToAce, string dealerVisibleCard, string action) {
            PlayerCardNextToAce = playerCardNextToAce;
            DealerVisibleCard = dealerVisibleCard;
            Action = action;
        }
    }

    public class PairStrategy {
        public int PlayerPairCard { get; set; }
        public string DealerVisibleCard { get; set; }
        public string Action { get; set; }

        // Constructor to ensure PlayerPairCard and DealerVisibleCard are provided
        public PairStrategy(int playerPairCard, string dealerVisibleCard, string action) {
            PlayerPairCard = playerPairCard;
            DealerVisibleCard = dealerVisibleCard;
            Action = action;
        }
    }

    public class BasicStrategy {
        public List<HardSumStrategy> HardSum { get; set; } = new List<HardSumStrategy>();
        public List<SoftSumStrategy> SoftSum { get; set; } = new List<SoftSumStrategy>();
        public List<PairStrategy> Pair { get; set; } = new List<PairStrategy>();
    }
}
