using Newtonsoft.Json;

namespace WebApplication1.Models {
    public class Game
    {
        public const bool HARDCODED_MAUNUAL_SPLIT = false;
        public const bool HARDCODED_MANUAL_RESPLIT = false;
        public const bool HARDCODED_SIM_SPLIT = false;
        public const bool HARDCODED_SIM_RESPLIT = false;

        public const bool USE_DICTIONARY = true;

        public GameRules gameRules { get; set; }

        public BettingStrategy bettingStrategy { get; set; }

        public Shoe shoe { get; set; }
        public BasicStrategy strategy { get; set; }

        public Dictionary<string, string> strategyDictionary { get; set; }

        public int currentGameHandCount { get; set; }

        public struct SimulationResults {
            public int normalWins;

            public bool anomaly;

            public int doubleWins;

            public int normalLoses;
            public int doubleLoses;

            public int surrenders;

            public int pushes;
            public int blackJacks;

            public int handsPlayed;

            public float riskOfRuinInPercent;
            public float expectedValuePerGame;
            public float averageFinalBankroll;

        }

        public struct HandEvaluation
        {
            public int Value;
            public bool IsSoft;
            public bool IsPair;
        }

        public struct SingleHand
        {
            public List<Card> Cards;
            public int Bet;
            public bool Busted;
        }

        public Game()
        {
            gameRules = new GameRules(
                canDoubleAfterSplit: true,
                canSurrender: true,
                doesDealerHitOnSoft17: true,
                numberOfAllowedHands: 2,
                numberOfDecksInShoe: 8,
                deckPenetration: 6,
                blackjackPayout: 1.5f
            );

            bettingStrategy = new BettingStrategy(
                baseBankroll: 1000000,
                basicBet: 100,
                overT1Bet: 100,
                overT2Bet: 100,
                overT3Bet: 100,
                overT4Bet: 100,
                overT5Bet: 100,
                overT6Bet: 100
            );


            var strategyJson = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "RunTimeResources", "BasicStrategyH17.json"));
            strategy = JsonConvert.DeserializeObject<BasicStrategy>(strategyJson) ?? throw new Exception("Invalid strategy JSON");

            //hash table version
            strategyDictionary = new Dictionary<string, string>();

            foreach (var item in strategy.HardSum)
            {
                string key = $"HardSum_{item.PlayerHandValue}_{item.DealerVisibleCard}";
                strategyDictionary[key] = item.Action;
            }

            foreach (var item in strategy.SoftSum)
            {
                string key = $"SoftSum_{item.PlayerCardNextToAce}_{item.DealerVisibleCard}";
                strategyDictionary[key] = item.Action;
            }

            foreach (var item in strategy.Pair)
            {
                string key = $"Pair_{item.PlayerPairCard}_{item.DealerVisibleCard}";
                strategyDictionary[key] = item.Action;
            }


            // Initialize shoe only if it doesn't exist
            shoe = new Shoe(gameRules.numberOfDecksInShoe);
            shoe.FillShoe();
        }

        public void SetNewGameRules(GameRules newGameRules)
        {
            gameRules = newGameRules;
            shoe = new Shoe(gameRules.numberOfDecksInShoe);
            shoe.FillShoe();
        }

        public void SetNewBettingStrategy(BettingStrategy newBettingStrategy)
        {
            bettingStrategy = newBettingStrategy;
        }

        public void Reshuffle(){
            shoe = new Shoe(gameRules.numberOfDecksInShoe);
            shoe.FillShoe();
        }

        private HandEvaluation EvaluateHand(List<Card> cards)
        {
            int handValue = cards.Sum(card => CardIntValue(card));
            int numberOfAces = cards.Count(card => card.Value == "A");

            while (handValue > 21 && numberOfAces > 0)
            {
                handValue -= 10;
                numberOfAces--;
            }

            return new HandEvaluation
            {
                Value = handValue,
                IsSoft = numberOfAces > 0,
                IsPair = cards.Count == 2 && cards[0].Value == cards[1].Value
            };
        }

        private int CardIntValue(Card card)
        {
            switch (card.Value)
            {
                case "A":
                    return 11;
                case "J":
                case "Q":
                case "K":
                    return 10;
                default:
                    return int.Parse(card.Value);
            }
        }

        private bool HasBlackjack(List<Card> cards){
            return cards.Count == 2 && cards.Sum(CardIntValue) == 21;
        }


        private string DetermineAction(List<Card> playerCards, Card dealerCard)
        {
            string dealerCardValue = dealerCard.Value = CardIntValue(dealerCard).ToString();
            HandEvaluation evaluatedHand = EvaluateHand(playerCards);

            if (evaluatedHand.IsPair)
            {
                if(HandlePairStrategy(playerCards, dealerCardValue, evaluatedHand) == "Split")
                {
                    return "Split";
                }
            }

            if (evaluatedHand.IsSoft)
            {
                return HandleSoftStrategy(playerCards, dealerCardValue, evaluatedHand);
            }

            return HandleHardStrategy(playerCards, dealerCardValue, evaluatedHand);
        }

        private string HandlePairStrategy(List<Card> playerCards, string dealerCardValue, HandEvaluation evaluatedHand)
        {
            string pairStrategy;
            if(USE_DICTIONARY){
                pairStrategy = strategyDictionary["Pair_"+CardIntValue(playerCards[0]).ToString() + "_" + dealerCardValue]
                ?? throw new Exception($"No strategy found for pair hand with PlayerPairCard: {evaluatedHand.Value / 2} and DealerVisibleCard: {dealerCardValue}");
            }
            else{
#pragma warning disable CS0162 // Unreachable code detected
                pairStrategy = strategy.Pair.FirstOrDefault(pair => pair.PlayerPairCard == evaluatedHand.Value / 2 && pair.DealerVisibleCard == dealerCardValue).Action
                ?? throw new Exception($"No strategy found for pair hand with PlayerPairCard: {evaluatedHand.Value / 2} and DealerVisibleCard: {dealerCardValue}");
#pragma warning restore CS0162 // Unreachable code detected
            }
            if(pairStrategy=="NoSplit"){
                return "NoSplit";
            }
            if(currentGameHandCount + 1 > gameRules.numberOfAllowedHands){
                return "NoSplit";
            }
            if(pairStrategy=="SplitWithDAS" && !gameRules.canDoubleAfterSplit){
                return "NoSplit";
            }
            return "Split";
        }

        private string HandleSoftStrategy(List<Card> playerCards, string dealerCardValue, HandEvaluation evaluatedHand)
        {
            int nextToAceValue = evaluatedHand.Value - 11;

            string softStrategy;
            if(USE_DICTIONARY){
                softStrategy = strategyDictionary["SoftSum_"+nextToAceValue+"_"+dealerCardValue]
                ?? throw new Exception($"No strategy found for soft hand {nextToAceValue} and DealerVisibleCard: {dealerCardValue}");
            }
            else{
#pragma warning disable CS0162 // Unreachable code detected
                softStrategy = strategy.SoftSum.FirstOrDefault(soft => soft.PlayerCardNextToAce == nextToAceValue && soft.DealerVisibleCard == dealerCardValue).Action
                ?? throw new Exception($"No strategy found for soft hand {nextToAceValue} and DealerVisibleCard: {dealerCardValue}");
#pragma warning restore CS0162 // Unreachable code detected
            }
            return softStrategy switch
            {
                "Double" => CanDouble(playerCards) ? "Double" : "Hit",
                "DoubleStand" => CanDouble(playerCards) ? "Double" : "Stand",
                _ => softStrategy
            };
        }

        private string HandleHardStrategy(List<Card> playerCards, string dealerCardValue, HandEvaluation evaluatedHand)
        {
            string hardStrategy;
            if(USE_DICTIONARY){
                hardStrategy = strategyDictionary["HardSum_"+evaluatedHand.Value+"_"+dealerCardValue]
                ?? throw new Exception($"No strategy found for hard hand {evaluatedHand.Value} and DealerVisibleCard: {dealerCardValue}");
            }
            else{
#pragma warning disable CS0162 // Unreachable code detected
                hardStrategy = strategy.HardSum.FirstOrDefault(hard => hard.PlayerHandValue == evaluatedHand.Value && hard.DealerVisibleCard == dealerCardValue).Action
                ?? throw new Exception($"No strategy found for hard hand {evaluatedHand.Value} and DealerVisibleCard: {dealerCardValue}");
#pragma warning restore CS0162 // Unreachable code detected
            }
            return hardStrategy switch
            {
                "Surrender" => gameRules.canSurrender && playerCards.Count == 2 ? "Surrender" : "Hit",
                "Double" => CanDouble(playerCards) ? "Double" : "Hit",
                _ => hardStrategy
            };
        }

        private bool CanDouble(List<Card> playerCards) {
            return playerCards.Count == 2 && (currentGameHandCount == 1 || gameRules.canDoubleAfterSplit);
        }


        public string playGameAndDisplayCardsLeft(ref int bankRoll){
            string log = "";
            log +=PlayGame(ref bankRoll);


            log += CheckShoeState();

            return log;
        }

        private int DetermineBet(){
            float trueCount = shoe.runningCount / (shoe.cardsRemaining() / 52f);
            if (trueCount >= 6)
            {
                return bettingStrategy.overT6Bet;
            }
            else if (trueCount >= 5)
            {
                return bettingStrategy.overT5Bet;
            }
            else if (trueCount >= 4)
            {
                return bettingStrategy.overT4Bet;
            }
            else if (trueCount >= 3)
            {
                return bettingStrategy.overT3Bet;
            }
            else if (trueCount >= 2)
            {
                return bettingStrategy.overT2Bet;
            }
            else if (trueCount >= 1)
            {
                return bettingStrategy.overT1Bet;
            }
            else
            {
                return bettingStrategy.basicBet;
            }
        }

        #region Manual
        private string PlayGame(ref int bankRoll)
        {
            string log = "";
            List<Card> dealerHand = new() { shoe.DealCard(), shoe.DealCard() };

            Card customSplit = shoe.DealCard();
            List<SingleHand> playerHands = new() // list of single hands is implemented to handle splits
            {
                new SingleHand
                {
                    Bet = DetermineBet(),

                    Cards = new List<Card> { HARDCODED_MAUNUAL_SPLIT ? customSplit : shoe.DealCard(),  HARDCODED_MAUNUAL_SPLIT ? customSplit : shoe.DealCard() }
                }
            };
            currentGameHandCount = 1;

            log+= $"Running count: {shoe.runningCount}\n";
            log+=$"Initial Bankroll: {bankRoll}\n";
            log+= $"Bet: {playerHands[0].Bet}\n";

            if (HandleInitialBlackjack(playerHands[0], dealerHand, ref bankRoll, ref log))
            {
                return log;
            }

            log += $"Player hand: {playerHands[0].Cards[0].Value} {playerHands[0].Cards[1].Value}\n";
            log += $"Dealer hand: {dealerHand[0].Value} ?\n";

            for (int i = 0; i < playerHands.Count; i++)
            {
                log += $"Hand {i + 1}:\n";
                playerHands[i] = PlaySingleHand(playerHands[i], dealerHand, ref log, ref playerHands);
            }

            if (playerHands.All(hand => hand.Busted))
            {
                log += "All player hands busted, dealer wins";
                foreach (var hand in playerHands)
                {
                    bankRoll -= hand.Bet;
                }
                return log;
            }


            PlayDealerHand(dealerHand, ref log);

            foreach (var hand in playerHands)
            {
                log += $"Hand {playerHands.IndexOf(hand) + 1}: ";
                if (hand.Busted)
                {
                    log += "Player busted\n";
                    bankRoll -= hand.Bet;
                }
                else if (EvaluateHand(dealerHand).Value > 21 || EvaluateHand(hand.Cards).Value > EvaluateHand(dealerHand).Value)
                {
                    log += "Player wins\n";
                    bankRoll += hand.Bet;
                }
                else if (EvaluateHand(hand.Cards).Value < EvaluateHand(dealerHand).Value)
                {
                    log += "Dealer wins\n";
                    bankRoll -= hand.Bet;
                }
                else
                {
                    log += "Push\n";
                }
            }

            return log;
        }
    
        private bool HandleInitialBlackjack(SingleHand playerHand, List<Card> dealerHand, ref int bankRoll, ref string log)
        {
            if (HasBlackjack(playerHand.Cards))
            {
                if (HasBlackjack(dealerHand))
                {
                    log += "Double blackjack, push";
                    return true;
                }
                else
                {
                    log += "Player blackjack, player wins " + gameRules.blackjackPayout;
                    bankRoll += (int)(playerHand.Bet * gameRules.blackjackPayout);
                    return true;
                }
            }

            if (HasBlackjack(dealerHand))
            {
                log += "Dealer blackjack, dealer wins " + gameRules.blackjackPayout;
                bankRoll -= playerHand.Bet;
                return true;
            }

            return false;
        }

        private SingleHand PlaySingleHand(SingleHand hand, List<Card> dealerHand, ref string log, ref List<SingleHand> playerHands)
        {
            bool handCompleted = false;
            int iterations = 0;

            while (true)
            {
                iterations++;
                if (EvaluateHand(hand.Cards).Value > 21)
                {
                    hand.Busted = true;
                    break;
                }
				else if (handCompleted || iterations > 10)
				{
					break;
				}

                string action = DetermineAction(hand.Cards, dealerHand[0]);
                log += action;

                switch (action)
                {
                    case "Stand":
                        log += "\n";
                        handCompleted = true;
                        break;
                    case "Hit":
                        hand.Cards.Add(shoe.DealCard());
                        log += $" {hand.Cards[^1].Value}\n";
                        break;
                    case "Double":
                        hand.Cards.Add(shoe.DealCard());
                        hand.Bet *= 2;
                        handCompleted = true;
                        log += $" {hand.Cards[^1].Value}\n";
                        break;
                    case "Split":
                        log += "\n";
                        SplitHand(ref hand, ref playerHands,ref log);
                        break;
                    case "Surrender":
                        log += "\n";
                        SurrenderHand(ref hand);
                        handCompleted = true;
                        break;
                }
            }

            return hand;
        }

        private void SplitHand(ref SingleHand hand, ref List<SingleHand> playerHands, ref string log)
        {
            currentGameHandCount++;


            Card splitCard1;
            if(currentGameHandCount == 1 && HARDCODED_MANUAL_RESPLIT){
                splitCard1 = hand.Cards[0];
            }
            else{
                splitCard1 = shoe.DealCard();
            }

            var splitHand = new SingleHand { Bet = hand.Bet, Cards = new List<Card> { hand.Cards[0], splitCard1 } };
            var newHand = new SingleHand { Bet = hand.Bet, Cards = new List<Card> { hand.Cards[1], shoe.DealCard() } };
            log += "into " + splitHand.Cards[0].Value + " " + splitHand.Cards[1].Value + " and " + newHand.Cards[0].Value + " " + newHand.Cards[1].Value + "\n";
            hand = splitHand;
            playerHands.Add(newHand);
        }

        private void PlayDealerHand(List<Card> dealerHand, ref string log)
        {
            while (EvaluateHand(dealerHand).Value < 17 ||
                  (EvaluateHand(dealerHand).Value == 17 && EvaluateHand(dealerHand).IsSoft && gameRules.doesDealerHitOnSoft17))
            {
                dealerHand.Add(shoe.DealCard());
            }
            log += "Dealer hand: ";
            foreach (var card in dealerHand)
            {
                log += card.Value + " ";
            }
            log += "\n";
        }
    
        #endregion

        #region Simulation
        private void PlaySimGame(ref int bankRoll, ref SimulationResults results)
        {
            List<Card> dealerHand = new() { shoe.DealCard(), shoe.DealCard() };

            results.handsPlayed++;
            currentGameHandCount = 1;
            
            Card customSplit = shoe.DealCard();
            List<SingleHand> playerHands = new() // list of single hands is implemented to handle splits
            {
                new SingleHand
                {
                    Bet = 100,

                    Cards = new List<Card> { HARDCODED_SIM_SPLIT ? customSplit : shoe.DealCard(),  HARDCODED_SIM_SPLIT ? customSplit : shoe.DealCard() }
                }
            };

            if (HandleSimBlackjack(playerHands[0], dealerHand, ref bankRoll, ref results))
            {
                return;
            }

            for (int i = 0; i < playerHands.Count; i++)
            {
                playerHands[i] = PlaySimHand(playerHands[i], dealerHand, ref playerHands, ref results);
            }

            if (playerHands.All(hand => hand.Busted))
            {
                foreach (var hand in playerHands)
                {
                    bankRoll -= hand.Bet;
                    if(hand.Bet<bettingStrategy.basicBet){
                        results.surrenders++;
                    }
                    else if(hand.Bet>bettingStrategy.basicBet){
                        results.doubleLoses++;
                    } else{
                    results.normalLoses++;
                    }
                }
                return;
            }



            PlayDealerHand(dealerHand);

            foreach (var hand in playerHands)
            {
                if (hand.Busted)
                {
                    bankRoll -= hand.Bet;
                    if(hand.Bet<bettingStrategy.basicBet){
                        results.surrenders++;
                    }
                    else if(hand.Bet>bettingStrategy.basicBet){
                        results.doubleLoses++;
                    } else{
                    results.normalLoses++;
                    }
                }
                else if (EvaluateHand(dealerHand).Value > 21 || EvaluateHand(hand.Cards).Value > EvaluateHand(dealerHand).Value)
                {
                    
                    bankRoll += hand.Bet;
                    if(hand.Bet>bettingStrategy.basicBet){
                        results.doubleWins++;
                    } else{
                    results.normalWins++;
                    }
                }
                else if (EvaluateHand(hand.Cards).Value < EvaluateHand(dealerHand).Value)
                {
                    
                    bankRoll -= hand.Bet;
                    if(hand.Bet>bettingStrategy.basicBet){
                        results.doubleLoses++;
                    } else{
                    results.normalLoses++;
                    }
                }
                else
                {
                    //Push
                    results.pushes++;
                }
            }

            return ;
        }


        private bool HandleSimBlackjack(SingleHand playerHand, List<Card> dealerHand, ref int bankRoll, ref SimulationResults results)
        {
            if (HasBlackjack(playerHand.Cards))
            {
                if (HasBlackjack(dealerHand))
                {
                    results.pushes++;
                    return true;
                }
                else
                {
                    results.blackJacks++;
                    bankRoll += (int)(playerHand.Bet*gameRules.blackjackPayout);
                    return true;
                }
            }

            if (HasBlackjack(dealerHand))
            {
                results.normalLoses++;
                bankRoll -= playerHand.Bet;
                return true;
            }

            return false;
        }
        
        private SingleHand PlaySimHand(SingleHand hand, List<Card> dealerHand, ref List<SingleHand> playerHands, ref SimulationResults results)
        {
            bool handCompleted = false;
            int iterations = 0;

            while (true)
            {
                iterations++;
                if (EvaluateHand(hand.Cards).Value > 21)
                {
                    hand.Busted = true;
                    break;
                }
				else if (handCompleted || iterations > 10)
				{
					break;
				}

                string action = DetermineAction(hand.Cards, dealerHand[0]);


                switch (action)
                {
                    case "Stand":
                        handCompleted = true;
                        break;
                    case "Hit":
                        hand.Cards.Add(shoe.DealCard());
                        break;
                    case "Double":
                        hand.Cards.Add(shoe.DealCard());
                        hand.Bet *= 2;
                        handCompleted = true;
                        break;
                    case "Split":
                        SplitHand(ref hand, ref playerHands);
                        results.handsPlayed++;
                        break;
                    case "Surrender":
                        SurrenderHand(ref hand);
                        handCompleted = true;
                        break;
                }
            }

            return hand;
        }


        private void PlayDealerHand(List<Card> dealerHand)
        {
            while (EvaluateHand(dealerHand).Value < 17 ||
                  (EvaluateHand(dealerHand).Value == 17 && EvaluateHand(dealerHand).IsSoft && gameRules.doesDealerHitOnSoft17))
            {
                dealerHand.Add(shoe.DealCard());
            }
        }

        private void SplitHand(ref SingleHand hand, ref List<SingleHand> playerHands)
        {
            currentGameHandCount++;

            Card splitCard1;
            if(currentGameHandCount == 1 && HARDCODED_SIM_RESPLIT){
                splitCard1 = hand.Cards[0];
            }
            else{
                splitCard1 = shoe.DealCard();
            }
            var splitHand = new SingleHand { Bet = hand.Bet, Cards = new List<Card> { hand.Cards[0], splitCard1 } };
            var newHand = new SingleHand { Bet = hand.Bet, Cards = new List<Card> { hand.Cards[1], shoe.DealCard() } };
            hand = splitHand;
            playerHands.Add(newHand);
        }

        #endregion
        
        private void SurrenderHand(ref SingleHand hand)
        {
            hand.Busted = true;
            hand.Bet /= 2;
        }
        
        private string CheckShoeState(){
            string log ="";
            int cardsLeft = shoe.cardsRemaining();
            log+=$"\nCards left in shoe: {cardsLeft}";

            if(cardsLeft <= (gameRules.numberOfDecksInShoe - gameRules.deckPenetration)*52){
                log+="\nShoe is running out of cards, reshuffling";
                shoe.FillShoe();
            }
            return log;
        }
    
        public SimulationResults Simulation(int amountOfSimulations, int maxGamesInSimulation){
            SimulationResults results = new SimulationResults();

            int maxFinalBankroll = 0;
            int ruinedGames = 0;
            int totalDelta = 0;
            int sumOfFinalBankrolls = 0;

            for(int i = 0; i < amountOfSimulations; i++){
                int gamesPlayed = 0;
                int bankroll = bettingStrategy.baseBankroll;
                Reshuffle();
                while(gamesPlayed < maxGamesInSimulation && bankroll > 0){
                    PlaySimGame(ref bankroll, ref results);
                    CheckShoeState();
                    gamesPlayed++;
                }

                if(bankroll <= 0){
                    ruinedGames++;
                }
                if(bankroll > maxFinalBankroll){
                    maxFinalBankroll = bankroll;
                }

                sumOfFinalBankrolls += bankroll;
                totalDelta += bankroll - bettingStrategy.baseBankroll;
            }
            results.averageFinalBankroll = (float) sumOfFinalBankrolls / amountOfSimulations; // Average final bankroll
            results.expectedValuePerGame = (float)totalDelta / results.handsPlayed; // Average expected value per game
            results.riskOfRuinInPercent = (float)ruinedGames / amountOfSimulations * 100; // Corrected risk calculation


            return results;
        }

    }
}
