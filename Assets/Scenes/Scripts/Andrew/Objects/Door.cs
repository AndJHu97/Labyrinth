using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Object_
{
    public Room nextRoom = null;
    public override bool CanReceiveActions() => true;   // locked doors still get kicked, picked, etc.

    public bool isFinalDoor = false; // If true, this door leads to the ending
    public override void ReceivePlayerAction(PlayerIntentSO playerIntentSO)
    {
        CheckAndRunConditions(playerIntentSO);

        stats.health += playerIntentSO.netHealthImpact;

        CheckEnter(playerIntentSO.motorSensorKey);
        CheckBrokenToOpen();
    }

    public void CheckEnter(MotorSensorKey motorSensoryKey)
    {
        if (isActive)
        {
            if(CanEnter(motorSensoryKey))
            {
                if (!isFinalDoor)
                {
                    Debug.Log($"Player has entered the next room {nextRoom.name_}");

                    GameManager.Instance.currentRoom = nextRoom;
                    GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"You entered the next room {nextRoom.name_}" }, this);
                    GameManager.Instance.LogDisplayTextsToRecentLog(nextRoom.roomDescription, this);

                    GameManager.Instance.CheckAndRunAllTriggers();
                }
                //Initiate ending
                else
                {
                    GameManager.Instance.WonGame();
                }
                
                
            }
        }
    }

    public void CheckBrokenToOpen()
    {
        if (!isActive)
        {
            if (stats.health <= 0)
            {
                Debug.Log($"{name_} is unlocked.");
                GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"{name_} is unlocked." }, this);
                isActive = true;
            }
            else
            {
                Debug.Log($"{name_} is locked.");
                //GameManager.Instance.LogDisplayTextsToRecentLog(new List<string> { $"{name_} is locked." }, this);
                isActive = false;
            }
        }
        
    }

    public bool CanEnter(MotorSensorKey motorSensorKey)
    {
        return isActive
            && nextRoom != null
            && (motorSensorKey == MotorSensorKey.Manipulate
                || motorSensorKey == MotorSensorKey.Arm
                || motorSensorKey == MotorSensorKey.Leg);
    }
}

