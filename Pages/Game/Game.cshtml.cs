using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models; // Adjust namespace as per your project


public class GameModel : PageModel
{
    private const string GameSessionKey = "Game";
    private const string BankrollSessionKey = "Bankroll";

    public required string Log { get; set; } = "";

    public int Bankroll { get; set; }

    public void OnGet()
    {
        var gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey);
        gameInstance = gameInstance ?? new Game();

        Bankroll = HttpContext.Session.GetInt32(BankrollSessionKey) ?? gameInstance.bettingStrategy.baseBankroll;

        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
        HttpContext.Session.SetInt32(BankrollSessionKey, Bankroll);
    }



    public IActionResult OnPost()
    {
        var gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey);
        gameInstance = gameInstance ?? new Game();

        Bankroll = HttpContext.Session.GetInt32(BankrollSessionKey) ?? gameInstance.bettingStrategy.baseBankroll;

        int tmp = Bankroll;
        Log += gameInstance.playGameAndDisplayCardsLeft(ref tmp);
        Bankroll = tmp;

        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
        HttpContext.Session.SetInt32(BankrollSessionKey, Bankroll);

        return Page();
    }
}
