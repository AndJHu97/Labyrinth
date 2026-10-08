using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Object_
{
    public Room nextRoom = null;

    public override void ReceivePlayerAction(PlayerIntentSO playerIntentSO)
    {
        CheckConditions(playerIntentSO);

        stats.health += playerIntentSO.netHealthImpact;

        CheckEnter(playerIntentSO.motorSensorKey);
        CheckBrokenToOpen();
    }

    public void CheckEnter(MotorSensorKey motorSensoryKey)
    {
        if (isActive)
        {
            if(motorSensoryKey == MotorSensorKey.Manipulate || motorSensoryKey == MotorSensorKey.Arm || motorSensoryKey == MotorSensorKey.Leg)
            {
                Debug.Log($"Player has entered the next room {nextRoom.name_}");
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
                isActive = true;
            }
            else
            {
                Debug.Log($"{name_} is locked.");
                isActive = false;
            }
        }
        
    }
}

