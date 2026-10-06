using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public stats playerStats;
    [SerializeField]
    public List<PlayerIntentSO> playerIntentions = new();
}
