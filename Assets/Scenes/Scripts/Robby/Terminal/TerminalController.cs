using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TerminalController : MonoBehaviour
{
    [SerializeField] bool allowConsoleTimeout = false;
    [SerializeField] float consoleTimeout = 3f;
    [SerializeField] Color consoleBackgroundColor = Color.black;
    [SerializeField] Color consoleFontColor = Color.white;
    [Space]
    [SerializeField] bool showDebuggingMessages = false;
    [SerializeField] KeyCode keyToPressToCreateDebugMessage = KeyCode.Space;
    [SerializeField] string debuggingMessage = "This is a test and I'll make sure this works!";
    [Space]
    [SerializeField] KeyCode consolePreviousLineCode = KeyCode.UpArrow;
    [SerializeField] KeyCode consoleNextLineCode = KeyCode.DownArrow;
    [Space]
    [SerializeField] SpriteRenderer backgroundSpriteRenderer;
    [SerializeField] GameObject textParent;
    [Space]
    [SerializeField] bool isVisible = false;
    [SerializeField] float timer;
    [Space]
    [SerializeField] TextMeshPro[] textMeshes;
    [SerializeField] List<TerminalMessage> messages = new List<TerminalMessage>();
    [SerializeField] int currentLine = 0;

    Color baseConsoleBGColor;
    Color baseConsoleFontColor;
    bool isFirstTime = true;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(keyToPressToCreateDebugMessage))
        {
            WriteLine("MESSAGE TESTING please work please work................................................Is this really going to work?  I need to work if paragraphs are going to work right");
        }
    }

    public void WriteLine(string message)
    {
        if (!isFirstTime)
        {
            textMeshes[0].text += "\n";
        }
        else
            isFirstTime = false;

        Write(message);
    }

    public void Write(string message)
    {
        var terminalMessage = new TerminalMessage(message, DateTime.Now.TimeOfDay.ToString(), 1);

        messages.Add(terminalMessage);
        textMeshes[0].text += BuildLineString(message);
    }

    string BuildLineString(string message)
    {
        string result = "Username> " + message;
        return result;
    }
}
