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
    public float healthMultiplier = 1f;
    public int additiveHealthValue;

    public bool setThreatImpact;
    public int newThreatImpact;
    public float threatMultiplier = 1f;
    public int additiveThreatValue;

    public bool setAllegianceImpact;
    public int newAllegianceImpact;
    public float allegianceMultiplier = 1f;
    public int additiveAllegianceValue;
}
