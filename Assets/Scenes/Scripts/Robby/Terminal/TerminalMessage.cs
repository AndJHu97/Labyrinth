using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerminalMessage
{
    [SerializeField] string message;
    [SerializeField] string timeEntered;
    [SerializeField] int roundNumber;

    public string Message { get => message; set => message = value; }
    public string TimeEntered { get => timeEntered; set => timeEntered = value; }
    public int RoundNumber { get => roundNumber; set => roundNumber = value; }

    public TerminalMessage(string message, string timeEntered, int roundNumber) 
    {
        this.message = message;
        this.timeEntered = timeEntered;
        this.roundNumber = roundNumber;
    }
}
