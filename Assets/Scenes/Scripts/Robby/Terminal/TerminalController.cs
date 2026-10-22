using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

//This listens to the game events and turns them into text for the terminal. It also listens to the player controller for typed input and displays it in the terminal.
//Listens to GameManger.OnTextDisplayed
public class TerminalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController playerController;
    [SerializeField] WorldTextScroller scroller;
    [SerializeField] SpriteRenderer backgroundSpriteRenderer;
    [SerializeField] GameObject textParent;   // don't use the object this script is on
    [SerializeField] TextMeshPro[] textMeshes;

    [Header("Look")]
    [SerializeField] Color consoleBackgroundColor = Color.black;
    [SerializeField] Color consoleFontColor = Color.white;
    [SerializeField] string inputPrefix = "> ";
    [SerializeField] bool showCursor = true;

    [Header("History")]
    [SerializeField] int maxLines = 200;
    [SerializeField] List<TerminalMessage> messages = new List<TerminalMessage>();

    [Header("Timeout (optional)")]
    [SerializeField] bool allowConsoleTimeout = false;
    [SerializeField] float consoleTimeout = 3f;
    [SerializeField] bool isVisible = true;
    [SerializeField] float timer;

    [Header("Debugging")]
    [SerializeField] bool showDebuggingMessages = false;
    [SerializeField] KeyCode keyToPressToCreateDebugMessage = KeyCode.Space;

    readonly StringBuilder sb = new StringBuilder();
    string currentInput = "";

    [SerializeField] float cursorBlinkSeconds = 0.5f;   // 0 = no blinking
    float blinkTimer;
    bool cursorOn = true;

    bool notebookWriting;   // the notebook's state the last time we looked

    // The cursor shows only if enabled in the inspector AND the notebook isn't being written in
    bool CursorEnabled => showCursor && !Notebook.IsWriting;

    TextMeshPro Text => (textMeshes != null && textMeshes.Length > 0) ? textMeshes[0] : null;

    void Awake()
    {
        if (backgroundSpriteRenderer != null) backgroundSpriteRenderer.color = consoleBackgroundColor;
        if (Text != null) Text.color = consoleFontColor;
    }

    void OnEnable()
    {
        //Give this the function to run. 
        GameManager.OnTextDisplayed += AddLine;

        //These are events that will be invoked when called in the player controller. This is how we get the typed input to show up in the terminal.
        if (playerController != null)
        {
            playerController.OnSubmitted += HandleSubmitted;
            playerController.OnBufferChanged += HandleBufferChanged;
        }
    }

    //Take out the event listeners when this object is disabled to avoid memory leaks
    void OnDisable()
    {
        GameManager.OnTextDisplayed -= AddLine;

        if (playerController != null)
        {
            playerController.OnSubmitted -= HandleSubmitted;
            playerController.OnBufferChanged -= HandleBufferChanged;
        }
    }

    void Start()
    {
        SetVisible(true);
        Refresh();
    }

    void Update()
    {

        if (allowConsoleTimeout && isVisible)
        {
            timer += Time.deltaTime;
            if (timer >= consoleTimeout) SetVisible(false);
        }

        // Redraw once when the notebook starts or stops being written in
        if (Notebook.IsWriting != notebookWriting)
        {
            notebookWriting = Notebook.IsWriting;
            cursorOn = true;      // cursor comes back solid, not mid-blink
            blinkTimer = 0f;
            Refresh(false);
        }

        if (CursorEnabled && cursorBlinkSeconds > 0f && isVisible)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= cursorBlinkSeconds)
            {
                blinkTimer = 0f;
                cursorOn = !cursorOn;
                Refresh(false);
            }
        }
    }

    void Refresh(bool snapToBottom = true)
    {
        var tmp = Text;
        if (tmp == null) return;

        sb.Clear();
        foreach (var m in messages) sb.AppendLine(m.Text);

        sb.Append(inputPrefix).Append(currentInput);

        //Not CursorEnabled? Then no cursor at all, not even a transparent one. Like when switching to notebook writing. CursorEnabled is true when showCursor is true AND the notebook isn't being written in.
        if (CursorEnabled) sb.Append(cursorOn ? "_" : "<alpha=#00>_");

        tmp.text = sb.ToString();

        if (scroller != null && snapToBottom) scroller.ScrollToBottom();
    }


    //Playercontroller events
    //Will add to message log and the current output
    void HandleSubmitted(string typed)
    {
        currentInput = "";
        AddLine(inputPrefix + typed);   // echo what the player entered
    }

    //Will update the current output to show what the player is typing
    void HandleBufferChanged()
    {
        currentInput = string.Concat(playerController.Buffer);
        Wake();
        Refresh();
    }

    //This is hooked onto the game manager event, so when showed display text is called, it will add the text to the terminal. This is also called by the player controller when the player submits typed input.
    //This doesn't use the game log. 
    public void AddLine(string text)
    {
        if (text == null) return;

        int round = GameManager.Instance != null ? GameManager.Instance.roundNumber : 1;

        //messages is the history of messages
        messages.Add(new TerminalMessage(text, round));

        while (messages.Count > maxLines) messages.RemoveAt(0);

        Wake();
        Refresh();
    }

    public void Clear()
    {
        messages.Clear();
        Refresh();
    }

    //Internals

    void Wake()
{
    timer = 0f;
    cursorOn = true;
    blinkTimer = 0f;
    if (!isVisible) SetVisible(true);
}

    void SetVisible(bool visible)
    {
        isVisible = visible;
        timer = 0f;

        if (textParent != null && textParent != gameObject) textParent.SetActive(visible);
        if (backgroundSpriteRenderer != null) backgroundSpriteRenderer.enabled = visible;
        if (Text != null) Text.enabled = visible;
    }

    void Refresh()
    {
        var tmp = Text;
        if (tmp == null) return;

        sb.Clear();
        foreach (var m in messages) sb.AppendLine(m.Text);

        //Live input line at the bottom
        sb.Append(inputPrefix).Append(currentInput);
        if (showCursor) sb.Append('_');

        tmp.text = sb.ToString();

        if (scroller != null) scroller.ScrollToBottom();
    }
}