using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public enum NPCActionEmotionType
{
    Friendly,
    Fearful,
    Like,
    Dislike,
    Neutral,
    Aggressive
}

public enum PlayerActionEmotionType
{
    Friendly,
    Fearful,
    Neutral,
    Aggressive
}

public struct InputNPCProcessing
{
    public float threatRelationshipValue;
    public float allegianceRelationshipValue;
    public stats playerStats;
    public stats npcStats;
    public PlayerActionEmotionType playerActionEmotionType;
}



public static class InteractionProcessing 
{

    public static int THREATCUTOFF_HIGH = 70;
    public static int THREATCUTOFF_MEDIUM = 40;
    public static int ALLEGIANCECUTOFF_HIGH = 70;
    public static int ALLEGIANCECUTOFF_LOW = 30;
    public static int AGGRESSIVESTRENGTHPERCENTAGEDIFFERENCE_CUTOFF = 20;
    public static int FEARFULSTRENGTHPERCENTAGEDIFFERENCE_CUTOFF = 50;
    public static int FORTITUDECUTOFF_LOW = 30;
    public static NPCActionEmotionType DetermineNPCEmotionResponse(InputNPCProcessing inputNPCProcessing)
    {
        /**
        Take relationship value, stat values
        Aggressive: 
            If capable: Must have greater strength than player and a sufficient fortitude. 
            Threat level high, automatic aggression
            Threat level medium + neutral or aggressive, 
            Threat level low and allegiance not high + aggression

         Fearful: 
            Uncapable: Low fortitude or below the strength percentage 
            Threat level high, automatic fear
            threat level medium + neutral or aggressive,
            Threat level low and allegiance not high + aggression

        Dislike: 
            Player aggressive and not aggressive nor fearful reaction, then dislike

       
        Friendly:
            Player friendly or neutral, then friendly if allegiance high 
            Allegiance medium and player friendly, then friendly
        
        Like:
            Player friendly, then like. 
        return new NPCActionEmotionType(
        Neutral: Everything else
        **/

        float threatValue = inputNPCProcessing.threatRelationshipValue;
        float allegianceValue = inputNPCProcessing.allegianceRelationshipValue;
        stats playerStats = inputNPCProcessing.playerStats;
        stats npcStats = inputNPCProcessing.npcStats;
        PlayerActionEmotionType playerActionEmotionType = inputNPCProcessing.playerActionEmotionType;

        //If feels capable, then chance of aggressive response. These are all threatening. If not capable, then fearful
        if(npcStats.strength > playerStats.strength * (1f + AGGRESSIVESTRENGTHPERCENTAGEDIFFERENCE_CUTOFF/100f) && npcStats.fortitude >= FORTITUDECUTOFF_LOW)
        {
            //No matter the action, if threatening then aggressive response
            if (threatValue >= THREATCUTOFF_HIGH)
            {
                return NPCActionEmotionType.Aggressive;
            }

            //Threat level medium + neutral or aggressive, then aggressive response
            if(threatValue >= THREATCUTOFF_MEDIUM && (playerActionEmotionType == PlayerActionEmotionType.Neutral || playerActionEmotionType == PlayerActionEmotionType.Aggressive))
            {
                return NPCActionEmotionType.Aggressive;
            }

            //Threat level low and allegiance not high + aggression, then aggressive response
            if(threatValue < THREATCUTOFF_MEDIUM && allegianceValue < ALLEGIANCECUTOFF_HIGH && playerActionEmotionType == PlayerActionEmotionType.Aggressive)
            {
                return NPCActionEmotionType.Aggressive;
            }
        }
        //Fearful response (not capable)
        else if (npcStats.strength <= playerStats.strength * (1f - FEARFULSTRENGTHPERCENTAGEDIFFERENCE_CUTOFF / 100f) || npcStats.fortitude < FORTITUDECUTOFF_LOW)
        {
            //No matter the action, if threatening then fearful response
            if (threatValue >= THREATCUTOFF_HIGH)
            {
                return NPCActionEmotionType.Fearful;
            }

            //Threat level medium + neutral or aggressive, then fearful response
            if (threatValue >= THREATCUTOFF_MEDIUM && (playerActionEmotionType == PlayerActionEmotionType.Neutral || playerActionEmotionType == PlayerActionEmotionType.Aggressive))
            {
                return NPCActionEmotionType.Fearful;
            }

            //Threat level low and allegiance not high + aggression, then fearful response
            if (threatValue < THREATCUTOFF_MEDIUM && allegianceValue < ALLEGIANCECUTOFF_HIGH && playerActionEmotionType == PlayerActionEmotionType.Aggressive)
            {
                return NPCActionEmotionType.Fearful;
            }
        }

        //If not fearful nor aggressive and player is aggressive, then automatic dislike
        if (playerActionEmotionType == PlayerActionEmotionType.Aggressive)
        {
            return NPCActionEmotionType.Dislike;
        } 
       
        //Friendly Response
        if(playerActionEmotionType == PlayerActionEmotionType.Friendly || playerActionEmotionType == PlayerActionEmotionType.Neutral)
        {
            if(allegianceValue >= ALLEGIANCECUTOFF_HIGH)
            {
                return NPCActionEmotionType.Friendly;
            }
            else if(allegianceValue >= ALLEGIANCECUTOFF_LOW && playerActionEmotionType == PlayerActionEmotionType.Friendly)
            {
                return NPCActionEmotionType.Friendly;
            }
        }

        //Like Response
        if(playerActionEmotionType == PlayerActionEmotionType.Friendly)
        {
            return NPCActionEmotionType.Like;
        }


        return NPCActionEmotionType.Neutral;
    }

    public static float CalculateNewRelationshipValue(float currentValue, float netHealthImpact, bool setNetRelationshipImpact, float newNetRelationshipImpact, float multiplicativeValue, float additiveValue, float learningRate)
    {
        if (!setNetRelationshipImpact)
        {
           
            currentValue += netHealthImpact * learningRate;

        }
        else
        {
            currentValue += newNetRelationshipImpact;
        }


        currentValue *= multiplicativeValue;
        currentValue += additiveValue;

        if(currentValue > 100f)
        {
            currentValue = 100f;
        }
        else if(currentValue < 0f)
        {
            currentValue = 0f;
        }

        return currentValue;
    }

    public static float CalculateNewHealthValue
    (
    float currentHealth,
    float netHealthImpact,
    bool setNetHealthImpact,
    float newNetHealthImpact,
    float multiplier,
    float additiveValue)
    {
        if (setNetHealthImpact)
        {
            netHealthImpact = newNetHealthImpact;
        }

        currentHealth += netHealthImpact;

        currentHealth *= multiplier;
        currentHealth += additiveValue;

        return currentHealth;
    }
}
