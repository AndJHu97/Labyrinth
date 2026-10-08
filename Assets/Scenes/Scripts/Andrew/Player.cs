using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public stats playerStats;
    [SerializeField]
    public List<PlayerIntentSO> playerIntentions = new();
    [SerializeField]
    public int experiencePoints = 0;
    [SerializeField]
    public int totalStats = 15;
    [SerializeField]
    public int faithLevel = 0;

    [SerializeField]
    public int level = 1;

    [SerializeField]
    public List<ItemSO> inventory = new();

    public void Start()
    {
        
    }

    public void GainExperience(int amount)
    {
        experiencePoints += Mathf.Max(0, experiencePoints + amount);
        CheckLevelUp();
    }

    public void CheckLevelUp()
    {
        if(experiencePoints >= level * 100)
        {
            LevelUp();
            experiencePoints = 0;
        }
    }

    public void LevelUp()
    {
        level++;
        totalStats += 1;
    }

    //pick right keys to perform on the NPC
    //TODO: Will have to include one to act on onself
    public void PerformAction(EmotionKey emotionKey, MotorSensorKey motorSensorKey, ConcentrationKey concentrationKey, Object_ object_)
    {
        foreach(var intent in playerIntentions)
        {
            if(intent.emotionKey == emotionKey && intent.motorSensorKey == motorSensorKey)
            {
                playerStats.health += intent.netSelfHealthImpact;

                string targetID = object_ != null ? object_.id : null;
                GameManager.Instance.LogAction(intent, emotionKey, motorSensorKey, concentrationKey,
                               GameManager.Instance.currentRoom, targetID);
                if (object_ != null)
                {
                    object_.ReceivePlayerAction(intent);
                }
                else
                {
                    // Nothing there: the action text is the whole result
                    Debug.Log(intent.actionText);
                    GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { intent.actionText });
                }

                ObjectRegistry.ForEach(o => o.CheckTriggerConditions(intent));

                if (playerStats.health <= 0) GameManager.Instance.PlayerDeath();
                return;
            }

            
        }
        Debug.Log($"No intent matches {emotionKey} + {motorSensorKey}.");
    }

    public void PerformAction(EmotionKey emotionKey, MotorSensorKey motorSensorKey)
    {
        foreach(var intent in playerIntentions)
        {
            if(intent.emotionKey == emotionKey && intent.motorSensorKey == motorSensorKey)
            {
                // Perform the action associated with the intent
                Debug.Log($"Performing action: {intent.actionText}");
                playerStats.health += intent.netSelfHealthImpact;
                // Here you would implement the logic for acting on oneself
                //TODO
                return;
            }
        }
    }


}
