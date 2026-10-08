using System.Collections.Generic;
using UnityEngine;

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
        CheckConditions(intent);
    }

    public void CheckConditions(PlayerIntentSO intent)
    {
        foreach (var conditionConsequence in conditionConsequences)
        {

            InteractionProcessing.ProcessPairing(conditionConsequence);
        }
        Debug.Log($"{name_} has no reaction to '{intent.name}'.");
    }

    public void CheckTriggerConditions(PlayerIntentSO intent)
    {
        foreach (var conditionConsequence in triggeredResponses)
        {
            InteractionProcessing.ProcessPairing(conditionConsequence);
        }
    }
}
