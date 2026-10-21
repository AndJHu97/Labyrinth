using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Action Response", menuName = "Interactions/NPC Action Response")]
public class NPCActionResponseSO : ScriptableObject
{
    public string name_;
    public List<string> displayTexts;
    public int netHealthImpactOnPlayer;

    public bool setHealthImpact;
    public int newHealthImpact;
    public int additiveHealthValue;

    public bool setThreatImpact;
    public int newThreatImpact;
    public int additiveThreatValue;

    public bool setAllegianceImpact;
    public int newAllegianceImpact;
    public int additiveAllegianceValue;
}
