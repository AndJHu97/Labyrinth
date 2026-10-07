using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Person", menuName = "Objects/Person")]
public class PersonSO : ScriptableObject
{
    public string personID;
    public string name_;
    public stats personStats;
    public float threatRelationshipValue;
    public float allegianceRelationshipValue;
    public bool isActive;
    public float learningRate = 0.1f;
    public int experiencePointsGainedIfKilled = 5;

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
}
