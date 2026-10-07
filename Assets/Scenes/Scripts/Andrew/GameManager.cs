using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameState
{
    public int roundNumber;
    public int actionStep;
    public int totalActionStep;
    public PlayerIntentSO playerIntent;
    public RoomSO room;

    public string targetID;
}

public class GameManager : MonoBehaviour
{
    public Player player;
    public int roundNumber = 1;
    public int actionStep = 0;
    public int totalActionStep = 0;
    public RoomSO currentRoom = null;
    public List<GameState> gameLog = new();
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayerDeath()
    {
        Debug.Log("Player has died.");
    }
}
