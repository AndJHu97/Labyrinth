using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameState
{
    public int roundNumber;
    public int actionStep;
    public int totalActionStep;
    public PlayerIntentSO playerIntent;
    public EmotionKey emotionKey;
    public MotorSensorKey motorSensorKey;
    public ConcentrationKey concentrationKey;
    public Room room;
    public List<string> displayTexts;
    public Object_ target;
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

    public void LogAction(PlayerIntentSO intent, EmotionKey emotionKey, MotorSensorKey motorSensorKey,
                       ConcentrationKey concentrationKey, Room room, Object_ target)
    {
        actionStep++;
        totalActionStep++;

        gameLog.Add(new GameState
        {
            roundNumber = roundNumber,
            actionStep = actionStep,
            totalActionStep = totalActionStep,
            playerIntent = intent,
            emotionKey = emotionKey,
            motorSensorKey = motorSensorKey,
            concentrationKey = concentrationKey,
            room = room,
            target = target,
            displayTexts = new List<string>()
        });

        ObjectRegistry.ForEach(o => o.CheckTriggerConditions(intent));
    }

    public void LogDisplayTextsToRecentLog(List<string> texts, Object_ self = null)
    {
        if (texts == null || gameLog.Count == 0) return;

        GameState last = gameLog[gameLog.Count - 1];
        if (last.displayTexts == null) last.displayTexts = new List<string>();

        foreach (var raw in texts)
        {
            string formatted = TextFormatter.Format(raw, self);
            Debug.Log(formatted);
            last.displayTexts.Add(formatted);
        }

        gameLog[gameLog.Count - 1] = last;
    }
}
