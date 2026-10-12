using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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
    public int experienceGainedWhenNewAction = 10; // Amount of experience gained when a new action is performed

    public static event System.Action<string> OnTextDisplayed;

    private void Awake()
    {
        Instance = this;
    }

   public void Start()
    {
        if (currentRoom == null)
            return;

        gameLog.Add(new GameState
        {
            roundNumber = roundNumber,
            actionStep = actionStep,
            totalActionStep = totalActionStep,
            room = currentRoom,
            displayTexts = new List<string>()
        });

        LogDisplayTextsToRecentLog(currentRoom.roomDescription);
    }

    public void LogAction(PlayerIntentSO intent, EmotionKey emotionKey, MotorSensorKey motorSensorKey,
                       ConcentrationKey concentrationKey, Room room, Object_ target)
    {
        actionStep++;
        totalActionStep++;

        bool hasPerformedActionBefore = false;
        foreach (GameState state in gameLog)
        {
            if(state.playerIntent == intent && state.room == room && state.target == target && state.concentrationKey == concentrationKey)
            {
                
                hasPerformedActionBefore = true;
            }
        }

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

        if (!hasPerformedActionBefore)
        {
            player.GainExperience(experienceGainedWhenNewAction);
            LogDisplayTextsToRecentLog(new List<string> { $"Yes, keep trying... Gain {experienceGainedWhenNewAction} experience for discovering a new action." });
        }

        ObjectRegistry.ForEach(o => o.CheckAndRunTriggerConditions(intent));
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

            //This invokes AddLine which puts it in the screen. 
            OnTextDisplayed?.Invoke(formatted);
            last.displayTexts.Add(formatted);
        }

        gameLog[gameLog.Count - 1] = last;
    }
}
