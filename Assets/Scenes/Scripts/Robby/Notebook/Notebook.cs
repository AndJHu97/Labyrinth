using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public class NotebookSection
{
    public string id;      //"general" or the room's id. General is not room specific
    public string title;   // text shown on the tab. Room name or "General"
    public List<string> lines = new List<string>();
}

public class Notebook : MonoBehaviour
{
    // True while the player is typing a note. PlayerController ignores input while this is set.
    public static bool IsWriting { get; private set; }

    [Header("Display")]
    [SerializeField] TextMeshPro tabsText;       //the row of tab names (clickable)
    [SerializeField] TextMeshPro textMeshPro;    //the notes for the selected tab
    [Tooltip("The paper. Used to check the mouse is over the notebook when scrolling.")]
    [SerializeField] SpriteRenderer pageSprite;

    [Header("Keys")]
    [SerializeField] KeyCode toggleWritingKey = KeyCode.Tab;
    [SerializeField] KeyCode stopWritingKey = KeyCode.Escape;
    [SerializeField] KeyCode previousTabKey = KeyCode.LeftArrow;
    [SerializeField] KeyCode nextTabKey = KeyCode.RightArrow;
    [SerializeField] KeyCode pageUpKey = KeyCode.PageUp;
    [SerializeField] KeyCode pageDownKey = KeyCode.PageDown;

    [Header("Notes (visible here for debugging)")]
    [SerializeField] List<NotebookSection> sections = new List<NotebookSection>();
    [SerializeField] int tabIndex = 0;

    const string GeneralId = "general";

    [SerializeField] bool clickAwayStopsWriting = true;

    readonly List<NotebookSection> visibleTabs = new List<NotebookSection>();
    readonly StringBuilder sb = new StringBuilder();
    Room lastRoom;
    int notesPage = 1;   //which page of the notes is showing (starts at 1)

    [SerializeField] float cursorBlinkSeconds = 0.5f;
    float blinkTimer;
    bool cursorOn = true;

    NotebookSection CurrentSection =>
        (tabIndex >= 0 && tabIndex < visibleTabs.Count) ? visibleTabs[tabIndex] : null;

    void Awake()
    {
        // Notes that don't fit the box are split into pages we can flip through
        if (textMeshPro != null) textMeshPro.overflowMode = TextOverflowModes.Page;
    }

    void Start()
    {
        lastRoom = GameManager.Instance != null ? GameManager.Instance.currentRoom : null;
        RebuildTabs(true);
    }

    // Never leave the player locked out of the game controls
    void OnDisable()
    {
        IsWriting = false;
    }

    void Update()
    {
        // Notice when the player moves to another room
        Room room = GameManager.Instance != null ? GameManager.Instance.currentRoom : null;
        if (room != lastRoom)
        {
            lastRoom = room;
            RebuildTabs(true);   // jump to that room's tab
        }

        HandleScroll();
        HandleClicks();

        if (IsWriting) HandleWriting();
        else HandleBrowsing();

        if (IsWriting && cursorBlinkSeconds > 0f)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= cursorBlinkSeconds)
            {
                blinkTimer = 0f;
                cursorOn = !cursorOn;
                Redraw();
            }
        }
    }

    // ---------- Input ----------

    void HandleBrowsing()
    {
        if (Input.GetKeyDown(toggleWritingKey))
        {
            SetWriting(true);
            return;
        }

        if (Input.GetKeyDown(previousTabKey)) SwitchTab(-1);
        if (Input.GetKeyDown(nextTabKey)) SwitchTab(1);
    }

    void HandleWriting()
    {
        if (Input.GetKeyDown(stopWritingKey) || Input.GetKeyDown(toggleWritingKey))
        {
            SetWriting(false);
            return;
        }

        NotebookSection section = CurrentSection;
        if (section == null) return;

        bool changed = false;

        foreach (char c in Input.inputString)
        {
            if (c == '\n' || c == '\r')
            {
                EnsureLine(section);
                section.lines.Add("");               // Enter starts a new line
                changed = true;
            }
            else if (c == '\b')
            {
                changed |= Backspace(section);
            }
            else if (char.IsControl(c) || c == '<')  // '<' would be read as a text tag
            {
                continue;
            }
            else
            {
                EnsureLine(section);
                section.lines[section.lines.Count - 1] += c;
                changed = true;
            }
        }

        if (changed)
        {
            cursorOn = true;
            blinkTimer = 0f;
            Redraw(true);
        }
    }

    //Returns true if a tab name was clicked
    bool TryClickTab()
    {
        if (tabsText == null) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        int link = TMP_TextUtilities.FindIntersectingLink(tabsText, Input.mousePosition, cam);
        if (link < 0) return false;

        string linkId = tabsText.textInfo.linkInfo[link].GetLinkID();
        if (int.TryParse(linkId, out int index) && index >= 0 && index < visibleTabs.Count)
        {
            tabIndex = index;
            notesPage = 1;
            Redraw();
        }
        return true;
    }

    // Click the page to write, click elsewhere to go back to the game
    void HandleClicks()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (TryClickTab()) return;          // tab names switch tabs and don't change focus
        if (pageSprite == null) return;     // can't tell where the page is

        bool onPage = MouseOverPage();

        if (onPage && !IsWriting) SetWriting(true);
        else if (!onPage && IsWriting && clickAwayStopsWriting) SetWriting(false);
    }

    // Mouse wheel over the page, or PageUp / PageDown, flips between pages of notes
    void HandleScroll()
    {
        int delta = 0;

        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.01f && MouseOverPage()) delta = wheel < 0 ? 1 : -1;

        if (Input.GetKeyDown(pageDownKey)) delta = 1;
        if (Input.GetKeyDown(pageUpKey)) delta = -1;

        if (delta == 0) return;

        int target = Mathf.Clamp(notesPage + delta, 1, PageCount());
        if (target == notesPage) return;

        notesPage = target;
        Redraw();
    }

    bool MouseOverPage()
    {
        if (pageSprite == null) return true;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = pageSprite.bounds.center.z;
        return pageSprite.bounds.Contains(world);
    }

    static void EnsureLine(NotebookSection section)
    {
        if (section.lines.Count == 0) section.lines.Add("");
    }

    // Deletes the last character, or removes an empty last line
    static bool Backspace(NotebookSection section)
    {
        if (section.lines.Count == 0) return false;

        int last = section.lines.Count - 1;
        string line = section.lines[last];

        if (line.Length > 0) section.lines[last] = line.Substring(0, line.Length - 1);
        else section.lines.RemoveAt(last);

        return true;
    }

    void SetWriting(bool writing)
    {
        IsWriting = writing;

        if (writing)
        {
            cursorOn = true;
            blinkTimer = 0f;
            Redraw(true);
        }
        else RebuildTabs(false);   //hides tabs that ended up empty. So even visited room page thgat 
    }

    void SwitchTab(int delta)
    {
        if (visibleTabs.Count == 0) return;

        tabIndex = Mathf.Clamp(tabIndex + delta, 0, visibleTabs.Count - 1);
        notesPage = 1;
        Redraw();
    }

    // ---------- Tabs ----------

    // Tabs shown: General, the current room, and any other room that has notes
    void RebuildTabs(bool selectCurrentRoom)
    {
        NotebookSection previous = CurrentSection;   // remember what was selected
        visibleTabs.Clear();

        visibleTabs.Add(GetOrCreate(GeneralId, "General"));

        NotebookSection roomSection = null;
        if (lastRoom != null)
        {
            roomSection = GetOrCreate(RoomId(lastRoom), RoomTitle(lastRoom));
            visibleTabs.Add(roomSection);
        }

        foreach (var s in sections)
            if (!visibleTabs.Contains(s) && HasContent(s))
                visibleTabs.Add(s);

        NotebookSection target = (selectCurrentRoom && roomSection != null) ? roomSection : previous;
        tabIndex = Mathf.Max(0, visibleTabs.IndexOf(target));

        if (CurrentSection != previous) notesPage = 1;   // different tab, start at its first page

        Redraw();
    }

    NotebookSection GetOrCreate(string id, string title)
    {
        foreach (var s in sections)
        {
            if (s.id == id)
            {
                s.title = title;
                return s;
            }
        }

        var created = new NotebookSection { id = id, title = title };
        sections.Add(created);
        return created;
    }

    static string RoomId(Room room) => string.IsNullOrEmpty(room.id) ? room.gameObject.name : room.id;
    static string RoomTitle(Room room) => string.IsNullOrEmpty(room.name_) ? room.gameObject.name : room.name_;
    static bool HasContent(NotebookSection s) => s.lines.Exists(l => l.Length > 0);

    // ---------- Display ----------

    int PageCount()
    {
        return textMeshPro != null ? Mathf.Max(1, textMeshPro.textInfo.pageCount) : 1;
    }

    //followEnd = jump to the last page (used while typing)
    void Redraw(bool followEnd = false)
    {

        if (IsWriting) sb.Append(cursorOn ? "_" : "<alpha=#00>_");
        // Notes first, so the page count is known for the tab row
        if (textMeshPro != null)
        {
            sb.Clear();

            NotebookSection section = CurrentSection;
            if (section != null)
            {
                for (int i = 0; i < section.lines.Count; i++)
                {
                    if (i > 0) sb.Append('\n');
                    sb.Append(section.lines[i]);
                }

                if (IsWriting) sb.Append('_');   // cursor at the end of the last line
            }

            textMeshPro.text = sb.ToString();
            textMeshPro.ForceMeshUpdate();       // so the page count is up to date

            int pageCount = PageCount();
            notesPage = followEnd ? pageCount : Mathf.Clamp(notesPage, 1, pageCount);
            textMeshPro.pageToDisplay = notesPage;
        }

        if (tabsText != null)
        {
            sb.Clear();

            for (int i = 0; i < visibleTabs.Count; i++)
            {
                if (i > 0) sb.Append("   ");

                // The link tag is what makes the name clickable
                sb.Append("<link=\"").Append(i).Append("\">");
                if (i == tabIndex) sb.Append("<b><u>").Append(visibleTabs[i].title).Append("</u></b>");
                else sb.Append(visibleTabs[i].title);
                sb.Append("</link>");
            }

            if (PageCount() > 1) sb.Append("   page ").Append(notesPage).Append('/').Append(PageCount());
            if (IsWriting) sb.Append("   (writing - Esc to stop)");

            tabsText.text = sb.ToString();
        }
    }
}