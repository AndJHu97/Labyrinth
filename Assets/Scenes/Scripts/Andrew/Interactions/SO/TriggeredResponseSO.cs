using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This happens every time step. Not only when acted on. Deleted and just use the consequencecondition instead. Keeping in case I need it. 
public class TriggeredResponseSO : ScriptableObject
{
    public List<Condition> conditions;
    public NPCActionResponseSO response;
    public string displayText;
}
