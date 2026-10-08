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
    public static int FORTITUDECUTOFF_LOW = 3;
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

    public static bool ProcessConditions(List<Condition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;

        var gm = GameManager.Instance;
        var groupResults = new Dictionary<int, bool>();

        foreach (var condition in conditions)
        {
            if (!groupResults.TryGetValue(condition.groupNumber, out bool groupPassing))
                groupPassing = true;

            if (groupPassing)
                groupPassing = EvaluateCondition(condition, gm.player, gm);

            groupResults[condition.groupNumber] = groupPassing;
        }

        foreach (var result in groupResults.Values)
            if (result) return true;

        return false;
    }

    private static int GetStat(stats s, StatType type)
    {
        switch (type)
        {
            case StatType.Health: return s.health;
            case StatType.Fortitude: return s.fortitude;
            case StatType.Strength: return s.strength;
            case StatType.Agility: return s.agility;
            default: return 0;
        }
    }
    private static bool EvaluateCondition(Condition c, Player player, GameManager gm)
    {
        if (c.requiredItems != null)
            foreach (var item in c.requiredItems)
                if (!player.inventory.Contains(item)) return false;

        if (c.mustBeAbsentItems != null)
            foreach (var item in c.mustBeAbsentItems)
                if (player.inventory.Contains(item)) return false;

        if (c.relationshipConditions != null)
        {
            foreach (var rc in c.relationshipConditions)
            {
                Object_ target = rc.target;
                if (target == null)
                {
                    Debug.LogWarning($"Condition: no Person with id '{rc.target}' for relationship check.");
                    return false;
                }

                float value = rc.relationshipType == RelationshipType.Allegiance
                    ? target.allegianceRelationshipValue
                    : target.threatRelationshipValue;

                if (!Compare(value, rc.comparison, rc.requiredValue)) return false;
            }
        }

        if (c.activeConditions != null)
        {
            foreach (var ac in c.activeConditions)
            {
                Object_ target = ac.target;
                if (target == null)
                {
                    Debug.LogWarning($"Condition: no Object with id '{ac.target}' for active check.");
                    return false;
                }
                if (target.isActive != ac.requiredActiveStatus) return false;
            }
        }

        // Player stat <comparison> target's stat
        if (c.statChecks != null)
        {
            foreach (var sc in c.statChecks)
            {
                Object_ target = sc.target;
                if (target == null)
                {
                    Debug.LogWarning($"Condition: no Object with id '{sc.target.name_}' for stat check.");
                    return false;
                }

                int playerValue = GetStat(player.playerStats, sc.statType);
                int targetValue = GetStat(target.stats, sc.statType);

                if (!Compare(playerValue, sc.comparison, targetValue)) return false;
            }
        }

        if (c.actionConditions != null)
            foreach (var ac in c.actionConditions)
                if (!EvaluateActionCondition(ac, gm)) return false;

        return true;
    }

    private static bool EvaluateActionCondition(ActionCondition ac, GameManager gm)
    {
        int matchCount = 0;

        foreach (var entry in gm.gameLog)
        {
            // WHAT (each filter is optional; off = any)
            if (ac.useEmotionKey && entry.emotionKey != ac.emotionKey) continue;
            if (ac.useMotorSensorKey && entry.motorSensorKey != ac.motorSensorKey) continue;
            if (ac.useConcentrationKey && entry.concentrationKey != ac.concentrationKey) continue;
            if (ac.room != null && entry.room != ac.room) continue;

            // Frequency scope
            if (ac.useFrequency && ac.frequencyScope == FrequencyScope.CurrentRound
                && entry.roundNumber != gm.roundNumber) continue;

            // WHEN
            if (ac.useRound && !MatchesRound(ac, entry, gm)) continue;
            if (ac.useStep && !MatchesStep(ac, entry, gm)) continue;

            matchCount++;
        }

        if (ac.useFrequency)
            return Compare(matchCount, ac.frequencyComparison, ac.frequency);

        return matchCount > 0;
    }

    private static bool MatchesRound(ActionCondition ac, GameState entry, GameManager gm)
    {
        switch (ac.roundReference)
        {
            case RoundReference.AbsoluteRound:
                return Compare(entry.roundNumber, ac.roundComparison, ac.roundValue);
            case RoundReference.RoundsAgo:
                return Compare(entry.roundNumber, ac.roundComparison, gm.roundNumber - ac.roundValue);
            case RoundReference.CurrentRound:
                return entry.roundNumber == gm.roundNumber;
            default:
                return false;
        }
    }

    private static bool MatchesStep(ActionCondition ac, GameState entry, GameManager gm)
    {
        switch (ac.stepReference)
        {
            case StepReference.StepInRound:
                return Compare(entry.actionStep, ac.stepComparison, ac.stepValue);
            case StepReference.StepsBeforeDeath:
                int stepsBeforeEnd = GetLastStepOfRound(gm, entry.roundNumber) - entry.actionStep;
                return Compare(stepsBeforeEnd, ac.stepComparison, ac.stepValue);
            case StepReference.StepsAgo:
                return Compare(gm.totalActionStep - entry.totalActionStep, ac.stepComparison, ac.stepValue);
            case StepReference.AbsoluteStep:
                return Compare(entry.totalActionStep, ac.stepComparison, ac.stepValue);
            default:
                return false;
        }
    }

    private static int GetLastStepOfRound(GameManager gm, int round)
    {
        int last = 0;
        foreach (var e in gm.gameLog)
            if (e.roundNumber == round && e.actionStep > last) last = e.actionStep;
        return last;
    }

    private static bool Compare(float actual, ComparisonType comparison, float target)
    {
        switch (comparison)
        {
            case ComparisonType.GreaterThanOrEqual: return actual >= target;
            case ComparisonType.LessThanOrEqual: return actual <= target;
            case ComparisonType.Equal: return Mathf.Approximately(actual, target);
            case ComparisonType.NotEqual: return !Mathf.Approximately(actual, target);
            case ComparisonType.GreaterThan: return actual > target;
            case ComparisonType.LessThan: return actual < target;
            default: return false;
        }
    }

    public static void ApplyConsequences(List<Consequence> consequences)
    {
        if (consequences == null) return;

        foreach (var consequence in consequences)
            ApplyConsequence(consequence);
    }

    public static void ApplyConsequence(Consequence c)
    {
        var player = GameManager.Instance.player;

        // Items
        if (c.itemsToAdd != null)
            foreach (var item in c.itemsToAdd)
                player.inventory.Add(item);

        if (c.itemsToRemove != null)
            foreach (var item in c.itemsToRemove)
                if (!player.inventory.Remove(item))
                    Debug.LogWarning($"Consequence: tried to remove '{item}' but it isn't in the inventory.");

        // Relationships (clamped 0-100, same as CalculateNewRelationshipValue)
        if (c.relationshipConsequences != null)
        {
            foreach (var rc in c.relationshipConsequences)
            {
                Object_ target = rc.target;
                if (target == null)
                {
                    Debug.LogWarning($"Consequence: no Person with id '{rc.target.name_}' for relationship change.");
                    continue;
                }

                if (rc.relationshipType == RelationshipType.Allegiance)
                    target.allegianceRelationshipValue = Mathf.Clamp(target.allegianceRelationshipValue + rc.valueChange, 0f, 100f);
                else
                    target.threatRelationshipValue = Mathf.Clamp(target.threatRelationshipValue + rc.valueChange, 0f, 100f);
            }
        }

        // Active status
        if (c.activeConsequences != null)
        {
            foreach (var ac in c.activeConsequences)
            {
                Object_ target = ac.target;
                if (target == null)
                {
                    Debug.LogWarning($"Consequence: no Object with id '{ac.target.name_}' for active change.");
                    continue;
                }
                target.isActive = ac.setActiveStatus;
            }
        }

        if (c.experiencePointsChange != 0)
            player.GainExperience(c.experiencePointsChange);

        // Display text
        if (c.displayTexts != null)
        {
            foreach (var text in c.displayTexts)
            {
                Debug.Log(text);
                //OnDisplayText?.Invoke(text);
            }
        }

        // Health changes
        if (c.netHealthImpact != null)
        {
            foreach (var hi in c.netHealthImpact)
            {
                if (hi.isPlayer)
                {
                    player.playerStats.health += hi.healthChange;

                }
                else
                {
                    Object_ target = hi.target;
                    if (target == null)
                    {
                        Debug.LogWarning($"Consequence: no Object with id '{hi.target.name_}' for health change.");
                        continue;
                    }

                    target.stats.health += hi.healthChange;
                }
            }
        }
    }

    // Convenience: check conditions, then apply the matching consequence list
    public static bool ProcessPairing(ConditionConsequencePairings pairing)
    {
        bool success = ProcessConditions(pairing.conditions);
        ApplyConsequences(success ? pairing.successfulConsequences : pairing.failingConsequences);
        return success;
    }
}
