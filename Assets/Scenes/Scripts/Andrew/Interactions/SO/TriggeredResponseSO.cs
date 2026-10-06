using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TriggeredResponseSO", menuName = "Interactions/TriggeredResponseSO", order = 1)]
public class TriggeredResponseSO : ScriptableObject
{
    public List<Condition> conditions;
    public NPCActionResponseSO response;
    public string text;
}
