using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct DisplayTextResponse
{
    public string name_;
    public bool UseEmotionKey;
    public EmotionKey emotionKey;
    public bool UseMotorSensorKey;
    public MotorSensorKey motorSensorKey;
    public List<string> displayTexts;
}
public class Object_ : MonoBehaviour
{
    // Start is called before the first frame update
    public string name_;
    public stats stats;

    public float threatRelationshipValue;
    public float allegianceRelationshipValue;
    public bool isActive = true;
    
    public List<ConditionConsequencePairings> conditionConsequences = new();
    public List<ConditionConsequencePairings> triggeredResponses = new();

    public List<DisplayTextResponse> UniqueResponseToPlayerDisplayTexts = new();
    public List<ActionOnObjectSO> allActionsOnObject = new List<ActionOnObjectSO>();
    protected virtual void Awake()
    {
        ObjectRegistry.Register(this);
    }

    protected virtual void OnDestroy()
    {
        ObjectRegistry.Unregister(this);
    }

    public virtual void ReceivePlayerAction(PlayerIntentSO intent)
    {
        CheckAndRunConditions(intent);
        
    }



    public void CheckAndRunConditions(PlayerIntentSO intent)
    {
        Debug.Log($"{name_}: Checking intent '{intent.name}'");

        foreach (var pairing in conditionConsequences)
        {
            Debug.Log($"{name_}: Checking pairing");

            if (pairing.conditions == null || pairing.conditions.Count == 0)
            {
                Debug.LogWarning($"{name_}: Pairing has no conditions.");
                continue;
            }

            foreach (var condition in pairing.conditions)
            {
                bool passed = InteractionProcessing.ProcessConditions(
                    new List<Condition> { condition }
                );

                Debug.Log(
                    $"{name_}: Condition group {condition.groupNumber} " +
                    $"individually passed = {passed}"
                );
            }

            bool allPassed = InteractionProcessing.ProcessConditions(
                pairing.conditions
            );

            Debug.Log($"{name_}: Entire pairing passed = {allPassed}");

            if (allPassed)
            {
                InteractionProcessing.ApplyConsequences(
                    pairing.successfulConsequences,
                    this
                );
            }
        }
    }



    public void CheckAndRunTriggerConditions()
    {
        foreach (var conditionConsequence in triggeredResponses)
        {
            InteractionProcessing.ProcessPairing(conditionConsequence, this);
        }


    }

    // Can the player interact with this object right now?
    public virtual bool CanReceiveActions()
    {
        if (isActive) return true;

        // Inactive objects only respond if one of their own conditions
        // explicitly targets itself with "required active status = false"
        return HasSelfInactiveCondition(conditionConsequences) ||
               HasSelfInactiveCondition(triggeredResponses);
    }

    bool HasSelfInactiveCondition(List<ConditionConsequencePairings> pairings)
    {
        foreach (var pairing in pairings)
        {
            if (pairing.conditions == null) continue;
            foreach (var condition in pairing.conditions)
            {
                if (condition.activeConditions == null) continue;
                foreach (var ac in condition.activeConditions)
                    if (ac.target == this && !ac.requiredActiveStatus) return true;
            }
        }
        return false;
    }
}
