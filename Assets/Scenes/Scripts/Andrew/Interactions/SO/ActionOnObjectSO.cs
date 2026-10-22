using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action On NPC", menuName = "Interactions/Action On NPC")]
public class ActionOnObjectSO : ScriptableObject
{
    public PlayerIntentSO playerIntent;
    public string name_;
    public List<string> defaultInteractingDisplayText;
    public bool overrideIntentsHealthImpact;
    public bool setHealthValue;
    public int newHealthValue;
    public int overrideNewHealthImpact;

    public bool overrideIntentsThreatImpact;
    public bool setThreatValue;
    public int newThreatValue;
    public int overrideNewThreatImpact;

    public bool overrideIntentsAllegianceImpact;
    public bool setAllegianceValue;
    public int newAllegianceValue;
    public int overrideAllegianceImpact;

}
