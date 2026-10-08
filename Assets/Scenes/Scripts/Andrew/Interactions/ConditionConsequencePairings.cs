using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
public enum RelationshipType
{
    Allegiance,
    Threat
}

public enum ComparisonType
{
    GreaterThanOrEqual,
    LessThanOrEqual,
    Equal,
    NotEqual,
    GreaterThan,
    LessThan
}


[System.Serializable]
public class RelationshipCondition
{
    public Object_ target;
    public int requiredValue;
    public ComparisonType comparison;
    public RelationshipType relationshipType;
}

[System.Serializable]
public class ActiveCondition
{
    public Object_ target;
    public bool requiredActiveStatus;
}

[System.Serializable]
public struct RelationshipConsequence
{
    public Object_ target;
    public int valueChange;
    public RelationshipType relationshipType;
}

[System.Serializable]
public struct ActiveConsequence
{
    public Object_ target;
    public bool setActiveStatus;
}

[System.Serializable]
public enum RoundReference
{
    AbsoluteRound,      // Round 2
    RoundsAgo,          // 1 round ago
    CurrentRound        // Current round
}

[System.Serializable]
public enum StepReference
{
    StepInRound,        // Step 3 of the round
    StepsBeforeDeath,   // 3 steps before dying
    StepsAgo,           // 10 total steps ago
    AbsoluteStep        // Absolute total step 50
}

[System.Serializable]
public enum FrequencyScope
{
    AllTime,
    CurrentRound
}

[System.Serializable]
public struct ActionCondition
{
    public string name_;
    // WHAT
    public bool useEmotionKey;
    public EmotionKey emotionKey;
    public bool useMotorSensorKey;
    public MotorSensorKey motorSensorKey;
    public bool useConcentrationKey;
    public ConcentrationKey concentrationKey;
    public Room room;

    // FREQUENCY
    public bool useFrequency;
    public int frequency;
    public ComparisonType frequencyComparison;
    public FrequencyScope frequencyScope;

    // ROUND
    public bool useRound;
    public RoundReference roundReference;
    public int roundValue;
    public ComparisonType roundComparison;

    // STEP
    public bool useStep;
    public StepReference stepReference;
    public int stepValue;
    public ComparisonType stepComparison;
}

[System.Serializable]
public struct StatCheck
{
    public StatType statType;
    public ComparisonType comparison;
    public Object_ target;
}

[System.Serializable]
public struct Condition
{
    public string name_;
    public List<ItemSO> requiredItems;
    public List<ItemSO> mustBeAbsentItems;
    public List<ActionCondition> actionConditions;
    public List<RelationshipCondition> relationshipConditions;
    public List<ActiveCondition> activeConditions;
    public List<StatCheck> statChecks;
    public int groupNumber;
}

[System.Serializable]
public struct HealthImpact
{
    public bool isPlayer;
    public int healthChange;
    public Object_ target;
}

[System.Serializable]
public struct Consequence
{
    public string name_;
    public List<ItemSO> itemsToAdd;
    public List<ItemSO> itemsToRemove;
    public List<RelationshipConsequence> relationshipConsequences;
    public List<ActiveConsequence> activeConsequences;
    public List<HealthImpact> netHealthImpact;
    public List<string> displayTexts;
    public int experiencePointsChange;
}

[System.Serializable]
public struct ConditionConsequencePairings
{
    public string name_;
    public List<Condition> conditions;
    public List<Consequence> successfulConsequences;
    public List<Consequence> failingConsequences;
}
