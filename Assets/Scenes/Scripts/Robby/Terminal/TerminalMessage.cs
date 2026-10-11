using System;
using UnityEngine;

[Serializable]
public class TerminalMessage
{
    [SerializeField] string text;
    [SerializeField] string timeEntered;
    [SerializeField] int roundNumber;

    public string Text => text;
    public string TimeEntered => timeEntered;
    public int RoundNumber => roundNumber;

    public TerminalMessage(string text, int roundNumber)
    {
        this.text = text;
        this.roundNumber = roundNumber;
        timeEntered = DateTime.Now.ToString("HH:mm:ss");
    }
}