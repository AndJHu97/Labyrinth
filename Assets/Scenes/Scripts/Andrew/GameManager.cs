using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameState
{
    public int roundNumber;
    public int actionStep;
    public int totalActionStep;
    public PlayerIntentSO playerIntent;
    public Room room;
    public List<string> displayTexts;
    public string targetID;
}

public class GameManager : MonoBehaviour
{
    public Player player;
    public int roundNumber = 1;
    public int actionStep = 0;
    public int totalActionStep = 0;
    public Room currentRoom = null;
    public List<GameState> gameLog = new();
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayerDeath()
    {
        Debug.Log("Player has died.");
        roundNumber++;
        actionStep = 0;
    }

    public void LogAction(PlayerIntentSO playerIntent, Room room, string targetID)
    {
        GameState gameState = new GameState
        {
            roundNumber = roundNumber,
            actionStep = actionStep,
            totalActionStep = totalActionStep,
            playerIntent = playerIntent,
            room = room,
            targetID = targetID
        };
        gameLog.Add(gameState);

        actionStep++;
        totalActionStep++;
    }

    public void LogDisplayTextsToRecentLog(List<string> consoleTexts)
    {
        if (gameLog.Count > 0)
        {
            GameState lastGameState = gameLog[gameLog.Count - 1];
            lastGameState.displayTexts = new List<string>(consoleTexts);
            gameLog[gameLog.Count - 1] = lastGameState;
        }
    }
}
