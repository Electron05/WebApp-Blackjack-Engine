using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models; 

public class GameRulesModel : PageModel
{
    private const string GameSessionKey = "Game";

    [BindProperty]
    public bool canDoubleAfterSplit { get; set; }
    [BindProperty]
    public bool canSurrender { get; set; }
    [BindProperty]
    public bool doesDealerHitOnSoft17 { get; set; }
    [BindProperty]
    public int numberOfAllowedHands { get; set; }
    [BindProperty]
    public int numberOfDecksInShoe { get; set; }
    [BindProperty]
    public int deckPenetration { get; set; }
    [BindProperty]
    public int blackjackPayoutTimesTen { get; set; }

    public required List<SelectListItem> BlackjackPayoutOptions { get; set; }


    public void OnGet()
    {
        var gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey) ?? new Game();
        // Assuming Game has a public GameRules property
		
        canDoubleAfterSplit = gameInstance.gameRules.canDoubleAfterSplit;
        canSurrender = gameInstance.gameRules.canSurrender;
        doesDealerHitOnSoft17 = gameInstance.gameRules.doesDealerHitOnSoft17;
        numberOfAllowedHands = gameInstance.gameRules.numberOfAllowedHands;
        numberOfDecksInShoe = gameInstance.gameRules.numberOfDecksInShoe;
        deckPenetration = gameInstance.gameRules.deckPenetration;
        blackjackPayoutTimesTen = (int)(gameInstance.gameRules.blackjackPayout*10);

        FillBlackJackPayouts();


        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
    }

    public IActionResult OnPost()
    {
        var gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey) ?? new Game();
        // Update game rules based on form submission
        GameRules newGameRules = new GameRules(
            canDoubleAfterSplit,
            canSurrender,
            doesDealerHitOnSoft17,
            numberOfAllowedHands,
            numberOfDecksInShoe,
            deckPenetration,
            blackjackPayoutTimesTen/10f
        );
        gameInstance.SetNewGameRules(newGameRules);


        FillBlackJackPayouts();


        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
        return Page();
    }

    private void FillBlackJackPayouts(){
        if(blackjackPayoutTimesTen == 15){
            BlackjackPayoutOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "15", Text = "3:2" },
                new SelectListItem { Value = "12", Text = "6:5" },
            };
        }
        else{
            BlackjackPayoutOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "12", Text = "6:5" },
                new SelectListItem { Value = "15", Text = "3:2" },
            };
        }
    }
}