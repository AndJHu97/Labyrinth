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
    public string targetPersonID;
    public int requiredValue;
    public ComparisonType comparison;
    public RelationshipType relationshipType;
}

[System.Serializable]
public class LivingCondition
{
    public string targetPersonID;
    public bool requiredAliveStatus;
}

[System.Serializable]
public struct RelationshipConsequence
{
    public string targetPersonID;
    public int valueChange;
    public RelationshipType relationshipType;
}

[System.Serializable]
public struct LivingConsequence
{
    public string targetPersonID;
    public bool setAliveStatus;
}

[System.Serializable]
public struct Condition
{
    public List<ItemSO> requiredItems;
    public List<ItemSO> mustBeAbsentItems;
    
    public List<RelationshipCondition> relationshipConditions;
    public List<LivingCondition> livingConditions;
    public List<PlayerIntentSO> requiredPlayerActions;
    public int groupNumber;
}

[System.Serializable]
public struct Consequence
{
    public List<ItemSO> itemsToAdd;
    public List<ItemSO> itemsToRemove;
    public List<RelationshipConsequence> relationshipConsequences;
    public List<LivingConsequence> livingConsequences;
    public List<string> texts;
}

[CreateAssetMenu(fileName = "ConditionConsequencesPairings", menuName = "Interactions/Condition Consequences Pairings")]
public class ConditionConsequencesPairingsSO : ScriptableObject
{
    public List<Condition> conditions;
    public List<Consequence> successfulConsequences;
    public List<Consequence> failingConsequences;
}
