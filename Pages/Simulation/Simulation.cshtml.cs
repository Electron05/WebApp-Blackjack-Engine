using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models; // Adjust namespace as per your project


public class SimulationModel : PageModel
{
    private const string GameSessionKey = "Game";

    public required string log { get; set; } = "";

    public int baseBankroll { get; set; }

	[BindProperty]
	public int amountOfSimulations { get; set; }

	[BindProperty]
	public int maxGamesInSimulation { get; set; }

	public float expectedValuePerGame { get; set; }

	public float riskOfRuinInPercent { get; set; }

	public int maxFinalBankroll { get; set; }
	public float averageFinalBankroll { get; set; }

	public int wins {get;set;}
	public int doubleWins {get;set;}
	
	public int loses {get;set;}
	public int doubleLoses {get;set;}
	public int pushes {get;set;}
	public int surrenders {get;set;}
	public int blackjacks {get;set;}

	public bool anomaly {get;set;}

	public required Game gameInstance;

    public void OnGet()
    {
        gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey)!;
        gameInstance = gameInstance ?? new Game();

		baseBankroll = gameInstance.bettingStrategy.baseBankroll;

		//default values
		maxFinalBankroll = 0;
		amountOfSimulations = 1; 
		maxGamesInSimulation = 1000000;

		wins =0;
		doubleWins =0;
		loses =0;
		doubleLoses = 0;
		surrenders = 0;
		pushes =0;
		blackjacks=0;
		anomaly = false;


        HttpContext.Session.SetObject(GameSessionKey, gameInstance);
    }

    public IActionResult OnPost()
    {
		gameInstance = HttpContext.Session.GetObject<Game>(GameSessionKey) ?? new Game();
		gameInstance = gameInstance ?? new Game();
		
		baseBankroll = gameInstance.bettingStrategy.baseBankroll;
		Simulate();

        HttpContext.Session.SetObject(GameSessionKey, gameInstance);

        return Page();
    }

	private void Simulate(){

		var results = gameInstance.Simulation(amountOfSimulations, maxGamesInSimulation);
		
		wins = results.normalWins;
		loses = results.normalLoses;
		doubleWins = results.doubleWins;
		doubleLoses = results.doubleLoses;

		surrenders = results.surrenders;
		pushes = results.pushes;
		blackjacks = results.blackJacks;

		anomaly = results.anomaly;

		expectedValuePerGame = results.expectedValuePerGame;
		averageFinalBankroll = results.averageFinalBankroll;




	}

}
