using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public stats baseStats;
    public stats statsModifications;

    public stats totalStats
    {
        get
        {
            return new stats
            {
                health = baseStats.health + statsModifications.health,
                maxHealth = baseStats.maxHealth + statsModifications.maxHealth,
                strength = baseStats.strength + statsModifications.strength,
                agility = baseStats.agility + statsModifications.agility,
                fortitude = baseStats.fortitude + statsModifications.fortitude,
                //capped at one stat. 
                faith = baseStats.faith + statsModifications.faith > 10 ? 10 : baseStats.faith + statsModifications.faith
            };
        }
    }

        [SerializeField]
    public List<PlayerIntentSO> playerIntentions = new();
    [SerializeField]
    public int experiencePoints = 0;
    [SerializeField]
    public int totalStatPoints = 15;
    [SerializeField]
    public int faithLevel = 0;

    [SerializeField]
    public int level = 1;

    public int experienceGainedUponDeath = 50; // Amount of experience gained upon death

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
        GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"You have increased your Value level. You are now Value Level {level + 1}." });   
        level++;
        totalStatPoints += 1;
    }

    //pick right keys to perform on the NPC
    //TODO: Will have to include one to act on onself
    public void PerformActionObject(EmotionKey emotionKey, MotorSensorKey motorSensorKey, ConcentrationKey concentrationKey, Object_ object_)
    {
        foreach(var intent in playerIntentions)
        {
            if(intent.emotionKey == emotionKey && intent.motorSensorKey == motorSensorKey)
            {
                baseStats.health += intent.netSelfHealthImpact;

                if (object_ != null && !object_.CanReceiveActions())
                    object_ = null;   // treat it as an idle action instead

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

                    //Only show this if you did a non zero damage on yourself
                    if (intent.netSelfHealthImpact != 0)
                    {
                        GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"Your {intent.name_} did {intent.netSelfHealthImpact} damage on yourself." });

                    }

                    object_.ReceivePlayerAction(intent);

                    
                }
                //Idle chats
                else
                {
                    
                    ResolveAndLogInteractionTexts(intent);
                    
                }

                ObjectRegistry.ForEach(o => o.CheckAndRunTriggerConditions());

                if (totalStats.health <= 0) PlayerDeath();
                return;
            }

            
        }
        Debug.Log($"No intent matches {emotionKey} + {motorSensorKey}.");
    }

    public void PlayerDeath()
    {
        Debug.Log("Player has died.");

        experiencePoints += (int)experienceGainedUponDeath;
        RemoveAllInventory();
        baseStats.health = baseStats.maxHealth;
        GameManager.Instance.ResetRound();
    }

    public void ResolveAndLogInteractionTexts(PlayerIntentSO intent, Object_ object_ = null, ActionOnObjectSO actionOnObject = null)
    {
        
        if (object_ != null)
        {

            Door door = object_.GetComponent<Door>();
            //If going through door then can do a specific response
            if (door != null && door.CanEnter(intent.motorSensorKey))
            {
                if (object_.GetComponent<Door>().isActive)
                {
                    GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { "You focus on the {target} {direction} and open the door." }, object_);
                    return;
                }
            }

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
            //Only if no other conditions are met, we will display the default text from the intent
            if (!InteractionProcessing.CheckAllTriggersAndConditions())
            {
                GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { "You focus on the {target} {direction}, but your action does nothing to it. Instead, you react to yourself." }, object_);
                GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { intent.defaultNoninteractableText }, object_);
                return;
            }
        }

        if (!InteractionProcessing.CheckAllTriggersAndConditions())
        {
            GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { "There was nothing to interact with {direction}." });
            GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { intent.defaultNoninteractableText });
            return;
        }
        

    }

    //PERFORMING ACTION ON ONESELF
    public void PerformActionOnSelf(EmotionKey emotionKey, MotorSensorKey motorSensorKey)
    {
        foreach(var intent in playerIntentions)
        {
            if(intent.emotionKey == emotionKey && intent.motorSensorKey == motorSensorKey)
            {
                // Perform the action associated with the intent
                Debug.Log($"Performing action: {intent.defaultNoninteractableText}");
                baseStats.health += intent.netSelfHealthImpact;
                // Here you would implement the logic for acting on oneself
                //TODO
                return;
            }
        }
    }

    public void AddItemToInventory(ItemSO item)
    {
        if (item != null && !inventory.Contains(item))
        {
            inventory.Add(item);
            //get the new stats modifications
            statsModifications = InteractionProcessing.TotalItemModification();
            GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"You have acquired: {item.name}" });
            Debug.Log($"Added {item.name} to inventory.");
        }
    }

    void ApplyItemStats(ItemSO item, int sign)
    {
        statsModifications.health += sign * item.statsModification.health;
        statsModifications.maxHealth += sign * item.statsModification.maxHealth;
        statsModifications.strength += sign * item.statsModification.strength;
        statsModifications.agility += sign * item.statsModification.agility;
        statsModifications.fortitude += sign * item.statsModification.fortitude;
        statsModifications.faith += sign * item.statsModification.faith;
    }

    public void RemoveItemFromInventory(ItemSO item)
    {
        if (item == null || !inventory.Remove(item)) return;

        ApplyItemStats(item, -1);
        GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"You lost {item.name_}." });
    }

    public void RemoveAllInventory()
    {
        if (inventory.Count == 0) return;

        foreach (var item in inventory)
            ApplyItemStats(item, -1);

        inventory.Clear();
        GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { "You lost everything you were carrying." });
    }

}
