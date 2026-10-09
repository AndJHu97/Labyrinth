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

                GameManager.Instance.LogAction(intent, emotionKey, motorSensorKey, concentrationKey,
                               GameManager.Instance.currentRoom, object_);

                
                //Check if interacting with anything
                if (object_ != null)
                {
                    ActionOnObjectSO actionOnObjectInObject = null;
                    //Find the what the intent does to the object (like angry arm does punch to one object but to another it is a slap)
                    foreach (ActionOnObjectSO action in object_.allActionsOnObject)
                    {
                        if (action.playerIntent == intent)
                        {
                            //action may have specific texts to display
                            actionOnObjectInObject = action;
                        }
                    }

                    //if object_ has unique response, highest priority
                    //if actionOnObject in the object has response, second priority
                    //if no object_ or actionOnObject in the object has response, use default text from intent (idle)
                    ResolveAndLogInteractionTexts(intent, object_, actionOnObjectInObject);

                    object_.ReceivePlayerAction(intent);
                }
                //Idle chats
                else
                {
                    ResolveAndLogInteractionTexts(intent);
                }

                ObjectRegistry.ForEach(o => o.CheckTriggerConditions(intent));

                if (playerStats.health <= 0) GameManager.Instance.PlayerDeath();
                return;
            }

            
        }
        Debug.Log($"No intent matches {emotionKey} + {motorSensorKey}.");
    }

    public void ResolveAndLogInteractionTexts(PlayerIntentSO intent, Object_ object_ = null, ActionOnObjectSO actionOnObject = null)
    {
        
        if (object_ != null)
        {
          
            if (object_.UniqueResponseToPlayerDisplayTexts != null && object_.UniqueResponseToPlayerDisplayTexts.Count > 0)
            {
                foreach(var response in object_.UniqueResponseToPlayerDisplayTexts)
                {

                    //match the key binds to the object's unique response and log the display texts if they match
                    if ((!response.UseEmotionKey || response.emotionKey == intent.emotionKey) &&
(!response.UseMotorSensorKey || response.motorSensorKey == intent.motorSensorKey))
                    {
                        GameManager.Instance.LogDisplayTextsToRecentLog(response.displayTexts, object_);
                        return;
                    }
                }
            }

            
            if (actionOnObject != null && actionOnObject.defaultInteractingDisplayText != null && actionOnObject.defaultInteractingDisplayText.Count > 0)
            {
                GameManager.Instance.LogDisplayTextsToRecentLog(actionOnObject.defaultInteractingDisplayText, object_);
                return;
            }

            //this means there are no direct texts to display for the object or action, so we will use the default text from the intent
            return;
        }

        GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { intent.defaultNoninteractableText });


    }

    //PERFORMING ACTION ON ONESELF
    public void PerformAction(EmotionKey emotionKey, MotorSensorKey motorSensorKey)
    {
        foreach(var intent in playerIntentions)
        {
            if(intent.emotionKey == emotionKey && intent.motorSensorKey == motorSensorKey)
            {
                // Perform the action associated with the intent
                Debug.Log($"Performing action: {intent.defaultNoninteractableText}");
                playerStats.health += intent.netSelfHealthImpact;
                // Here you would implement the logic for acting on oneself
                //TODO
                return;
            }
        }
    }


}
