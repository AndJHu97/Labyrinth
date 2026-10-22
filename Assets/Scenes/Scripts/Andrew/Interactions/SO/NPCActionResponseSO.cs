using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Action Response", menuName = "Interactions/NPC Action Response")]
public class NPCActionResponseSO : ScriptableObject
{
    public string name_;
    public List<string> displayTexts;
    public int netHealthImpactOnPlayer;

    public bool setNewSelfHealthImpact;
    public int newSelfHealthImpact;
    public int additiveSelfHealthValueFromPlayer;

    public bool setNewSelfThreatImpact;
    public int newSelfThreatImpact;
    public int additiveSelfThreatValueFromPlayer;

    public bool setNewSelfAllegianceImpact;
    public int newSelfAllegianceImpact;
    public int additiveSelfAllegianceValueFromPlayer;
}
