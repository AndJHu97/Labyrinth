using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Objects/Item")]
public class ItemSO : ScriptableObject
{
    public string name_;
    public stats statsModification;
}
