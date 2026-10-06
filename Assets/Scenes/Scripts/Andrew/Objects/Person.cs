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


public class Person : Object
{
    [SerializeField]
    public PersonSO personSO;
    public string personID;
    public string name_;
    public stats personStats;
    public float threatRelationshipValue;
    public float allegianceRelationshipValue;
    public bool isAlive;
    public float learningRate = 0.1f;

    public List<ActionOnNPCSO> friendlyActionOnNPC = new List<ActionOnNPCSO>();
    public List<ActionOnNPCSO> fearfulActionOnNPC = new List<ActionOnNPCSO>();
    public List<ActionOnNPCSO> neutralActionOnNPC = new List<ActionOnNPCSO>();
    public List<ActionOnNPCSO> aggressiveActionOnNPC = new List<ActionOnNPCSO>();


    public List<NPCActionResponseSO> friendlyResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> fearfulResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> likeResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> dislikeResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> neutralResponses = new List<NPCActionResponseSO>();
    public List<NPCActionResponseSO> aggressiveResponses = new List<NPCActionResponseSO>();

    public List<TriggeredResponseSO> triggeredResponses = new List<TriggeredResponseSO>();

    public GameManager gameManager;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        personID = personSO.personID;
        name_ = personSO.name_;
        personStats = personSO.personStats;
        threatRelationshipValue = personSO.threatRelationshipValue;
        allegianceRelationshipValue = personSO.allegianceRelationshipValue;
        isAlive = personSO.isAlive;
        learningRate = personSO.learningRate;

        friendlyActionOnNPC = personSO.friendlyActionOnNPC;
        fearfulActionOnNPC = personSO.fearfulActionOnNPC;
        neutralActionOnNPC = personSO.neutralActionOnNPC;
        aggressiveActionOnNPC = personSO.aggressiveActionOnNPC;

        friendlyResponses = personSO.friendlyResponses;
        fearfulResponses = personSO.fearfulResponses;
        likeResponses = personSO.likeResponses;
        dislikeResponses = personSO.dislikeResponses;
        neutralResponses = personSO.neutralResponses;
        aggressiveResponses = personSO.aggressiveResponses;

        triggeredResponses = personSO.triggeredResponses;

        gameManager = FindObjectOfType<GameManager>();

        player = gameManager.player;
    }

    public void PlayerActionOnNPC(PlayerIntentSO playerActionSO)
    {
        //Check what type of action this is on the NPC by finding this in the list
        //threat relationship value and allegiance relationship value of the NPC
        //person stats
        //npc stats
        
        //Then pick random action from the list type returned by the processing.
        
        //Do action on player
        ActionOnNPCSO actionOnNPC = null;
        PlayerActionEmotionType actionEmotionType = PlayerActionEmotionType.Neutral;
        InputNPCProcessing inputNPCProcessing = new InputNPCProcessing();
        inputNPCProcessing.threatRelationshipValue = threatRelationshipValue;
        inputNPCProcessing.allegianceRelationshipValue = allegianceRelationshipValue;
        inputNPCProcessing.playerStats = player.playerStats;
        inputNPCProcessing.npcStats = personStats;


        // Check Friendly
        foreach (ActionOnNPCSO action in friendlyActionOnNPC)
        {
            if (action.playerAction == playerActionSO)
            {
                actionOnNPC = action;
                actionEmotionType = PlayerActionEmotionType.Friendly;
                break;
            }
        }

        // Check Fearful if not found
        if (actionOnNPC == null)
        {
            foreach (ActionOnNPCSO action in fearfulActionOnNPC)
            {
                if (action.playerAction == playerActionSO)
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
            foreach (ActionOnNPCSO action in neutralActionOnNPC)
            {
                if (action.playerAction == playerActionSO)
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
            foreach (ActionOnNPCSO action in aggressiveActionOnNPC)
            {
                if (action.playerAction == playerActionSO)
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

        Debug.Log($"NPC response: {npcActionResponse.name}");

        //This is strange part because NPCActionResponseSO also has the change in relationship automatically in there for the NPC while also holding action to the player. So we need to apply the relationship change to the NPC and then apply the action to the player.
        // ------------------------------------------------
        // APPLY NPC HEALTH
        // ------------------------------------------------

        int storedHealth = personStats.health;

        personStats.health = Mathf.RoundToInt(
            InteractionProcessing.CalculateNewHealthValue(
                personStats.health,
                playerActionSO.netHealthImpact,
                npcActionResponse.setHealthImpact,
                npcActionResponse.newHealthImpact,
                npcActionResponse.healthMultiplier,
                npcActionResponse.additiveHealthValue
            )
        );

        int netHealthImpactOnNPC = personStats.health - storedHealth;

        // ------------------------------------------------
        // APPLY NPC THREAT
        // ------------------------------------------------

        threatRelationshipValue =
            InteractionProcessing.CalculateNewRelationshipValue(
                threatRelationshipValue,
                netHealthImpactOnNPC,
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

        player.playerStats.health +=
            npcActionResponse.netHealthImpactOnPlayer;

        if (player.playerStats.health <= 0)
        {
            GameManager.Instance.PlayerDeath();
        }

        // ------------------------------------------------
        // DISPLAY RESPONSE
        // ------------------------------------------------

        Debug.Log(npcActionResponse.actionText);

        
    }

}
