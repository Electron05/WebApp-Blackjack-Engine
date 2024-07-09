using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models; 

public class BettingStrategyModel : PageModel
{
    private const string GameSessionKey = "Game";
    private const string BankrollSessionKey = "Bankroll";
    [BindProperty]
	public int baseBankroll { get; set; }
	[BindProperty]
	public int basicBet { get; set; }
	[BindProperty]
	public int OverT1Bet { get; set; }
	[BindProperty]
	public int OverT2Bet { get; set; }
	[BindProperty]
	public int OverT3Bet { get; set; }
	[BindProperty]
	public int OverT4Bet { get; set; }
	[BindProperty]
	public int OverT5Bet { get; set; }
	[BindProperty]
	public int OverT6Bet { get; set; }



    public void OnGet()
    {
        var gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey) ?? new Game();
        // Assuming Game has a public GameRules property

		BettingStrategy currentBettingStrategy = gameInstance.bettingStrategy;
		baseBankroll = currentBettingStrategy.baseBankroll;
		basicBet = currentBettingStrategy.basicBet;
		OverT1Bet = currentBettingStrategy.overT1Bet;
		OverT2Bet = currentBettingStrategy.overT2Bet;
		OverT3Bet = currentBettingStrategy.overT3Bet;
		OverT4Bet = currentBettingStrategy.overT4Bet;
		OverT5Bet = currentBettingStrategy.overT5Bet;
		OverT6Bet = currentBettingStrategy.overT6Bet;


        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
    }

    public IActionResult OnPost()
    {
        var gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey) ?? new Game();

		BettingStrategy bettingStrategy = new BettingStrategy(
			baseBankroll,
			basicBet,
			OverT1Bet,
			OverT2Bet,
			OverT3Bet,
			OverT4Bet,
			OverT5Bet,
			OverT6Bet
		);

		HttpContext.Session.SetInt32(BankrollSessionKey, baseBankroll);
		gameInstance.SetNewBettingStrategy(bettingStrategy);


        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
        return Page();
    }

}