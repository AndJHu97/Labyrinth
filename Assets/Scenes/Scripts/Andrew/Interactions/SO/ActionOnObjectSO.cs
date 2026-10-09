using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action On NPC", menuName = "Interactions/Action On NPC")]
public class ActionOnObjectSO : ScriptableObject
{
    public PlayerIntentSO playerIntent;
    public string name_;
    public List<string> defaultInteractingDisplayText;
    public bool setHealthValue;
    public int newHealthValue;

    public float healthMultiplier = 1f;
    public int additiveHealthValue;

    public bool setThreatValue;
    public int newThreatValue;
    public float threatMultiplier = 1f;
    public int additiveThreatValue;

    public bool setAllegianceValue;
    public int newAllegianceValue;
    public float allegianceMultiplier = 1f;
    public int additiveAllegianceValue;

}
