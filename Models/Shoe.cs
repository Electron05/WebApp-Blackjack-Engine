namespace WebApplication1.Models{
    public class Shoe
    {


        public List<Card> cards { get; set; }
        public int numberOfDecks { get; set; }

        public int runningCount { get; set; }

        public Random rng;

        public Shoe(int numberOfDecks)
        {
            cards = new List<Card>();
            rng = new Random();

            this.numberOfDecks = numberOfDecks;
        }

        public void FillShoe(){
            InitializeShoe();
            Shuffle();
        }

        private void InitializeShoe()
        {
            cards = new List<Card>();

            runningCount = 0;

            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            for (int deck = 0; deck < numberOfDecks; deck++)
            {
                foreach (var suit in suits)
                {
                    foreach (var value in values)
                    {
                        cards.Add(new Card { Suit = suit, Value = value });
                    }
                }
            }
        }

        public void Shuffle()
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public Card DealCard()
        {
            if (cards.Count > 0)
            {
                Card cardToDeal = cards[0];

                UpdateRunningCount(cardToDeal);

                cards.RemoveAt(0);
                return cardToDeal;
            }
            else
            {
                // Optionally handle the case where the shoe is empty
                throw new InvalidOperationException("The shoe is empty.");
            }
        }

        private void UpdateRunningCount(Card cardDealt){
            if (cardDealt.Value == "2" || cardDealt.Value == "3" || cardDealt.Value == "4" || cardDealt.Value == "5" || cardDealt.Value == "6")
            {
                runningCount++;
            }
            else if (cardDealt.Value == "10" || cardDealt.Value == "J" || cardDealt.Value == "Q" || cardDealt.Value == "K" || cardDealt.Value == "A")
            {
                runningCount--;
            }
        }

        public int cardsRemaining()
        {
            return cards.Count;
        }
    }
}