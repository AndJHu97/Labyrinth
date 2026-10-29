using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public class NotebookSection
{
    public Room room;      //"general" or the room's id. General is not room specific
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


    [SerializeField] bool clickAwayStopsWriting = true;

    readonly List<NotebookSection> visibleTabs = new List<NotebookSection>();
    readonly StringBuilder sb = new StringBuilder();
    Room lastRoom;
    int notesPage = 1;   //which page of the notes is showing (starts at 1)

    [SerializeField] float cursorBlinkSeconds = 0.5f;
    float blinkTimer;
    bool cursorOn = true;

    int cursorLine = 0;
    int cursorColumn = 0;

    NotebookSection helpSection;

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

        RefreshItemsSection(GetItemsSection());
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

        // Help and Items are read-only.
        if (section == GetHelpSection() || section.title == "Items")
        {
            IsWriting = false;
            RebuildTabs(false);
            return;
        }

        // Help is read-only.
        if (section == GetHelpSection())
{
    IsWriting = false;
    RebuildTabs(false);
    return;
}

        EnsureLine(section);
        bool changed = false;

        // Move the cursor with the arrow keys.
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            changed = MoveCursorLeft(section);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            changed = MoveCursorRight(section);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            changed = MoveCursorVertical(section, -1);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            changed = MoveCursorVertical(section, 1);

        // Process typed characters.
        foreach (char c in Input.inputString)
        {
            if (c == '\n' || c == '\r')
            {
                string line = section.lines[cursorLine];

                // Split the current line at the cursor.
                string before = line.Substring(0, cursorColumn);
                string after = line.Substring(cursorColumn);

                section.lines[cursorLine] = before;
                section.lines.Insert(cursorLine + 1, after);

                cursorLine++;
                cursorColumn = 0;
                changed = true;
            }
            else if (c == '\b')
            {
                changed |= DeleteBeforeCursor(section);
            }
            else if (char.IsControl(c) || c == '<')
            {
                continue;
            }
            else
            {
                string line = section.lines[cursorLine];

                section.lines[cursorLine] =
                    line.Insert(cursorColumn, c.ToString());

                cursorColumn++;
                changed = true;
            }
        }

        if (changed)
        {
            cursorOn = true;
            blinkTimer = 0f;
            Redraw();
        }
    }


    bool MoveCursorLeft(NotebookSection section)
    {
        if (cursorColumn > 0)
        {
            cursorColumn--;
            return true;
        }

        // Move to the end of the previous line.
        if (cursorLine > 0)
        {
            cursorLine--;
            cursorColumn = section.lines[cursorLine].Length;
            return true;
        }

        return false;
    }

    bool MoveCursorRight(NotebookSection section)
    {
        if (cursorColumn < section.lines[cursorLine].Length)
        {
            cursorColumn++;
            return true;
        }

        // Move to the beginning of the next line.
        if (cursorLine < section.lines.Count - 1)
        {
            cursorLine++;
            cursorColumn = 0;
            return true;
        }

        return false;
    }

    bool MoveCursorVertical(NotebookSection section, int direction)
    {
        int targetLine = cursorLine + direction;

        if (targetLine < 0 || targetLine >= section.lines.Count)
            return false;

        cursorLine = targetLine;

        // Keep the cursor as close as possible to its current column.
        cursorColumn = Mathf.Min(
            cursorColumn,
            section.lines[cursorLine].Length
        );

        return true;
    }

    bool DeleteBeforeCursor(NotebookSection section)
    {
        string line = section.lines[cursorLine];

        if (cursorColumn > 0)
        {
            section.lines[cursorLine] =
                line.Remove(cursorColumn - 1, 1);

            cursorColumn--;
            return true;
        }

        // At the beginning of a line, merge it with the previous line.
        if (cursorLine > 0)
        {
            int previousLine = cursorLine - 1;
            cursorColumn = section.lines[previousLine].Length;

            section.lines[previousLine] += line;
            section.lines.RemoveAt(cursorLine);
            cursorLine = previousLine;

            return true;
        }

        return false;
    }

    NotebookSection GetHelpSection()
    {
        if (helpSection == null)
        {
            helpSection = new NotebookSection
            {
                room = null,
                title = "Help",
                lines = new List<string>
            {
                "How to Play",
                "",
                "-Use a combination of these 3 keys: 1-4 emotion, a-f act, g-l direction.",
                "",
                "-Whatever key you don't input will be randomly chosen.",
                "-Each combination combines the emotions, action, and directions... try them out. Inputs will look like 1ag or 4fk, etc.",
                "-Death is rewarded. Do not be afraid of making a mistake.",
                "-Take notes here of patterns you discover",
                "-Go through the rooms within this labyrinth. You will learn to die."
            }
            };
        }

        return helpSection;
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


    void SetWriting(bool writing)
    {
        IsWriting = writing;

        if (writing)
        {
            NotebookSection section = CurrentSection;

            if (section != null)
            {
                EnsureLine(section);
                cursorLine = section.lines.Count - 1;
                cursorColumn = section.lines[cursorLine].Length;
            }

            cursorOn = true;
            blinkTimer = 0f;
            Redraw();
        }
        else
        {
            RebuildTabs(false);
        }
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
        NotebookSection previous = CurrentSection;
        visibleTabs.Clear();

        // 1. General notes first
        visibleTabs.Add(GetOrCreate(null, "General"));

        // 2. Current room second
        NotebookSection roomSection = null;

        if (lastRoom != null)
        {
            roomSection = GetOrCreate(lastRoom, RoomTitle(lastRoom));
            visibleTabs.Add(roomSection);
        }

        // 3. Items third
        visibleTabs.Add(GetItemsSection());

        // 4. Help fourth
        visibleTabs.Add(GetHelpSection());

        // 5. Other room sections that contain notes
        foreach (var s in sections)
        {
            if (s.title == "Items") continue;

            if (!visibleTabs.Contains(s) && HasContent(s))
                visibleTabs.Add(s);
        }

        NotebookSection target =
            (selectCurrentRoom && roomSection != null)
                ? roomSection
                : previous;

        tabIndex = Mathf.Max(0, visibleTabs.IndexOf(target));

        if (CurrentSection != previous)
            notesPage = 1;

        Redraw();
    }

    NotebookSection GetOrCreate(Room room, string title)
    {
        foreach (var s in sections)
        {
            if (s.room == room)
            {
                s.title = title;
                return s;
            }
        }

        var created = new NotebookSection { room = room, title = title };
        sections.Add(created);
        return created;
    }

    //static string RoomId(Room room) => string.IsNullOrEmpty(room.id) ? room.gameObject.name : room.id;
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
        if (CurrentSection != null && CurrentSection.title == "Items")
        {
            RefreshItemsSection(CurrentSection);
        }
        // Notes first, so the page count is known for the tab row
        if (textMeshPro != null)
        {
            sb.Clear();

            NotebookSection section = CurrentSection;

            if (section == null) return;

         
            if (section != null)
            {

                for (int i = 0; i < section.lines.Count; i++)
                {
                    if (i > 0) sb.Append('\n');

                    string line = section.lines[i];

                    if (IsWriting && i == cursorLine)
                    {
                        int column = Mathf.Clamp(cursorColumn, 0, line.Length);

                        sb.Append(line.Substring(0, column));
                        sb.Append(cursorOn ? "_" : "<alpha=#00>_");
                        sb.Append(line.Substring(column));
                    }
                    else
                    {
                        sb.Append(line);
                    }
                }

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

    NotebookSection GetItemsSection()
    {
        NotebookSection itemsSection = sections.Find(s => s.title == "Inventory");

        if (itemsSection == null)
        {
            itemsSection = new NotebookSection
            {
                room = null,
                title = "Inventory",
                lines = new List<string>()
            };

            sections.Add(itemsSection);
        }

        RefreshItemsSection(itemsSection);
        return itemsSection;
    }

    void RefreshItemsSection(NotebookSection section)
    {
        Player player = GameManager.Instance != null ? GameManager.Instance.player : null;
        if (player == null)
            player = FindFirstObjectByType<Player>();

        section.lines.Clear();

        section.lines.Add("Inventory");
        section.lines.Add("");

        if (player == null)
        {
            section.lines.Add("Player not found.");
            return;
        }

        List<string> items = player.GetInventoryNotes();

        if (items.Count == 0)
        {
            section.lines.Add("No items.");
            return;
        }

        foreach (string itemName in items)
        {
            section.lines.Add("- " + itemName);
        }
    }
}