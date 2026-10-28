using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct stats
{
    public int health;
    public int maxHealth;
    public int fortitude;
    public int strength;
    public int agility;
    public int faith;
}

[System.Serializable]
public enum StatType
{
    Health,
    MaxHealth,
    Fortitude,
    Strength,
    Agility, 
    Faith
}


public class Person : Object_
{
    public float learningRate = 0.5f;
    public int experiencePointsGainedIfKilled = 5;
    public List<ActionOnObjectSO> friendlyActionOnNPC = new List<ActionOnObjectSO>();
    public List<ActionOnObjectSO> fearfulActionOnNPC = new List<ActionOnObjectSO>();
    public List<ActionOnObjectSO> neutralActionOnNPC = new List<ActionOnObjectSO>();
    public List<ActionOnObjectSO> aggressiveActionOnNPC = new List<ActionOnObjectSO>();


    public List<NPCActionResponseSO> friendlyResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> fearfulResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> likeResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> dislikeResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> neutralResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> aggressiveResponses = new List<NPCActionResponseSO>();

   

    // Start is called before the first frame update
    void Start()
    {
        allActionsOnObject.AddRange(friendlyActionOnNPC);
        allActionsOnObject.AddRange(fearfulActionOnNPC);
        allActionsOnObject.AddRange(neutralActionOnNPC);
        allActionsOnObject.AddRange(aggressiveActionOnNPC);
    }

    public override void ReceivePlayerAction(PlayerIntentSO playerIntentSO)
    {
        //Check what type of action this is on the NPC by finding this in the list
        //threat relationship value and allegiance relationship value of the NPC
        //person stats
        //npc stats
        
        //Then pick random action from the list type returned by the processing.
        
        //Do action on player
        ActionOnObjectSO actionOnNPC = null;
        PlayerActionEmotionType actionEmotionType = PlayerActionEmotionType.Neutral;
        InputNPCProcessing inputNPCProcessing = new InputNPCProcessing();
        inputNPCProcessing.threatRelationshipValue = threatRelationshipValue;
        inputNPCProcessing.allegianceRelationshipValue = allegianceRelationshipValue;
        inputNPCProcessing.playerStats = GameManager.Instance.player.totalStats;
        inputNPCProcessing.npcStats = stats;

        CheckAndRunConditions(playerIntentSO);
        if (!isActive) return;   // dead: conditions can fire, but no combat

        // Check Friendly
        foreach (ActionOnObjectSO action in friendlyActionOnNPC)
        {
            if (action.playerIntent == playerIntentSO)
            {
                actionOnNPC = action;
                actionEmotionType = PlayerActionEmotionType.Friendly;
                break;
            }
        }

        // Check Fearful if not found
        if (actionOnNPC == null)
        {
            foreach (ActionOnObjectSO action in fearfulActionOnNPC)
            {
                if (action.playerIntent == playerIntentSO)
                {
                    actionOnNPC = action;
                    actionEmotionType = PlayerActionEmotionType.Fearful;
                    break;
                }
            }
        }

        // Check Neutral if not found
        if (actionOnNPC == null)
        {
            foreach (ActionOnObjectSO action in neutralActionOnNPC)
            {
                if (action.playerIntent == playerIntentSO)
                {
                    actionOnNPC = action;
                    actionEmotionType = PlayerActionEmotionType.Neutral;
                    break;
                }
            }
        }

        // Check Aggressive if not found
        if (actionOnNPC == null)
        {
            foreach (ActionOnObjectSO action in aggressiveActionOnNPC)
            {
                if (action.playerIntent == playerIntentSO)
                {
                    actionOnNPC = action;
                    actionEmotionType = PlayerActionEmotionType.Aggressive;
                    break;
                }
            }
        }

        if (actionOnNPC == null)
        {
            Debug.Log("No ActionOnNPC found for this PlayerAction.");
            return;
        }

        inputNPCProcessing.playerActionEmotionType = actionEmotionType;


        //Determine the emotional response of NPC
        NPCActionEmotionType npcActionEmotionType = InteractionProcessing.DetermineNPCEmotionResponse(inputNPCProcessing);

        //Get action based on emotional response
        List<NPCActionResponseSO> responseList = null;

        switch (npcActionEmotionType)
        {
            case NPCActionEmotionType.Friendly:
                responseList = friendlyResponses;
                break;

            case NPCActionEmotionType.Fearful:
                responseList = fearfulResponses;
                break;

            case NPCActionEmotionType.Like:
                responseList = likeResponses;
                break;

            case NPCActionEmotionType.Dislike:
                responseList = dislikeResponses;
                break;

            case NPCActionEmotionType.Neutral:
                responseList = neutralResponses;
                break;

            case NPCActionEmotionType.Aggressive:
                responseList = aggressiveResponses;
                break;
        }

        if (responseList == null || responseList.Count == 0)
        {
            Debug.Log($"No NPC response available for {npcActionEmotionType}");
            return;
        }

        // Pick a random response
        NPCActionResponseSO npcActionResponse =
            responseList[Random.Range(0, responseList.Count)];

        Debug.Log($"NPC response: {npcActionResponse.name_}");

        //This is strange part because NPCActionResponseSO also has the change in relationship automatically in there for the NPC while also holding action to the player. So we need to apply the relationship change to the NPC and then apply the action to the player.
        // ------------------------------------------------
        // APPLY NPC HEALTH
        // ------------------------------------------------

        int storedHealth = stats.health;

        int netHealthImpactOnNPC = 0;

        //Choose whether to use the actionNPC or the intent default values
        if(actionOnNPC.overrideIntentsHealthImpact)
        {
            //WOULD NEED TO REDO SO THESE VALUES AREN'T FROM NPC ACTION RESPONSE. WILL HAVE TO CREATE NEW LIST FOR SPECIFIC HEALTH IMPACT. TIER 2
            netHealthImpactOnNPC = Mathf.RoundToInt(
            InteractionProcessing.CalculateNewHealthImpactForObjects(
                actionOnNPC.overrideNewHealthImpact,
                npcActionResponse.setNewSelfHealthImpact,
                npcActionResponse.newSelfHealthImpact,
                npcActionResponse.additiveSelfHealthValueFromPlayer
            )
        );
        }
        else
        {
            netHealthImpactOnNPC = Mathf.RoundToInt(
            InteractionProcessing.CalculateNewHealthImpactForObjects(
                playerIntentSO.netHealthImpact,
                npcActionResponse.setNewSelfHealthImpact,
                npcActionResponse.newSelfHealthImpact,
                npcActionResponse.additiveSelfHealthValueFromPlayer
            )
        );
        }



        netHealthImpactOnNPC = -BattleMechanics.CalculateDamage(
            -netHealthImpactOnNPC,
            GameManager.Instance.player.totalStats.strength,
            stats.fortitude,
            stats.agility,
            this,
            true
        );

        stats.health += netHealthImpactOnNPC;

        CheckPersonDeath();

        if(stats.health<= 0)
        {
            // If the NPC is dead, we don't need to apply further relationship changes nor display any response, so we can return early.
            return;
        }


        // ------------------------------------------------
        // APPLY NPC THREAT
        // ------------------------------------------------

        threatRelationshipValue = 
            InteractionProcessing.CalculateNewRelationshipValue(
                threatRelationshipValue,
                //negative because decreasing health makes it more threatening. 
                -netHealthImpactOnNPC,
                npcActionResponse.setNewSelfThreatImpact,
                npcActionResponse.newSelfThreatImpact,
                npcActionResponse.additiveSelfThreatValueFromPlayer,
                learningRate
            );

        // ------------------------------------------------
        // APPLY NPC ALLEGIANCE
        // ------------------------------------------------

        allegianceRelationshipValue =
            InteractionProcessing.CalculateNewRelationshipValue(
                allegianceRelationshipValue,
                netHealthImpactOnNPC,
                npcActionResponse.setNewSelfAllegianceImpact,
                npcActionResponse.newSelfAllegianceImpact,
                npcActionResponse.additiveSelfAllegianceValueFromPlayer,
                learningRate
            );

        // ------------------------------------------------
        // DISPLAY RESPONSE
        // ------------------------------------------------
        GameManager.Instance.LogDisplayTextsToRecentLog(npcActionResponse.displayTexts, this);

        // ------------------------------------------------
        // APPLY EFFECT TO PLAYER
        // ------------------------------------------------

        
        if(npcActionResponse.netHealthImpactOnPlayer == 0)
        {
            // If the net health impact on the player is zero, we can skip the damage calculation and return early.
            return;
        }
        int netHealthImpactOnPlayer = -BattleMechanics.CalculateDamage(
            //Negative because this is the default or starting damage. This will be added to attacker strength so if it does -5 damage, then it will substract the strength. So reverse signs to add it
            -npcActionResponse.netHealthImpactOnPlayer,
            stats.strength,
            GameManager.Instance.player.totalStats.fortitude,
            GameManager.Instance.player.totalStats.agility,
            this,
            false
        );

        

        GameManager.Instance.player.baseStats.health +=
            netHealthImpactOnPlayer;

    }

    public void CheckPersonDeath()
    {
        if(stats.health<= 0)
        {
            // Handle NPC death logic here
            Debug.Log($"{name_} has died.");

            GameManager.Instance.player.experiencePoints += experiencePointsGainedIfKilled;

            isActive = false; // Mark the NPC as inactive or dead

            GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"{name_} has died. You gained {experiencePointsGainedIfKilled} experience points." }, this);
        }

    }

}
