using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct DisplayTextResponse
{
    public bool UseEmotionKey;
    public EmotionKey emotionKey;
    public bool UseMotorSensorKey;
    public MotorSensorKey motorSensorKey;
    public List<string> displayTexts;
}
public class Object_ : MonoBehaviour
{
    // Start is called before the first frame update
    public string id;
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
        ObjectRegistry.Register(id, this);
    }

    protected virtual void OnDestroy()
    {
        ObjectRegistry.Unregister(id, this);
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



    public void CheckAndRunTriggerConditions(PlayerIntentSO intent)
    {
        foreach (var conditionConsequence in triggeredResponses)
        {
            InteractionProcessing.ProcessPairing(conditionConsequence, this);
        }


    }

   
}
