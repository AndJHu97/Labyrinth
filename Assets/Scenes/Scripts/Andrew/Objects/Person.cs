using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct stats
{
    public int health;
    public int fortitude;
    public int strength;
    public int agility;
}

[System.Serializable]
public enum StatType
{
    Health,
    Fortitude,
    Strength,
    Agility
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
        inputNPCProcessing.playerStats = GameManager.Instance.player.playerStats;
        inputNPCProcessing.npcStats = stats;

        CheckConditions(playerIntentSO);


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

        stats.health = Mathf.RoundToInt(
            InteractionProcessing.CalculateNewHealthValue(
                stats.health,
                playerIntentSO.netHealthImpact,
                npcActionResponse.setHealthImpact,
                npcActionResponse.newHealthImpact,
                npcActionResponse.healthMultiplier,
                npcActionResponse.additiveHealthValue
            )
        );

        int netHealthImpactOnNPC = stats.health - storedHealth;

        // ------------------------------------------------
        // APPLY NPC THREAT
        // ------------------------------------------------

        threatRelationshipValue =
            InteractionProcessing.CalculateNewRelationshipValue(
                threatRelationshipValue,
                //negative because decreasing health makes it more threatening. 
                -netHealthImpactOnNPC,
                npcActionResponse.setThreatImpact,
                npcActionResponse.newThreatImpact,
                npcActionResponse.threatMultiplier,
                npcActionResponse.additiveThreatValue,
                learningRate
            );

        // ------------------------------------------------
        // APPLY NPC ALLEGIANCE
        // ------------------------------------------------

        allegianceRelationshipValue =
            InteractionProcessing.CalculateNewRelationshipValue(
                allegianceRelationshipValue,
                netHealthImpactOnNPC,
                npcActionResponse.setAllegianceImpact,
                npcActionResponse.newAllegianceImpact,
                npcActionResponse.allegianceMultiplier,
                npcActionResponse.additiveAllegianceValue,
                learningRate
            );

        // ------------------------------------------------
        // APPLY EFFECT TO PLAYER
        // ------------------------------------------------

        GameManager.Instance.player.playerStats.health +=
            npcActionResponse.netHealthImpactOnPlayer;

        // ------------------------------------------------
        // DISPLAY RESPONSE
        // ------------------------------------------------
        GameManager.Instance.LogDisplayTextsToRecentLog(npcActionResponse.displayTexts);


    }

}
